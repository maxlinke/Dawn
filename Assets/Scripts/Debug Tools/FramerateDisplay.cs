using UnityEngine;
using UnityEngine.UI;
using CustomInputSystem;

namespace DebugTools {

    public class FramerateDisplay : MonoBehaviour {

        [Header("Settings")]
        [SerializeField] bool hideAfterInit = true;
        [SerializeField] DebugToolColorScheme colorScheme = default;

        [Header("Components")]
        [SerializeField] Canvas canvas = default;
        [SerializeField] RawImage image = default;
        [SerializeField] Image background = default;
        [SerializeField] Text rawFPSText = default;
        [SerializeField] Text avgFPSText = default;
        [SerializeField] Text minFPSText = default;
        [SerializeField] Text maxFPSText = default;

        bool visible {
            get {
                return canvas.enabled;
            } set {
                if(canvas.enabled != value){
                    canvas.enabled = value;
                    if(value){
                        OnShow();
                    }else{
                        OnHide();
                    }
                }
            }
        }

        private static FramerateDisplay instance;

        RectTransform imageRT => image.rectTransform;
        Texture2D tex;
        Color32 lineCol32;
        Color32 clearCol32;

        float[] framerates;
        float currentFPS;
        float avgFPS;
        float maxFPS;
        float minFPS;
        int currentFrameIndex;

        float texMin;
        float texMax;

        void Awake () {
            if(instance != null){
                Debug.LogError($"Singleton violation, instance of {nameof(FramerateDisplay)} is not null!");
                Destroy(this.gameObject);
                return;
            }
            instance = this;
            canvas.sortingOrder = (int)CanvasSortingOrder.DEBUG_FRAMERATE;
            ResetTexture();
            lineCol32 = colorScheme.FramerateLineColor;
            clearCol32 = Color.clear;
            InitUI();
            framerates = new float[tex.width];
            if(hideAfterInit){
                visible = false;
            }
        }

        void OnDestroy () {
            if(this == instance){
                instance = null;
            }
        }

        void OnShow () {
            currentFrameIndex = 0;
        }

        void OnHide () {

        }

        void Update () {
            if(Bind.TOGGLE_FRAMERATE_DISPLAY.GetKeyDown()){
                visible = !visible;
            }
            if(!visible){
                return;
            }
            currentFPS = 1f / Time.unscaledDeltaTime;
            if(currentFrameIndex == 0){
                tex.SetPixels(clearCol32, false, false);
                avgFPS = currentFPS;
                minFPS = currentFPS;
                maxFPS = currentFPS;
                var texDelta = tex.height / 2f;
                texMin = currentFPS - texDelta;
                texMax = currentFPS + texDelta;
            }else{
                var div = (float)(currentFrameIndex + 2);
                avgFPS = ((div - 1f) / div) * avgFPS + (1f / div) * currentFPS;
                minFPS = Mathf.Min(minFPS, currentFPS);
                maxFPS = Mathf.Max(maxFPS, currentFPS);
            }
            framerates[currentFrameIndex] = currentFPS;
            UpdateTexture();
            UpdateTextFields();
            currentFrameIndex = (currentFrameIndex + 1) % framerates.Length;
        }

        void UpdateTexture () {
            int startIndex = currentFrameIndex;
            if(currentFPS < texMin || currentFPS > texMax){
                texMin = Mathf.Min(texMin, currentFPS);
                texMax = Mathf.Max(texMax, currentFPS);
                tex.SetPixels(clearCol32, false, false);
                startIndex = 0;
            }
            float lastValue = (startIndex == 0) ? framerates[0] : framerates[startIndex-1];
            for(int i=startIndex; i<=currentFrameIndex; i++){
                var currentValue = framerates[i];
                int lastY = ToPixelY(lastValue);
                int currentY = ToPixelY(currentValue);
                int minY = Mathf.Min(currentY, lastY);
                int maxY = Mathf.Max(currentY, lastY);
                for(int y=minY; y<=maxY; y++){
                    tex.SetPixel(i, y, lineCol32);
                }
                lastValue = currentValue;
            }
            tex.Apply(false);

            int ToPixelY (float inputFramerate) {
                var normed = (inputFramerate - texMin) / (texMax - texMin);
                return (int)(Mathf.Clamp01(normed) * (tex.height - 1));
            }
        }

        void UpdateTextFields () {
            rawFPSText.text = $"Raw: {currentFPS:F1}";
            avgFPSText.text = $"Avg: {avgFPS:F1}";
            minFPSText.text = $"Min: {minFPS:F1}";
            maxFPSText.text = $"Max: {maxFPS:F1}";
        }

        void ResetTexture () {
            if(tex != null){
                tex.Resize(
                    width: (int)(imageRT.rect.width), 
                    height: (int)(imageRT.rect.height)
                );
            }else{
                tex = new Texture2D(
                    width: (int)(imageRT.rect.width), 
                    height: (int)(imageRT.rect.height), 
                    textureFormat: TextureFormat.RGBA32,
                    mipChain: false,
                    linear: false
                );
            }
            tex.SetPixels(clearCol32, true, false);
        }

        void InitUI () {
            image.texture = tex;
            background.color = colorScheme.BackgroundColor;
            rawFPSText.color = colorScheme.TextColor;
            avgFPSText.color = colorScheme.TextColor;
            minFPSText.color = colorScheme.TextColor;
            maxFPSText.color = colorScheme.TextColor;
        }
        
    }

}