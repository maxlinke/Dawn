using System.Collections.Generic;
using UnityEngine;

public abstract class WaterBody : MonoBehaviour {

    public const float CONTAINS_THRESHOLD_DIST = 0.01f;
    public const float CONTAINS_THRESHOLD_DIST_SQR = CONTAINS_THRESHOLD_DIST * CONTAINS_THRESHOLD_DIST;

    public const float MAX_RIGIDBODY_SIZE_ASSUMPTION = 100f;

    [Header("Water Properties")]
    [SerializeField] WaterPhyicsSettings physics = default;
    [SerializeField] WaterFog fog = default;

    static List<WaterBody> waterBodies = new List<WaterBody>();

    bool initialized = false;
    List<Rigidbody> rbs;

    public WaterFog Fog => fog;

    protected abstract bool CollidersPresent { get; }
    protected abstract Collider MainCollider { get; }
    public abstract IEnumerator<Collider> GetEnumerator ();

    protected virtual void Start () {
        if(!AllImportantThingsOK()){
            return;
        }
        if(!AllTriggersOnWaterLayer(this.transform)){
            Debug.LogWarning($"{nameof(WaterBody)} \"{gameObject.name}\" is not 100% on the layer \"{Layer.Water.name}\". Check the children?", this.gameObject);
            Debug.DrawRay(transform.position, Vector3.up * 100f, Color.red, float.PositiveInfinity);
        }
        if(!ValidateTriggers(out var triggerMsg)){
            Debug.LogWarning($"{nameof(WaterBody)} \"{gameObject.name}\" has some invalid triggers!\n{triggerMsg}", this.gameObject);
            Debug.DrawRay(transform.position, Vector3.up * 100f, Color.red, float.PositiveInfinity);
        }
        rbs = new List<Rigidbody>();
        waterBodies.Add(this);
        AdditionalStartChecks();
        initialized = true;

        bool AllImportantThingsOK () {
            bool physicsOk = (physics != null);
            bool fogOk = (fog != null);
            bool colsOk = CollidersPresent;
            var output = true;
            if(!physicsOk || !fogOk || !colsOk){
                if(!physicsOk){
                    Debug.LogError($"No {nameof(WaterPhyicsSettings)} assigned on {nameof(WaterBody)} \"{gameObject.name}\"!");
                }
                if(!fogOk){
                    Debug.LogError($"No {nameof(WaterFog)} assigned on {nameof(WaterBody)} \"{gameObject.name}\"!");
                }
                if(!colsOk){
                    Debug.LogError($"No collider(s) assigned on {nameof(WaterBody)} \"{gameObject.name}\"!");
                }
                output = false;
            }
            if(!output){
                Debug.DrawRay(transform.position, Vector3.up * 100f, Color.red, float.PositiveInfinity);
            }
            return output;
        }

        bool AllTriggersOnWaterLayer (Transform transformToCheck) {
            if(transformToCheck.gameObject.layer != Layer.Water.index){
                return false;
            }
            int cc = transformToCheck.childCount;
            var childrenOnWaterLayer = true;
            for(int i=0; i<cc; i++){
                childrenOnWaterLayer &= AllTriggersOnWaterLayer(transformToCheck.GetChild(i));
            }
            return childrenOnWaterLayer;
        }

        bool ValidateTriggers (out string message) {
            message = string.Empty;
            foreach(var col in this){
                bool colOK = true;
                string temp = $"{col.GetType()} \"{col.name}\" ";
                if(col == null){
                    temp += "is null!";
                    colOK = false;
                }else{
                    if(!col.isTrigger){
                        temp += "| isn't a trigger! ";
                        colOK = false;
                    }
                    if(!CanDoContainsCheck(col)){
                        temp += "| can't do contains check (non-convex mesh collider?) ";
                        colOK = false;
                    }
                }
                if(!colOK){
                    message += $"{temp}\n";
                }
            }
            return message.Length == 0;
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
        Vector3 ownVelocity = Vector3.zero;
        var ownRB = MainCollider.attachedRigidbody;
        if(ownRB != null){
            ownVelocity = ownRB.velocity;
        }
        bool gravityPresent = Physics.gravity.sqrMagnitude > 0;
        Vector3 gravityDir = Physics.gravity.normalized;
        foreach(var rb in rbs){
            if(gravityPresent){     // use this? if(rb.useGravity && gravityPresent){
                AddBuoyancy(rb, gravityDir);
            }
            AddDrag(rb, ownVelocity);
        }
        rbs.Clear();
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

    void AddBuoyancy (Rigidbody rb, Vector3 gravityDir) {
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
            var colSize = col.bounds.size.magnitude;
            var rl = 1.2f * colSize;
            var ro = rbPos - (gravityDir * 1.1f * colSize);
            var rd = gravityDir;
            if(col.Raycast(new Ray(ro, rd), out var hit, rl)){
                depth = (hit.point - rbPos).magnitude;
                applyBuoyancy = true;
                break;                  // this isn't right. best-case: use a surface to determine depth
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
        var lerp = (rb.drag - physics.MinBuoyancyRBDrag) / (physics.StandardBuoyancyRBDrag - physics.MinBuoyancyRBDrag);
        var buoyancy = Mathf.LerpUnclamped(physics.MinBuoyancy, physics.StandardBuoyancy, lerp);
        if(buoyancy > 1){
            buoyancy = Mathf.Lerp(1f, buoyancy, depth / physics.BuoyancyNeutralizationDepth);
        }

        // TODO clear labels on waterbodyphyics
        // maybe even get the hints from consts here?
        // that e.g. at drag = 1 the object will float exactly at its position (if that's how i do it)

        // zero lerp test point -> closest to center of water body.. i guess active trigger will have to do?
        // lerp to one only if greater
        // lerp to zero always
        rb.AddForceAtPosition(
            force: -Physics.gravity * buoyancy,
            position: rb.transform.position,
            mode: ForceMode.Force
        );
    }

    protected bool CanDoContainsCheck (Collider col) {
        if(col is MeshCollider mc){
            return mc.convex;
        }
        return true;
    }

    public abstract bool ContainsPoint (Vector3 worldPoint);

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
