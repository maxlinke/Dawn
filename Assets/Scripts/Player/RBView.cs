using System.Collections.Generic;
using UnityEngine;

namespace PlayerController {

    public class RBView : View {

        public Transform smoothRotationParent { private get; set; }

        [SerializeField] float debugOutput = default;

        FogSettings airFogSettings;
        // List<Collider> waterTriggers;
        WaterBody lastWaterBody;
        WaterBody currentWaterBody;

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
        }

        void OnTriggerStay (Collider otherCollider) {
            if(currentWaterBody != null){
                return;
            }
            if(otherCollider.gameObject.layer == Layer.Water){
                if(otherCollider is MeshCollider mc){
                    if(!mc.convex){
                        return;
                    }
                }
                // if(!waterTriggers.Contains(otherCollider)){
                //     waterTriggers.Add(otherCollider);
                // }
                var hPos = head.position;
                var cPos = otherCollider.ClosestPoint(hPos);
                if((hPos - cPos).sqrMagnitude < 0.0001f){
                    currentWaterBody = otherCollider.GetComponentInParent<WaterBody>();
                }
            }
        }

        void FixedUpdate () {
            lastWaterBody = currentWaterBody;
            currentWaterBody = null;
        }

        void Update () {
            // foreach(var waterTrigger in waterTriggers){

            // }
            var headUnderWater = currentWaterBody != null;
            var headWasUnderWater = lastWaterBody != null;
            if(headUnderWater && !headWasUnderWater){
                if(currentWaterBody.Fog.OverrideFog){
                    currentWaterBody.Fog.FogSettings.Apply();
                }
            }else if(!headUnderWater && headWasUnderWater){
                airFogSettings.Apply();
            }
        }
        
    }

}