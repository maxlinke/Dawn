using UnityEngine;
using CustomInputSystem;

namespace PlayerController {

    public class View : MonoBehaviour {

        public enum ControlMode {
            FULL,
            BLOCK_INPUT,
            TARGETED
        }

        PlayerControllerProperties pcProps;
        Player player;
        Rigidbody rb;
        Transform head;

        public float headTilt { get; private set; }
        public float headPan { get; private set; }
        public float headRoll { get; private set; }

        public ControlMode controlMode;
        public Transform viewTarget;

        public void Initialize (PlayerControllerProperties pcProps, Player player, Rigidbody rb, Transform head) {
            this.pcProps = pcProps;
            this.player = player;
            this.rb = rb;
            this.head = head;
        }

        Vector2 GetViewInput () {
            var dx = Bind.LOOK_RIGHT.GetValue() - Bind.LOOK_LEFT.GetValue();
            var dy = Bind.LOOK_DOWN.GetValue() - Bind.LOOK_UP.GetValue();
            return new Vector2(dx, dy);
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

        // TODO velocity based head rolling (in every case)
        public void Look (bool readInput) {
            switch(controlMode){
                case ControlMode.FULL: 
                    DeltaLook(readInput ? GetViewInput() : Vector2.zero);
                    break;
                case ControlMode.TARGETED:
                    TargetLook(viewTarget.position);
                    break;
                case ControlMode.BLOCK_INPUT:
                    break;
                default: 
                    Debug.LogError($"Unknown {typeof(ControlMode)} \"{controlMode}\"!");
                    break;
            }
        }

        void DeltaLook (Vector2 viewDelta) {
            viewDelta *= 60f * Time.deltaTime;
            headTilt = Mathf.Clamp(headTilt + viewDelta.y, -90f, 90f);
            headPan = Mathf.Repeat(headPan + viewDelta.x, 360f);
            ApplyHeadEuler();
        }

        void TargetLook (Vector3 viewTargetPoint) {
            var toTargetLocal = rb.transform.InverseTransformDirection(viewTargetPoint - head.position).normalized;
            headPan = Mathf.Rad2Deg * Mathf.Atan2(toTargetLocal.x, toTargetLocal.z);
            headTilt = -1f * Mathf.Rad2Deg * Mathf.Asin(toTargetLocal.y);
            ApplyHeadEuler();
        }
        
    }

}