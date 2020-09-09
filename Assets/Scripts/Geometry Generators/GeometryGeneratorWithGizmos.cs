using UnityEngine;

namespace GeometryGenerators {

    public abstract class GeometryGeneratorWithGizmos : GeometryGenerator {
        
        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private Color gizmoColor = Color.cyan;

        void OnDrawGizmosSelected () {
            if(!drawGizmos){
                return;
            }
            #if UNITY_EDITOR
            if(UnityEditor.Selection.activeGameObject != this.gameObject){
                return;
            }
            #endif
            var colCache = Gizmos.color;
            Gizmos.color = gizmoColor;
            DrawGizmos();
            Gizmos.color = colCache;
        }

        protected abstract void DrawGizmos ();

    }

}