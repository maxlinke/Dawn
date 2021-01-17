using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Test : MonoBehaviour {

    [SerializeField] ScrollingTextDisplay textDisplay = default;
    [SerializeField, CustomRange(1, int.MaxValue, false)] int charactersOnLine = default;
    [SerializeField, CustomRange(1, int.MaxValue, false)] int lines = default;

    void Awake () {

    }

    void Update () {
              
    }

    public void AddText () {
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
        // textDisplay.AppendLine(text.Trim());
        textDisplay.AppendLine(text);
    }

    public void Clear () {
        textDisplay.Clear();
    }
	
}

#if UNITY_EDITOR

[CustomEditor(typeof(Test))]
public class TestEditor : Editor {

    public override void OnInspectorGUI () {
        base.OnInspectorGUI();
        if(GUILayout.Button("Add Text")){
            ((Test)target).AddText();
        }
        if(GUILayout.Button("Clear")){
            ((Test)target).Clear();
        }
    }

}

#endif
