using UnityEngine;

namespace PlayerController {

    public abstract class Movement : MonoBehaviour {

        protected PlayerControllerProperties pcProps;
        protected Player player;

        private bool m_initialized = false;
        public bool initialized => m_initialized;

        public abstract float Height { get;}
        public abstract Vector3 Velocity { get; }
        public abstract Vector3 WorldCenter { get; }

        public virtual void Initialize (PlayerControllerProperties pcProps, Player player) {
            this.pcProps = pcProps;
            this.player = player;
            m_initialized = true;
        }

        // "target speed" lerp between walk and run depending on the VALUE of that key
        // then lerp between that and the crouch speed using the normalized collider height

    }

}