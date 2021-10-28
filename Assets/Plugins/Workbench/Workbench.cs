using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace WorkbenchUtil {

    public class Workbench : ScriptableObject {

        const string fileName = "Workbench.asset";

        [SerializeField, NoLabel] private List<UnityEngine.Object> m_workbench;

        public static Workbench instance { get; private set; }

        public static bool TryLoadInstance () {
            var assetPaths = AssetDatabase.FindAssets($"t:{nameof(Workbench)}").Select(AssetDatabase.GUIDToAssetPath).ToArray();
            if(assetPaths.Length < 1){
                Debug.LogWarning($"No instances of {nameof(Workbench)} found!");
                instance = null;
                return false;
            }
            if(assetPaths.Length > 1){
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"Found multiple instances of {nameof(Workbench)}:");
                foreach(var path in assetPaths){
                    sb.AppendLine($" - {path}");
                }
                Debug.LogWarning(sb.ToString());
            }
            instance = AssetDatabase.LoadAssetAtPath(assetPaths[0], typeof(Workbench)) as Workbench;
            return (instance != null);
        }

        public static void CreateInstance () {
            Debug.Log("Creating new workbench");
            var newInstance = ScriptableObject.CreateInstance<Workbench>();
            newInstance.m_workbench = new List<Object>();
            var scriptInstance = MonoScript.FromScriptableObject(newInstance);
            var scriptPath = AssetDatabase.GetAssetPath(scriptInstance);
            var newFilePath = $"{System.IO.Path.GetDirectoryName(scriptPath)}\\{fileName}";
            AssetDatabase.CreateAsset(newInstance, newFilePath);
            AssetDatabase.SaveAssets();
            instance = newInstance;
        }

    }

}