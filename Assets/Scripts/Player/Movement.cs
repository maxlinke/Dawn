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
            public float normedStaticSurfaceFriction;
            public float normedDynamicSurfaceFriction;
            public bool touchingGround;
            public bool touchingWall;
            public bool facingWall;
            public WaterBody waterBody;
            public bool isInWater;
            public bool canCrouchInWater;
            public bool canJump;
            public PhysicMaterial surfacePhysicMaterial;
            public MoveType moveType;
            public Vector3 worldPosition;
            public Vector3 incomingWorldVelocity;
            public Vector3 incomingLocalVelocity;
            public bool startedJump;
            public bool midJump;
            public int frame;
            public int groundStickBlockTimer;
            public bool executedGroundStick;
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

        public abstract float LocalColliderHeight { get; }
        public abstract float LocalColliderRadius { get; }
        public abstract Vector3 LocalColliderCenter { get; }

        public abstract Vector3 Velocity { get; set; }
        public abstract ControlMode controlMode { get; set; }

        protected abstract Transform PlayerTransform { get; }

        protected Properties pcProps { get; private set; }
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
            public CollisionPoint closestPoint;
            public CollisionPoint ladderPoint;
            public bool touchingWall;
            public bool facingWall;
        }

        protected virtual CollisionProcessorOutput ProcessCollisionPoints (IEnumerable<CollisionPoint> points, Vector3 worldColliderBottomSphere) {
            CollisionProcessorOutput output;
            output.touchingWall = false;
            output.facingWall = false;
            output.flattestPoint = null;
            output.closestPoint = null;
            output.ladderPoint = null;
            float wallDot = 0.0175f;     // cos(89°)
            float maxDot = wallDot;
            float minSqrDist = float.PositiveInfinity;
            float minLadderDot = float.PositiveInfinity;
            foreach(var point in points){
                var dot = Vector3.Dot(point.normal, PlayerTransform.up);
                if(dot > maxDot){
                    output.flattestPoint = point;
                    maxDot = dot;
                }else if(!output.facingWall && (Mathf.Abs(dot) < wallDot) && ColliderIsSolid(point.otherCollider)){
                    output.touchingWall = true;
                    output.facingWall = Vector3.Dot(PlayerTransform.forward, point.normal) < -0.707f;   // -cos(45°)
                }
                var dist = (point.point - worldColliderBottomSphere).sqrMagnitude;
                if(dot > wallDot && dist < minSqrDist){
                    output.closestPoint = point;
                    minSqrDist = dist;
                }
                if(dot < minLadderDot && dot > -wallDot){
                    if((point.otherCollider != null && TagManager.CompareTag(Tag.Ladder, point.otherCollider.gameObject))){
                        output.ladderPoint = point;
                        minLadderDot = dot;
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
                canSwim = (otherCollider.ClosestPoint(swimPoint) - swimPoint).sqrMagnitude < 0.0001f;           // TODO replace with the static waterbody collider contains thing?
                canCrouch = (otherCollider.ClosestPoint(crouchPoint) - crouchPoint).sqrMagnitude >= 0.0001f; 
                return true;
            }
            return false;
        }

        protected bool ColliderIsSolid (Collider otherCollider) {
            if(otherCollider == null) return false;
            var otherRB = otherCollider.attachedRigidbody;
            if(otherRB == null) return true;
            return otherRB.isKinematic;
        }

        protected float CrouchLerpFactor () {
            return Mathf.Clamp01((LocalColliderHeight - pcProps.CrouchHeight) / (pcProps.NormalHeight - pcProps.CrouchHeight));
        }

        protected float RawSpeedMultiplier (float run) {
            var standingSpeed = Mathf.Lerp(pcProps.WalkSpeedMultiplier, pcProps.RunSpeedMultiplier, Mathf.Clamp01(run));
            var crouchSpeed = pcProps.CrouchSpeedMultiplier;
            return Mathf.Lerp(crouchSpeed, standingSpeed, CrouchLerpFactor());
        }

        protected float JumpSpeed () {
            var jumpHeight = Mathf.Lerp(pcProps.CrouchedJumpHeight, pcProps.StandingJumpHeight, CrouchLerpFactor());
            return Mathf.Sqrt(2f * pcProps.NormalGravity * jumpHeight);
        }

        protected Vector3 GetJumpVelocity (Vector3 localVelocity, float jumpStrength) {
            switch(pcProps.JumpVelMode){
                case Properties.JumpVelocityMode.AddGlobalVelocity:
                    return PlayerTransform.up * JumpSpeed() * jumpStrength;
                case Properties.JumpVelocityMode.SetLocalVelocity:
                    float lvVert = Vector3.Dot(localVelocity, PlayerTransform.up);
                    if(pcProps.LimitDescentJumpHeight && lvVert < 0f){
                        lvVert = 0f;
                    }
                    float lvJump = JumpSpeed() * jumpStrength;      // add this strength as a parameter and put it in the square root?
                    float minJump = pcProps.MinJumpVelocity * lvJump;
                    return PlayerTransform.up * Mathf.Max(minJump, lvJump - lvVert);
                default:
                    Debug.LogError($"Unknown {nameof(Properties.JumpVelocityMode)} \"{pcProps.JumpVelMode}\"!");
                    return Vector3.zero;
            }
        }

        protected Vector3 GetJumpSpeedBoost (Vector3 localVelocity, float localSpeed, float rawTargetSpeed) {
            localVelocity = localVelocity.ProjectOnPlane(PlayerTransform.up);
            Vector3 relevantVelocity;
            float sign;
            switch(pcProps.BoostDirection){
                case Properties.JumpBoostDirection.Forward:
                    relevantVelocity = localVelocity.ProjectOnVector(PlayerTransform.forward);
                    sign = Mathf.Clamp01(Mathf.Sign(Vector3.Dot(localVelocity, PlayerTransform.forward)));
                    break;
                case Properties.JumpBoostDirection.ForwardAndBack:
                    relevantVelocity = localVelocity.ProjectOnVector(PlayerTransform.forward);
                    sign = 1f;
                    break;
                case Properties.JumpBoostDirection.OmniDirectional:
                    relevantVelocity = localVelocity;
                    sign = 1f;
                    break;
                default:
                    Debug.LogError($"Unknown {nameof(Properties.JumpBoostDirection)} \"{pcProps.BoostDirection}\"!");
                    return Vector3.zero;
            }
            float boostMultiplier = pcProps.BoostMultiplier;
            if(!pcProps.EnableOverBoosting && (boostMultiplier != 1f)){
                var lerp = (localSpeed - rawTargetSpeed) / ((rawTargetSpeed * boostMultiplier) - rawTargetSpeed);
                boostMultiplier = Mathf.Lerp(boostMultiplier, 1f, lerp);
            }
            return sign * ((relevantVelocity * boostMultiplier) - relevantVelocity);
        }

        protected Vector3 GetLandingBrake (Vector3 localVelocity, bool jumpInput) {
            if(jumpInput && pcProps.EnableBunnyHopping){
                return Vector3.zero;
            }
            return (localVelocity * pcProps.LandingMultiplier) - localVelocity;
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

        protected bool TryAlignWithGravity (Transform referenceTransform, out Quaternion newRotation) {
            if(controlMode == ControlMode.ANCHORED){
                newRotation = Quaternion.identity;
                return false;
            }
            var gravityRotation = GetTargetGravityRotation(referenceTransform);
            var normedGravityStrength = Mathf.Clamp01(Physics.gravity.magnitude / pcProps.NormalGravity);
            var degreesPerSecond = pcProps.GravityTurnSpeed * Mathf.Max(normedGravityStrength, pcProps.MinGravityTurnSpeedMultiplier);
            newRotation = Quaternion.RotateTowards(referenceTransform.rotation, gravityRotation, Time.deltaTime * degreesPerSecond);
            return true;
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

        protected bool CanUncrouch (bool onGround) {
            if(onGround){
                return UncrouchPathFree(checkUpward: true);
            }else{
                if(!UncrouchPathFree(checkUpward: false)){
                    return UncrouchPathFree(checkUpward: true);
                }
            }
            return true;
        }

        protected bool UncrouchPathFree (bool checkUpward) {
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
            if(lastState.executedGroundStick){
                this.Velocity = Vector3.ClampMagnitude(this.Velocity, lastState.incomingWorldVelocity.magnitude);
            }
            Vector3 worldColliderBottomSphere = PlayerTransform.TransformPoint(LocalColliderBottomSphere);
            var colResult = ProcessCollisionPoints(collisionPoints, worldColliderBottomSphere);
            var sp = colResult.flattestPoint;
            var lp = colResult.ladderPoint;
            if(lastState.startedJump){
                sp = null;
                lp = null;
            }
            MoveState output;
            output.surfacePoint = sp;
            output.ladderPoint = lp;
            output.touchingGround = sp != null;
            output.touchingWall = colResult.touchingWall;
            output.facingWall = colResult.facingWall;
            output.waterBody = null;
            output.isInWater = false;
            output.canCrouchInWater = true;
            var swim = false;
            Vector3 averageTriggerVelocity = Vector3.zero;
            foreach(var trigger in triggerStays){
                if(CheckTriggerForWater(trigger, out var canSwimInTrigger, out var canCrouchInTrigger)){
                    output.isInWater = true;
                    if(output.waterBody == null){
                        output.waterBody = trigger.GetComponent<WaterBody>();       // TODO nonononono
                    }
                }
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
                output.normedStaticSurfaceFriction = float.NaN;
                output.normedDynamicSurfaceFriction = float.NaN;
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
                    output.normedStaticSurfaceFriction = otherPM.staticFriction / defaultPM.staticFriction;
                    output.normedDynamicSurfaceFriction = otherPM.dynamicFriction / defaultPM.dynamicFriction;
                    output.surfacePhysicMaterial = otherPM;
                }else{
                    output.normedStaticSurfaceFriction = 1f;
                    output.normedDynamicSurfaceFriction = 1f;
                    output.surfacePhysicMaterial = null;
                }
                if(sp.otherCollider != null && TagManager.CompareTag(Tag.Slippery, sp.otherCollider.gameObject)){
                    output.moveType = MoveType.SLOPE;
                }else{
                    if(surfaceAngle <= pcProps.HardSlopeLimit){
                        output.moveType = MoveType.GROUND;
                    }else{
                        output.moveType = ((lp != null) ? MoveType.LADDER : MoveType.SLOPE);
                    }
                }
            }
            output.groundStickBlockTimer = Mathf.Max(0, lastState.groundStickBlockTimer - 1);
            output.canJump = (output.moveType == MoveType.GROUND) || (output.moveType == MoveType.LADDER);
            output.midJump = (lastState.startedJump || lastState.midJump) && (output.moveType == MoveType.AIR);
            output.incomingWorldVelocity = this.Velocity;
            output.worldPosition = this.PlayerTransform.position;
            output.frame = Time.frameCount;
            // these just need to be initialized
            output.startedJump = false;
            output.executedGroundStick = false;
            return output;
        }

        protected bool TryEnforceGroundStick (ref MoveState state, float targetSpeed, Vector3 localVelocity) {
            if(state.groundStickBlockTimer > 0){
                return false;
            }
            float lerpFactor = state.surfaceSolidness * state.normedStaticSurfaceFriction * pcProps.GroundStickiness;
            if(lerpFactor <= 0f){
                return false;
            }
            Vector3 rayOrigin = PlayerTransform.TransformPoint(LocalColliderBottomSphere);
            Vector3 rayDir = -state.surfacePoint.normal;
            float rayLength = targetSpeed * Time.deltaTime * Mathf.Tan(Mathf.Deg2Rad * pcProps.HardSlopeLimit);
            rayLength += LocalColliderRadius;
            bool groundCastHit = Physics.Raycast(rayOrigin, rayDir, out var hit, rayLength, collisionCastMask, QueryTriggerInteraction.Ignore); 
            if(!groundCastHit){
                return false;
            }
            bool distOK = (hit.distance > (LocalColliderRadius + 0.01f));
            bool dotOK = (Vector3.Dot(localVelocity, hit.normal) > 0.01f);
            if(distOK && dotOK){
                bool angleOK = (Vector3.Angle(hit.normal, PlayerTransform.up) < pcProps.HardSlopeLimit);
                if(angleOK){
                    Vector3 newCapsuleContact = rayOrigin - (LocalColliderRadius * hit.normal);
                    Vector3 delta = hit.point - newCapsuleContact;      // doesn't take velocity into consideration...
                    // Vector3 movePos = (delta / Time.deltaTime) * 1.1f;
                    Vector3 movePos = 1.1f * delta.ProjectOnVector(PlayerTransform.up) / Time.deltaTime;
                    movePos += this.Velocity.ProjectOnPlane(hit.normal);

                    if(movePos.sqrMagnitude > localVelocity.sqrMagnitude){
                        movePos = movePos.normalized * localVelocity.magnitude;
                    }
                    // if(movePos.sqrMagnitude > targetSpeed){
                    //     movePos = movePos.normalized * targetSpeed;
                    // }

                    this.Velocity = Vector3.Lerp(this.Velocity, movePos, lerpFactor);   // TODO i do need to clamp the velocity on the next tick tho...
                    Debug.Log($"sticking (lerp {lerpFactor:F2})!");

                    state.executedGroundStick = true;
                    state.groundStickBlockTimer = pcProps.GroundStickInterval + 1;

                    var bwCol = Color.Lerp(Color.black, Color.white, lerpFactor);
                    Debug.DrawLine(rayOrigin, newCapsuleContact, bwCol, 10f);
                    Debug.DrawLine(rayOrigin, hit.point, bwCol, 10f);
                    Debug.DrawRay(hit.point, hit.normal, bwCol, 10f);

                    return true;
                }
            }
            return false;
        }
        
    }

}