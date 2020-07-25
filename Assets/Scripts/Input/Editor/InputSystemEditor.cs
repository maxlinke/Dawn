using UnityEngine;
using UnityEditor;

namespace CustomInputSystem {

    [CustomEditor(typeof(InputSystem))]
    public class InputSystemEditor : Editor {

        public override void OnInspectorGUI () {
            DrawDefaultInspector();
            GUILayout.Space(20f);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Open Config Directory", GUILayout.Width(140))){
                Persistence.FileHelper.OpenConfigDirectory();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);
        }
        
    }
}