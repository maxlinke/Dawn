using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RigidbodyMassHelper))]
public class RigidbodyMassHelperEditor : Editor {

    public override void OnInspectorGUI () {
        DrawDefaultInspector();
    }
	
}
