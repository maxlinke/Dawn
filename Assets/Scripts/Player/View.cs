using UnityEngine;

namespace PlayerUtils {

    public abstract class View : MonoBehaviour {

        protected PlayerConfig config;
        protected Player player;
        protected Transform head;

        private bool m_initialized = false;
        public bool initialized => m_initialized;

        public virtual void Initialize (PlayerConfig config, Player player, Transform head) {
            this.config = config;
            this.player = player;
            this.head = head;
            m_initialized = true;
        }

        public abstract void ExecuteUpdate ();

        public abstract void ExecuteFixedUpdate ();

        protected void UpdateHeadLocalPosition () {
            head.transform.localPosition = new Vector3(0f, player.Height + config.EyeOffset, 0f);
        }
        
    }

}