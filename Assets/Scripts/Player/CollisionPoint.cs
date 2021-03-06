﻿using UnityEngine;

namespace PlayerController {

    public class CollisionPoint {

        public readonly Vector3 point;
        public readonly Vector3 normal;
        public readonly Collider otherCollider;
        public Rigidbody otherRB => (otherCollider != null ? otherCollider.attachedRigidbody : null);

        public CollisionPoint (ContactPoint contactPoint) {
            this.point = contactPoint.point;
            this.normal = contactPoint.normal;
            this.otherCollider = contactPoint.otherCollider;
        }

        public CollisionPoint (ControllerColliderHit hit) {
            this.point = hit.point;
            this.normal = hit.normal;
            this.otherCollider = hit.collider;
        }

        public CollisionPoint (Vector3 point, Vector3 normal, Collider otherCollider) {
            this.point = point;
            this.normal = normal;
            this.otherCollider = otherCollider;
        }

        public override string ToString () {
            return $"[{nameof(point)}: {point}, {nameof(normal)}: {normal}, {nameof(otherCollider)}: {otherCollider}]";
        }

        public Vector3 GetVelocity () {
            if(otherRB == null){
                return Vector3.zero;
            }
            return otherRB.velocity;
        }

        public string GetName () {
            if(otherCollider == null){
                return "null";
            }
            return otherCollider.name;
        }
        
    }

}