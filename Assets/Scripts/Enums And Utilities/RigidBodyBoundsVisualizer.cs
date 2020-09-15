using UnityEngine;

public class RigidbodyBoundsVisualizer : MonoBehaviour {

    [Header("Important")]
    [SerializeField] Vector3 size = Vector3.one;
    [SerializeField, Range(2, 31)] int count = 5;

    [Header("Gizmos")]
    [SerializeField] Color gizmoColor = Color.green;
    [SerializeField] float gizmoSize = 0.1f;

    void OnDrawGizmosSelected () {
        var rb = GetComponent<Rigidbody>();
        if(rb == null){
            return;
        }
        var gc = Gizmos.color;
        Gizmos.color = gizmoColor;
        DrawAllTheGizmos(rb);
        Gizmos.color = gc;
    }

    void DrawAllTheGizmos (Rigidbody rb) {
        var div = count - 1;
        var gs = Vector3.one * gizmoSize;
        for(int i=0; i<count; i++){
            var x = size.x * (((float)i / div) - 0.5f);
            for(int j=0; j<count; j++){
                var y = size.y * (((float)j / div) - 0.5f);
                for(int k=0; k<count; k++){
                    var z = size.z * (((float)k / div) - 0.5f);
                    var p = rb.transform.TransformPoint(new Vector3(x, y, z));
                    var b = rb.ClosestPointOnBounds(p);
                    Gizmos.DrawLine(p, b);
                    Gizmos.DrawCube(b, gs);
                }
            }
        }
    }
	
}
