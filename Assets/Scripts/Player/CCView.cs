using UnityEngine;

namespace PlayerController {

    public class CCView : View {

        public void UpdateHeadLocalPosition () {
            head.localPosition = new Vector3(0f, player.Height + pcProps.EyeOffset, 0f);
        }

        protected override void DeltaLook (Vector2 viewDelta) {
            headTilt = Mathf.Clamp(headTilt + viewDelta.y, -90f, 90f);
            ApplyHeadEuler();
            playerTransform.Rotate(new Vector3(0f, viewDelta.x, 0f), Space.Self);
        }

        protected override void TargetLook (Vector3 viewTargetPoint) {
            var toTargetLocal = playerTransform.InverseTransformDirection(viewTargetPoint - head.position).normalized;
            headTilt = -1f * Mathf.Rad2Deg * Mathf.Asin(toTargetLocal.y);
            ApplyHeadEuler();
            var pan = Mathf.Rad2Deg * Mathf.Atan2(toTargetLocal.x, toTargetLocal.z);
            playerTransform.Rotate(new Vector3(0f, pan, 0f), Space.Self);
        }

    }

}