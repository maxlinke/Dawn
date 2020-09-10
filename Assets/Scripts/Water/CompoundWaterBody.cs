using System.Collections.Generic;
using UnityEngine;

public class CompoundWaterBody : WaterBody {

    [Header("Components")]
    [SerializeField] Collider[] cols = default;

    [Header("Settings")]
    [SerializeField] bool runtimeStatic = false;
    [SerializeField] bool showBoundingVolume = false;

    Bounds worldBounds;

    protected override bool CollidersPresent => (cols != null && cols.Length > 0);
    protected override Collider MainCollider => cols[0];

    public override IEnumerator<Collider> GetEnumerator () {
        foreach(var col in cols){
            yield return col;
        }
    }

    protected override void AdditionalStartChecks () {
        base.AdditionalStartChecks();
        if(runtimeStatic){
            worldBounds = CalculateWorldBounds();
        }else{
            Debug.LogAssertion($"{nameof(CompoundWaterBody)} \"{this.gameObject.name}\" is not marked \"{nameof(runtimeStatic)}\". You should have a good reason for that. It's much more efficient!");
        }
    }

    Bounds CalculateWorldBounds () {
        var output = new Bounds(this.transform.position, Vector3.zero);
        if(cols == null || cols.Length == 0){
            return output;
        }
        var encapsulate = false;
        for(int i=0; i<cols.Length; i++){
            if(cols[i] == null){
                continue;
            }
            if(!encapsulate){
                output = cols[i].bounds;
                encapsulate = true;
            }else{
                output.Encapsulate(cols[i].bounds);
            }
        }
        return output;
    }

    public override bool ContainsPoint (Vector3 worldPoint) {
        if(cols.Length > 1){
            var bounds = (runtimeStatic ? worldBounds : CalculateWorldBounds());
            if(!bounds.Contains(worldPoint)){
                return false;
            }
        }
        foreach(var col in cols){
            if(!CanDoContainsCheck(col)){
                continue;
            }
            if((col.ClosestPoint(worldPoint) - worldPoint).sqrMagnitude < CONTAINS_THRESHOLD_DIST_SQR){
                return true;
            }
        }
        return false;
    }

    void OnDrawGizmosSelected () {
        if(!showBoundingVolume){
            return;
        }
        var gc = Gizmos.color;
        Gizmos.color = Color.cyan;
        var wb = CalculateWorldBounds();
        Gizmos.DrawWireCube(wb.center, wb.extents * 2f);
        Gizmos.color = gc;
    }
	
}
