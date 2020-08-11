using UnityEngine;
using UnityEditor;

namespace CustomInputSystem {

    [CustomEditor(typeof(InputSystem))]
    public class InputSystemEditor : Editor {

        public override void OnInspectorGUI () {
            DrawDefaultInspector();
            GUILayout.Space(20f);
            CenteredButton("Open Config Directory", 140, Persistence.FileHelper.OpenConfigDirectory);
            if(EditorApplication.isPlaying){
                CenteredButton("Reset Keybinds", 140, () => {Bind.ResetToDefault();});
            }
            GUILayout.Space(20f);

            void CenteredButton (string label, float width, System.Action onClick) {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if(GUILayout.Button(label, GUILayout.Width(width))){
                    onClick();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }
        
    }
}