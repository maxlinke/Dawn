using UnityEngine;
using UnityEditor;

namespace WorkbenchUtil {

    public class WorkbenchWindow : EditorWindow {

        SerializedObject serializedObject;
        SerializedProperty listProperty;
        Vector2 scrollPos;
        string errorMessage;

        [MenuItem("Window/Workbench", false, 9)]
        static void ShowWindow () {
            var consoleType = System.Type.GetType("UnityEditor.ConsoleWindow,UnityEditor.dll");
            GetWindow<WorkbenchWindow>("Workbench", true, consoleType);
        }

        void OnEnable () {
            errorMessage = string.Empty;
            scrollPos = Vector2.zero;
            if(!Workbench.TryLoadInstance()){
                Workbench.CreateInstance();
                if(Workbench.instance == null){
                    errorMessage = "Something went wrong!";
                    return;
                }
            }
            serializedObject = new SerializedObject(Workbench.instance);
            listProperty = serializedObject.FindProperty("m_workbench");
        }

        void OnGUI () {
            if(!string.IsNullOrEmpty(errorMessage)){
                DrawErrorMessage();
            }else{
                DrawWorkbench();
            }
        }

        void DrawErrorMessage () {
            using(new EditorGUILayout.VerticalScope()){
                GUILayout.FlexibleSpace();
                using(new EditorGUILayout.HorizontalScope()){
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.FlexibleSpace();
            }
        }

        void DrawWorkbench () {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            serializedObject.Update();
            listProperty.isExpanded = true;
            var sizeBefore = listProperty.arraySize;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(listProperty);
            if(EditorGUI.EndChangeCheck()){
                var sizeAfter = listProperty.arraySize;
                if(sizeAfter > sizeBefore){
                    listProperty.GetArrayElementAtIndex(sizeAfter - 1).objectReferenceValue = null;
                }
            }
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndScrollView();
        }

    }

}