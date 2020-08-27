using UnityEngine;

namespace PlayerController {

    public class RBView : View {

        [SerializeField] float debugOutput = default;

        public void UpdateHeadLocalPosition (bool instantly) {
            // var targetPos = new Vector3(0f, (0.5f * player.Height) + pcProps.EyeOffset, 0f);
            var targetPos = new Vector3(0f, player.Height + pcProps.EyeOffset, 0f);
            if(instantly){
                head.localPosition = targetPos;
            }else{
                var maxDelta = pcProps.HeightChangeSpeed * Time.deltaTime;
                var deltaPos = targetPos - head.localPosition;
                if(deltaPos.sqrMagnitude > (maxDelta * maxDelta)){
                    deltaPos = deltaPos.normalized * maxDelta;
                }
                head.localPosition += deltaPos;
                debugOutput = head.localPosition.y;
            }
        }

        protected override void DeltaLook (Vector2 viewDelta) {
            headTilt = Mathf.Clamp(headTilt + viewDelta.y, -90f, 90f);
            headPan = Mathf.Repeat(headPan + viewDelta.x, 360f);
            ApplyHeadEuler();
        }

        protected override void TargetLook (Vector3 viewTargetPoint) {
            var toTargetLocal = playerTransform.InverseTransformDirection(viewTargetPoint - head.position).normalized;
            headPan = Mathf.Rad2Deg * Mathf.Atan2(toTargetLocal.x, toTargetLocal.z);
            headTilt = -1f * Mathf.Rad2Deg * Mathf.Asin(toTargetLocal.y);
            ApplyHeadEuler();
        }
        
    }

}