using UnityEngine;

namespace PlayerController {

    public class RBView : View {

        [SerializeField] float debugOutput = default;

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