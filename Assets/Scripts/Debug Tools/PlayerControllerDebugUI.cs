using UnityEngine;
using UnityEngine.UI;
using CustomInputSystem;

namespace DebugTools {

    public class PlayerControllerDebugUI : MonoBehaviour {

        [SerializeField] bool hideAfterInit = true;
        [SerializeField] DebugToolColorScheme colorScheme = default;

        [Header("Components")]
        [SerializeField] Canvas canvas = default;
        [SerializeField] Text movementText = default;
        [SerializeField] Text movementTextBG = default;
        [SerializeField] Text viewText = default;
        [SerializeField] Text viewTextBG = default;

        bool visible {
            get {
                return canvas.enabled;
            } set {
                if(canvas.enabled != value){
                    canvas.enabled = value;
                    if(value){
                        ApplyText();
                    }
                }
            }
        }

        private static PlayerControllerDebugUI instance;

        private static string m_movementInfo;
        public static string MovementInfo { 
            get => m_movementInfo;
            set { 
                if(value == null){
                    value = string.Empty;
                }
                m_movementInfo = value.Trim(); 
            }
        }

        private static string m_viewInfo;
        public static string ViewInfo { 
            get => m_viewInfo;
            set { 
                if(value == null){
                    value = string.Empty;
                }
                m_viewInfo = value.Trim(); 
            }
        }

        void Awake () {
            if(instance != null){
                Debug.LogError($"Singleton violation, instance of {nameof(PlayerControllerDebugUI)} is not null!");
                Destroy(this.gameObject);
                return;
            }
            instance = this;
            canvas.sortingOrder = (int)(CanvasSortingOrder.DEBUG_PLAYERCONTROLLER);
            InitUI();
            ResetText();
            ApplyText();
            if(hideAfterInit){
                visible = false;
            }
        }

        void OnDestroy () {
            if(instance == this){
                instance = null;
            }
        }

        void Update () {
            if(Bind.TOGGLE_PLAYERCONTROLLER_DEBUG.GetKeyDown()){
                visible = !visible;
            }
        }

        void LateUpdate () {
            if(!visible){
                return;
            }
            if(Player.Instance == null){
                ResetText();
                visible = false;
                return;
            }
            ApplyText();
        }

        void InitUI () {
            movementText.color = colorScheme.PCDebugForeground;
            viewText.color = colorScheme.PCDebugForeground;
            movementTextBG.color = colorScheme.PCDebugBackground;
            viewTextBG.color = colorScheme.PCDebugBackground;

            // void AddBGImage (RectTransform targetRT) {
            //     var newBG = new GameObject("Background", typeof(RectTransform), typeof(Image));
            //     var newBGRT = newBG.GetComponent<RectTransform>();
            //     newBGRT.SetParent(targetRT);
            //     newBGRT.localPosition = Vector3.zero;
            //     newBGRT.localRotation = Quaternion.identity;
            //     newBGRT.localScale = Vector3.one;
            //     newBGRT.SetToFill();
            //     var newImg = newBG.GetComponent<Image>();
            //     newImg.sprite = null;
            //     newImg.color = colorScheme.BackgroundColor;
            // }
        }

        void ResetText () {
            MovementInfo = string.Empty;
            ViewInfo = string.Empty;
        }

        void ApplyText () {
            movementText.text = MovementInfo;
            movementTextBG.text = MovementInfo;
            viewText.text = ViewInfo;
            viewTextBG.text = ViewInfo;
        }
        
    }

}