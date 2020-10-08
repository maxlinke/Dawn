using UnityEditor;

public abstract class GenericEditor : Editor{

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

}
