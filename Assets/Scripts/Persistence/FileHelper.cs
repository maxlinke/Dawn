using UnityEngine;
using System.IO;
using System.Text;

namespace Persistence {

    public static class FileHelper  {

        private static string dataDirectory => Application.persistentDataPath;
        private static string saveFilePath => $"{dataDirectory}/Save";
        private static string configFilePath => $"{dataDirectory}/Config";

        private static string MakeFilePath (string directoryPath, string fileName) {
            return $"{directoryPath}/{fileName}";
        }

#if UNITY_EDITOR

        [UnityEditor.MenuItem("Data/Open Config Directory", true)]
        private static bool EditorOpenConfigDir_Validate () => ConfigDirectoryExists;

        [UnityEditor.MenuItem("Data/Open Config Directory")]
        private static void EditorOpenConfigDir () => OpenConfigDirectory();

        [UnityEditor.MenuItem("Data/Open Save File Directory", true)]
        private static bool EditorOpenSaveFileDir_Validate () => SaveFileDirectoryExists;

        [UnityEditor.MenuItem("Data/Open Save File Directory")]
        private static void EditorOpenSaveFileDir () => OpenSaveFileDirectory();

        [UnityEditor.MenuItem("Data/Open Persistent Data Directory")]
        public static void EditorOpenDataDir () => OpenDataDirectory();

#endif

        public static bool ConfigDirectoryExists => Directory.Exists(configFilePath);

        public static bool SaveFileDirectoryExists => Directory.Exists(saveFilePath);

        public static void OpenConfigDirectory () {
            System.Diagnostics.Process.Start(configFilePath);
        }
        
        public static void OpenSaveFileDirectory () {
            System.Diagnostics.Process.Start(saveFilePath);
        }

        public static void OpenDataDirectory () {
            System.Diagnostics.Process.Start(dataDirectory);
        }

        public static void DeleteAllConfigData () {
            if(ConfigDirectoryExists){
                try{
                    Directory.Delete(configFilePath, true);
                }catch(System.Exception e){
                    Debug.LogWarning($"Problem deleting all config data: \n{e.Message}");
                }
            }
        }

        public static void DeleteAllSaveData () {
            if(SaveFileDirectoryExists){
                try{
                    Directory.Delete(saveFilePath, true);
                }catch(System.Exception e){
                    Debug.LogWarning($"Problem deleting all save data: \n{e.Message}");
                }
            }
        }

        public static void DeleteAllData () {
            DeleteAllConfigData();
            DeleteAllSaveData();
        }

        public static void SaveConfigFile (string fileName, string fileContents, out string path) {
            SaveToFile(configFilePath, fileName, fileContents, out path);
        }

        public static void SaveSaveFile (string fileName, string fileContents, out string path) {
            SaveToFile(saveFilePath, fileName, fileContents, out path);
        }

        public static void SaveToFile (string directoryPath, string fileName, string fileContents, out string filePath) {
            try{
                Directory.CreateDirectory(directoryPath);
                filePath = MakeFilePath(directoryPath, fileName);
                using(var fs = File.Create(filePath)){
                    var bytes = new UTF8Encoding(true).GetBytes(fileContents);
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Flush(true);
                }
            }catch(System.Exception e){
                filePath = string.Empty;
                Debug.LogWarning($"Problem saving to file: \n{e.Message}");
            }
        }

        public static bool ConfigFileExists (string fileName, out string path) {
            path = MakeFilePath(configFilePath, fileName);
            return FileExists(path);
        }

        public static bool SaveFileExists (string fileName, out string path) {
            path = MakeFilePath(saveFilePath, fileName);
            return FileExists(path);
        }

        private static bool FileExists (string filePath) {
            return File.Exists(filePath);
        }

        public static bool TryLoadConfigFile (string fileName, out string fileContents) {
            return TryLoadFile(MakeFilePath(configFilePath, fileName), out fileContents);
        }

        public static bool TryLoadSaveFile (string fileName, out string fileContents) {
            return TryLoadFile(MakeFilePath(saveFilePath, fileName), out fileContents);
        }

        public static bool TryLoadFile (string filePath, out string fileContents) {
            try{
                fileContents = File.ReadAllText(filePath, Encoding.UTF8);
                return true;
            }catch(System.Exception e){
                Debug.LogWarning($"Problem loading file: \n{e.Message}");
                fileContents = null;
                return false;
            }
        }
        
    }

}
