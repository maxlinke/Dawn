using System.Collections.Generic;
using UnityEngine;

namespace PlayerController {

    public abstract partial class Movement {

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

        private struct CollisionProcessorOutput {
            public CollisionPoint flattestPoint;
            public CollisionPoint closestPoint;
            public CollisionPoint ladderPoint;
            public bool touchingWall;
            public bool facingWall;
        }

        private CollisionProcessorOutput ProcessCollisionPoints (IEnumerable<CollisionPoint> points, Vector3 worldColliderBottomSphere) {
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

    }

}