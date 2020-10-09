using UnityEngine;

namespace PlayerController {

    public class RBView : View {

        public Transform smoothRotationParent { private get; set; }

        protected override void DeltaLook (Vector2 viewDelta) {
            headTilt = Mathf.Clamp(headTilt + viewDelta.y, -90f, 90f);
            // headPan = Mathf.Repeat(headPan + viewDelta.x, 360f);
            headPan = 0f;
            ApplyHeadEuler();
            smoothRotationParent.Rotate(0f, viewDelta.x, 0f, Space.Self);
        }

        // TODO max delta option
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
        
    }

}