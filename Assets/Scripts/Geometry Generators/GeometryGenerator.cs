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
            if(TryGetTargets(out var mfs, out var mcs)){
                var mesh = CreateMesh();
                if(mesh != null){
                    #if UNITY_EDITOR
                        MeshUtility.Optimize(mesh);
                    #endif
                    AssignMeshToTargets(mfs, mcs, mesh);
                }
            }
        }

        [ContextMenu("Clear")]
        public void Clear () {
            if(TryGetTargets(out var mfs, out var mcs)){
                AssignMeshToTargets(mfs, mcs, null);
            }
        }

        protected void AssignMeshToTargets (IEnumerable<MeshFilter> mfs, IEnumerable<MeshCollider> mcs, Mesh mesh) {
            foreach(var mf in mfs){
                mf.sharedMesh = mesh;
            }
            foreach(var mc in mcs){
                mc.sharedMesh = mesh;
            }
        }

        protected bool TryGetTargets (out IEnumerable<MeshFilter> outputMFs, out IEnumerable<MeshCollider> outputMCs) {
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

        [ContextMenu("Save mesh as asset")]
        public void SaveMeshAsAsset () {
            #if !UNITY_EDITOR
                Debug.LogError("Saving meshes as assets is only supported in the editor!");
            #else
                if(!TryGetTargets(out var mfs, out var mcs)){
                    return;
                }
                if(!TryGetMesh(mfs, mcs, out Mesh mesh)){
                    return;
                }
                var path = EditorUtility.SaveFilePanel("Save Generated Mesh", "Assets/", mesh.name, "asset");
                if(string.IsNullOrEmpty(path)){
                    return;
                }
                path = FileUtil.GetProjectRelativePath(path);
                var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(existing != null){
                    if(mesh.subMeshCount > 1){
                        // TODO on upgrade to 2019, fix this
                        Debug.LogWarning($"Mesh has {mesh.subMeshCount} submeshes. Overwriting an existing asset means losing them. If you want to keep the submeshes, save as a new asset.");
                    }
                    existing.CopyDataFrom(mesh);
                    AssignMeshToTargets(mfs, mcs, existing);
                }else{
                    AssetDatabase.CreateAsset(mesh, path);
                }
                AssetDatabase.SaveAssets();
            #endif
        }

        bool TryGetMesh (IEnumerable<MeshFilter> mfs, IEnumerable<MeshCollider> mcs, out Mesh output) {
            output = null;
            Mesh mesh = null;
            bool matching = true;
            foreach(var mf in mfs){
                matching &= CompareOrAssign(mf.sharedMesh);
            }
            foreach(var mc in mcs){
                matching &= CompareOrAssign(mc.sharedMesh);
            }
            if(mesh == null){
                Debug.LogError("Mesh is null!");
                return false;
            }
            if(!matching){
                Debug.LogError("Mismatched meshes!");
                return false;
            }
            output = mesh;
            return true;

            bool CompareOrAssign (Mesh input) {
                if(mesh == null){
                    mesh = input;
                    return true;
                }else if(mesh == input){
                    return true;
                }
                return false;
            }
        }
        
    }

}