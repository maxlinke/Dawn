using System.Collections.Generic;
using UnityEngine;

namespace PlayerController {

    public class CCMovement : Movement {

        [SerializeField] CharacterController cc = default;
        
        protected override Transform PlayerTransform => cc.transform;
        protected override Transform GravityAlignmentReferenceTransform => cc.transform;

        public override float LocalColliderHeight {
            get => cc.height;
            protected set => cc.height = value;
        }

        public override float LocalColliderRadius {
            get => cc.radius;
            protected set => cc.radius = value;
        }

        public override Vector3 LocalColliderCenter {
            get => cc.center;
            protected set => cc.center = value;
        }

        // TODO this is global velocity, movement should always by local velocity, when parented
        // so this should add the parent's velocity!
        public override Vector3 Velocity { 
            get => Vector3.zero;
            set { }
        }

        public override ControlMode controlMode {
            get => m_controlMode;
            set => m_controlMode = value;
        }

        private Vector3 m_ownVelocity;
        private ControlMode m_controlMode;

        bool m_initialized = false;

        List<CollisionPoint> contactPoints;
        List<Collider> triggerStays;

        public void Initialize (Properties pcProps, Transform head, Transform model) {
            base.Init(pcProps, head, model);
            contactPoints = new List<CollisionPoint>();
            triggerStays = new List<Collider>();
            m_initialized = true;    
        }

        // probably should cache the foot-object (esp. if it's a monobehaviour moving platform)
        // so some refactoring of the rb movement is needed
        // or maybe not, if i juse use the last state as my cache...
        // TODO just make this virtual, i can pretty much copy the stuff from rbmovement

        // important note: when INSIDE a collider and moving down, i will get pushed up
        // the collision flags of the move will say "below" but there won't be a concontrollercolliderhit event!
        // possible solution: if move and flags != none but collisions == 0 -> do last "vertical move" again?
        public void Move (MoveInput move) {
            MoveState currentState = GetCurrentState(contactPoints, triggerStays);
            contactPoints.Clear();
            triggerStays.Clear();
            UpdateColliderSizeIfNeeded(currentState, false);
        }

        // TODO extract the movement stuff, put it into partial class Movement (Movements.cs (notice the s))
        // little bit of abstract/virtual stuff like walking into slopes

        // on collision with actual physics object
        // either give it the mother of all pushes
        // or project own velocity just like with everything else
        void OnControllerColliderHit (ControllerColliderHit hit) {
            if(!m_initialized){
                return;
            }
            contactPoints.Add(new CollisionPoint(hit));
        }

        protected override void ApplyGravityRotation (Quaternion newRotation) {
            Vector3 wcPos = WorldCenterPos;
            PlayerTransform.rotation = newRotation;
            WorldCenterPos = wcPos;
        }

        protected override void OnColliderSizeUpdated (bool onGround) {
            model.localPosition = Vector3.zero;
            head.localPosition = new Vector3(0f, LocalColliderHeight - props.EyeOffset, 0f);
        }

        protected override bool ColliderIsSolid (Collider otherCollider) {
            // if(otherCollider == null) return false;
            // var otherRB = otherCollider.attachedRigidbody;
            // if(otherRB == null) return true;
            // return otherRB.isKinematic;
            return true;
        }

        protected override void GetVelocityAndSolidness (CollisionPoint surfacePoint, out Vector3 velocity, out float solidness) {
            // var otherRB = surfacePoint.otherRB;
            // velocity = (otherRB == null ? Vector3.zero : otherRB.velocity);
            // if(ColliderIsSolid(surfacePoint.otherCollider)){
            //     solidness = 1f;    
            // }else if(otherRB != null){
            //     solidness = Mathf.Clamp01((otherRB.mass - rbProps.FootRBNonSolidMass) / (rbProps.FootRBSolidMass - rbProps.FootRBNonSolidMass));
            // }else{
            //     solidness = 0f;
            // }
            velocity = Vector3.zero;
            solidness = 1f;
        }
        
    }

}