using UnityEngine;

namespace PlayerController {

    public class View : MonoBehaviour {

        PlayerControllerProperties pcProps;
        Player player;
        Rigidbody rb;
        Transform head;

        public float headTilt { get; private set; }
        public float headPan { get; private set; }
        public float headRoll { get; private set; }

        public void Initialize (PlayerControllerProperties pcProps, Player player, Rigidbody rb, Transform head) {
            this.pcProps = pcProps;
            this.player = player;
            this.rb = rb;
            this.head = head;
        }

        public void SetHeadOrientation (float headTilt, float headPan, float headRoll) {
            this.headTilt = headTilt;
            this.headPan = headPan;
            this.headRoll = headRoll;
        }

        void ApplyHeadEuler () {
            head.localEulerAngles = new Vector3(headTilt, headPan, headRoll);
        }

        public void UpdateHeadLocalPosition () {
            head.localPosition = new Vector3(0f, player.Height + pcProps.EyeOffset, 0f);
        }

        public void MatchRBRotationToHead () {
            var tiltCache = headTilt;
            headTilt = 0f;
            ApplyHeadEuler();
            rb.rotation = Quaternion.LookRotation(head.forward, rb.transform.up);
            headTilt = tiltCache;
            headPan = 0f;
            ApplyHeadEuler();
        }

        public void Look (Vector2 viewDelta) {
            viewDelta *= 60f * Time.deltaTime;
            headTilt = Mathf.Clamp(headTilt + viewDelta.y, -90f, 90f);
            headPan = Mathf.Repeat(headPan + viewDelta.x, 360f);
            ApplyHeadEuler();
        }

        public void LookAt (Transform viewTarget) {
            if(viewTarget == null){
                return;
            }
            LookAt(viewTarget.position);
        }

        public void LookAt (Vector3 worldPoint) {
            var toTargetLocal = rb.transform.InverseTransformDirection(worldPoint - head.position).normalized;
            headPan = Mathf.Rad2Deg * Mathf.Atan2(toTargetLocal.x, toTargetLocal.z);
            headTilt = -1f * Mathf.Rad2Deg * Mathf.Asin(toTargetLocal.y);
            ApplyHeadEuler();
        }
        
    }

}