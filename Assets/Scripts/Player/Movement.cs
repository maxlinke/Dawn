using UnityEngine;

namespace PlayerUtils {

    public abstract class Movement : MonoBehaviour {

        protected PlayerConfig config;
        protected Player player;

        private bool m_initialized = false;
        public bool initialized => m_initialized;

        public abstract float Height { get;}
        public abstract Vector3 Velocity { get; }

        public virtual void Initialize (PlayerConfig config, Player player) {
            this.config = config;
            this.player = player;
            m_initialized = true;
        }

        public abstract void ExecuteUpdate ();

        public abstract void ExecuteFixedUpdate ();

        protected Vector3 TEMP_GetLocalSpaceMoveInput () {
            float move = (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0);
            float strafe = (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0);
            return new Vector3(strafe, 0f, move);
        }

    }

}