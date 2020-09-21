using UnityEngine;
using UnityEditor;

public abstract class GenericEditor : Editor{

    public const float BACKGROUND_TINT_STRENGTH = 0.3f;

    protected virtual void OnEnable () { }

    public override void OnInspectorGUI () {
        serializedObject.Update();
        var currentProperty = serializedObject.GetIterator();
        currentProperty.NextVisible(true);                                                  // necessary to initialize iterator    
        EditorTools.DrawDisabled(() => EditorGUILayout.PropertyField(currentProperty));     // script reference
        while(currentProperty.NextVisible(false)){                                          // all properties
            if(!DrawPropertyCustom(currentProperty)){
                EditorGUILayout.PropertyField(currentProperty, true);
            }
        }
        serializedObject.ApplyModifiedProperties();
    }

    protected abstract bool DrawPropertyCustom (SerializedProperty property);

    protected virtual void ObjectFieldRedBackgroundIfNull (SerializedProperty property) {
        var bgCol = GUI.backgroundColor;
        if(property.objectReferenceValue == null){
            GUI.backgroundColor = Color.Lerp(bgCol, Color.red, BACKGROUND_TINT_STRENGTH);
        }
        EditorGUILayout.PropertyField(property, true);
        GUI.backgroundColor = bgCol;
    }
	
}
