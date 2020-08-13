using UnityEngine;

namespace PlayerController {

    public class SurfacePoint {

        public readonly Vector3 point;
        public readonly Vector3 normal;
        public readonly Collider otherCollider;
        public readonly Rigidbody otherRB;
        public readonly Vector3 otherVelocity;
        public readonly bool isSolid;
        // public readonly bool isStatic;

        public SurfacePoint (ContactPoint contactPoint) {
            this.point = contactPoint.point;
            this.normal = contactPoint.normal;
            this.otherCollider = contactPoint.otherCollider;
            if(otherCollider == null){
                this.otherRB = null;
                this.otherVelocity = Vector3.zero;
            }else{
                this.otherRB = otherCollider.attachedRigidbody;
                this.otherVelocity = otherRB != null ? otherRB.velocity : Vector3.zero;
            }
            isSolid = ColliderIsSolid(otherCollider);
        }

        public SurfacePoint (ControllerColliderHit hit) {
            this.point = hit.point;
            this.normal = hit.normal;
            this.otherCollider = hit.collider;
            this.otherRB = hit.rigidbody;
            if(otherRB == null){
                this.otherVelocity = Vector3.zero;
            }else{
                this.otherVelocity = otherRB.velocity;
            }
            isSolid = ColliderIsSolid(otherCollider);
        }

        public SurfacePoint (Vector3 point, Vector3 normal, Collider otherCollider) {
            this.point = point;
            this.normal = normal;
            this.otherCollider = otherCollider;
            if(otherCollider == null){
                this.otherRB = null;
                this.otherVelocity = Vector3.zero;
            }else{
                this.otherRB = otherCollider.attachedRigidbody;
                this.otherVelocity = otherRB.velocity;
            }
            isSolid = ColliderIsSolid(otherCollider);
        }

        public override string ToString () {
            return $"[{nameof(point)}: {point}, {nameof(normal)}: {normal}, {nameof(otherCollider)}: {otherCollider}]";
        }

        bool ColliderIsSolid (Collider otherCollider) {
            if(otherCollider == null) return false;
            var otherRB = otherCollider.attachedRigidbody;
            if(otherRB == null) return true;
            return otherRB.isKinematic;
        }

        // bool ColliderIsStatic (Collider otherCollider) {

        // }
        
    }

}