using UnityEngine;
using CustomInputSystem;

namespace PlayerController {

    public class View : MonoBehaviour {

        public enum ControlMode {
            FULL,
            BLOCK_INPUT,
            TARGETED
        }

        [SerializeField] UnityEngine.UI.Text DEBUGTEXTFIELD;

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
            Debug.Log(LayerMaskUtils.MaskToBinaryString(LayerMaskUtils.EverythingMask));
            interactMask = ~LayerMaskUtils.CreateDirectMask(Layer.PlayerController.index);
            Debug.Log(LayerMaskUtils.MaskToBinaryString(interactMask));
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

        // TODO lower interact range when crouching is implemented?
        public void InteractCheck (bool readInput) {
            Color rayCol;
            string debugLogA = string.Empty;
            string debugLogB = string.Empty;
            if(Bind.INTERACT.GetKeyDown()){
                debugLogA = $"{Time.frameCount} | trying to interact";
            }
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
            DEBUGTEXTFIELD.text = description;
            Debug.DrawRay(head.transform.position, head.transform.forward * pcProps.InteractRange, rayCol, 0f);
        }
        
    }

}