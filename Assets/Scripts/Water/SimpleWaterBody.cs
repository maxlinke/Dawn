using System.Collections.Generic;
using UnityEngine;

public class SimpleWaterBody : WaterBody {

    [Header("Components")]
    [SerializeField] Collider col = default;

    protected override bool CollidersNotNull => col != null;
    protected override Collider MainCollider => col;

    public override IEnumerator<Collider> GetEnumerator () {
        yield return col;
    }

    public override bool ContainsPoint (Vector3 worldPoint) {
        return ((col.ClosestPoint(worldPoint) - worldPoint).sqrMagnitude < CONTAINS_THRESHOLD_DIST_SQR);
    }

    public override bool ContainsAnyPoint (IEnumerable<Vector3> worldPoints) {
        foreach(var point in worldPoints){
            if(this.ContainsPoint(point)){
                return true;
            }
        }
        return false;
    }

    public override bool ContainsAllPoints (IEnumerable<Vector3> worldPoints) {
        foreach(var point in worldPoints){
            if(!this.ContainsPoint(point)){
                return false;
            }
        }
        return true;
    }
	
}
