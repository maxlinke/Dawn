using UnityEngine;
using UnityEngine.UI;
using CustomInputSystem;
using System.Collections.Generic;

namespace DebugTools {

    public class FramerateDisplay : MonoBehaviour {

        // if i feel like giving this a rework
        // i could make the line graph constantly keep the most recent value to the right
        // start drawing there and shift all the columns of pixels one over
        // even works when initialized as clear, so that's nice
        // and make it so that when the fps is <=59 the line is a bit reddish there
        // i'll be updating the WHOLE texture every frame though, so that's ONE thing to keep in mind
        // so definitely profile before and after and see how bad it is!

        // still todo for the current graph: rollover last texmin/max (sort of)
        // obviously don't use the max(max) and min(min) because then they'll never change
        // rather use the recorded maxfps and minfps of the last cycle to get their max/min and use those

        public enum Mode {
            Hidden,
            Small,
            Detailed
        }

        [Header("Settings")]
        [SerializeField, RedIfEmpty] DebugToolColorScheme colorScheme = default;
        [SerializeField, RangedUnit("s", 0, 1)] float textUpdateInterval = 0.1f;

        [Header("Components")]
        [SerializeField, RedIfEmpty] Canvas canvas = default;

        [Header("Small Mode")]
        [SerializeField, RedIfEmpty] RectTransform smallParent = default;
        [SerializeField, RedIfEmpty] Image smallBackground = default;
        [SerializeField, RedIfEmpty] Text smallFPSText = default;

        [Header("Detailed Mode")]
        [SerializeField, RedIfEmpty] RectTransform detailedParent = default;
        [SerializeField, RedIfEmpty] RawImage graphImage = default;
        [SerializeField, RedIfEmpty] Image detailedBackground = default;
        [SerializeField, RedIfEmpty] Text rawFPSText = default;
        [SerializeField, RedIfEmpty] Text avgFPSText = default;
        [SerializeField, RedIfEmpty] Text minFPSText = default;
        [SerializeField, RedIfEmpty] Text maxFPSText = default;

        public Mode mode { get; private set; }

        private static FramerateDisplay instance;

        private const int SMALL_MODE_FPS_AVERAGE_LENGTH = 30;

        bool initialized = false;

        RectTransform imageRT => graphImage.rectTransform;
        Texture2D tex;
        int texWidth;
        int texHeight;
        Color32[] pixels;
        Color32 lineCol32;
        Color32 prevLineCol32;
        Color32 clearCol32;

        Mode lastMode;
        float nextTextUpdateTime;

        Queue<float> smallModeDeltaTimes;
        float smallModeDeltaTimeSum;

        int[] framerates;
        float avgDeltaTime;
        float avgFPS => 1f / avgDeltaTime;
        float maxFPS;
        float minFPS;
        int currentFrameIndex;

        int texMinFPS;
        int texMaxFPS;

        public void Initialize () {
            if(instance != null){
                Debug.LogError($"Singleton violation, instance of {nameof(FramerateDisplay)} is not null!");
                Destroy(this.gameObject);
                return;
            }
            instance = this;
            this.enabled = true;
            this.transform.SetParent(null);
            this.gameObject.SetActive(true);
            DontDestroyOnLoad(this.gameObject);
            canvas.sortingOrder = CanvasSortingOrder.FrameRateDisplay;
            ApplyUIColors();
            lineCol32 = colorScheme.FramerateLineColor;
            prevLineCol32 = colorScheme.FramerateLinePrevCycleColor;
            clearCol32 = Color.clear;
            tex = new Texture2D(
                width: (int)(imageRT.rect.width), 
                height: (int)(imageRT.rect.height), 
                textureFormat: TextureFormat.RGBA32,
                mipChain: false,
                linear: false
            );
            tex.filterMode = FilterMode.Point;
            graphImage.texture = tex;
            texWidth = tex.width;
            texHeight = tex.height;
            pixels = tex.GetPixels32();
            ClearImage();
            framerates = new int[texWidth];
            smallModeDeltaTimes = new Queue<float>();
            ResetAllValues();
            lastMode = Mode.Hidden;
            mode = Mode.Hidden;     // so that setmode doesn't quit because the set mode is the same as the current one
            SetMode(Mode.Small);
            initialized = true;
        }

        void ApplyUIColors () {
            smallBackground.color = colorScheme.BackgroundColor;
            smallFPSText.color = colorScheme.TextColor;
            detailedBackground.color = colorScheme.BackgroundColor;
            rawFPSText.color = colorScheme.TextColor;
            avgFPSText.color = colorScheme.TextColor;
            minFPSText.color = colorScheme.TextColor;
            maxFPSText.color = colorScheme.TextColor;
        }

        void OnDestroy () {
            if(this == instance){
                instance = null;
            }
        }

        void ResetAllValues () {
            for(int i=0; i<framerates.Length; i++){
                framerates[i] = 0;
            }
            currentFrameIndex = -1;
            var dt = Time.unscaledDeltaTime;
            smallModeDeltaTimes.Clear();
            smallModeDeltaTimeSum = 0f;
            for(int i=0; i<SMALL_MODE_FPS_AVERAGE_LENGTH; i++){
                smallModeDeltaTimes.Enqueue(dt);
                smallModeDeltaTimeSum += dt;
            }
            nextTextUpdateTime = Time.unscaledTime;
        }

        void ClearImage () {
            for(int i=0; i<pixels.Length; i++){
                pixels[i] = clearCol32;
            }
        }

        public void SetMode (Mode newMode) {
            if(newMode != this.mode){
                this.mode = newMode;
                switch(mode){
                    case Mode.Hidden:
                        canvas.enabled = false;
                        break;
                    case Mode.Small:
                        smallParent.gameObject.SetActive(true);
                        detailedParent.gameObject.SetActive(false);
                        canvas.enabled = true;
                        break;
                    case Mode.Detailed:
                        smallParent.gameObject.SetActive(false);
                        detailedParent.gameObject.SetActive(true);
                        canvas.enabled = true;
                        break;
                    default:
                        Debug.LogError($"Unknown {nameof(Mode)} \"{mode}\"!");
                        break;
                }
            }
        }

        void LateUpdate () {
            if(!initialized){
                return;
            }
            if(Bind.TOGGLE_FRAMERATE_DISPLAY.GetKeyDown()){
                var currentModeIndex = (int)mode;
                var modeCount = System.Enum.GetValues(typeof(Mode)).Length;
                var nextMode = (Mode)((currentModeIndex + 1) % modeCount);
                SetMode(nextMode);
            }
            currentFrameIndex = (currentFrameIndex + 1) % framerates.Length;    // works properly because default value is -1
            var currentDeltaTime = Time.unscaledDeltaTime;
            var currentFPS = 1f / currentDeltaTime;
            var modeUpdated = (mode != lastMode);
            var updateText = modeUpdated || Time.unscaledTime > nextTextUpdateTime;
            if(updateText){
                nextTextUpdateTime = Time.unscaledTime + textUpdateInterval;
            }
            CollectCurrentDeltaTime(currentDeltaTime);
            CollectCurrentFPS(currentFPS);
            switch(mode){
                case Mode.Hidden:
                    break;
                case Mode.Small:
                    if(updateText){
                        UpdateSmallTextField();
                    }
                    break;
                case Mode.Detailed:
                    UpdateTexture(modeUpdated);
                    UpdateDetailedTextFields(currentFPS, updateText);
                    break;
                default:
                    Debug.LogError($"Unknown mode \"{mode}\"!");
                    break;
            }
            lastMode = mode;
        }

        void UpdateSmallTextField () {
            var smallAvgDT = smallModeDeltaTimeSum / SMALL_MODE_FPS_AVERAGE_LENGTH;
            var smallAvgFPS = Mathf.RoundToInt(1f / smallAvgDT);
            smallFPSText.text = Mathf.Min(smallAvgFPS, 999).ToString();
        }

        void UpdateDetailedTextFields (float currentFPS, bool updateText) {
            rawFPSText.text = $"Raw: {currentFPS:F1}";
            if(updateText){
                avgFPSText.text = $"Avg: {avgFPS:F1}";
                minFPSText.text = $"Min: {minFPS:F1}";
                maxFPSText.text = $"Max: {maxFPS:F1}";
            }
        }

        void CollectCurrentDeltaTime (float currentDeltaTime) {
            smallModeDeltaTimeSum -= smallModeDeltaTimes.Dequeue();
            smallModeDeltaTimeSum += currentDeltaTime;
            smallModeDeltaTimes.Enqueue(currentDeltaTime);            
        }

        void CollectCurrentFPS (float currentFPS) {
            if(currentFrameIndex == 0){
                avgDeltaTime = 1f / currentFPS;
                texMinFPS = (int)(avgFPS - (texHeight / 2f));
                texMaxFPS = texMinFPS + texHeight;
                minFPS = currentFPS;
                maxFPS = currentFPS;
            }else{
                var numPrev = currentFrameIndex;
                var avgPrev = avgDeltaTime;
                var curr = 1f / currentFPS;
                var numCurr = currentFrameIndex + 1;
                avgDeltaTime = ((avgPrev * numPrev) + curr) / numCurr;
                minFPS = Mathf.Min(minFPS, currentFPS);
                maxFPS = Mathf.Max(maxFPS, currentFPS);
            }
            framerates[currentFrameIndex] = Mathf.RoundToInt(currentFPS);
        }

        void UpdateTexture (bool startAtZero) {
            int currentFPS = framerates[currentFrameIndex];
            int startIndex = startAtZero ? 0 : currentFrameIndex;
            int nextFrameIndex = (currentFrameIndex + 1) % framerates.Length;
            bool gotPrev = framerates[nextFrameIndex] > 0f;
            bool redrawPrevious = ((startIndex == 0) && gotPrev);
            if(currentFPS < texMinFPS || currentFPS > texMaxFPS){
                texMinFPS = Mathf.Min(texMinFPS, currentFPS - 10);
                texMaxFPS = Mathf.Max(texMaxFPS, currentFPS + 10);
                startIndex = 0;
                redrawPrevious |= gotPrev;
            }
            int toPixelYDivider = texMaxFPS - texMinFPS;
            DrawValues(framerates, startIndex, currentFrameIndex+1, lineCol32);
            if(redrawPrevious){
                DrawValues(framerates, currentFrameIndex+1, framerates.Length, prevLineCol32);
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, false);

            void DrawValues (int[] values, int start, int end, Color32 drawCol32) {
                int lastValue = (start == 0) ? values[0] : values[start-1];
                int lastY = ToPixelY(lastValue);
                for(int i=start; i<end; i++){
                    int currentValue = values[i];
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
                    lastY = currentY;
                }
            }

            int ToPixelY (int inputFramerate) {
                int rawY = ((inputFramerate - texMinFPS) * texHeight) / toPixelYDivider;
                return Mathf.Max(0, Mathf.Min(rawY, texHeight - 1));
            }
        }
        
    }

}