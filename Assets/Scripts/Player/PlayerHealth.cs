using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerController {

    public class PlayerHealth : MonoBehaviour {

        private float m_health;

        public float Health {
            get => m_health;
            private set  {
                if(!invulnerable){
                    m_health = value;
                    OnHealthUpdated.Invoke(m_health);
                }
            }
        }

        public event System.Action<float> OnHealthUpdated = delegate {};

        PlayerHealthSettings healthSettings;
        bool invulnerable;

        // TODO
        // TODO
        // actually implement fall damage (from movement or from player? laststate stores deltav between exit and incoming? only deltav between incomings?)
        // notify of landing (generic)?
        // if static collider, simple
        // if destructible do goomba stomp damage, if it's dead, no fall damage because it absorbed it?

        // port water stuff from view
        // see if i can make it non-enable dependent (no awake, no (fixed)update)
        // TODO
        // TODO

        public void Initialize (PlayerHealthSettings healthSettings) {
            this.healthSettings = healthSettings;
            invulnerable = false;
        }

        public void SetHealth (float value) {
            if(healthSettings == null){
                Debug.LogWarning("Not initialized, aborting!");
                return;
            }
            Health = Mathf.Max(value, healthSettings.MaxHealth);
        }

        // TODO player subscribes to commmand for godmode
        public void SetInvulnerable (bool value) {
            if(healthSettings == null){
                Debug.LogWarning("Not initialized, aborting!");
                return;
            }
            invulnerable = value;
        }

        public void NotifyOfFallDamage (float verticalDeltaV) {
            if(verticalDeltaV <= 0f){
                return;
            }
            float vSqr = verticalDeltaV * verticalDeltaV;
            float g = healthSettings.FallDamageReferenceGravity;
            float virtualHeight = vSqr / (2f * g);
            float hMin = healthSettings.MinFallDamageHeight;
            float hMax = healthSettings.OneHundredFallDamageHeight;
            float normedHeight = Mathf.Max(0f, (virtualHeight - hMin) / (hMax - hMin));
            float fallDmg = 100f * normedHeight;
            Health -= fallDmg;
        }

        // TODO internal on health update stuff
        // TODO gib head flying off
        // TODO more methods
        
    }

}