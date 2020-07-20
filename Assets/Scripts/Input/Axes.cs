using System.Collections.Generic;
using UnityEngine;

public static class Axes {
                                                                // referring to unity input manager settings: 
    private const string mouseX = "Mouse X";                    // < sensitivity kept at 0.1
    private const string mouseY = "Mouse Y";                    // < sensitivity kept at 0.1
    private const string mouseScroll = "Mouse ScrollWheel";     // < sensitivity upped to 1

    private const string leftStickX = "Left Stick Horizontal";  // < (and down) 0 deadzone, 1 sensitivity
    private const string leftStickY = "Left Stick Vertical";
    private const string rightStickX = "Right Stick Horizontal";
    private const string rightStickY = "Right Stick Vertical";
    private const string dpadX = "DPad Horizontal";
    private const string dpadY = "DPad Vertical";
    private const string triggers = "Triggers";
    private const string leftTrigger = "Left Trigger";
    private const string rightTrigger = "Right Trigger";

    private static AxisConfig neutralAxisConfig;                // for anything that should not get any deadzone/sensitivity (ever)
    private static AxisConfig leftStickConfig;                  // i've decided not to make individual x/y axis configs here
    private static AxisConfig rightStickConfig;                 // TODO should i split the axes in all their configs and make axes for all the other possible ones? idk...
    private static AxisConfig triggersConfig;

    public const float MIN_DEADZONE = 0f;
    public const float MAX_DEADZONE = 0.999f;           // because 1 would result in NaN problems
    public const float MIN_SENSITIVITY = 1f;
    public const float MAX_SENSITIVITY = 2f;

    public enum ID {
        MOUSE_X,
        MOUSE_Y,
        MOUSE_SCROLL,

        LEFT_STICK_X,
        LEFT_STICK_Y,
        RIGHT_STICK_X,
        RIGHT_STICK_Y,
        DPAD_X,
        DPAD_Y,
        // TRIGGERS,                // this makes treating both triggers as buttons impossible
        LEFT_TRIGGER,
        RIGHT_TRIGGER
    }

    // this would be much nicer but i can't switch over structs/classes it seems
    // and i need a way to link the axis config to the id... hmmmm....
    // public class ID {
        
    //     public static readonly ID MOUSE_X =       new ID("Mouse X",                "Mouse X+",        "Mouse X-");
    //     public static readonly ID MOUSE_Y =       new ID("Mouse Y",                "Mouse Y+",        "Mouse Y-");
    //     public static readonly ID MOUSE_SCROLL =  new ID("Mouse ScrollWheel",      "Mouse Scroll Up", "Mouse Scroll Down");

    //     public static readonly ID LEFT_STICK_X =  new ID("Left Stick Horizontal",  "Left Stick X+",   "Left Stick X-");
    //     public static readonly ID LEFT_STICK_Y =  new ID("Left Stick Vertical",    "Left Stick Y+",   "Left Stick Y-");
    //     public static readonly ID RIGHT_STICK_X = new ID("Right Stick Horizontal", "Right Stick X+", "Right Stick X-");
    //     public static readonly ID RIGHT_STICK_Y = new ID("Right Stick Vertical",   "Right Stick Y+", "Right Stick Y-");
    //     public static readonly ID DPAD_X =        new ID("DPad Horizontal",        "DPad Right",     "DPad Left");
    //     public static readonly ID DPAD_Y =        new ID("DPad Vertical",          "DPad Up",        "DPad Down");
    //     public static readonly ID LEFT_TRIGGER =  new ID("Left Trigger",           "Left Trigger",   string.Empty);
    //     public static readonly ID RIGHT_TRIGGER = new ID("Right Trigger",          "Right Trigger",  string.Empty);

    //     public readonly string unityInputIdentifier;
    //     public readonly string positiveName;
    //     public readonly string negativeName;
        
    //     private ID (string unityInputIdentifier, string positiveName, string negativeName) {
    //         this.unityInputIdentifier = unityInputIdentifier;
    //         this.positiveName = positiveName;
    //         this.negativeName = negativeName;
    //     }
    // }

    public class AxisConfig {

        public float deadzone;
        public float sensitivity;

        private float d => deadzone;
        private float s => sensitivity;

        public float ApplyConfig (float rawValue) {
            return Mathf.Sign(rawValue) * Mathf.Clamp01((s / (1f - d)) * (Mathf.Abs(rawValue) - d));
        }

        public AxisConfig (float deadzone, float sensitivity) {
            this.deadzone = deadzone;
            this.sensitivity = sensitivity;
        }

    }

    static Axes () {
        neutralAxisConfig = new AxisConfig(0f, 1f);
        if(!TryLoadingConfigsFromPlayerPrefs()){
            Debug.LogWarning("Couldn't load axis configs from PlayerPrefs, loading defaults!");
            ResetAxisConfigsToDefault();
            SaveAxisConfigsToPlayerPrefs();
        }

        bool TryLoadingConfigsFromPlayerPrefs () {
            if(!PlayerPrefs.HasKey(nameof(leftStickConfig)) || 
               !PlayerPrefs.HasKey(nameof(rightStickConfig)) ||
               !PlayerPrefs.HasKey(nameof(triggersConfig))){
                return false;
            }
            leftStickConfig = JsonUtility.FromJson<AxisConfig>(PlayerPrefs.GetString(nameof(leftStickConfig)));
            rightStickConfig = JsonUtility.FromJson<AxisConfig>(PlayerPrefs.GetString(nameof(rightStickConfig)));
            triggersConfig = JsonUtility.FromJson<AxisConfig>(PlayerPrefs.GetString(nameof(triggersConfig)));
            return true;
        }
    }

    public static void ResetAxisConfigsToDefault () {
        leftStickConfig = new AxisConfig(0.2f, 1.0f);
        rightStickConfig = new AxisConfig(0.2f, 1.0f);
        triggersConfig = new AxisConfig(0.2f, 1.0f);
    }

    public static void SaveAxisConfigsToPlayerPrefs () {
        PlayerPrefs.SetString(nameof(leftStickConfig), JsonUtility.ToJson(leftStickConfig));
        PlayerPrefs.SetString(nameof(rightStickConfig), JsonUtility.ToJson(rightStickConfig));
        PlayerPrefs.SetString(nameof(triggersConfig), JsonUtility.ToJson(triggersConfig));
    }

    public static void DeleteAxisConfigsFromPlayerPrefs () {
        PlayerPrefs.DeleteKey(nameof(leftStickConfig));
        PlayerPrefs.DeleteKey(nameof(rightStickConfig));
        PlayerPrefs.DeleteKey(nameof(triggersConfig));
    }

    public static IEnumerable<ID> IDs () {
        foreach(var idObj in System.Enum.GetValues(typeof(ID))){
            yield return (ID)idObj;
        }
    }

    private static (string name, AxisConfig config) GetAxis (ID id) {
        switch(id){
            case ID.MOUSE_X:       return (mouseX,       neutralAxisConfig);
            case ID.MOUSE_Y:       return (mouseY,       neutralAxisConfig);
            case ID.MOUSE_SCROLL:  return (mouseScroll,  neutralAxisConfig);
            case ID.LEFT_STICK_X:  return (leftStickX,   leftStickConfig);
            case ID.LEFT_STICK_Y:  return (leftStickY,   leftStickConfig);
            case ID.RIGHT_STICK_X: return (rightStickX,  rightStickConfig);
            case ID.RIGHT_STICK_Y: return (rightStickY,  rightStickConfig);
            case ID.DPAD_X:        return (dpadX,        neutralAxisConfig);
            case ID.DPAD_Y:        return (dpadY,        neutralAxisConfig);
            // case ID.TRIGGERS:      return (triggers,     triggersConfig);
            case ID.LEFT_TRIGGER:  return (leftTrigger,  triggersConfig);
            case ID.RIGHT_TRIGGER: return (rightTrigger, triggersConfig);
            default:
                Debug.LogError($"Unknown or unsupported {nameof(ID)} for axis \"{id}\"!");
                return (string.Empty, default);
        }
    }

    public static float GetAxisRaw (ID id) {
        var axis = GetAxis(id);
        return axis.config.ApplyConfig(Input.GetAxisRaw(axis.name));
    }

    public static float GetAxisSmoothed (ID id) {
        var axis = GetAxis(id);
        return axis.config.ApplyConfig(Input.GetAxis(axis.name));
    }

    public static bool AxisIsConfigurable (ID id) {
        var axis = GetAxis(id);
        return (axis.config != neutralAxisConfig);
    }

    public static void SetAxisDeadzone (ID id, float newValue) {
        if(!AxisIsConfigurable(id)){
            Debug.LogError($"Attempt to modify axis \"{id}\" but it is not modifiable!");
            return;
        }
        GetAxis(id).config.deadzone = newValue;
    }

    public static void SetAxisSensitivity (ID id, float newValue) {
        if(!AxisIsConfigurable(id)){
            Debug.LogError($"Attempt to modify axis \"{id}\" but it is not modifiable!");
            return;
        }
        GetAxis(id).config.sensitivity = newValue;
    }

    public static float GetAxisDeadzone (ID id) {
        return GetAxis(id).config.deadzone;
    }

    public static float GetAxisSensitivity (ID id) {
        return GetAxis(id).config.sensitivity;
    }

    // related to the names in KeyCodeUtils...
    public static string NiceSubAxisName (ID id, bool positive) {
        switch(id){
            case ID.MOUSE_X:       return $"Mouse X{DefaultSuffix()}";
            case ID.MOUSE_Y:       return $"Mouse Y{DefaultSuffix()}";
            case ID.MOUSE_SCROLL:  return $"Mouse Scroll{DefaultSuffix()}";
            case ID.LEFT_STICK_X:  return $"Left Stick X{DefaultSuffix()}";
            case ID.LEFT_STICK_Y:  return $"Left Stick Y{DefaultSuffix()}";
            case ID.RIGHT_STICK_X: return $"Right Stick X{DefaultSuffix()}";
            case ID.RIGHT_STICK_Y: return $"Right Stick Y{DefaultSuffix()}";
            case ID.DPAD_X:        return $"DPad {(positive ? "Right" : "Left")}";
            case ID.DPAD_Y:        return $"DPad {(positive ? "Up" : "Down")}";
            // case ID.TRIGGERS:      return $"{(positive ? "Right" : "Left")} Trigger";
            case ID.LEFT_TRIGGER:  return "Left Trigger";
            case ID.RIGHT_TRIGGER: return "Right Trigger";
            default:
                Debug.LogError($"Unknown or unsupported {nameof(ID)} for axis \"{id}\"!");
                return string.Empty;
        }

        string DefaultSuffix() {
            return (positive ? "+" : "-");
        }
    }

}
