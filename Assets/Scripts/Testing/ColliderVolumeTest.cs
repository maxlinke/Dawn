using UnityEngine;

public class ColliderVolumeTest : MonoBehaviour {

    [SerializeField] Collider targetCollider = default;
    [SerializeField] Vector3 rayDirection = default;
    [SerializeField] float rayLength = default;
    [SerializeField] Color gizmoColor = default;
    [SerializeField] float debugOutput = default;

    void OnDrawGizmos () {
        if(targetCollider == null){
            return;
        }
        var cc = Gizmos.color;
        Gizmos.color = gizmoColor;
        var origin = transform.position;
        bool testClosestPoint = true;
        if(targetCollider is MeshCollider mc){
            testClosestPoint = mc.convex;
        }
        if(testClosestPoint){
            var closestPoint = targetCollider.ClosestPoint(origin);
            debugOutput = (closestPoint - origin).magnitude;
            Gizmos.DrawLine(origin, closestPoint);
            Gizmos.DrawSphere(closestPoint, 0.05f);
        }else{
            debugOutput = float.NaN;
        }
        var ray = new Ray(origin, rayDirection);
        if(targetCollider.Raycast(ray, out var hit, rayLength)){
            Gizmos.DrawLine(origin, hit.point);
            Gizmos.DrawSphere(hit.point, 0.1f);
        }else{
            var inv = Color.white - gizmoColor;
            Gizmos.color = new Color(inv.r, inv.g, inv.b, gizmoColor.a);
            Gizmos.DrawRay(origin, rayDirection.normalized * rayLength);
        }
        Gizmos.color = cc;
    }
	
}
