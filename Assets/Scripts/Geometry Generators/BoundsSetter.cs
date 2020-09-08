using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GeometryGenerators {

    public class BoundsSetter : MonoBehaviour {

        [SerializeField] Vector3 newCenter = Vector3.zero;
        [SerializeField] Vector3 newExtents = Vector3.one;

        bool TryGetMesh (out Mesh mesh, out MeshFilter mf) {
            mesh = null;
            mf = GetComponent<MeshFilter>();
            if(mf == null){
                Debug.LogError($"No {nameof(MeshFilter)} found!");
                return false;
            }
            mesh = mf.sharedMesh;
            if(mesh == null){
                Debug.LogError($"No {nameof(Mesh)} on {nameof(MeshFilter)}!");
                return false;
            }
            if(!mesh.isReadable){
                Debug.LogError($"{nameof(Mesh)} isn't readable!");
                return false;
            }
            return true;
        }

        public void CopyAndApply () {
            if(TryGetMesh(out var mesh, out var mf)){
                #if UNITY_EDITOR
                Undo.RecordObject(mf, "Duplicate Mesh");
                #endif
                var newMesh = Instantiate(mesh);
                mf.sharedMesh = newMesh;
                ApplyBounds(newMesh);
            }
        }

        public void ApplyBounds (Mesh mesh = null) {
            if(mesh != null || TryGetMesh(out mesh, out _)){
                mesh.bounds = new Bounds(newCenter, newExtents);
            }
        }

        public void ResetBounds (Mesh mesh = null) {
            if(mesh != null || TryGetMesh(out mesh, out _)){
                mesh.RecalculateBounds();
            }
        }
        
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(BoundsSetter))]
    public class BoundsSetterEditor : Editor {

        public override void OnInspectorGUI () {
            DrawDefaultInspector();
            var bs = target as BoundsSetter;
            if(GUILayout.Button("! Copy Mesh and Apply Bounds !")){
                bs.CopyAndApply();
            }
            if(GUILayout.Button("Apply Bounds")){
                bs.ApplyBounds();
            }
            if(GUILayout.Button("Reset Bounds")){
                bs.ResetBounds();
            }
        }

    }
    #endif

}