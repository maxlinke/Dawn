﻿using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CenterOfMassSetter : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] Vector3 newCenterOfMass = Vector3.zero;
    [SerializeField] bool setOnStart = true;

    [Header("Gizmos")]
    [SerializeField] bool drawGizmos = true;
    [SerializeField] Color gizmoColor = Color.cyan;
    [SerializeField] float gizmoSize = 0.1f;

    void Start () {
        if(setOnStart){
            UpdateCenterOfMass();
        }
    }

    public void UpdateCenterOfMass () {
        var rb = GetComponent<Rigidbody>();
        if(rb != null){
            rb.centerOfMass = newCenterOfMass;
        }
    }

    void OnDrawGizmosSelected () {
        if(!drawGizmos){
            return;
        }
        #if UNITY_EDITOR
        if(Selection.activeGameObject != this.gameObject){
            return;
        }
        #endif
        var rb = GetComponent<Rigidbody>();
        if(rb == null){
            return;
        }
        var gc = Gizmos.color;
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(rb.worldCenterOfMass, gizmoSize);
        Gizmos.DrawSphere(rb.transform.TransformPoint(newCenterOfMass), 0.5f * gizmoSize);
        Gizmos.color = gc;
    }
	
}

#if UNITY_EDITOR

[CustomEditor(typeof(CenterOfMassSetter))]
public class CenterOfMassSetterEditor : Editor {

    public override void OnInspectorGUI () {
        DrawDefaultInspector();
        if(EditorApplication.isPlaying){
            if(GUILayout.Button("Update Center Of Mass")){
                ((CenterOfMassSetter)target).UpdateCenterOfMass();
            }
        }
    }

}

#endif
