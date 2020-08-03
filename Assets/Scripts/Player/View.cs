using UnityEngine;

namespace PlayerUtils {

    public class View : MonoBehaviour {

        protected PlayerControllerProperties pcProps;
        protected Player player;
        protected Transform head;

        public float headTilt { get; private set; }

        public virtual void Initialize (PlayerControllerProperties pcProps, Player player, Transform head, float headTilt) {
            this.pcProps = pcProps;
            this.player = player;
            this.head = head;
            this.headTilt = headTilt;
        }

        // TODO some kind of univeral "update" thing, like head side tilt with (local) velocity and stuff

        public void UpdateHeadLocalPosition () {
            head.localPosition = new Vector3(0f, player.Height + pcProps.EyeOffset, 0f);
        }

        public void Look (Vector2 viewDelta) {
            viewDelta *= 60f * Time.deltaTime;
            headTilt = Mathf.Clamp(headTilt + viewDelta.y, -90f, 90f);
            head.localRotation = Quaternion.Euler(headTilt, 0f, 0f);
            transform.Rotate(new Vector3(0f, viewDelta.x, 0f), Space.Self);
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