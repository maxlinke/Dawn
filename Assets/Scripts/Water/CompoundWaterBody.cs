using System.Collections.Generic;
using UnityEngine;

public class CompoundWaterBody : WaterBody {

    [Header("Components")]
    [SerializeField] Collider[] cols = default;

    [Header("Settings")]
    [SerializeField] bool runtimeStatic = false;
    [SerializeField] bool showBoundingVolume = false;

    Bounds worldBounds;

    protected override Collider MainCollider => cols[0];
    protected override bool CollidersNotNull { get {
        if(cols != null && cols.Length > 0){
            return false;
        }
        foreach(var col in cols){
            if(col == null){
                return false;
            }
        }
        return true;
    } } 

    // TODO subtractive volumes too!
    // that's a different type. main collider and then the subtractions!
    // if the rigidbody is fully inside a subtractive trigger
    // or in combination fully inside, don't add to the list of rigidbodies
    // don't forget about containspoint tho

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
            if((col.ClosestPoint(worldPoint) - worldPoint).sqrMagnitude < CONTAINS_THRESHOLD_DIST_SQR){
                return true;
            }
        }
        return false;
    }
    
    // TODO basically use containspoint but do it the other way round?
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
