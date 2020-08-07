using UnityEngine;

namespace PlayerController {

    public class SurfacePoint {

        public readonly Vector3 point;
        public readonly Vector3 normal;
        public readonly Collider otherCollider;
        public readonly bool isSolid;
        // public readonly bool isStatic;

        public SurfacePoint (ContactPoint contactPoint) {
            this.point = contactPoint.point;
            this.normal = contactPoint.normal;
            this.otherCollider = contactPoint.otherCollider;
            isSolid = ColliderIsSolid(contactPoint.otherCollider);
        }

        public SurfacePoint (Vector3 point, Vector3 normal, Collider otherCollider) {
            this.point = point;
            this.normal = normal;
            this.otherCollider = otherCollider;
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