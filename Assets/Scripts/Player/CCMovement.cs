using System.Collections.Generic;
using UnityEngine;

namespace PlayerController {

    public class CCMovement : Movement {

        [SerializeField] CharacterController cc = default;
        
        protected override Transform PlayerTransform => cc.transform;

        public override float LocalColliderHeight => cc.height;
        public override float LocalColliderRadius => cc.radius;
        public override Vector3 LocalColliderCenter => cc.center;

        public override Vector3 Velocity { 
            get => m_velocity;
            set => m_velocity = value;
        }

        public override ControlMode controlMode {
            get => m_controlMode;
            set => m_controlMode = value;
        }

        private Vector3 m_velocity;
        private ControlMode m_controlMode;

        bool m_initialized = false;

        List<CollisionPoint> contactPoints;

        public void Initialize (Properties pcProps, Transform head, Transform model) {
            base.Init(pcProps, head, model);
            contactPoints = new List<CollisionPoint>();

            m_initialized = true;    
        }

        // probably should cache the foot-object (esp. if it's a monobehaviour moving platform)
        // so some refactoring of the rb movement is needed
        // TODO just make this virtual, i can pretty much copy the stuff from rbmovement
        public void Move (MoveInput move) {
            contactPoints.Clear();
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

        public void AlignWithGravityIfAllowed () {
            if(!TryAlignWithGravity(PlayerTransform, out var newRotation)){
                return;
            }
            Vector3 wcPos = WorldCenterPos;
            PlayerTransform.rotation = newRotation;
            WorldCenterPos = wcPos;
        }        
        
    }

}