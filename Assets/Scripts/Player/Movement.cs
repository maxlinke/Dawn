using UnityEngine;
using CustomInputSystem;

namespace PlayerController {

    public abstract class Movement : MonoBehaviour {

        [SerializeField] protected UnityEngine.UI.Text DEBUGTEXTFIELD = default;

        public enum ControlMode {
            FULL,
            BLOCK_INPUT,
            ANCHORED
        }

        public enum MoveType {
            GROUND,
            SLOPE,
            AIR,
            DODGE
        }

        public struct State {
            public SurfacePoint surfacePoint;
            public float surfaceAngle;
            public float surfaceDot;
            public MoveType moveType;
            public Vector3 worldPosition;
            public Vector3 incomingWorldVelocity;
            public Vector3 incomingLocalVelocity;       // technically only the incoming local velocity, unless i assign the updated value in the end
            public bool jumped;
            public int frame;
        }

        public abstract float Height { get; }
        public abstract Vector3 WorldCenterPos { get; }
        public abstract Vector3 WorldFootPos { get; }
        public abstract Vector3 Velocity { get; set; }
        public abstract ControlMode controlMode { get; set; }

        protected abstract Transform PlayerTransform { get; }
        protected abstract Vector3 WorldLowerCapsuleSphereCenter { get; }
        protected abstract float CapsuleRadius { get; }

        protected int groundCastMask;

        protected PlayerControllerProperties pcProps;

        public virtual void Initialize (PlayerControllerProperties pcProps) {
            this.pcProps = pcProps;
            groundCastMask = ~LayerMaskUtils.CreateDirectMask(Layer.PlayerController.index);
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

        protected State GetCurrentState (SurfacePoint sp, State lastState) {
            State output;
            if(lastState.jumped){
                sp = null;
            }
            output.surfacePoint = sp;
            if(sp == null){
                output.surfaceDot = float.NaN;
                output.surfaceAngle = float.NaN;
                output.moveType = MoveType.AIR;
                output.incomingLocalVelocity = this.Velocity;   // TODO potentially check for a trigger (such as in a moving train car...)
            }else{
                output.incomingLocalVelocity = this.Velocity - output.surfacePoint.otherVelocity;
                var surfaceDot = Vector3.Dot(sp.normal, PlayerTransform.up);
                var surfaceAngle = Mathf.Rad2Deg * Mathf.Acos(surfaceDot);  // the dot uses normalized vectors, so no dividing by product of magnitudes necessary
                output.surfaceDot = surfaceDot;
                output.surfaceAngle = surfaceAngle;
                if(surfaceAngle < pcProps.HardSlopeLimit){
                    output.moveType = MoveType.GROUND;
                }else{
                    output.moveType = MoveType.SLOPE;
                }

                // TODO validate ground
                // example: sphere
                // surface angle can be below limit even though i SHOULD totally fall off
                // solution: raycast/small-ish spherecast down to where said ground should BE
                // if it isn't there, then i am floating and should probably slide down
            }
            output.incomingWorldVelocity = this.Velocity;
            output.worldPosition = this.PlayerTransform.position;
            output.frame = Time.frameCount;
            output.jumped = false;                      // needs to be initialized
            return output;
        }
        
    }

}