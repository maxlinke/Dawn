using System.Collections.Generic;
using UnityEngine;
using CustomInputSystem;

namespace PlayerController {

    public class Movement : MonoBehaviour {

        public enum ControlMode {
            FULL,
            BLOCK_INPUT,
            ANCHORED
        }

        public enum MoveType {
            GROUND,
            AIR,
            DODGE
        }

        public struct State {
            public SurfacePoint surfacePoint;
            public MoveType moveType;
            public Vector3 localVelocity;
            public bool jumped;
            // local velocity
            // in- and outgoing velocity?
            // jumped?
        }

        PlayerControllerProperties pcProps;
        Player player;
        CharacterController cc;

        private bool m_initialized = false;
        public bool initialized => m_initialized;

        public float Height => cc.height;
        public Vector3 Velocity { get; private set;}

        public ControlMode controlMode;

        List<ControllerColliderHit> contactPoints;
        State lastState;

        [Multiline] public string DEBUGOUTPUTSTRINGTHING;

        // TODO dodge is separate from rest
        // accelerate to dodge speed (or decelerate, but there is a maximum amount of time that acceleration can take. fuck it. it's a timed acceleration)
        // then keep that speed (not actively tho)
        // no gravity while dodge
        // not even input
        // at the end lerp speed to walk speed and done
        
        // TODO no input means auto-uncrouch (if possible)

        // TODO a charactercontroller can enter triggers, right?

        public void Initialize (PlayerControllerProperties pcProps, Player player, CharacterController cc) {
            this.pcProps = pcProps;
            this.player = player;
            this.cc = cc;
            contactPoints = new List<ControllerColliderHit>();
            m_initialized = true;
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

        Vector3 HorizontalComponent (Vector3 vector) {
            return Vector3.ProjectOnPlane(vector, cc.transform.up);
        }

        Vector3 VerticalComponent (Vector3 vector) {
            return Vector3.Project(vector, cc.transform.up);
        }

        Vector3 ClampedDeltaVAcceleration (Vector3 currentVelocity, Vector3 targetVelocity, float maxAcceleration) {
            var dV = targetVelocity - currentVelocity;
            var dVAccel = dV / Time.deltaTime;
            if(dVAccel.sqrMagnitude > (maxAcceleration * maxAcceleration)){
                return dV.normalized * maxAcceleration;
            }
            return dVAccel;
        }

        Vector3 GroundMoveVector (Vector3 worldMoveInput, Vector3 groundNormal) {
            return ProjectOnPlaneAlongVector(worldMoveInput, groundNormal, cc.transform.up);
        }

        Vector3 ProjectOnPlaneAlongVector (Vector3 vector, Vector3 normal, Vector3 projectVector) {
            float x = Vector3.Dot(normal, vector) / Vector3.Dot(normal, projectVector);
            return (vector - (x * projectVector));
        }

        float HeightRelatedSpeed () {
            return Mathf.Lerp(pcProps.MoveSpeedCrouch, pcProps.MoveSpeed, Mathf.Clamp01((cc.height - pcProps.CrouchHeight) / (pcProps.NormalHeight - pcProps.CrouchHeight)));
        }

        float JumpSpeed () {
            return Mathf.Sqrt(2f * pcProps.JumpCalcGravity * pcProps.JumpHeight);
        }

        public void Move (bool readInput) {
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

        void StartMove (out State currentState) {
            var sp = DetermineSurfacePoint();
            currentState.surfacePoint = sp;
            if(sp == null){
                currentState.moveType = MoveType.AIR;
                currentState.localVelocity = this.Velocity; // TODO potentially check for a trigger (such as in a moving train car...)
            }else{
                currentState.moveType = MoveType.GROUND;
                currentState.localVelocity = this.Velocity - currentState.surfacePoint.otherVelocity;
            }
            currentState.jumped = false;
            contactPoints.Clear();

            // might need an alternative here...
            SurfacePoint DetermineSurfacePoint () {
                int flattestPoint = -1;
                float maxDot = 0.0175f;     // cos(89°), to exclude walls
                for(int i=0; i<contactPoints.Count; i++){
                    var dot = Vector3.Dot(contactPoints[i].normal, cc.transform.up);
                    if(dot > maxDot){
                        flattestPoint = i;
                        maxDot = dot;
                    }
                }
                return (flattestPoint != -1) ? new SurfacePoint(contactPoints[flattestPoint]) : null;
            }
        }

        void FinishMove (State currentState) {
            lastState = currentState;
        }

        void ExecuteMove (bool readInput, State currentState) {
            switch(currentState.moveType){
                case MoveType.AIR:
                    AerialMovement(readInput, currentState);
                    break;
                case MoveType.GROUND:
                    GroundedMovement(readInput, currentState);
                    break;
                default:
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
                Velocity = cc.transform.TransformDirection(GetLocalSpaceMoveInput()) * pcProps.MoveSpeed;
                if(Input.GetKey(KeyCode.Mouse1)){
                    // Velocity *= 0.1f;
                    Velocity += cc.transform.up * 0.1f;
                }
            }
            cc.Move(Velocity * Time.deltaTime); // only slides along walls, not the ground, so gravity sliding will have to be implemted explicitly
        }

        // TODO player debug screen (mostly line graphs)
        // -> move type (colored line? different heights as well?)
        // -> speed/Velocity magnitude (as line graph)
        // -> acceleration (as line graph)
        // for the speed/acceleration stuff somehow make it work with the deltatime kinda?
        // either it doesn't have an update and gets executed BY the player, or it executes automatically after the player (script execution order and such)
        // player needs singleton? or the player registers to the debug thing...

        // TODO implement interaction asap
        // set up layers and layer masks i guess
        
        // TODO make some in-world / in-game modifiable world things
        // -> slopes
        // -> steps
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
        // what about the OnControllerColliderHit velocity projection?

        // TODO test movement with controller for jerkiness (0.5 input) because friction/drag etc

        // TODO slope limit, custom gravity
        void GroundedMovement (bool readInput, State currentState) {
            var localVelocity = currentState.localVelocity;
            var groundFriction = ClampedDeltaVAcceleration(localVelocity, Vector3.zero, pcProps.GroundDrag);
            groundFriction *= Time.deltaTime;
            Velocity += groundFriction;
            localVelocity += groundFriction;
            var localSpeed = localVelocity.magnitude;
            var rawInput = (readInput ? GetLocalSpaceMoveInput() : Vector3.zero);
            var rawInputMag = rawInput.magnitude;
            var targetSpeed = Mathf.Max(HeightRelatedSpeed(), localSpeed);
            var targetDirection = GroundMoveVector(cc.transform.TransformDirection(rawInput), currentState.surfacePoint.normal);
            var targetVelocity = targetDirection.normalized * rawInputMag * targetSpeed;
            var moveAccel = ClampedDeltaVAcceleration(localVelocity, targetVelocity, rawInputMag * pcProps.GroundAccel);
            if(Bind.JUMP.GetKeyDown()){
                moveAccel = new Vector3(moveAccel.x, JumpSpeed() / Time.deltaTime, moveAccel.z);
            }
            Velocity += moveAccel * Time.deltaTime;
            Velocity += Physics.gravity * Time.deltaTime;
        }

        // TODO limit fall velocity? only in aerial or everywhere?
        void AerialMovement (bool readInput, State currentState) {
            var horizontalLocalVelocity = HorizontalComponent(currentState.localVelocity);
            // var decelFactor = (hVelocityMag > pcProps.MoveSpeed) ? 1 : (1f - rawInputMag);
            var dragDeceleration = ClampedDeltaVAcceleration(horizontalLocalVelocity, Vector3.zero, pcProps.AirDrag);
            dragDeceleration *= Time.deltaTime;
            Velocity += dragDeceleration;
            horizontalLocalVelocity += dragDeceleration;
            var horizontalLocalSpeed = horizontalLocalVelocity.magnitude;
            var rawInput = (readInput ? GetLocalSpaceMoveInput() : Vector3.zero);
            var rawInputMag = rawInput.magnitude;
            var targetSpeed = Mathf.Max(HeightRelatedSpeed(), horizontalLocalSpeed);
            var targetVelocity = cc.transform.TransformDirection(rawInput) * targetSpeed;   // raw input magnitude is contained in raw input vector
            var moveAcceleration = ClampedDeltaVAcceleration(horizontalLocalVelocity, targetVelocity, rawInputMag * pcProps.AirAccel);
            Velocity += (dragDeceleration + moveAcceleration) * Time.deltaTime;
            Velocity += Physics.gravity * Time.deltaTime;
        }

        // ladder = trigger
        // reason: so you can have those caged ladders when going like this _|'''
        // ladder movement is basically free movement
        // transform input vector by head instead of body
        // what about jumping off?

        void OnControllerColliderHit (ControllerColliderHit hit) {
            contactPoints.Add(hit);
            if(Vector3.Dot(hit.normal, Velocity) < 0){  // TODO RELATIVE VELOCITY!!!
                Velocity = Vector3.ProjectOnPlane(Velocity, hit.normal);
            }
        }

    }

}