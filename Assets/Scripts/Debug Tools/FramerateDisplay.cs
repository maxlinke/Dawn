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
        Color32 prevLineCol32;
        Color32 clearCol32;

        float[] framerates;
        float[] prevFramerates;
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
            prevLineCol32 = colorScheme.FramerateLinePrevCycleColor;
            clearCol32 = Color.clear;
            InitUI();
            framerates = new float[tex.width];
            prevFramerates = null;
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
            prevFramerates = null;
        }

        void OnHide () {
            tex.SetPixels(clearCol32, true, false);
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
                SetupTextureForNextCycle(false);
                var texDelta = tex.height / 2f;
                texMin = avgFPS - texDelta;
                texMax = avgFPS + texDelta;
                avgFPS = currentFPS;
                minFPS = currentFPS;
                maxFPS = currentFPS;
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
            if(currentFrameIndex == 0){
                if(prevFramerates == null){
                    prevFramerates = new float[framerates.Length];
                }
                framerates.CopyTo(prevFramerates, 0);
            }
        }

        void UpdateTexture () {
            int startIndex = currentFrameIndex;
            bool redrawPrevious = ((currentFrameIndex == 0) && (prevFramerates != null));
            if(currentFPS < texMin || currentFPS > texMax){
                texMin = Mathf.Min(texMin, currentFPS);
                texMax = Mathf.Max(texMax, currentFPS);
                tex.SetPixels(clearCol32, false, false);
                startIndex = 0;
                redrawPrevious |= prevFramerates != null;
            }
            DrawValues(framerates, startIndex, currentFrameIndex+1, lineCol32);
            if(redrawPrevious){
                DrawValues(prevFramerates, currentFrameIndex+1, prevFramerates.Length, prevLineCol32);
            }
            tex.Apply(false);

            void DrawValues (float[] values, int start, int end, Color32 drawCol32) {
                float lastValue = (start == 0) ? values[0] : values[start-1];
                for(int i=start; i<end; i++){
                    var currentValue = values[i];
                    int lastY = ToPixelY(lastValue);
                    int currentY = ToPixelY(currentValue);
                    int minY = Mathf.Min(currentY, lastY);
                    int maxY = Mathf.Max(currentY, lastY);
                    for(int y=minY; y<=maxY; y++){
                        tex.SetPixel(i, y, drawCol32);
                    }
                    for(int y=0; y<minY; y++){
                        tex.SetPixel(i, y, clearCol32);
                    }
                    for(int y=maxY+1; y<tex.height; y++){
                        tex.SetPixel(i, y, clearCol32);
                    }
                    lastValue = currentValue;
                }
            }

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

        void SetupTextureForNextCycle (bool apply) {
            var newPixels = new Color32[tex.width * tex.height];
            var origPixels = tex.GetPixels32();
            for(int i=0; i<newPixels.Length; i++){
                var c32 = origPixels[i];
                newPixels[i] = (c32.Equals(lineCol32) ? prevLineCol32 : clearCol32);
            }
            tex.SetPixels32(newPixels);
            if(apply){
                tex.Apply(false, false);
            }
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