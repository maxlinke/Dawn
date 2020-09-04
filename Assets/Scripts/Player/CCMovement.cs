using System.Collections.Generic;
using UnityEngine;
using CustomInputSystem;

namespace PlayerController {

    public class CCMovement : Movement {
        
        [SerializeField] CharacterController cc = default;
        
        protected override Transform PlayerTransform => cc.transform;

        public override float LocalColliderHeight => cc.height;
        public override float LocalColliderRadius => cc.radius;
        public override Vector3 LocalColliderCenter => cc.center;

        public override Vector3 Velocity {
            get { return m_velocity; }
            set { m_velocity = value; }
        }

        public override ControlMode controlMode {
            get { return m_controlMode; }
            set { m_controlMode = value; }
        }

        bool initialized = false;
        ControlMode m_controlMode = ControlMode.FULL;
        
        List<CollisionPoint> contactPoints;
        Vector3 m_velocity;

        [Multiline] public string DEBUGOUTPUTSTRINGTHING;

        public void Initialize (PlayerControllerProperties pcProps, Transform head, Transform model) {
            base.Init(pcProps, head, model);
            contactPoints = new List<CollisionPoint>();
            initialized = true;
        }

        // TODO dodge is separate from rest
        // accelerate to dodge speed (or decelerate, but there is a maximum amount of time that acceleration can take. fuck it. it's a timed acceleration)
        // then keep that speed (not actively tho)
        // no gravity while dodge
        // not even input
        // at the end lerp speed to walk speed and done
        
        // TODO no input means auto-uncrouch (if possible)

        public void Move (bool readInput) {
            if(!initialized){
                Debug.LogWarning($"{nameof(CCMovement)} isn't initialized yet!");
                return;
            }
            StartMove(out var currentState);
            switch(controlMode){
                case ControlMode.FULL:
                    ExecuteMove(readInput, currentState);
                    break;
                case ControlMode.BLOCK_INPUT:
                    ExecuteMove(false, currentState);
                    break;
                case ControlMode.ANCHORED:
                    Velocity = Vector3.zero;
                    break;
                default:
                    Debug.LogError($"Unknown {nameof(ControlMode)} \"{controlMode}\"!");
                    Velocity = Vector3.zero;
                    break;
            }
            FinishMove(currentState);
        }

        void StartMove (out MoveState currentState) {
            var EMPTYTRIGGERLIST = new List<Collider>();
            currentState = GetCurrentState(contactPoints, EMPTYTRIGGERLIST);
            contactPoints.Clear();
        }

        void FinishMove (MoveState currentState) {
            lastState = currentState;
        }

        void ExecuteMove (bool readInput, MoveState currentState) {
            switch(currentState.moveType){
                case MoveType.AIR:
                    AerialMovement(readInput, currentState);
                    break;
                case MoveType.GROUND:                      // TODO ground slide
                    GroundedMovement(readInput, currentState);
                    break;
                default:
                    Debug.LogError($"Unknown {nameof(MoveType)} \"{currentState.moveType}\"!");
                    Velocity += Physics.gravity * Time.deltaTime;
                    break;
            }
            if(Input.GetKeyDown(KeyCode.Alpha1)){
                Application.targetFrameRate = -1;
                QualitySettings.vSyncCount = 0;
            }
            if(Input.GetKeyDown(KeyCode.Alpha2)){
                Application.targetFrameRate = 60;
                QualitySettings.vSyncCount = 0;
            }
            if(Input.GetKeyDown(KeyCode.Alpha3)){
                Application.targetFrameRate = 20;
                QualitySettings.vSyncCount = 0;
            }
            if(Input.GetKey(KeyCode.Mouse0)){
                Velocity = cc.transform.TransformDirection(GetLocalSpaceMoveInput()) * pcProps.Ground.Speed * pcProps.RunSpeedMultiplier;
                // if(Input.GetKey(KeyCode.Mouse1)){
                //     Velocity += cc.transform.up * 0.1f;
                // }
                if(Input.GetKey(KeyCode.Space)){
                    Velocity += cc.transform.up;
                }
                if(Input.GetKey(KeyCode.LeftShift)){
                    Velocity -= cc.transform.up;
                }
            }
            cc.Move(Velocity * Time.deltaTime); // only slides along walls, not the ground, so gravity sliding will have to be implemted explicitly
        }

        void ApplyDrag (float drag, ref Vector3 localVelocity) {
            ApplyDrag(drag, Time.deltaTime, ref localVelocity);
        }

        // TODO player debug screen (mostly line graphs)
        // -> move type (colored line? different heights as well?)
        // -> speed/Velocity magnitude (as line graph) and as number (without gravity?) (no, after move the gravity velocity won't be there if grounded anyways...)
        // -> acceleration (as line graph)
        // for the speed/acceleration stuff somehow make it work with the deltatime kinda?
        // either it doesn't have an update and gets executed BY the player, or it executes automatically after the player (script execution order and such)
        // player needs singleton? or the player registers to the debug thing...

        // TODO make some in-world / in-game modifiable world things
        // -> slopes (manual and presets)
        // -> steps  (manual and presets)
        //    - as one mesh and as a lot of cubes?
        // -> terrain (just less-simple geometry to walk on...) (NOT UNITY TERRAIN THO!!!!)
        // make an area with sloped ground

        // TODO what exactly counts as a wall? slope limit? investigate!!!
        // and does the slope limit mean you cant walk straight into a slope and go up or what?
        // slope limit zero means moving in the cc plane only will not get me up slopes
        // BUT if i have even the SLIGHTEST amount of velocity upwards, i WILL go up slopes, no matter the limit

        // TODO custom step offset implementation
        // because high framerate -> bounce off corners and move very slowly
        // low framerate -> butter smooth stepping
        // stepping works best when moving straight into the thing
        // ----> if moving into point and normal faces me, move forward*, otherwise, move along ground? <-----
        // determine if it's a face or an edge first via raycast(s)
        // it it's an edge AND IT'S BELOW THE STEP LIMIT then move forward
        // otherwise, always do the velocity project stuff
        // TODO
        // TODO more stuff in OnControllerColliderHit
        // TODO
        // what about the OnControllerColliderHit velocity projection?

        // TODO test movement with controller for jerkiness (0.5 input) because friction/drag etc

        // TODO slope limit, custom gravity (might not need custom gravity because of the way the charactercontroller handles downward collisions...)
        void GroundedMovement (bool readInput, MoveState currentState) {
            var localVelocity = currentState.incomingLocalVelocity;
            ApplyDrag(pcProps.Ground.Drag, ref localVelocity);
            var localSpeed = localVelocity.magnitude;
            var rawInput = (readInput ? GetLocalSpaceMoveInput() : Vector3.zero);
            var rawInputMag = rawInput.magnitude;
            var targetSpeed = Mathf.Max(pcProps.Ground.Speed * RawSpeedMultiplier(GetRunInput(readInput)), localSpeed);
            var targetDirection = GroundMoveVector(cc.transform.TransformDirection(rawInput), currentState.surfacePoint.normal);
            var targetVelocity = targetDirection.normalized * rawInputMag * targetSpeed;
            var moveAccel = ClampedDeltaVAcceleration(localVelocity, targetVelocity, rawInputMag * pcProps.Ground.Accel, Time.deltaTime);
            if(readInput && Bind.JUMP.GetKeyDown()){
                moveAccel = new Vector3(moveAccel.x, JumpSpeed() / Time.deltaTime, moveAccel.z);
            }
            Velocity += moveAccel * Time.deltaTime;
            Velocity += Physics.gravity * Time.deltaTime;
        }

        // TODO limit fall velocity? only in aerial or everywhere?
        void AerialMovement (bool readInput, MoveState currentState) {
            var horizontalLocalVelocity = HorizontalComponent(currentState.incomingLocalVelocity);
            // var decelFactor = (hVelocityMag > pcProps.MoveSpeed) ? 1 : (1f - rawInputMag);
            var dragDeceleration = ClampedDeltaVAcceleration(horizontalLocalVelocity, Vector3.zero, pcProps.Air.Drag, Time.deltaTime);
            dragDeceleration *= Time.deltaTime;
            Velocity += dragDeceleration;
            horizontalLocalVelocity += dragDeceleration;
            var horizontalLocalSpeed = horizontalLocalVelocity.magnitude;
            DEBUGOUTPUTSTRINGTHING = $"{horizontalLocalSpeed.ToString():F2}";
            var rawInput = (readInput ? GetLocalSpaceMoveInput() : Vector3.zero);
            var rawInputMag = rawInput.magnitude;
            var targetSpeed = Mathf.Max(pcProps.Air.Speed * RawSpeedMultiplier(GetRunInput(readInput)), horizontalLocalSpeed);
            var targetVelocity = cc.transform.TransformDirection(rawInput) * targetSpeed;   // raw input magnitude is contained in raw input vector
            var moveAcceleration = ClampedDeltaVAcceleration(horizontalLocalVelocity, targetVelocity, rawInputMag * pcProps.Air.Accel, Time.deltaTime);
            Velocity += (dragDeceleration + moveAcceleration) * Time.deltaTime;
            Velocity += Physics.gravity * Time.deltaTime;
        }

        // ladder = trigger
        // reason: so you can have those caged ladders when going like this _|'''
        // ladder movement is basically free movement
        // transform input vector by head instead of body
        // what about jumping off?

        void OnControllerColliderHit (ControllerColliderHit hit) {
            if(!initialized){
                return;
            }
            // am i moving into it?
            // collider raycast
            // two raycasts?
            // slightly offset using velocity
            // if they don't match the normal (within a reasonable margin... test that!!!) it's a corner
            contactPoints.Add(new CollisionPoint(hit));
            if(Vector3.Dot(hit.normal, Velocity) < 0){  // TODO RELATIVE VELOCITY!!!
                Velocity = Vector3.ProjectOnPlane(Velocity, hit.normal);
            }
        }

        Vector3 GetLocalSpaceMoveInput () {
            float move = Bind.MOVE_FWD.GetValue() - Bind.MOVE_BWD.GetValue();
            float strafe = Bind.MOVE_RIGHT.GetValue() - Bind.MOVE_LEFT.GetValue();
            var output = new Vector3(strafe, 0, move);
            if(output.sqrMagnitude > 1){
                return output.normalized;
            }
            return output;
        }

        float GetRunInput (bool readInput) {
            if(!readInput){
                return 0f;
            }
            return 1f - Mathf.Clamp01(Bind.WALK_OR_RUN.GetValue());     // hardcoded inversion "for now"
        }

    }

}