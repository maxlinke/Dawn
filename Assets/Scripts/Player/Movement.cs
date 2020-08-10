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
            contactPoints.Clear();
            currentState.surfacePoint = sp;
            if(sp == null){
                currentState.moveType = MoveType.AIR;
                currentState.localVelocity = this.Velocity; // TODO potentially check for a trigger (such as in a moving train car...)
            }else{
                currentState.moveType = MoveType.GROUND;
                currentState.localVelocity = this.Velocity - currentState.surfacePoint.otherVelocity;
            }
            currentState.jumped = false;

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
            cc.Move(Velocity * Time.deltaTime);
        }

        void GroundedMovement (bool readInput, State currentState) {
            var rawInput = (readInput ? GetLocalSpaceMoveInput() : Vector3.zero);
            var rawInputMag = rawInput.magnitude;
            
            var groundFriction = ClampedDeltaVAcceleration(currentState.localVelocity, Vector3.zero, pcProps.GroundDrag);

            var moveAccel = groundFriction;
            if(Bind.JUMP.GetKey()){
                moveAccel = new Vector3(moveAccel.x, JumpSpeed() / Time.deltaTime, moveAccel.z);
            }
            Velocity += moveAccel * Time.deltaTime;
            Velocity += Physics.gravity * Time.deltaTime;
        }

        void AerialMovement (bool readInput, State currentState) {
            var rawInput = (readInput ? GetLocalSpaceMoveInput() : Vector3.zero);
            var rawInputMag = rawInput.magnitude;
            var hVelocity = HorizontalComponent(currentState.localVelocity);
            var hVelocityMag = hVelocity.magnitude;
            // var decelFactor = (hVelocityMag > pcProps.MoveSpeed) ? 1 : (1f - rawInputMag);
            var dragDeceleration = ClampedDeltaVAcceleration(hVelocity, Vector3.zero, pcProps.AirDrag);
            var targetSpeed = Mathf.Max(HeightRelatedSpeed(), hVelocityMag);
            var targetVelocity = cc.transform.TransformDirection(rawInput) * targetSpeed;   // raw input magnitude is contained in raw input vector
            var moveAcceleration = ClampedDeltaVAcceleration(hVelocity, targetVelocity, rawInputMag * pcProps.AirAccel);
            Velocity += (dragDeceleration + moveAcceleration) * Time.deltaTime;
            Velocity += Physics.gravity * Time.deltaTime;
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

        void OnControllerColliderHit (ControllerColliderHit hit) {
            contactPoints.Add(hit);
            if(Vector3.Dot(hit.normal, Velocity) < 0){  // TODO RELATIVE VELOCITY!!!
                Velocity = Vector3.ProjectOnPlane(Velocity, hit.normal);
            }
        }

    }

}