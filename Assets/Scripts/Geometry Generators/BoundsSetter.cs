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

        bool TryGetMesh (out Mesh mesh) {
            mesh = null;
            var mf = GetComponent<MeshFilter>();
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

        public void ApplyBounds (Mesh mesh = null) {
            if(mesh != null || TryGetMesh(out mesh)){
                mesh.bounds = new Bounds(newCenter, newExtents);
            }
        }

        public void ResetBounds (Mesh mesh = null) {
            if(mesh != null || TryGetMesh(out mesh)){
                mesh.RecalculateBounds();
            }
        }
        
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(BoundsSetter))]
    public class BoundsSetterEditor : Editor {

        public override void OnInspectorGUI () {
            DrawDefaultInspector();
            if(GUILayout.Button("Apply Bounds")){
                ((BoundsSetter)target).ApplyBounds();
            }
            if(GUILayout.Button("Reset Bounds")){
                ((BoundsSetter)target).ResetBounds();
            }
        }

    }
    #endif

}