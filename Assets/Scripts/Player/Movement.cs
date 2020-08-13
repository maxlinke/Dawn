using UnityEngine;
using CustomInputSystem;

namespace PlayerController {

    public abstract class Movement : MonoBehaviour {

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
            // public Vector3 worldPosition;
            // public Vector3 worldVelocity;
            public Vector3 localVelocity;
            public bool jumped;
            // local velocity
            // in- and outgoing velocity?
            // jumped?
        }

        public abstract float Height { get; }
        public abstract Vector3 WorldCenterPos { get; }
        public abstract Vector3 WorldFootPos { get; }
        public abstract Vector3 Velocity { get; set; }

        protected abstract Transform PlayerTransform { get; }

        protected PlayerControllerProperties pcProps;

        public virtual void Initialize (PlayerControllerProperties pcProps) {
            this.pcProps = pcProps;
        }

        protected Vector3 HorizontalComponent (Vector3 vector) {
            return vector.ProjectOnPlane(PlayerTransform.up);
        }

        protected Vector3 VerticalComponent (Vector3 vector) {
            return vector.ProjectOnVector(PlayerTransform.up);
        }

        protected Vector3 GroundMoveVector (Vector3 vector, Vector3 groundNormal) {
            return vector.ProjectOnPlaneAlongVector(groundNormal, PlayerTransform.up);
        }

        protected float HeightRelatedSpeed () {
            return Mathf.Lerp(pcProps.MoveSpeedCrouch, pcProps.MoveSpeed, Mathf.Clamp01((Height - pcProps.CrouchHeight) / (pcProps.NormalHeight - pcProps.CrouchHeight)));
        }

        protected float JumpSpeed () {
            return Mathf.Sqrt(2f * pcProps.JumpCalcGravity * pcProps.JumpHeight);
        }

        protected Vector3 GetLocalSpaceMoveInput () {
            float move = Bind.MOVE_FWD.GetValue() - Bind.MOVE_BWD.GetValue();
            float strafe = Bind.MOVE_RIGHT.GetValue() - Bind.MOVE_LEFT.GetValue();
            var output = new Vector3(strafe, 0, move);
            if(output.sqrMagnitude > 1){
                return output.normalized;
            }
            return output;
        }

        protected Vector3 ClampedDeltaVAcceleration (Vector3 currentVelocity, Vector3 targetVelocity, float maxAcceleration, float timeStep) {
            var dV = targetVelocity - currentVelocity;
            var dVAccel = dV / timeStep;
            if(dVAccel.sqrMagnitude > (maxAcceleration * maxAcceleration)){
                return dV.normalized * maxAcceleration;
            }
            return dVAccel;
        }
        
    }

}