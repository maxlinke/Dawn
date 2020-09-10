using System.Collections.Generic;
using UnityEngine;

public class SimpleWaterBody : WaterBody {

    [Header("Components")]
    [SerializeField] Collider col = default;

    protected override bool CollidersPresent => col != null;
    protected override Collider MainCollider => col;

    public override IEnumerator<Collider> GetEnumerator () {
        yield return col;
    }

    public override bool ContainsPoint (Vector3 worldPoint) {
        if(!CanDoContainsCheck(col)){
            return false;
        }
        return ((col.ClosestPoint(worldPoint) - worldPoint).sqrMagnitude < CONTAINS_THRESHOLD_DIST_SQR);
    }
	
}
