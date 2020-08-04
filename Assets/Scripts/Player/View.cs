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

        // TODO test
        // when looking at moving objects, this should always be executed after everything has already moved
        // otherwise 
        //   a) the target won't be in focus when the frame is drawn
        //   b) the target will jitter
        public void LookAt (Transform viewTarget) {
            if(viewTarget == null){
                return;
            }
            LookAt(viewTarget.position);
        }

        // to fix potentially unavoidable jitter, this method could be used with smoothing applied by the caller...
        public void LookAt (Vector3 worldPoint) {
            var toTarget = worldPoint - head.position;
            var tFwd = Vector3.ProjectOnPlane(toTarget, transform.up);
            if(!(tFwd.sqrMagnitude > 0)){
                tFwd = transform.forward;
            }
            transform.localRotation = Quaternion.LookRotation(tFwd, transform.up);
            var toTargetLocal = transform.InverseTransformDirection(toTarget);
            headTilt = Mathf.Asin(toTargetLocal.normalized.y);
            head.localRotation = Quaternion.Euler(headTilt, 0f, 0f);  
        }
        
    }

}