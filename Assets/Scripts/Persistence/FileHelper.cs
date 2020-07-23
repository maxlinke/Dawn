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

        public static void OpenDataDirectory () {
            System.Diagnostics.Process.Start(dataDirectory);
        }

        public static void DeleteAllData () {
            if(Directory.Exists(configFilePath)){
                try{
                    Directory.Delete(configFilePath, true);
                }catch(System.Exception e){
                    Debug.LogError(e);
                }
            }
        }

        public static void SaveConfigFile (string fileName, string fileContents) {
            SaveToFile(configFilePath, fileName, fileContents);
        }

        public static void SaveSaveFile (string fileName, string fileContents) {
            SaveToFile(saveFilePath, fileName, fileContents);
        }

        private static void SaveToFile (string directoryPath, string fileName, string fileContents) {
            try{
                Directory.CreateDirectory(directoryPath);
                var filePath = MakeFilePath(directoryPath, fileName);
                using(var fs = File.Create(filePath)){
                    var bytes = new UTF8Encoding(true).GetBytes(fileContents);
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Flush(true);
                }
            }catch(System.Exception e){
                Debug.LogError(e.Message);
            }
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
                Debug.LogError(e.Message);
                fileContents = null;
                return false;
            }
        }
        
    }

}
