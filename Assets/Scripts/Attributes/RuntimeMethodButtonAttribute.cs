using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
#endif

[AttributeUsage(AttributeTargets.Method)]
public class RuntimeMethodButtonAttribute : Attribute { }

// If this editor is placed in a new script in an "Editor" folder, scripts can't use this editor
// locally anymore. Since the point of this is to enable scripts to have buttons without needing
// to set up an actual custom editor, it is within these #if-defines.

#if UNITY_EDITOR
public abstract class RuntimeMethodButtonEditor : Editor {

    MethodInfo[] methods;

    public override void OnInspectorGUI () {
        DrawInspector();
        DrawButtons();
    }

    protected virtual void DrawInspector () {
        DrawDefaultInspector();
    }

    private void DrawButtons () {
        if(EditorApplication.isPlaying){
            if(methods == null){
                FindAppropriateMethods();
            }
            if(methods.Length > 0){
                GUILayout.Space(10f);
            }
            foreach(var method in methods){
                if(GUILayout.Button(method.Name)){
                    method.Invoke(target, new object[method.GetParameters().Length]);
                }
            }
        }

        void FindAppropriateMethods () {
            var methodList = new List<MethodInfo>();
            foreach(var method in target.GetType().GetMethods()){
                foreach(var attribute in method.GetCustomAttributes()){
                    if(attribute is RuntimeMethodButtonAttribute){
                        methodList.Add(method);
                    }
                }
            }
            methods = methodList.ToArray();
        }
    }
}
#else
public abstract class RuntimeMethodButtonEditor { }
#endif

