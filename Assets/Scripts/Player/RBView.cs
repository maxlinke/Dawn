using System.Collections.Generic;
using UnityEngine;

namespace PlayerController {

    public class RBView : View {

        // TODO copy water stuff to player health system
        // but the fog stuff can go

        private struct WaterState {
            public bool headUnderWater;
            public Collider trigger;
            public WaterBody waterBody;
        }

        public Transform smoothRotationParent { private get; set; }

        FogSettings airFogSettings;

        List<Collider> waterTriggers;
        WaterState lastWaterState;
        bool fogIsOverriden;

        protected override void DeltaLook (Vector2 viewDelta) {
            headTilt = Mathf.Clamp(headTilt + viewDelta.y, -90f, 90f);
            // headPan = Mathf.Repeat(headPan + viewDelta.x, 360f);
            headPan = 0f;
            ApplyHeadEuler();
            smoothRotationParent.Rotate(0f, viewDelta.x, 0f, Space.Self);
        }

        protected override void TargetLook (Vector3 viewTargetPoint) {
            // var toTargetLocal = playerTransform.InverseTransformDirection(viewTargetPoint - head.position).normalized;
            // headPan = Mathf.Rad2Deg * Mathf.Atan2(toTargetLocal.x, toTargetLocal.z);
            var toTargetLocal = smoothRotationParent.InverseTransformDirection(viewTargetPoint - head.position).normalized;
            headTilt = -1f * Mathf.Rad2Deg * Mathf.Asin(toTargetLocal.y);
            headPan = 0f;
            ApplyHeadEuler();
            var pan = Mathf.Rad2Deg * Mathf.Atan2(toTargetLocal.x, toTargetLocal.z);
            smoothRotationParent.Rotate(0f, pan, 0f, Space.Self);
        }

        void Awake () {
            airFogSettings = FogSettings.GetCurrent();
            fogIsOverriden = false;
            waterTriggers = new List<Collider>();
        }

        void OnTriggerStay (Collider otherCollider) {
            // TODO if fog volume thingy, update airfog and apply (with lerp?)
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

        void FixedUpdate () {
            waterTriggers.Clear();
        }

        void Update () {
            WaterState currentWaterState = GetCurrentWaterState();
            var isUnderWater = currentWaterState.headUnderWater;
            var wasUnderWater = lastWaterState.headUnderWater;
            if(isUnderWater && !wasUnderWater){
                if(currentWaterState.waterBody.Fog.OverrideFog){
                    currentWaterState.waterBody.Fog.FogSettings.Apply();
                    fogIsOverriden = true;
                }
            }else if(!isUnderWater && wasUnderWater && fogIsOverriden){
                airFogSettings.Apply();
                fogIsOverriden = false;
            }
            lastWaterState = currentWaterState;
        }

        WaterState GetCurrentWaterState () {
            WaterState output;
            output.headUnderWater = false;
            output.trigger = null;
            output.waterBody = null;
            if(waterTriggers.Count <= 0){
                return output;
            }
            float minSqrDist = float.PositiveInfinity;
            foreach(var waterTrigger in waterTriggers){
                var hPos = head.position;
                var cPos = waterTrigger.ClosestPoint(hPos);
                var sqrDist = (hPos - cPos).sqrMagnitude;
                if(sqrDist < minSqrDist){
                    output.trigger = waterTrigger;
                }
                if((hPos - cPos).sqrMagnitude < 0.0001f){
                    output.headUnderWater = true;
                    break;
                }
            }
            if(output.trigger != null && output.trigger != lastWaterState.trigger){
                output.waterBody = output.trigger.GetComponentInParent<WaterBody>();
            }else{
                output.waterBody = lastWaterState.waterBody;
            }
            return output;
        }
        
    }

}