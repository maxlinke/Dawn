using System.Collections.Generic;
using UnityEngine;

public class WaterBody : MonoBehaviour {

    public const float DEFAULT_MIN_CONTAINS_DIST = 0.01f;

    [Header("Components")]
    [SerializeField] WaterPhyicsSettings physics = default;
    [SerializeField] WaterFog fog = default;
    [SerializeField] Collider[] cols = default;

    [Header("Settings")]
    [SerializeField] bool runtimeStatic = false;
    [SerializeField] bool showBoundingVolume = false;

    static List<WaterBody> waterBodies = new List<WaterBody>();

    bool initialized = false;
    List<Rigidbody> rbs;
    Bounds worldBounds;

    public WaterFog Fog => fog;

    void Start () {
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
        if(runtimeStatic){
            worldBounds = CalculateWorldBounds();
        }else{
            Debug.LogAssertion($"{nameof(WaterBody)} \"{this.gameObject.name}\" is not marked \"{nameof(runtimeStatic)}\". You should have a good reason for that. It's much more efficient!");
        }
        initialized = true;

        bool AllImportantThingsOK () {
            bool physicsOk = (physics != null);
            bool fogOk = (fog != null);
            bool colsOk = (cols != null && cols.Length > 0);
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
            foreach(var col in cols){
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
        var ownRB = cols[0].attachedRigidbody;
        if(ownRB != null){
            ownVelocity = ownRB.velocity;
        }
        foreach(var rb in rbs){
            AddBuoyancy(rb);
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

    void AddBuoyancy (Rigidbody rb) {
        if(Physics.gravity.sqrMagnitude <= 0f){
            return;
        }
        var gravityDir = Physics.gravity.normalized;
        var worldCOM = rb.worldCenterOfMass;
        bool applyBuoyancy = false;
        float depth = 0f;
        foreach(var col in cols){
            var rl = 2f * col.bounds.extents.magnitude;
            var ro = worldCOM - (gravityDir * rl);
            var rd = gravityDir;
            if(col.Raycast(new Ray(ro, rd), out var hit, rl)){
                depth = (hit.point - worldCOM).magnitude;
                applyBuoyancy = true;
                break;
            }
        }
        if(!applyBuoyancy){
            return;
        }
        var lerp = (rb.drag - physics.MinBuoyancyRBDrag) / (physics.StandardBuoyancyRBDrag - physics.MinBuoyancyRBDrag);
        var buoyancy = Mathf.LerpUnclamped(physics.MinBuoyancy, physics.StandardBuoyancy, lerp);
        if(buoyancy > 1){
            buoyancy = Mathf.Lerp(1f, buoyancy, depth / physics.BuoyancyNeutralizationDepth);
        }        
        Debug.DrawRay(rb.position, Vector3.up * buoyancy, Color.green, Time.fixedDeltaTime);
        rb.velocity -= Physics.gravity * buoyancy * Time.fixedDeltaTime;
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

    bool CanDoContainsCheck (Collider col) {
        if(col is MeshCollider mc){
            return mc.convex;
        }
        return true;
    }

    public bool ContainsPoint (Vector3 worldPoint, float minDist = DEFAULT_MIN_CONTAINS_DIST) {
        if(cols.Length > 1){
            var bounds = (runtimeStatic ? worldBounds : CalculateWorldBounds());
            if(!bounds.Contains(worldPoint)){
                return false;
            }
        }
        float minDistSqr = minDist * minDist;
        foreach(var col in cols){
            if(!CanDoContainsCheck(col)){
                continue;
            }
            if((col.ClosestPoint(worldPoint) - worldPoint).sqrMagnitude < minDistSqr){
                return true;
            }
        }
        return false;
    }

    public static bool IsInAnyWaterBody (Vector3 worldPoint, out WaterBody outputWB, float minDist = DEFAULT_MIN_CONTAINS_DIST) {
        foreach(var wb in waterBodies){
            if(wb.ContainsPoint(worldPoint, minDist)){
                outputWB = wb;
                return true;
            }
        }
        outputWB = null;
        return false;
    }
	
}
