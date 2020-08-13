using UnityEngine;
using CustomInputSystem;

namespace PlayerController {

    public abstract class View : MonoBehaviour {

        public enum ControlMode {
            FULL,
            BLOCK_INPUT,
            TARGETED
        }

        [SerializeField] UnityEngine.UI.Text DEBUGTEXTFIELD = default;

        protected PlayerControllerProperties pcProps;
        protected Player player;
        protected Transform head;
        protected Transform playerTransform => player.transform;

        public float headTilt { get; protected set; }
        public float headPan { get; protected set; }
        public float headRoll { get; protected set; }

        public ControlMode controlMode;
        public Transform viewTarget;

        int interactMask;

        public void Initialize (PlayerControllerProperties pcProps, Player player, Transform head) {
            this.pcProps = pcProps;
            this.player = player;
            this.head = head;
            SetupInteractMask();
        }

        protected virtual void SetupInteractMask () {
            interactMask = ~LayerMaskUtils.CreateDirectMask(Layer.PlayerController.index);
        }

        protected Vector2 GetViewInput () {
            var dx = Bind.LOOK_RIGHT.GetValue() - Bind.LOOK_LEFT.GetValue();
            var dy = Bind.LOOK_DOWN.GetValue() - Bind.LOOK_UP.GetValue();
            return new Vector2(dx, dy);
        }

        public void SetHeadOrientation (float headTilt, float headPan, float headRoll) {
            this.headTilt = headTilt;
            this.headPan = headPan;
            this.headRoll = headRoll;
        }

        public void UpdateHeadLocalPosition () {
            head.localPosition = new Vector3(0f, player.Height + pcProps.EyeOffset, 0f);
        }

        protected void ApplyHeadEuler () {
            head.localEulerAngles = new Vector3(headTilt, headPan, headRoll);
        }

        public void RotateBodyInLookDirection () {
            var tiltCache = headTilt;
            headTilt = 0f;
            ApplyHeadEuler();
            playerTransform.rotation = Quaternion.LookRotation(head.forward, playerTransform.up);
            headTilt = tiltCache;
            headPan = 0f;
            ApplyHeadEuler();
        }

        public void Look (bool readInput) {
            switch(controlMode){
                case ControlMode.FULL: 
                    DeltaLook(readInput ? GetViewInput() * 60f * Time.deltaTime : Vector2.zero);
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

        protected abstract void DeltaLook (Vector2 viewDelta);
        protected abstract void TargetLook (Vector3 targetPoint);

        public void InteractCheck (bool readInput) {
            Color rayCol;
            var description = string.Empty;
            if(Physics.Raycast(head.transform.position, head.transform.forward, out var hit, pcProps.InteractRange, interactMask, QueryTriggerInteraction.Collide)){
                var interactable = hit.collider.GetComponent<IInteractable>();
                if(interactable != null && interactable.CanBeInteractedWith){
                    description = interactable.InteractionDescription;
                    if(readInput && Bind.INTERACT.GetKeyDown()){
                        Debug.Log($"{Time.frameCount} | interacting with \"{interactable.GetType()}\"");
                        interactable.Interact(player);      // would be nice if i had player-specific interactions, as to avoid getcomponent-calls
                    }
                }else{
                    if(interactable == null){
                        description = $"no interactable (hit {hit.collider.name})";
                    }else if(!interactable.CanBeInteractedWith){
                        description = "cant be interacted with";
                    }
                }
                rayCol = Color.green;
            }else{
                rayCol = Color.red;
            }
            if(DEBUGTEXTFIELD != null){
                DEBUGTEXTFIELD.text = description;
            }
            Debug.DrawRay(head.transform.position, head.transform.forward * pcProps.InteractRange, rayCol, 0f);
        }
        
    }

}