using UnityEngine;

namespace PlayerUtils {

    public abstract class Movement : MonoBehaviour {

        protected PlayerConfig config;
        protected Player player;

        private bool m_initialized = false;
        public bool initialized => m_initialized;

        public abstract float Height { get;}
        public abstract Vector3 Velocity { get; }
        public abstract Vector3 WorldCenter { get; }

        public virtual void Initialize (PlayerConfig config, Player player) {
            this.config = config;
            this.player = player;
            m_initialized = true;
        }



    }

}