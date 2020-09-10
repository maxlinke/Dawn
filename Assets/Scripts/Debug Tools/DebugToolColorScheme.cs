using UnityEngine;

namespace DebugTools {

    [CreateAssetMenu(menuName = "Debug Tool Color Scheme", fileName = "New DebugToolColorScheme")]
    public class DebugToolColorScheme : ScriptableObject {

        [Header("Generic UI Element Colors")]
        [SerializeField] Color textColor = Color.black;
        [SerializeField] Color backgroundColor = Color.black;
        [SerializeField] Color midgroundColor = Color.black;
        [SerializeField] Color foregroundColor = Color.black;

        public Color TextColor => textColor;
        public Color BackgroundColor => backgroundColor;
        public Color MidgroundColor => midgroundColor;
        public Color ForegroundColor => foregroundColor;

        [Header("Debug Log Colors")]
        [SerializeField] Color debugLogColor = Color.black;
        [SerializeField] Color debugWarningColor = Color.black;
        [SerializeField] Color debugErrorColor = Color.black;
        [SerializeField] Color debugExceptionColor = Color.black;

        public Color DebugLogColor => debugLogColor;
        public Color DebugWarningColor => debugWarningColor;
        public Color DebugErrorColor => debugErrorColor;
        public Color DebugExceptionColor => debugExceptionColor;

        [Header("Framerate Display")]
        [SerializeField] Color framerateLineColor = Color.white;
        [SerializeField] Color framerateLinePrevCycleColor = Color.grey;

        public Color FramerateLineColor => framerateLineColor;
        public Color FramerateLinePrevCycleColor => framerateLinePrevCycleColor;

        [Header("Player Controller Debug")]
        [SerializeField] Color pcDebugForeground = Color.white;
        [SerializeField] Color pcDebugBackground = Color.black;

        public Color PCDebugForeground => pcDebugForeground;
        public Color PCDebugBackground => pcDebugBackground;
        
    }

}