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
            WATER,
            LADDER
        }

        public struct State {
            public CollisionPoint surfacePoint;
            public float surfaceAngle;
            public float surfaceDot;
            public float surfaceSolidness;
            public float normedSurfaceFriction;
            public float clampedNormedSurfaceFriction;
            public bool touchingGround;
            public bool touchingWall;
            public bool isInWater;
            public PhysicMaterial surfacePhysicMaterial;
            public MoveType moveType;
            public Vector3 worldPosition;
            public Vector3 incomingWorldVelocity;
            public Vector3 incomingLocalVelocity;
            public bool jumped;
            public int frame;
        }

        public abstract float LocalColliderHeight { get; }
        public abstract float LocalColliderRadius { get; }
        public abstract Vector3 LocalColliderCenter { get; }

        public abstract Vector3 Velocity { get; set; }
        public abstract ControlMode controlMode { get; set; }

        protected abstract Transform PlayerTransform { get; }

        protected PlayerControllerProperties pcProps { get; private set; }
        protected Transform head  { get; private set; }

        protected Vector3 LocalHalfHeight => new Vector3(0f, 0.5f * LocalColliderHeight, 0f);
        protected Vector3 LocalCapsuleSphereOffset => new Vector3(0f, (0.5f * LocalColliderHeight) - LocalColliderRadius, 0f);

        protected Vector3 LocalColliderTop => LocalColliderCenter + LocalHalfHeight;
        protected Vector3 LocalColliderBottom => LocalColliderCenter - LocalHalfHeight;
        protected Vector3 LocalColliderTopSphere => LocalColliderCenter + LocalCapsuleSphereOffset;
        protected Vector3 LocalColliderBottomSphere => LocalColliderCenter - LocalCapsuleSphereOffset;

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

        public virtual void Initialize (PlayerControllerProperties pcProps, Transform head) {
            this.pcProps = pcProps;
            this.head = head;
            defaultPM = new PhysicMaterial();
            collisionCastMask = LayerMaskUtils.EverythingMask;      // TODO use actual mask (set up proper layer collision)
            collisionCastMask &= ~LayerMaskUtils.CreateDirectMask(Layer.PlayerController.index);
            collisionCastMask &= ~LayerMaskUtils.CreateDirectMask(Layer.Water.index);
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

        protected virtual CollisionPoint DetermineSurfacePoint (IEnumerable<CollisionPoint> points, out bool touchingWall) {
            touchingWall = false;
            CollisionPoint flattestPoint = null;
            float wallDot = 0.0175f;     // cos(89°)
            float maxDot = wallDot;
            foreach(var point in points){
                var dot = Vector3.Dot(point.normal, PlayerTransform.up);
                if(dot > maxDot){
                    flattestPoint = point;
                    maxDot = dot;
                }else if((Mathf.Abs(dot) < wallDot) && ColliderIsSolid(point.otherCollider)){
                    touchingWall = true;
                }
            }
            return flattestPoint;
        }

        protected void CheckTriggerForWater (Collider otherCollider, out bool isWater, out bool canSwim) {
            isWater = false;
            canSwim = false;
            if(otherCollider.gameObject.layer == Layer.Water.index){
                isWater = true;
                if(otherCollider is MeshCollider mc){
                    if(!mc.convex){
                        canSwim = false;
                        return;
                    }
                }
                var origin = WorldCenterPos;
                canSwim = (otherCollider.ClosestPoint(origin) - origin).sqrMagnitude < 0.01f; 
            }
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

        protected float RawTargetSpeed (bool readInput) {
            float runWalkLerp = readInput ? Bind.WALK_OR_RUN.GetValue() : 0f;
            // TODO if always run is off : runWalkLerp = 1f - runWalkLerp;
            var standingSpeed = Mathf.Lerp(pcProps.MoveSpeedRun, pcProps.MoveSpeedWalk, Mathf.Clamp01(runWalkLerp));
            var crouchSpeed = pcProps.MoveSpeedCrouch;
            var crouchFactor = Mathf.Clamp01((LocalColliderHeight - pcProps.CrouchHeight) / (pcProps.NormalHeight - pcProps.CrouchHeight));
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

        protected Quaternion GetGravityRotation () {
            if(Physics.gravity.sqrMagnitude <= 0f){
                return PlayerTransform.rotation;
            }
            var newUp = -Physics.gravity;
            var newFwd = PlayerTransform.forward;
            if(Mathf.Abs(Vector3.Dot(newFwd, newUp)) > 0.999f){
                newFwd = PlayerTransform.forward + PlayerTransform.up;
            }
            newFwd = newFwd.ProjectOnPlane(newUp);
            return Quaternion.LookRotation(newFwd, newUp);
        }

        protected bool CanUncrouch (State inputState) {
            Vector3 rayStart, rayDir;
            if(inputState.surfacePoint == null){
                rayStart = PlayerTransform.TransformPoint(LocalColliderBottomSphere);
                rayDir = -PlayerTransform.up;
            }else{
                rayStart = PlayerTransform.TransformPoint(LocalColliderTopSphere);
                rayDir = PlayerTransform.up;
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

        // TODO ladder point
        // ladder movement decided in actual movement
        // unless we're airborne or whatever
        // if on ground and move into ladder, do ladder movement
        // if on ground and not ladder movement, don't do ladder movement
        // TODO touching wall. basically do the determine surface point here i guess... includes ladders.
        //
        // TODO pull the sp == null default assignments and movetype assignment apart
        //      do all the sp processing first
        //      movetype last
        // TODO also crouching. what if uncrouch in water? because it's basically in the air, so the bottom of the cc gets modified -> DO NOTHING FIRST, SEE IF IT'S SMOOTH ANYWAYS
        // canuncrouch should use spherecast and explicitly not collide with water. use the layermaskutils for physics collision and !AND the water mask
        // canuncrouch should also know whether to cast up or down
        protected virtual State GetCurrentState (State lastState, IEnumerable<CollisionPoint> collisionPoints, IEnumerable<Collider> triggerStays) {
            State output;
            CollisionPoint sp = DetermineSurfacePoint(collisionPoints, out var touchingWall);
            if(lastState.jumped){
                sp = null;
            }
            output.surfacePoint = sp;
            output.touchingGround = sp != null;
            output.touchingWall = touchingWall;
            output.isInWater = false;
            var swim = false;
            foreach(var trigger in triggerStays){
                CheckTriggerForWater(trigger, out var triggerIsWater, out var canSwimInTrigger);
                output.isInWater |= triggerIsWater;
                if(canSwimInTrigger){
                    swim = true;
                    break;
                }
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
                var otherRB = (sp.otherCollider == null ? null : sp.otherCollider.attachedRigidbody);
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