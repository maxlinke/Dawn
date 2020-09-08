using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GeometryGenerators {

    public class BoundsSetter : MonoBehaviour {

        [Header("Settings")]
        [SerializeField] Vector3 newCenter = Vector3.zero;
        [SerializeField] Vector3 newExtents = Vector3.one;

        [Header("Gizmos")]
        [SerializeField] bool drawGizmos = true;
        [SerializeField] Color gizmoColor = Color.white;

        void OnDrawGizmosSelected () {
            if(!drawGizmos){
                return;
            }
            #if UNITY_EDITOR
            if(Selection.activeGameObject != this.gameObject){
                return;
            }
            #endif
            if(TryGetMesh(out var mesh, out var mf)){
                var mr = GetComponent<MeshRenderer>();
                var gc = Gizmos.color;
                Gizmos.color = gizmoColor;
                if(mr != null){
                    Gizmos.DrawWireCube(mr.bounds.center, 2f * mr.bounds.extents);
                    Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, gizmoColor.a * 0.5f);
                }
                var gm = Gizmos.matrix;
                Gizmos.matrix = mf.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(mesh.bounds.center, 2f * mesh.bounds.extents);
                Gizmos.matrix = gm;
                Gizmos.color = gc;
            }
        }

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

        public void InstanceMesh () {
            if(TryGetMesh(out var mesh, out var mf)){
                #if UNITY_EDITOR
                Undo.RecordObject(mf, "Duplicate Mesh");
                #endif
                var newMesh = Instantiate(mesh);
                mf.sharedMesh = newMesh;
            }
        }

        public void ApplyBounds (Mesh mesh = null) {
            if(mesh != null || TryGetMesh(out mesh, out _)){
                #if UNITY_EDITOR
                Undo.RecordObject(mesh, "Update bounds");
                #endif
                mesh.bounds = new Bounds(newCenter, newExtents);
            }
        }

        public void ResetBounds (Mesh mesh = null) {
            if(mesh != null || TryGetMesh(out mesh, out _)){
                #if UNITY_EDITOR
                Undo.RecordObject(mesh, "Reset bounds");
                #endif
                mesh.RecalculateBounds();
            }
        }
        
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(BoundsSetter))]
    public class BoundsSetterEditor : Editor {

        public override void OnInspectorGUI () {
            DrawDefaultInspector();
            GUILayout.Space(10f);
            var bs = target as BoundsSetter;
            if(GUILayout.Button("Instance Mesh")){
                bs.InstanceMesh();
            }
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Changes will only be saved if the mesh is instanced!", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
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