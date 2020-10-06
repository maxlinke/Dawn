using UnityEngine;
using UnityEngine.UI;
using CustomInputSystem;
using System.Collections.Generic;

namespace DebugTools {

    public class FramerateDisplay : MonoBehaviour {

        const int SMALL_MODE_FPS_AVERAGE_LENGTH = 30;

        enum Mode {
            Hidden,
            Small,
            Detailed
        }

        [Header("Settings")]
        [SerializeField] Mode mode;
        [SerializeField] DebugToolColorScheme colorScheme = default;

        [Header("Components")]
        [SerializeField] Canvas canvas = default;

        [Header("Small Mode")]
        [SerializeField] RectTransform smallParent = default;
        [SerializeField] Image smallBackground = default;
        [SerializeField] Text smallFPSText = default;

        [Header("Detailed Mode")]
        [SerializeField] RectTransform detailedParent = default;
        [SerializeField] RawImage graphImage = default;
        [SerializeField] Image detailedBackground = default;
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

        RectTransform imageRT => graphImage.rectTransform;
        Texture2D tex;
        int texWidth;
        int texHeight;
        Color32[] pixels;
        Color32 lineCol32;
        Color32 prevLineCol32;
        Color32 clearCol32;

        Queue<float> smallModeDeltaTimes;
        float smallModeDeltaTimeSum;

        float[] framerates;
        float[] prevFramerates;
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
            framerates = new float[texWidth];
            prevFramerates = null;
            smallModeDeltaTimes = new Queue<float>();
            ModeUpdated();
            if(visible){
                OnShow();
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
            smallModeDeltaTimeSum = 0f;
            var dt = Time.unscaledDeltaTime;
            for(int i=0; i<SMALL_MODE_FPS_AVERAGE_LENGTH; i++){
                smallModeDeltaTimes.Enqueue(dt);
                smallModeDeltaTimeSum += dt;
            }
        }

        void OnHide () {
            tex.SetPixels(clearCol32, true, false);
            smallModeDeltaTimes.Clear();
        }

        Mode NextMode (Mode input) {
            switch(input){
                case Mode.Hidden:
                    return Mode.Small;
                case Mode.Small:
                    return Mode.Detailed;
                case Mode.Detailed:
                    return Mode.Hidden;
                default:
                    Debug.LogError($"Unknown {nameof(Mode)} \"{mode}\"!");
                    return input;
            }
        }

        void ModeUpdated () {
            switch(mode){
                case Mode.Hidden:
                    visible = false;
                    break;
                case Mode.Small:
                    smallParent.gameObject.SetActive(true);
                    detailedParent.gameObject.SetActive(false);
                    visible = true;
                    break;
                case Mode.Detailed:
                    smallParent.gameObject.SetActive(false);
                    detailedParent.gameObject.SetActive(true);
                    visible = true;
                    break;
                default:
                    Debug.LogError($"Unknown {nameof(Mode)} \"{mode}\"!");
                    break;
            }
        }

        void Update () {
            bool modeUpdated = false;
            if(Bind.TOGGLE_FRAMERATE_DISPLAY.GetKeyDown()){
                mode = NextMode(mode);
                ModeUpdated();
                modeUpdated = true;
            }
            if(!visible){
                return;
            }
            var currentDeltaTime = Time.unscaledDeltaTime;
            UpdateSmallParts(currentDeltaTime);
            UpdateDetailedParts(currentDeltaTime, modeUpdated);
        }

        void UpdateSmallParts (float currentDeltaTime) {
            smallModeDeltaTimeSum -= smallModeDeltaTimes.Dequeue();
            smallModeDeltaTimeSum += currentDeltaTime;
            smallModeDeltaTimes.Enqueue(currentDeltaTime);
            if(mode == Mode.Small){
                var smallAvgDT = smallModeDeltaTimeSum / SMALL_MODE_FPS_AVERAGE_LENGTH;
                var smallAvgFPS = Mathf.RoundToInt(1f / smallAvgDT);
                smallFPSText.text = Mathf.Min(smallAvgFPS, 999).ToString();
            }
        }

        void UpdateDetailedParts (float currentDeltaTime, bool modeUpdated) {
            var currentFPS = 1f / currentDeltaTime;
            if(currentFrameIndex == 0){
                var texDelta = texHeight / 2f;
                texMin = avgFPS - texDelta;     // TODO roll over from last unless not
                texMax = avgFPS + texDelta;     // use prev min and maxfps
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
            if(mode == Mode.Detailed){
                UpdateTexture(currentFPS, startAtZero: modeUpdated);
                UpdateTextFields(currentFPS);
            }
            currentFrameIndex = (currentFrameIndex + 1) % framerates.Length;
            if(currentFrameIndex == 0){
                prevFramerates = framerates;
                framerates = new float[framerates.Length];
            }
        }

        void UpdateTexture (float currentFPS, bool startAtZero) {
            int startIndex = startAtZero ? 0 : currentFrameIndex;
            bool redrawPrevious = ((startIndex == 0) && (prevFramerates != null));
            if(currentFPS < texMin || currentFPS > texMax){
                texMin = Mathf.Min(texMin, currentFPS - 10);
                texMax = Mathf.Max(texMax, currentFPS + 10);
                startIndex = 0;
                redrawPrevious |= prevFramerates != null;
            }
            DrawValues(framerates, startIndex, currentFrameIndex+1, lineCol32);
            if(redrawPrevious){
                DrawValues(prevFramerates, currentFrameIndex+1, prevFramerates.Length, prevLineCol32);
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, false);

            void DrawValues (float[] values, int start, int end, Color32 drawCol32) {
                float lastValue = (start == 0) ? values[0] : values[start-1];
                for(int i=start; i<end; i++){
                    var currentValue = values[i];
                    int lastY = ToPixelY(lastValue);
                    int currentY = ToPixelY(currentValue);
                    int minY = Mathf.Min(currentY, lastY);
                    int maxY = Mathf.Max(currentY, lastY);
                    int index = i;
                    for(int y=0; y<minY; y++){
                        pixels[index] = clearCol32;
                        index += texWidth;
                    }
                    for(int y=minY; y<=maxY; y++){
                        pixels[index] = drawCol32;
                        index += texWidth;
                    }
                    for(int y=maxY+1; y<texHeight; y++){
                        pixels[index] = clearCol32;
                        index += texWidth;
                    }
                    lastValue = currentValue;
                }
            }

            int ToPixelY (float inputFramerate) {
                var normed = (inputFramerate - texMin) / (texMax - texMin);
                return (int)(Mathf.Clamp01(normed) * (texHeight - 1));
            }
        }

        void UpdateTextFields (float currentFPS) {
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
            texWidth = tex.width;
            texHeight = tex.height;
            tex.SetPixels(clearCol32, true, false);
            pixels = tex.GetPixels32();
        }

        void InitUI () {
            graphImage.texture = tex;
            smallBackground.color = colorScheme.BackgroundColor;
            smallFPSText.color = colorScheme.TextColor;
            detailedBackground.color = colorScheme.BackgroundColor;
            rawFPSText.color = colorScheme.TextColor;
            avgFPSText.color = colorScheme.TextColor;
            minFPSText.color = colorScheme.TextColor;
            maxFPSText.color = colorScheme.TextColor;
        }
        
    }

}