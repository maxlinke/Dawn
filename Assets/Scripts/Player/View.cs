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
        CharacterController cc;
        Transform head;

        public float headTilt { get; private set; }
        public float headPan { get; private set; }
        public float headRoll { get; private set; }

        public ControlMode controlMode;
        public Transform viewTarget;

        int interactMask;

        public void Initialize (PlayerControllerProperties pcProps, Player player, CharacterController cc, Transform head) {
            this.pcProps = pcProps;
            this.player = player;
            this.cc = cc;
            this.head = head;
            interactMask = unchecked((int)0b11111111111111111111111111111111);  // TODO proper masking (doesn't need its own layer, just gets created here)
            // TODO layers as enum
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

        public void RotateBodyInLookDirection () {
            var tiltCache = headTilt;
            headTilt = 0f;
            ApplyHeadEuler();
            cc.transform.rotation = Quaternion.LookRotation(head.forward, cc.transform.up);
            headTilt = tiltCache;
            headPan = 0f;
            ApplyHeadEuler();
        }

        // TODO velocity based head rolling (in every case) ( --!!!!!!-> LOCAL VELOCITY <-!!!!!-- )
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
            ApplyHeadEuler();
            cc.transform.Rotate(new Vector3(0f, viewDelta.x, 0f), Space.Self);
        }

        void TargetLook (Vector3 viewTargetPoint) {
            var toTargetLocal = cc.transform.InverseTransformDirection(viewTargetPoint - head.position).normalized;
            headTilt = -1f * Mathf.Rad2Deg * Mathf.Asin(toTargetLocal.y);
            ApplyHeadEuler();
            var pan = Mathf.Rad2Deg * Mathf.Atan2(toTargetLocal.x, toTargetLocal.z);
            cc.transform.Rotate(new Vector3(0f, pan, 0f), Space.Self);
        }

        public void InteractCheck (bool readInput) {
            // disable all player colliders?
            if(Physics.Raycast(head.transform.position, head.transform.forward, out var hit, pcProps.InteractRange, interactMask, QueryTriggerInteraction.Collide)){
                var interactable = hit.collider.GetComponent<IInteractable>();
                var description = string.Empty;
                if(interactable != null){
                    description = interactable.InteractionDescription;
                    if(readInput && Bind.INTERACT.GetKeyDown()){
                        interactable.Interact(player);  // TODO "canBeInteractedWith", specific for player, to avoid getcomponents etc
                    }
                }
            }
        }
        
    }

}