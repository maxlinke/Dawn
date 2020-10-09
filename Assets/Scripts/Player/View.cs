using UnityEngine;

namespace PlayerController {

    public abstract class View : MonoBehaviour {

        public enum ControlMode {
            FULL,
            BLOCK_INPUT,
            TARGETED
        }

        protected Properties pcProps;
        protected Player player;
        protected Transform head;
        protected Transform playerTransform => player.transform;

        public float headTilt { get; protected set; }
        public float headPan { get; protected set; }
        public float headRoll { get; protected set; }

        public ControlMode controlMode;
        public Transform viewTarget;

        int interactMask;

        private string m_debugInfo = string.Empty;
        public string debugInfo { 
            get => m_debugInfo; 
            protected set {
                m_debugInfo = value != null ? value : string.Empty;
            }
        }     // TODO a clearer reset point

        public void Initialize (Properties pcProps, Player player, Transform head) {
            this.pcProps = pcProps;
            this.player = player;
            this.head = head;
            SetupInteractMask();
        }

        protected virtual void SetupInteractMask () {
            interactMask = ~LayerMaskUtils.LayerToBitMask(Layer.PlayerControllerAndWorldModel);
        }

        public void SetHeadOrientation (float headTilt, float headPan, float headRoll) {
            this.headTilt = headTilt;
            this.headPan = headPan;
            this.headRoll = headRoll;
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

        public void Look (Vector2 viewInput) {
            switch(controlMode){
                case ControlMode.FULL: 
                    // DeltaLook(viewInput * 60f * Time.deltaTime);
                    DeltaLook(viewInput * 60f * Time.unscaledDeltaTime);
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

        public bool InteractCheck (out IInteractable outputInteractable, out string outputDescription) {
            Color rayCol;
            outputDescription = string.Empty;
            outputInteractable = null;
            debugInfo = string.Empty;
            if(Physics.Raycast(head.transform.position, head.transform.forward, out var hit, pcProps.InteractRange, interactMask, QueryTriggerInteraction.Collide)){
                var interactable = hit.collider.GetComponent<IInteractable>();
                if(interactable != null && interactable.CanBeInteractedWith){
                    outputDescription = interactable.InteractionDescription;
                    debugInfo = outputDescription;
                }else{
                    if(interactable == null){
                        debugInfo = $"no interactable (hit {hit.collider.name})";
                    }else if(!interactable.CanBeInteractedWith){
                        debugInfo = "cant be interacted with";
                    }
                }
                rayCol = Color.green;
            }else{
                rayCol = Color.red;
            }
            Debug.DrawRay(head.transform.position, head.transform.forward * pcProps.InteractRange, rayCol, 0f);
            return outputInteractable != null;
        }
        
    }

}