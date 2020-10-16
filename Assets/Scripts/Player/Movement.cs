using UnityEngine;

namespace PlayerController {

    public abstract partial class Movement : MonoBehaviour {

        public enum ControlMode {
            FULL,
            BLOCK_INPUT,
            ANCHORED
        }

        public enum MoveType {
            GROUND,
            SLOPE,
            AIR,
            WATER,
            LADDER
        }

        public struct MoveInput {
            public Vector3 horizontalInput;
            public Vector3 verticalInput;
            public float run;
            public bool jump;
            public bool waterExitJump;

            public static MoveInput None { get {
                MoveInput output;
                output.horizontalInput = Vector3.zero;
                output.verticalInput = Vector3.zero;
                output.run = 0f;
                output.jump = false;
                output.waterExitJump = false;
                return output;
            } }
        }

        public abstract float LocalColliderHeight { get; protected set; }
        public abstract float LocalColliderRadius { get; protected set; }
        public abstract Vector3 LocalColliderCenter { get; protected set; }

        public abstract Vector3 Velocity { get; set; }
        public abstract ControlMode controlMode { get; set; }

        protected abstract Transform PlayerTransform { get; }
        protected abstract Transform GravityAlignmentReferenceTransform { get; }

        protected Properties props { get; private set; }
        protected Transform head  { get; private set; }
        protected Transform model { get; private set; }

        protected Vector3 LocalHalfHeight => new Vector3(0f, 0.5f * LocalColliderHeight, 0f);
        protected Vector3 LocalCapsuleSphereOffset => new Vector3(0f, (0.5f * LocalColliderHeight) - LocalColliderRadius, 0f);

        protected Vector3 LocalColliderTop => LocalColliderCenter + LocalHalfHeight;
        protected Vector3 LocalColliderBottom => LocalColliderCenter - LocalHalfHeight;
        protected Vector3 LocalColliderTopSphere => LocalColliderCenter + LocalCapsuleSphereOffset;
        protected Vector3 LocalColliderBottomSphere => LocalColliderCenter - LocalCapsuleSphereOffset;

        public MoveState lastState { get; protected set; }
        protected bool shouldCrouch = false;
        
        private string m_debugInfo = string.Empty;
        public string debugInfo { 
            get => m_debugInfo; 
            protected set {
                m_debugInfo = value ?? string.Empty;
            }
        }

        protected Vector3 WorldColliderBottomSphere => PlayerTransform.TransformPoint(LocalColliderBottomSphere);

        public Vector3 WorldCenterPos {
            get => PlayerTransform.TransformPoint(LocalColliderCenter);
            set => PositionPropertyUpdate(WorldCenterPos, value);
        }

        public Vector3 WorldBottomPos {
            get => PlayerTransform.TransformPoint(LocalColliderBottom);
            set => PositionPropertyUpdate(WorldBottomPos, value);
        }

        public Vector3 WorldTopPos {
            get => PlayerTransform.TransformPoint(LocalColliderTop);
            set => PositionPropertyUpdate(WorldTopPos, value);
        }

        private void PositionPropertyUpdate (Vector3 currentPos, Vector3 newPos) {
            var delta = newPos - currentPos;
            PlayerTransform.position += delta;
        }

        private PhysicMaterial defaultPM;
        private int collisionCastMask;

        protected virtual void Init (Properties pcProps, Transform head, Transform model) {
            this.props = pcProps;
            this.head = head;
            this.model = model;
            defaultPM = new PhysicMaterial();
            collisionCastMask = LayerMaskUtils.GetFullPhysicsCollisionMask(Layer.PlayerControllerAndWorldModel);
            collisionCastMask &= ~LayerMaskUtils.LayerToBitMask(Layer.PlayerControllerAndWorldModel, Layer.Water);
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

        protected Vector3 LadderMoveVector (Vector3 rawInput, Vector3 ladderNormal) {
            var lUp = ladderNormal;
            var lFwd = PlayerTransform.up;
            var lRight = Vector3.Cross(lUp, lFwd).normalized;
            lFwd = Vector3.Cross(lRight, lUp).normalized;
            if(Vector3.Dot(PlayerTransform.forward, ladderNormal) > 0f){
                rawInput.z *= -1f;
            }
            var upDown = lFwd * rawInput.z;
            var leftRight = PlayerTransform.TransformDirection(new Vector3(rawInput.x, 0f, 0f)).ProjectOnVector(lRight);
            return upDown + leftRight;
        }

        protected bool CheckTriggerForWater (Collider otherCollider, out bool canSwim, out bool canCrouch) {
            canSwim = false;
            canCrouch = true;
            if(otherCollider.gameObject.layer == Layer.Water){
                if(otherCollider is MeshCollider mc){
                    if(!mc.convex){
                        return false;
                    }
                }
                var swimPoint = PlayerTransform.TransformPoint(LocalColliderTop + new Vector3(0f, props.SwimOffset, 0f));
                var crouchPoint = PlayerTransform.TransformPoint(LocalColliderBottom + new Vector3(0f, props.CrouchHeight + props.SwimOffset, 0f));
                canSwim = (otherCollider.ClosestPoint(swimPoint) - swimPoint).sqrMagnitude < 0.0001f;           // TODO replace with the static waterbody collider contains thing?
                canCrouch = (otherCollider.ClosestPoint(crouchPoint) - crouchPoint).sqrMagnitude >= 0.0001f; 
                return true;
            }
            return false;
        }

        protected abstract bool ColliderIsSolid (Collider otherCollider);

        protected float CrouchLerpFactor () {
            return Mathf.Clamp01((LocalColliderHeight - props.CrouchHeight) / (props.NormalHeight - props.CrouchHeight));
        }

        protected float RawSpeedMultiplier (float run) {
            var standingSpeed = Mathf.Lerp(props.WalkSpeedMultiplier, props.RunSpeedMultiplier, Mathf.Clamp01(run));
            var crouchSpeed = props.CrouchSpeedMultiplier;
            return Mathf.Lerp(crouchSpeed, standingSpeed, CrouchLerpFactor());
        }

        protected float JumpSpeed () {
            var jumpHeight = Mathf.Lerp(props.CrouchedJumpHeight, props.StandingJumpHeight, CrouchLerpFactor());
            return Mathf.Sqrt(2f * props.NormalGravity * jumpHeight);
        }

        protected Vector3 GetJumpVelocity (Vector3 localVelocity, float jumpStrength) {
            switch(props.JumpVelocityMode){
                case JumpVelocityMode.AddGlobalVelocity:
                    return PlayerTransform.up * JumpSpeed() * jumpStrength;
                case JumpVelocityMode.SetLocalVelocity:
                    float lvVert = Vector3.Dot(localVelocity, PlayerTransform.up);
                    if(props.LimitDescentJumpHeight && lvVert < 0f){
                        lvVert = 0f;
                    }
                    float lvJump = JumpSpeed() * jumpStrength;      // add this strength as a parameter and put it in the square root?
                    float minJump = props.MinJumpVelocity * lvJump;
                    return PlayerTransform.up * Mathf.Max(minJump, lvJump - lvVert);
                default:
                    Debug.LogError($"Unknown {nameof(JumpVelocityMode)} \"{props.JumpVelocityMode}\"!");
                    return Vector3.zero;
            }
        }

        protected Vector3 GetJumpSpeedBoost (Vector3 localVelocity, float localSpeed, float rawTargetSpeed) {
            localVelocity = localVelocity.ProjectOnPlane(PlayerTransform.up);
            Vector3 relevantVelocity;
            float sign;
            switch(props.JumpBoostDirection){
                case JumpBoostDirection.Forward:
                    relevantVelocity = localVelocity.ProjectOnVector(PlayerTransform.forward);
                    sign = Mathf.Clamp01(Mathf.Sign(Vector3.Dot(localVelocity, PlayerTransform.forward)));
                    break;
                case JumpBoostDirection.ForwardAndBack:
                    relevantVelocity = localVelocity.ProjectOnVector(PlayerTransform.forward);
                    sign = 1f;
                    break;
                case JumpBoostDirection.OmniDirectional:
                    relevantVelocity = localVelocity;
                    sign = 1f;
                    break;
                default:
                    Debug.LogError($"Unknown {nameof(JumpBoostDirection)} \"{props.JumpBoostDirection}\"!");
                    return Vector3.zero;
            }
            float boostMultiplier = props.BoostMultiplier;
            if(!props.EnableOverBoosting && (boostMultiplier != 1f)){
                var lerp = (localSpeed - rawTargetSpeed) / ((rawTargetSpeed * boostMultiplier) - rawTargetSpeed);
                boostMultiplier = Mathf.Lerp(boostMultiplier, 1f, lerp);
            }
            return sign * ((relevantVelocity * boostMultiplier) - relevantVelocity);
        }

        protected Vector3 GetLandingBrake (Vector3 localVelocity, bool jumpInput) {
            if(jumpInput && props.EnableBunnyHopping){
                return Vector3.zero;
            }
            return (localVelocity * props.LandingMultiplier) - localVelocity;
        }

        protected Vector3 ClampedDeltaVAcceleration (Vector3 currentVelocity, Vector3 targetVelocity, float maxAcceleration, float deltaTime) {
            var dV = targetVelocity - currentVelocity;
            var dVAccel = dV / deltaTime;
            if(dVAccel.sqrMagnitude > (maxAcceleration * maxAcceleration)){
                return dV.normalized * maxAcceleration;
            }
            return dVAccel;
        }

        protected Vector3 ClampedDeltaVAcceleration (Vector3 currentVelocity, Vector3 targetVelocity, float maxAcceleration) {
            return ClampedDeltaVAcceleration(currentVelocity, targetVelocity, maxAcceleration, Time.deltaTime);
        }

        protected void ApplyDrag (float drag, ref Vector3 localVelocity, float deltaTime) {
            var dragDeceleration = ClampedDeltaVAcceleration(localVelocity, Vector3.zero, drag);
            dragDeceleration *= deltaTime;
            Velocity += dragDeceleration;
            localVelocity += dragDeceleration;
        }

        protected void ApplyDrag (float drag, ref Vector3 localVelocity) {
            ApplyDrag(drag, ref localVelocity, Time.deltaTime);
        }

        protected Quaternion GetTargetGravityRotation (Transform referenceTransform) {
            if(Physics.gravity.sqrMagnitude <= 0f){
                return referenceTransform.rotation;
            }
            var newUp = -Physics.gravity;
            var newFwd = referenceTransform.forward;
            if(Mathf.Abs(Vector3.Dot(newFwd, newUp)) > 0.999f){
                newFwd = referenceTransform.forward + referenceTransform.up;
            }
            newFwd = newFwd.ProjectOnPlane(newUp);
            return Quaternion.LookRotation(newFwd, newUp);
        }

        public bool TryAlignWithGravity () {
            if(controlMode == ControlMode.ANCHORED){
                return false;
            }
            Transform referenceTransform = GravityAlignmentReferenceTransform;
            Quaternion gravityRotation = GetTargetGravityRotation(referenceTransform);
            float normedGravityStrength = Mathf.Clamp01(Physics.gravity.magnitude / props.NormalGravity);
            float degreesPerSecond = props.GravityTurnSpeed * Mathf.Max(normedGravityStrength, props.MinGravityTurnSpeedMultiplier);
            Quaternion newRotation = Quaternion.RotateTowards(referenceTransform.rotation, gravityRotation, Time.deltaTime * degreesPerSecond);
            ApplyGravityRotation(newRotation);
            return true;
        }

        protected abstract void ApplyGravityRotation (Quaternion newRotation);
        
    }

}