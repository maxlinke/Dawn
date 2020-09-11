using System.Collections.Generic;
using UnityEngine;

public abstract class WaterBody : MonoBehaviour {

    public const float CONTAINS_THRESHOLD_DIST = 0.01f;
    public const float CONTAINS_THRESHOLD_DIST_SQR = CONTAINS_THRESHOLD_DIST * CONTAINS_THRESHOLD_DIST;

    public const float MAX_RIGIDBODY_SIZE_ASSUMPTION = 100f;

    private static List<WaterBody> waterBodies = new List<WaterBody>();

    [Header("Water Properties")]
    [SerializeField] WaterPhyicsSettings physics = default;
    [SerializeField] WaterFog fog = default;

    bool initialized = false;
    List<Rigidbody> rbs;

    public WaterFog Fog => fog;

    protected abstract bool CollidersNotNull { get; }
    protected abstract Collider MainCollider { get; }
    public abstract IEnumerator<Collider> GetEnumerator ();

    void Start () {
        if(!IsValid(out var errorMsg)){
            Debug.LogError(errorMsg);
            Debug.DrawRay(transform.position, Vector3.up * 100f, Color.red, float.PositiveInfinity);
            return;
        }
        if(!AllTriggersOnWaterLayer(this.transform)){
            Debug.LogWarning($"{nameof(WaterBody)} \"{gameObject.name}\" is not 100% on the layer \"{Layer.Water.name}\". Check the children?", this.gameObject);
        }
        rbs = new List<Rigidbody>();
        waterBodies.Add(this);
        AdditionalStartChecks();
        initialized = true;

        bool IsValid (out string message) {
            if(!ValidateMajorComponents(out message)){
                return false;
            }
            return ValidateTriggers(out message);

            bool ValidateMajorComponents (out string outputMsg) {
                bool physicsOk = (physics != null);
                bool fogOk = (fog != null);
                bool colsOk = CollidersNotNull;
                if(!physicsOk || !fogOk || !colsOk){
                    outputMsg = string.Empty;
                    if(!physicsOk){
                        outputMsg += $"No {nameof(WaterPhyicsSettings)} assigned on {nameof(WaterBody)} \"{gameObject.name}\"!\n";
                    }
                    if(!fogOk){
                        outputMsg += $"No {nameof(WaterFog)} assigned on {nameof(WaterBody)} \"{gameObject.name}\"!\n";
                    }
                    if(!colsOk){
                        outputMsg += $"One or more NULL {nameof(Collider)}(s) assigned on {nameof(WaterBody)} \"{gameObject.name}\"!\n";
                    }
                    return false;
                }
                outputMsg = string.Empty;
                return true;
            }

            bool ValidateTriggers (out string outputMsg) {
                outputMsg = string.Empty;
                foreach(var col in this){
                    var typeName = col.GetType().ToString();
                    if(!col.isTrigger){
                        outputMsg += $"{typeName} isn't a trigger\n";
                    }
                    if(col is MeshCollider mc){
                        if(!mc.convex){
                            outputMsg += $"{typeName} is not convex\n";
                        }
                    }
                }
                if(outputMsg.Length > 0){
                    outputMsg = $"One or more {nameof(Collider)}(s) on {nameof(WaterBody)} \"{gameObject.name}\" have problems:\n{outputMsg}";
                    return false;
                }
                return true;
            }
        }

        bool AllTriggersOnWaterLayer (Transform transformToCheck) {
            var triggerComponent = transformToCheck.GetComponent<Collider>();
            var transformIsTrigger = triggerComponent != null ? triggerComponent.isTrigger : false;
            var transformIsOnWaterLayer = transformToCheck.gameObject.layer == Layer.Water.index;
            if(transformIsTrigger && !transformIsOnWaterLayer){
                return false;
            }
            int cc = transformToCheck.childCount;
            var childrenOnWaterLayer = true;
            for(int i=0; i<cc; i++){
                childrenOnWaterLayer &= AllTriggersOnWaterLayer(transformToCheck.GetChild(i));
            }
            return childrenOnWaterLayer;
        }
    }

    protected virtual void AdditionalStartChecks () { }

    void OnDestroy () {
        if(waterBodies.Contains(this)){
            waterBodies.Remove(this);
        }
    }

    void FixedUpdate () {
        if(!initialized){
            return;
        }
        DoDrag();
        DoBuoyancy();
        rbs.Clear();

        void DoBuoyancy () {
            if(Physics.gravity.sqrMagnitude > 0){
                Vector3 gravityDir = Physics.gravity.normalized;
                if(physics.UseBetterBuoyancy){
                    foreach(var rb in rbs){
                        AddBetterBuoyancy(rb, gravityDir);
                    }
                }else{
                    foreach(var rb in rbs){
                        AddSimpleBuoyancy(rb, gravityDir);
                    }
                }
            }
        }

        void DoDrag () {
            Vector3 ownVelocity = Vector3.zero;
            var ownRB = MainCollider.attachedRigidbody;
            if(ownRB != null){
                ownVelocity = ownRB.velocity;
            }
            foreach(var rb in rbs){
                AddDrag(rb, ownVelocity);
            }
        }
    }

    void OnTriggerStay (Collider otherCollider) {
        if(!initialized){
            return;
        }
        var otherRB = otherCollider.attachedRigidbody;
        if(otherRB != null){
            if(!rbs.Contains(otherRB)){
                rbs.Add(otherRB);
            }
        }
    }

    void AddDrag (Rigidbody rb, Vector3 ownVelocity) {
        var deltaV = ownVelocity - rb.velocity;
        var deltaVDragAccel = deltaV / Time.fixedDeltaTime;
        var drag = Mathf.Lerp(physics.MinWaterDrag, physics.MaxWaterDrag, rb.drag / physics.WaterDragRBDragNormalizer);
        drag *= (rb.velocity.magnitude / physics.WaterDragRBVelocityNormalizer);
        if(deltaVDragAccel.sqrMagnitude > (drag * drag)){
            rb.velocity += deltaV.normalized * drag * Time.fixedDeltaTime;
        }else{
            rb.velocity += deltaVDragAccel * Time.fixedDeltaTime;
        }
    }

    // for subtractive?
    // protected abstract bool IsNotActuallyInWater (Vector3 point);

    // use trigger-mesh collider for surfaces?

    void AddBetterBuoyancy (Rigidbody rb, Vector3 gravityDir) {
        var rbPos = rb.position;
        bool applyBuoyancy = false;
        float depth = 0f;
        // i need dedicated surface colliders to figure out depth easily
        // not even colliders... point, normal and 2d-size are enough...
        // just have to figure out the math

        var boundsTestOffset = gravityDir * MAX_RIGIDBODY_SIZE_ASSUMPTION;
        var rbTop = rb.ClosestPointOnBounds(rbPos - boundsTestOffset);
        var rbBottom = rb.ClosestPointOnBounds(rbPos + boundsTestOffset);
        // find depth of top, bottom and position
        // use that for the lerping
        // TODO additional field(s) in physics to determine how high an object can float depending on its drag
        // TODO import beach ball and texture (no center of mass fudgery!)
        // TODO also test a log (should just about float, and flat!)
        // TODO plank (should float well and flat)
        // TODO other things
        foreach(var col in this){
            if(DepthCast(col, rbPos, gravityDir, out depth)){
                applyBuoyancy = true;
                break;  // break here is wrong, this is just legacy code
            }
        }
        if(!applyBuoyancy){
            return;
        }
        // TODO instead of the set neutralization depth value, use the size of the rigidbody (closestpointonbounds?)
        // to determine the start of the lerp towards 1 and if above the surface, towards 0!
        // this could also allow the beach ball to float higher than 0
        // if i make the 1-depth dependent on the drag...

        // if top and bottom are in the water, add at center of mass
        // otherwise, lerp to position ? or is this too much complexity?
        var buoyancy = RawRigidbodyBuoyancy(rb);
        if(buoyancy > 1){
            // buoyancy = Mathf.Lerp(1f, buoyancy, depth / physics.BuoyancyNeutralizationDepth);
        }

        // zero lerp test point -> closest to center of water body.. i guess active trigger will have to do?
        // lerp to one only if greater
        // lerp to zero always
        AddBuoyancyForce(rb, buoyancy);
    }

    // if it's buoyant enough to be buoyant, then move the position to the surface. very simple.
    void AddSimpleBuoyancy (Rigidbody rb, Vector3 gravityDir) {
        var rbPos = rb.position;
        float maxInDepth = 0f;
        float maxOutDepth = 0f;
        foreach(var col in this){
            var inside = ((col.ClosestPoint(rbPos) - rbPos).sqrMagnitude < CONTAINS_THRESHOLD_DIST_SQR);
            if(DepthCast(col, rbPos, gravityDir, out var depth)){
                if(inside){
                    maxInDepth = Mathf.Max(maxInDepth, depth);
                }else{
                    maxOutDepth = Mathf.Max(maxOutDepth, depth);
                }
            }
        }
        float buoyancy = RawRigidbodyBuoyancy(rb);
        // meh...
        // buoyancy *= (1f - Mathf.Clamp01(maxOutDepth));
        buoyancy *= Mathf.Clamp01(maxInDepth);

        // VVV jittery and dependent on neutralization range... VVV
        // if((maxInDepth > 0)){
        //     if(buoyancy > 1){
        //         buoyancy = Mathf.Lerp(1, buoyancy, maxInDepth / physics.SimpleBuoyancyNeutralizationRange);
        //     }
        // }else if(maxOutDepth > 0){
        //     buoyancy *= (1f - Mathf.Clamp01(maxOutDepth / physics.SimpleBuoyancyNeutralizationRange));
        // }else{
        //     buoyancy = 0f;
        // }
        
        AddBuoyancyForce(rb, buoyancy);
    }

    // maybe modify this?
    // drag ~ area
    // mass ~ volume
    // so heavy thing with same drag as light thing is less dense
    // ...
    // i really need some props to test this with. things that will float, things that wont
    // things that have drag, things that dont
    float RawRigidbodyBuoyancy (Rigidbody rb) {
        var lerp = (rb.drag - physics.MinBuoyancyRBDrag) / (physics.StandardBuoyancyRBDrag - physics.MinBuoyancyRBDrag);
        return Mathf.LerpUnclamped(physics.MinBuoyancy, physics.StandardBuoyancy, lerp);
    }

    void AddBuoyancyForce (Rigidbody rb, float buoyancy) {
        rb.AddForceAtPosition(
            force: -Physics.gravity * buoyancy,
            position: rb.transform.position,
            mode: ForceMode.Force
        );
    }

    bool DepthCast (Collider col, Vector3 rbPos, Vector3 gravityDir, out float depth) {
        var colSize = col.bounds.size.magnitude;
        var rl = 4f * colSize;
        var ro = rbPos - (gravityDir * 2f * colSize);
        var rd = gravityDir;
        if(col.Raycast(new Ray(ro, rd), out var hit, rl)){
            depth = (hit.point - rbPos).magnitude;
            return true;
        }
        depth = float.NaN;
        return false;
    }

    // TODO variants for ienumerables with allpoints and anypoint?

    public abstract bool ContainsPoint (Vector3 worldPoint);
    public abstract bool ContainsAnyPoint (IEnumerable<Vector3> worldPoints);
    public abstract bool ContainsAllPoints (IEnumerable<Vector3> worldPoints);

    public static bool IsInAnyWaterBody (Vector3 worldPoint, out WaterBody outputWB) {
        foreach(var wb in waterBodies){
            if(wb.ContainsPoint(worldPoint)){
                outputWB = wb;
                return true;
            }
        }
        outputWB = null;
        return false;
    }
	
}
