using UnityEngine;
using Persistence;

public partial class Axis {

    [System.Serializable]
    public class Config {

        public static readonly Config NEUTRAL = neutral;
        public static readonly Config ZERO = new Config(0f, 0f);
        public static readonly Config MOUSE = neutral;
        public static readonly Config LEFT_STICK = neutral;
        public static readonly Config TRIGGERS = neutral;
        public static readonly Config RIGHT_STICK = neutral;

        private static Config neutral => new Config(0f, 1f);
        private static Config mouseDefault => new Config(0f, 6f);
        private static Config controllerDefault => new Config(0.1f, 1f);

        public float deadzone;
        public float sensitivity;

        private float d => deadzone;
        private float s => sensitivity;

        public float ApplyConfig (float rawValue) {
            return Mathf.Sign(rawValue) * Mathf.Max(0f, (s / (1f - d)) * (Mathf.Abs(rawValue) - d));
        }

        public Config (float deadzone, float sensitivity) {
            this.deadzone = deadzone;
            this.sensitivity = sensitivity;
        }

        public void CopyDataFromOther (Config other) {
            this.deadzone = other.deadzone;
            this.sensitivity = other.sensitivity;
        }

        public override string ToString () {
            return $"[{nameof(deadzone)}: {deadzone:F3}, {nameof(sensitivity)}: {sensitivity:F3}]";
        }

        public static string GetLog () {
            return Line(nameof(NEUTRAL), NEUTRAL)
                + Line(nameof(ZERO), ZERO)
                + Line(nameof(MOUSE), MOUSE)
                + Line(nameof(LEFT_STICK), LEFT_STICK)
                + Line(nameof(RIGHT_STICK), RIGHT_STICK)
                + Line(nameof(TRIGGERS), TRIGGERS);

            string Line (string inputAxisName, Config inputAxisConfig) {
                return $"{inputAxisName}: {inputAxisConfig}\n";
            }
        }

        public static void SaveToDisk () {
            var fileName = FileNames.axisConfigs;
            var fileContents = JsonUtility.ToJson(SaveableAxisConfig.Create(), true);
            FileHelper.SaveConfigFile(fileName, fileContents);
            DebugConsole.Log("Saving axis configs to disk");
        }

        public static bool TryLoadFromDisk () {
            if(!FileHelper.ConfigFileExists(FileNames.axisConfigs)){
                DebugConsole.Log("No axis config file found");
                return false;
            }
            if(FileHelper.TryLoadConfigFile(FileNames.axisConfigs, out var json)){
                try{
                    JsonUtility.FromJson<SaveableAxisConfig>(json).Apply();
                    DebugConsole.Log("Successfully loaded axis configs");
                    return true;
                }catch(System.Exception e){
                    DebugConsole.LogError($"Issue loading axis configs \n{e.Message}");
                }
            }
            return false;
        }

        public static void ResetToDefault () {
            DebugConsole.Log("Loading axis config defaults");
            MOUSE.CopyDataFromOther(mouseDefault);
            NEUTRAL.CopyDataFromOther(neutral);
            LEFT_STICK.CopyDataFromOther(controllerDefault);
            RIGHT_STICK.CopyDataFromOther(controllerDefault);
            TRIGGERS.CopyDataFromOther(controllerDefault);
            SaveToDisk();
        }

        [System.Serializable]
        private class SaveableAxisConfig {

            public float mouseSensitivity;
            public Config leftStickConfig;
            public Config rightStickConfig;
            public Config triggersConfig;

            public static SaveableAxisConfig Create () {
                var output = new SaveableAxisConfig();
                output.mouseSensitivity = Config.MOUSE.sensitivity;
                output.leftStickConfig = Config.LEFT_STICK;
                output.rightStickConfig = Config.RIGHT_STICK;
                output.triggersConfig = Config.TRIGGERS;
                return output;
            }

            public void Apply () {
                Config.MOUSE.sensitivity = mouseSensitivity;
                Config.LEFT_STICK.CopyDataFromOther(leftStickConfig);
                Config.RIGHT_STICK.CopyDataFromOther(rightStickConfig);
                Config.TRIGGERS.CopyDataFromOther(triggersConfig);
            }
        }

    }
}