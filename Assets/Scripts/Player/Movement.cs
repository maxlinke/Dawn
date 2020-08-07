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
        Rigidbody rb;
        CapsuleCollider col;

        private bool m_initialized = false;
        public bool initialized => m_initialized;

        public float Height => col.height;
        public Vector3 Velocity { get; }
        public Vector3 WorldCenter => rb.transform.TransformPoint(col.center);

        public ControlMode controlMode;

        int lastMoveFrame;
        int lastFixedJumpFrame;
        bool gotValidJumpCommandBetweenMoves;
        List<ContactPoint> contactPoints;
        State lastState;

        // TODO dodge is separate from rest
        // accelerate to dodge speed (or decelerate, but there is a maximum amount of time that acceleration can take. fuck it. it's a timed acceleration)
        // then keep that speed (not actively tho)
        // no gravity while dodge
        // not even input
        // at the end lerp speed to walk speed and done

        public void Initialize (PlayerControllerProperties pcProps, Player player, Rigidbody rb, CapsuleCollider col) {
            this.pcProps = pcProps;
            this.player = player;
            this.rb = rb;       // see if i notice a difference between the different collision detection modes. i shouldn't get THIS fast... but the capsule also isn't the biggest object... at 40 m/s i move the entire diameter of the capsule
            this.col = col;
            contactPoints = new List<ContactPoint>();
            m_initialized = true;
            // TODO init rigidbody
        }

        // TODO partial class and move all the utilities there?

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
            return Vector3.ProjectOnPlane(vector, rb.transform.up);
        }

        Vector3 VerticalComponent (Vector3 vector) {
            return Vector3.Project(vector, rb.transform.up);
        }

        Vector3 ClampedDeltaVAcceleration (Vector3 currentVelocity, Vector3 targetVelocity, float maxAcceleration) {
            var dV = targetVelocity - currentVelocity;
            var dVAccel = dV / Time.fixedDeltaTime;
            if(dVAccel.sqrMagnitude > (maxAcceleration * maxAcceleration)){
                return dV.normalized * maxAcceleration;
            }
            return dVAccel;
        }

        Vector3 GroundMoveVector (Vector3 worldMoveInput, Vector3 groundNormal) {
            return ProjectOnPlaneAlongVector(worldMoveInput, groundNormal, rb.transform.up);
        }

        Vector3 ProjectOnPlaneAlongVector (Vector3 vector, Vector3 normal, Vector3 projectVector) {
            float x = Vector3.Dot(normal, vector) / Vector3.Dot(normal, projectVector);
            return (vector - (x * projectVector));
        }

        float HeightRelatedSpeed () {
            return Mathf.Lerp(pcProps.MoveSpeedCrouch, pcProps.MoveSpeed, Mathf.Clamp01((col.height - pcProps.CrouchHeight) / (pcProps.NormalHeight - pcProps.CrouchHeight)));
        }

        float JumpSpeed () {
            return Mathf.Sqrt(2f * pcProps.JumpCalcGravity * pcProps.JumpHeight);
        }

        // TODO use methods or properties to manage these things
        public void ExecuteUpdate () {
            if(Time.frameCount != lastMoveFrame){   // TODO and grounded (last state?)
                gotValidJumpCommandBetweenMoves |= Bind.JUMP.GetKeyDown();  // TODO also dodge
            }
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
                    rb.velocity = Vector3.zero;
                    break;
                    // anchored: important
                    // kinematic rb
                    // velocity 0
                    // disable interpolation
                    // collision detection discrete or speculative
                default:
                    Debug.LogError($"Unknown {nameof(ControlMode)} \"{controlMode}\"!");
                    rb.velocity = Vector3.zero;
                    break;
            }
            FinishMove(currentState);
        }

        void StartMove (out State currentState) {
            currentState.surfacePoint = DetermineSurfacePoint();
            currentState.localVelocity = DetermineLocalVelocity(currentState.surfacePoint);
            currentState.moveType = currentState.surfacePoint == null ? MoveType.AIR : MoveType.GROUND;
            currentState.jumped = false;

            // TODO try alternatives
            // - spherecast/capsulecast
            //   - doesn't return intersections so watch out
            //   - only if got collisions tho. if no collisions, no extra check needed
            //   - direction rb.down
            // - iteratively do raycasts
            //   - start somewhere
            //   - get normal
            //   - now use that normal
            //   - rinse repeat until got good normal match and distance?
            SurfacePoint DetermineSurfacePoint () {
                int flattestPoint = -1;
                // float maxDot = 0;
                float maxDot = 0.0175f;     // cos(89°), to exclude walls
                for(int i=0; i<contactPoints.Count; i++){
                    var dot = Vector3.Dot(contactPoints[i].normal, rb.transform.up);
                    if(dot > maxDot){
                        flattestPoint = i;
                        maxDot = dot;
                    }
                }
                return (flattestPoint != -1) ? new SurfacePoint(contactPoints[flattestPoint]) : null;
            }

            // potentially check for a trigger (such as in a moving train car...)
            Vector3 DetermineLocalVelocity (SurfacePoint sp) {
                if(sp == null || sp.otherCollider == null || sp.otherCollider.attachedRigidbody == null){
                    return rb.velocity;
                }else{
                    return rb.velocity - sp.otherCollider.attachedRigidbody.velocity;   // kinematic rigidbodies moved with rb.movepos still get a velocity
                }
            }
        }

        void FinishMove (State currentState) {
            contactPoints.Clear();
            lastMoveFrame = Time.frameCount;
            gotValidJumpCommandBetweenMoves = false;
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
        }

        // TODO no input means auto-uncrouch (if possible)
        void GroundedMovement (bool readInput, State currentState) {


            // doesn't take slope limit into account
            // so gravity still has to point into the slope, right?
            // and sliding down is projecting gravity onto the normal and using that as acceleration, right?
            // *sigh* guess i'll look at my old code then...
            // also i kinda want braking to have more acceleration than regular accelerating...

            // var rawInput = (readInput ? GetLocalSpaceMoveInput() : Vector3.zero);
            // var rawInputMag = rawInput.magnitude;
            // var targetVelocity = GroundMoveVector(rb.transform.TransformDirection(rawInput), surfacePoint.normal).normalized * rawInputMag * pcProps.MoveSpeed;
            // var targetAccel = pcProps.GroundAccel;
            // rb.velocity += ClampedDeltaVAcceleration(rb.velocity, targetVelocity, targetAccel) * Time.fixedDeltaTime;
            // rb.velocity += Physics.gravity * Time.fixedDeltaTime;

            rb.velocity += ClampedDeltaVAcceleration(rb.velocity, Vector3.zero, pcProps.GroundDrag) * Time.fixedDeltaTime;
            if(Bind.JUMP.GetKey()){
                rb.velocity += rb.transform.up * JumpSpeed();       // NO += AND this triggers twice... needs to check if jumped on last physics tick OR "this" fixedUpdateCount
            }
            rb.velocity += Physics.gravity * Time.fixedDeltaTime;
        }

        void AerialMovement (bool readInput, State currentState) {
            var rawInput = (readInput ? GetLocalSpaceMoveInput() : Vector3.zero);
            var rawInputMag = rawInput.magnitude;
            var hVelocity = HorizontalComponent(currentState.localVelocity);
            var hVelocityMag = hVelocity.magnitude;
            // first drag
            Vector3 dragDeceleration;
            // if(Input.GetKeyDown(KeyCode.Q)){     // TODO decide on this
            // i like the idea but maybe it needs the dot product of drag dir and input too?
            // what about lerping the drag also with respect to speed?
            //     dragDeceleration = ClampedDeltaVAcceleration(hVelocity, Vector3.zero, pcProps.AirDrag * (1f - rawInputMag)); 
            // }else{
                dragDeceleration = ClampedDeltaVAcceleration(hVelocity, Vector3.zero, pcProps.AirDrag);
            // }
            // then accel
            var targetSpeed = Mathf.Max(HeightRelatedSpeed(), hVelocityMag);
            var targetVelocity = rb.transform.TransformDirection(rawInput) * targetSpeed;   // raw input magnitude is contained in raw input vector
            var moveAcceleration = ClampedDeltaVAcceleration(hVelocity, targetVelocity, rawInputMag * pcProps.AirAccel);
            rb.velocity += (dragDeceleration + moveAcceleration) * Time.fixedDeltaTime;
            rb.velocity += Physics.gravity * Time.fixedDeltaTime;
        }

        // ladder = trigger
        // reason: so you can have those caged ladders when going like this _|'''
        // ladder movement is basically free movement
        // transform input vector by head instead of body
        // what about jumping off?

        // surface point instead of the flattest one maybe the "closest" one?
        // and i don't mean strictly closest to the capsule's lower center
        // i mean within a margin. flatness also matters.

        // "target speed" lerp between walk and run depending on the VALUE of that key
        // then lerp between that and the crouch speed using the normalized collider height

        // at least in 2018.4 collision STAY still occurs only on the next fixed update
        void OnCollisionEnter (Collision collision) {
            CacheContacts(collision);
        }

        void OnCollisionStay (Collision collision) {
            CacheContacts(collision);
        }

        // only works if the contact points are unique options, which can be turned off in the project settings...
        void CacheContacts (Collision collision) {
            int cc = collision.contactCount;
            for(int i=0; i<cc; i++){
                contactPoints.Add(collision.GetContact(i));
            }
        }

    }

}