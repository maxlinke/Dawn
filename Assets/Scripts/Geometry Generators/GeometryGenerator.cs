using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GeometryGenerators {

    public abstract class GeometryGenerator : MonoBehaviour {

        [Header("Targets")]
        [SerializeField] bool targetOnlySelf = true;
        [SerializeField] MeshFilter[] targetMeshFilters = default;
        [SerializeField] MeshCollider[] targetMeshColliders = default;

        protected abstract Mesh CreateMesh ();

        [ContextMenu("Generate")]
        public void Generate () {
            if(GetTargets(out var mfs, out var mcs)){
                var mesh = CreateMesh();
                if(mesh != null){
                    foreach(var mf in mfs){
                        mf.sharedMesh = mesh;
                    }
                    foreach(var mc in mcs){
                        mc.sharedMesh = mesh;
                    }
                }
            }
        }

        [ContextMenu("Clear")]
        public void Clear () {
            if(GetTargets(out var mfs, out var mcs)){
                foreach(var mf in mfs){
                    mf.sharedMesh = null;
                }
                foreach(var mc in mcs){
                    mc.sharedMesh = null;
                }
            }
        }

        protected bool GetTargets (out IEnumerable<MeshFilter> outputMFs, out IEnumerable<MeshCollider> outputMCs) {
            var meshFilters = new List<MeshFilter>();
            var meshColliders = new List<MeshCollider>();
            outputMFs = meshFilters;
            outputMCs = meshColliders;
            if(targetOnlySelf){
                var mf = GetComponent<MeshFilter>();
                var mc = GetComponent<MeshCollider>();
                if(mf == null && mc == null){
                    Debug.LogError($"No {nameof(MeshFilter)} or {nameof(MeshCollider)} found on {this.GetType()} \"{this.name}\"!");
                    return false;
                }
                if(mf != null){
                    meshFilters.Add(mf);
                }
                if(mc != null){
                    meshColliders.Add(mc);
                }
                return true;
            }else{
                var noMFs = targetMeshFilters == null || targetMeshFilters.Length <= 0;
                var noMCs = targetMeshColliders == null || targetMeshColliders.Length <= 0;
                if(noMFs && noMCs){
                    Debug.LogError($"No targets assigned on {this.GetType()} \"{this.name}\"!");
                    return false;
                }
                if(targetMeshFilters != null){
                    meshFilters.AddRange(targetMeshFilters);
                }
                if(targetMeshColliders != null){
                    meshColliders.AddRange(targetMeshColliders);
                }
                return true;
            }
        }

        #if UNITY_EDITOR

        public static void DrawInspectorTargets (SerializedObject serializedObject) {
            var selfProp = serializedObject.FindProperty(nameof(targetOnlySelf));
            EditorGUILayout.PropertyField(selfProp);
            if(!selfProp.boolValue){
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(targetMeshFilters)), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(targetMeshColliders)), true);
            }
        }

        public static void DrawButtons (GeometryGenerator targetGenerator, bool addSpace = true) {
            if(addSpace){
                GUILayout.Space(10f);
            }
            if(GUILayout.Button("Generate")){
                targetGenerator.Generate();
            }
            if(GUILayout.Button("Clear")){
                targetGenerator.Clear();
            }
        }

        #endif
        
    }

}