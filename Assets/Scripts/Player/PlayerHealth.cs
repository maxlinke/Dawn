using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerController {

    public class PlayerHealth : MonoBehaviour {

        [SerializeField] private float m_health;    // TODO temp debug serialization

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
        List<Collider> waterTriggers;
        Transform head;

        [SerializeField] float normedBreath = 1f;   // TODO temp debug serialization
        bool invulnerable = false;
        bool initialized = false;

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
        
        // TODO gib head flying off

        public void Initialize (PlayerHealthSettings healthSettings, Transform head) {
            this.healthSettings = healthSettings;
            this.head = head;
            waterTriggers = new List<Collider>();
            initialized = true;
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

        public void InternalHealthUpdate (float timeStep) {
            float breathDelta;
            if(HeadUnderWater()){
                breathDelta = -1f * timeStep / healthSettings.BreathTime;
            }else{
                breathDelta = timeStep / healthSettings.BreathRecoveryTime;
            }
            normedBreath = Mathf.Clamp01(normedBreath + breathDelta);
            // TODO health ticking down from drowning
            // TODO should breath go down when invulnerable?
            // rename invulnerable to godmode in here?
        }

        public void ClearWaterTriggerList () {
            waterTriggers.Clear();
        }

        void OnTriggerStay (Collider otherCollider) {
            if(!initialized){
                return;
            }
            if(otherCollider.gameObject.layer == Layer.Water){
                AddToWaterTriggersIfValid();
            }

            void AddToWaterTriggersIfValid () {
                if(otherCollider is MeshCollider mc){
                    if(!mc.convex){
                        return;
                    }
                }
                if(!waterTriggers.Contains(otherCollider)){
                    waterTriggers.Add(otherCollider);
                }
            }
        }

        bool HeadUnderWater () {
            if(waterTriggers.Count <= 0){
                return false;
            }
            foreach(var waterTrigger in waterTriggers){
                var hPos = head.position;
                var cPos = waterTrigger.ClosestPoint(hPos);
                if((hPos - cPos).sqrMagnitude < WaterBody.CONTAINS_THRESHOLD_DIST_SQR){
                    return true;
                }
            }
            return false;
        }
        
    }

}