using UnityEngine;
using System.Collections.Generic;

namespace PlayerController {

    public abstract class Movement : MonoBehaviour {

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

        public struct MoveState {
            public CollisionPoint surfacePoint;
            public CollisionPoint ladderPoint;      // it would be more correct to use a list of ladderpoints but when are you colliding with multiple ladders?
            public float surfaceAngle;
            public float surfaceDot;
            public float surfaceSolidness;
            public float normedSurfaceFriction;
            public float clampedNormedSurfaceFriction;
            public bool touchingGround;
            public bool touchingWall;
            public bool isInWater;
            public bool canCrouchInWater;
            public bool canJump;
            public PhysicMaterial surfacePhysicMaterial;
            public MoveType moveType;
            public Vector3 worldPosition;
            public Vector3 incomingWorldVelocity;
            public Vector3 incomingLocalVelocity;
            public bool jumped;
            public int frame;
        }

        public struct CrouchControlInput {
            public bool toggleCrouch;
            public bool crouchHold;
            public bool crouchHoldRelease;

            public static CrouchControlInput None { get {
                CrouchControlInput output;
                output.toggleCrouch = false;
                output.crouchHold = false;
                output.crouchHoldRelease = false;
                return output;
            } }
        }

        public abstract float LocalColliderHeight { get; }
        public abstract float LocalColliderRadius { get; }
        public abstract Vector3 LocalColliderCenter { get; }

        public abstract Vector3 Velocity { get; set; }
        public abstract ControlMode controlMode { get; set; }

        protected abstract Transform PlayerTransform { get; }

        protected PlayerControllerProperties pcProps { get; private set; }
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
                m_debugInfo = value != null ? value : string.Empty;
            }
        }

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

        protected virtual void Init (PlayerControllerProperties pcProps, Transform head, Transform model) {
            this.pcProps = pcProps;
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

        protected struct CollisionProcessorOutput {
            public CollisionPoint flattestPoint;
            public CollisionPoint ladderPoint;
            public bool touchingWall;
        }

        protected virtual CollisionProcessorOutput ProcessCollisionPoints (IEnumerable<CollisionPoint> points) {
            CollisionProcessorOutput output;
            output.touchingWall = false;
            output.flattestPoint = null;
            output.ladderPoint = null;
            float wallDot = 0.0175f;     // cos(89°)
            float maxDot = wallDot;
            float maxLadderDot = -wallDot;
            foreach(var point in points){
                var dot = Vector3.Dot(point.normal, PlayerTransform.up);
                if(dot > maxDot){
                    output.flattestPoint = point;
                    maxDot = dot;
                }else if((Mathf.Abs(dot) < wallDot) && ColliderIsSolid(point.otherCollider)){
                    output.touchingWall = true;
                }
                if(dot > maxLadderDot){
                    if((point.otherCollider != null && TagManager.CompareTag(Tag.Ladder, point.otherCollider.gameObject))){
                        output.ladderPoint = point;
                    }
                }
            }
            return output;
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
                var swimPoint = PlayerTransform.TransformPoint(LocalColliderTop + new Vector3(0f, pcProps.SwimOffset, 0f));
                var crouchPoint = PlayerTransform.TransformPoint(LocalColliderBottom + new Vector3(0f, pcProps.CrouchHeight + pcProps.SwimOffset, 0f));
                canSwim = (otherCollider.ClosestPoint(swimPoint) - swimPoint).sqrMagnitude < 0.0001f; 
                canCrouch = (otherCollider.ClosestPoint(crouchPoint) - crouchPoint).sqrMagnitude >= 0.0001f; 
                return true;
            }
            return false;
        }

        protected bool ColliderIsLadder (Collider otherCollider) {
            if(otherCollider == null) return false;
            return TagManager.CompareTag(Tag.Ladder, otherCollider.gameObject);
        }

        protected bool ColliderIsSolid (Collider otherCollider) {
            if(otherCollider == null) return false;
            var otherRB = otherCollider.attachedRigidbody;
            if(otherRB == null) return true;
            return otherRB.isKinematic;
        }

        protected float RawSpeedMultiplier (float run) {
            var standingSpeed = Mathf.Lerp(pcProps.WalkSpeedMultiplier, pcProps.RunSpeedMultiplier, Mathf.Clamp01(run));
            var crouchSpeed = pcProps.CrouchSpeedMultiplier;
            var crouchFactor = Mathf.Clamp01((LocalColliderHeight - pcProps.CrouchHeight) / (pcProps.NormalHeight - pcProps.CrouchHeight));
            return Mathf.Lerp(crouchSpeed, standingSpeed, crouchFactor);
        }

        protected float JumpSpeed () {
            return Mathf.Sqrt(2f * pcProps.JumpCalcGravity * pcProps.JumpHeight);
        }

        protected Vector3 ClampedDeltaVAcceleration (Vector3 currentVelocity, Vector3 targetVelocity, float maxAcceleration, float timeStep) {
            var dV = targetVelocity - currentVelocity;
            var dVAccel = dV / timeStep;
            if(dVAccel.sqrMagnitude > (maxAcceleration * maxAcceleration)){
                return dV.normalized * maxAcceleration;
            }
            return dVAccel;
        }

        protected void ApplyDrag (float drag, float timestep, ref Vector3 localVelocity) {
            var dragDeceleration = ClampedDeltaVAcceleration(localVelocity, Vector3.zero, drag, timestep);
            dragDeceleration *= timestep;
            Velocity += dragDeceleration;
            localVelocity += dragDeceleration;
        }

        protected Quaternion GetGravityRotation (Transform referenceTransform) {
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

        public void SetTryCrouch (bool value) {
            shouldCrouch = value;
        }

        public void UpdateCrouchState (CrouchControlInput ccInput) {
            if(lastState.isInWater && !lastState.canCrouchInWater){
                shouldCrouch = false;
                return;
            }
            if(controlMode != ControlMode.FULL){
                ccInput = CrouchControlInput.None;
            }
            if(ccInput.toggleCrouch){
                shouldCrouch = !shouldCrouch;
            }
            if(ccInput.crouchHold){
                shouldCrouch = true;
            }
            if(ccInput.crouchHoldRelease){
                shouldCrouch = false;
            }
        }

        protected bool CanUncrouch (bool checkUpward) {
            Vector3 rayStart, rayDir;
            if(checkUpward){
                rayStart = PlayerTransform.TransformPoint(LocalColliderTopSphere);
                rayDir = PlayerTransform.up;
            }else{
                rayStart = PlayerTransform.TransformPoint(LocalColliderBottomSphere);
                rayDir = -PlayerTransform.up;
            }
            var offset = 0.05f;
            var scale = PlayerTransform.lossyScale.Average();
            var rayRadius = (LocalColliderRadius * scale) - offset;
            var rayLength = ((pcProps.NormalHeight - LocalColliderHeight) * scale) + offset;
            if(Physics.SphereCast(rayStart, rayRadius, rayDir, out var hit, rayLength, collisionCastMask)){
                // could do a mass check here if hit is a rigidbody
                return false;
            }
            return true;
        }

        protected virtual MoveState GetCurrentState (IEnumerable<CollisionPoint> collisionPoints, IEnumerable<Collider> triggerStays) {
            MoveState output;
            var colResult = ProcessCollisionPoints(collisionPoints);
            var sp = colResult.flattestPoint;
            var lp = colResult.ladderPoint;
            if(lastState.jumped){
                sp = null;
                lp = null;
            }
            output.surfacePoint = sp;
            output.ladderPoint = lp;
            output.touchingGround = sp != null;
            output.touchingWall = colResult.touchingWall;
            output.isInWater = false;
            output.canCrouchInWater = true;
            var swim = false;
            Vector3 averageTriggerVelocity = Vector3.zero;
            foreach(var trigger in triggerStays){
                output.isInWater |= CheckTriggerForWater(trigger, out var canSwimInTrigger, out var canCrouchInTrigger);
                output.canCrouchInWater &= canCrouchInTrigger;
                if(canSwimInTrigger){
                    swim = true;
                    break;  // TODO when i properly do the average trigger velocity stuff, remove this break
                }
            }
            if(sp == null || swim){
                output.surfaceDot = float.NaN;
                output.surfaceAngle = float.NaN;
                output.surfaceSolidness = float.NaN;
                output.normedSurfaceFriction = float.NaN;
                output.clampedNormedSurfaceFriction = float.NaN;
                output.surfacePhysicMaterial = null;
                if(swim){
                    output.moveType = MoveType.WATER;
                    output.incomingLocalVelocity = this.Velocity - averageTriggerVelocity;
                }else if(output.ladderPoint != null){
                    output.moveType = MoveType.LADDER;
                    output.incomingLocalVelocity = this.Velocity - output.ladderPoint.GetVelocity();
                }else{
                    output.moveType = MoveType.AIR;
                    output.incomingLocalVelocity = this.Velocity - averageTriggerVelocity;
                }
            }else{
                var otherRB = sp.otherRB;
                var otherVelocity = (otherRB == null ? Vector3.zero : otherRB.velocity);
                output.incomingLocalVelocity = this.Velocity - otherVelocity;
                var surfaceDot = Vector3.Dot(sp.normal, PlayerTransform.up);
                var surfaceAngle = Vector3.Angle(sp.normal, PlayerTransform.up);    // just using acos (and rad2deg) on the surfacedot sometimes results in NaN errors...
                output.surfaceDot = surfaceDot;
                output.surfaceAngle = surfaceAngle;
                if(ColliderIsSolid(sp.otherCollider)){
                    output.surfaceSolidness = 1f;    
                }else if(otherRB != null){
                    output.surfaceSolidness = Mathf.Clamp01((otherRB.mass - pcProps.FootRBNonSolidMass) / (pcProps.FootRBSolidMass - pcProps.FootRBNonSolidMass));
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
                if(surfaceAngle < pcProps.HardSlopeLimit){
                    output.moveType = MoveType.GROUND;
                }else{
                    if(lp != null){
                        output.moveType = MoveType.LADDER;
                    }else{
                        output.moveType = MoveType.SLOPE;
                    }
                }
            }
            output.canJump = (output.moveType == MoveType.GROUND || output.moveType == MoveType.LADDER);
            output.incomingWorldVelocity = this.Velocity;
            output.worldPosition = this.PlayerTransform.position;
            output.frame = Time.frameCount;
            output.jumped = false;                      // needs to be initialized
            return output;
        }
        
    }

}