using UnityEngine;
using CustomInputSystem;
using System.Collections.Generic;

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
            WATER
        }

        public struct State {
            public SurfacePoint surfacePoint;
            public float surfaceAngle;
            public float surfaceDot;
            public float surfaceSolidness;
            public float normedSurfaceFriction;
            public float clampedNormedSurfaceFriction;
            public PhysicMaterial surfacePhysicMaterial;
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

        protected PlayerControllerProperties pcProps { get; private set; }
        protected Transform head  { get; private set; }

        private PhysicMaterial defaultPM;

        public virtual void Initialize (PlayerControllerProperties pcProps, Transform head) {
            this.pcProps = pcProps;
            this.head = head;
            defaultPM = new PhysicMaterial();
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

        // TODO maybe a two point check? below the eyeline and the inverse point?
        protected bool CanSwimInTrigger (Collider otherCollider) {
            if(otherCollider.gameObject.layer == Layer.Water.index){
                if(otherCollider is MeshCollider mc){
                    if(!mc.convex){
                        return false;
                    }
                }
                var origin = WorldCenterPos;
                return (otherCollider.ClosestPoint(origin) - origin).sqrMagnitude < 0.01f; 
            }
            return false;
        }

        protected float RawTargetSpeed (bool readInput) {
            float runWalkLerp = readInput ? Bind.WALK_OR_RUN.GetValue() : 0f;
            // TODO if always run is off : runWalkLerp = 1f - runWalkLerp;
            var standingSpeed = Mathf.Lerp(pcProps.MoveSpeedRun, pcProps.MoveSpeedWalk, Mathf.Clamp01(runWalkLerp));
            var crouchSpeed = pcProps.MoveSpeedCrouch;
            var crouchFactor = Mathf.Clamp01((Height - pcProps.CrouchHeight) / (pcProps.NormalHeight - pcProps.CrouchHeight));
            return Mathf.Lerp(crouchSpeed, standingSpeed, crouchFactor);
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

        protected State GetCurrentState (SurfacePoint sp, State lastState, IEnumerable<Collider> triggerStays) {
            State output;
            if(lastState.jumped){
                sp = null;
            }
            output.surfacePoint = sp;
            var swim = false;
            foreach(var trigger in triggerStays){
                swim |= CanSwimInTrigger(trigger);
            }
            if(sp == null || swim){
                output.surfaceDot = float.NaN;
                output.surfaceAngle = float.NaN;
                output.surfaceSolidness = float.NaN;
                output.normedSurfaceFriction = float.NaN;
                output.clampedNormedSurfaceFriction = float.NaN;
                output.surfacePhysicMaterial = null;
                output.moveType = (swim ? MoveType.WATER : MoveType.AIR);
                output.incomingLocalVelocity = this.Velocity;   // TODO potentially check for a trigger (such as in a moving train car...)
            }else{
                output.incomingLocalVelocity = this.Velocity - output.surfacePoint.otherVelocity;
                var surfaceDot = Vector3.Dot(sp.normal, PlayerTransform.up);
                var surfaceAngle = Vector3.Angle(sp.normal, PlayerTransform.up);    // just using acos (and rad2deg) on the surfacedot sometimes results in NaN errors...
                output.surfaceDot = surfaceDot;
                output.surfaceAngle = surfaceAngle;
                if(sp.isSolid){
                    output.surfaceSolidness = 1f;    
                }else if(sp.otherRB != null){
                    output.surfaceSolidness = Mathf.Clamp01((sp.otherRB.mass - pcProps.FootRBNonSolidMass) / (pcProps.FootRBSolidMass - pcProps.FootRBNonSolidMass));
                }else{
                    output.surfaceSolidness = 0f;
                }
                if(sp.otherCollider != null && sp.otherCollider.sharedMaterial != null){
                    var otherPM = sp.otherCollider.sharedMaterial;
                    output.normedSurfaceFriction = (otherPM.staticFriction + otherPM.dynamicFriction) / (defaultPM.staticFriction + defaultPM.dynamicFriction);
                    output.clampedNormedSurfaceFriction = Mathf.Clamp01(output.normedSurfaceFriction);;
                    output.surfacePhysicMaterial = otherPM;
                }else{
                    output.normedSurfaceFriction = 1f;
                    output.clampedNormedSurfaceFriction = 1f;
                    output.surfacePhysicMaterial = null;
                }
                output.moveType = (surfaceAngle < pcProps.HardSlopeLimit) ? MoveType.GROUND : MoveType.SLOPE;
            }
            output.incomingWorldVelocity = this.Velocity;
            output.worldPosition = this.PlayerTransform.position;
            output.frame = Time.frameCount;
            output.jumped = false;                      // needs to be initialized
            return output;
        }
        
    }

}