using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Testing {

    public class VariousLogTypeLogger : MonoBehaviour {

        [SerializeField, CustomRange(1, int.MaxValue, false)] int charactersOnLine = default;
        [SerializeField, CustomRange(1, int.MaxValue, false)] int lines = default;

        public string GetText () {
            string text = string.Empty;
            int c = 0;
            for(int i=0; i<lines; i++){
                var line = new char[charactersOnLine];
                for(int j=0; j<line.Length; j++){
                    line[j] = (char)(c + 'A');
                    c = (c+1) % ('Z' - 'A' + 1);
                }
                text += $"{new string(line)}\n";
            }
            return text.Trim();
        }
        
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(VariousLogTypeLogger))]
    public class VariousLogTypeLoggerEditor : Editor {

        public override void OnInspectorGUI () {
            base.OnInspectorGUI();
            if(!EditorApplication.isPlaying){
                return;
            }
            if(GUILayout.Button("Log")){
                Log(Debug.Log);
            }
            if(GUILayout.Button("Log Warning")){
                Log(Debug.LogWarning);
            }
            if(GUILayout.Button("Log Error")){
                Log(Debug.LogError);
            }
            if(GUILayout.Button("Throw Exception")){
                Log((s) => throw new System.Exception(s));
            }

            void Log (System.Action<string> log) {
                log(((VariousLogTypeLogger)target).GetText());
            }
        }

    }

#endif

}