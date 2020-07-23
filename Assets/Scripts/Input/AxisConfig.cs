using UnityEngine;
using Persistence;

[System.Serializable]
public class AxisConfig {

    private const string saveFileName = "axis_configuration";

    public static readonly AxisConfig NEUTRAL = defaultNeutral;
    public static readonly AxisConfig MOUSE = defaultMouse;
    public static readonly AxisConfig LEFT_STICK = defaultController;
    public static readonly AxisConfig RIGHT_STICK = defaultController;
    public static readonly AxisConfig TRIGGERS = defaultController;

    private static AxisConfig defaultNeutral => new AxisConfig(0f, 1f);
    private static AxisConfig defaultMouse => new AxisConfig(0f, 6f);
    private static AxisConfig defaultController => new AxisConfig(0.1f, 1f);

    public float deadzone;
    public float sensitivity;

    private float d => deadzone;
    private float s => sensitivity;

    public float ApplyConfig (float rawValue) {
        return Mathf.Sign(rawValue) * Mathf.Max(0f, (s / (1f - d)) * (Mathf.Abs(rawValue) - d));
    }

    public AxisConfig (float deadzone, float sensitivity) {
        this.deadzone = deadzone;
        this.sensitivity = sensitivity;
    }

    public void CopyDataFromOther (AxisConfig other) {
        this.deadzone = other.deadzone;
        this.sensitivity = other.sensitivity;
    }

    public override string ToString () {
        return $"[{nameof(deadzone)}: {deadzone:F3}, {nameof(sensitivity)}: {sensitivity:F3}]";
    }

    public static string GetLog () {
        return Line(nameof(NEUTRAL), NEUTRAL)
            + Line(nameof(MOUSE), MOUSE)
            + Line(nameof(LEFT_STICK), LEFT_STICK)
            + Line(nameof(RIGHT_STICK), RIGHT_STICK)
            + Line(nameof(TRIGGERS), TRIGGERS);

        string Line (string inputAxisName, AxisConfig inputAxisConfig) {
            return $"{inputAxisName}: {inputAxisConfig}\n";
        }
    }

    public static void SaveToDisk () {
        var fileName = FileNames.axisConfigs;
        var fileContents = JsonUtility.ToJson(SaveableAxisConfig.Create(), true);
        FileHelper.SaveConfigFile(fileName, fileContents);
        Debug.Log("saving");
    }

    public static bool TryLoadFromDisk () {
        if(FileHelper.TryLoadConfigFile(FileNames.axisConfigs, out var json)){
            try{
                JsonUtility.FromJson<SaveableAxisConfig>(json).Apply();
                return true;
            }catch(System.Exception e){
                Debug.LogError(e.Message);
            }            
        }
        return false;
    }

    public static void ResetToDefault () {
        MOUSE.CopyDataFromOther(defaultMouse);
        NEUTRAL.CopyDataFromOther(defaultNeutral);
        LEFT_STICK.CopyDataFromOther(defaultController);
        RIGHT_STICK.CopyDataFromOther(defaultController);
        TRIGGERS.CopyDataFromOther(defaultController);
        SaveToDisk();
    }

    [System.Serializable]
    private class SaveableAxisConfig {

        public float mouseSensitivity;
        public AxisConfig leftStickConfig;
        public AxisConfig rightStickConfig;
        public AxisConfig triggersConfig;

        public static SaveableAxisConfig Create () {
            var output = new SaveableAxisConfig();
            output.mouseSensitivity = AxisConfig.MOUSE.sensitivity;
            output.leftStickConfig = AxisConfig.LEFT_STICK;
            output.rightStickConfig = AxisConfig.RIGHT_STICK;
            output.triggersConfig = AxisConfig.TRIGGERS;
            return output;
        }

        public void Apply () {
            AxisConfig.MOUSE.sensitivity = mouseSensitivity;
            AxisConfig.LEFT_STICK.CopyDataFromOther(leftStickConfig);
            AxisConfig.RIGHT_STICK.CopyDataFromOther(rightStickConfig);
            AxisConfig.TRIGGERS.CopyDataFromOther(triggersConfig);
        }
    }
	
}
