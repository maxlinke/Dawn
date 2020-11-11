using System.Collections.Generic;
using UnityEngine;

public abstract class WaterBody : MonoBehaviour {

    public const float MAX_RIGIDBODY_SIZE_ASSUMPTION = 100f;

    private static List<WaterBody> waterBodies = new List<WaterBody>();

    [Header("Water Properties")]
    [SerializeField] WaterPhysics waterPhysics = default;
    [SerializeField] WaterFog fog = default;

    bool initialized = false;
    List<Rigidbody> rbs;
    Vector3 torqueNoise;

    public WaterFog Fog => fog;
    public WaterPhysics WaterPhysics => waterPhysics;

    protected abstract bool CollidersNotNull { get; }
    protected abstract Collider MainCollider { get; }
    public abstract IEnumerator<Collider> GetEnumerator ();

    public static bool ColliderContainsPoint (Collider col, Vector3 worldPoint) {
        return (col.ClosestPoint(worldPoint) - worldPoint).sqrMagnitude <= Mathf.Epsilon;
    }

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
    }

    protected virtual void AdditionalStartChecks () { }

    void OnDestroy () {
        waterBodies.Remove(this);
    }

    void FixedUpdate () {
        if(!initialized || !(rbs.Count > 0)){
            return;
        }
        torqueNoise = Random.insideUnitSphere;
        rbs.RemoveNullEntries();
        DoDrag();
        DoBuoyancy();

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

        void DoBuoyancy () {
            if(Physics.gravity.sqrMagnitude > 0){
                float densityNormalizer = waterPhysics.GetApproxDensityNormalizer();
                Vector3 gravityDir = Physics.gravity.normalized;
                foreach(var rb in rbs){
                    AddSimpleBuoyancy(rb, gravityDir, densityNormalizer);
                }
            }
        }
    }

    void OnTriggerEnter (Collider otherCollider) {
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

    void OnTriggerExit (Collider otherCollider) {
        if(!initialized){
            return;
        }
        var otherRB = otherCollider.attachedRigidbody;
        if(otherRB != null){
            rbs.Remove(otherRB);
        }
    }

    public void AddDrag (Rigidbody rb, Vector3 ownVelocity) {
        var localRBVelocity = rb.velocity - ownVelocity;
        localRBVelocity -= localRBVelocity * (rb.drag + WaterPhysics.Viscosity) * Time.deltaTime;
        rb.velocity = ownVelocity + localRBVelocity;
    }

    public Vector3 GetBuoyancyAcceleration (Rigidbody rb, float buoyancy) {
        return -Physics.gravity * buoyancy;
    }

    public void AddBuoyancyAndBuoyancyTorque (Rigidbody rb, float buoyancy, Vector3 gravityDir) {
        rb.AddForce(
            force: GetBuoyancyAcceleration(rb, buoyancy),
            mode: ForceMode.Acceleration
        );
        var torque = 10f * GetBuoyancyTorque(rb, gravityDir);
        rb.AddTorque(torque, ForceMode.Acceleration);
    }

    public Vector3 GetBuoyancyTorque (Rigidbody rb, Vector3 gravityDir) {
        var rbUp = rb.transform.up;
        switch(rb.tag){             // TODO this isn't multi tag safe, but i don't think it needs to be?
            case Tag.FloatYUp:
                return Vector3.Cross(gravityDir, rb.transform.up);
            case Tag.FloatYVertical:
                rbUp *= Mathf.Sign(Vector3.Dot(-gravityDir, rbUp));
                rbUp += torqueNoise * Mathf.Clamp01(0.01f - rbUp.sqrMagnitude);
                return Vector3.Cross(gravityDir, rbUp);
            case Tag.FloatYHorizontal:
                var flatUp = rbUp.ProjectOnPlane(gravityDir).normalized;
                flatUp += torqueNoise * Mathf.Clamp01(0.01f - flatUp.sqrMagnitude);
                return Vector3.Cross(rb.transform.up, flatUp);
            default:
                return Vector3.zero;
        }
    }

    void AddSimpleBuoyancy (Rigidbody rb, Vector3 gravityDir, float densityNormalizer) {
        float rbDensity = densityNormalizer * waterPhysics.FastApproxDensity(rb);
        float buoyancy = waterPhysics.BuoyancyFromDensity(rbDensity);
        if(buoyancy == 0f){
            return;
        }
        var rbCOM = rb.worldCenterOfMass;
        float maxInDepth = float.NegativeInfinity;
        float maxOutDepth = float.NegativeInfinity;
        foreach(var col in this){
            var inside = ColliderContainsPoint(col, rbCOM);
            if(DepthCast(col, rbCOM, gravityDir, out var depth)){
                if(inside){
                    maxInDepth = Mathf.Max(maxInDepth, depth);
                }else{
                    maxOutDepth = Mathf.Max(maxOutDepth, depth);
                }
            }
        }
        if((maxInDepth == maxOutDepth) && (float.IsNegativeInfinity(maxInDepth))){
            return;
        }
        if(maxInDepth > 0){
            if(buoyancy > 1){
                buoyancy = Mathf.Lerp(1, buoyancy, maxInDepth / waterPhysics.LowerBuoyancyNeutralizationRange);
            }
        }else{
            buoyancy = Mathf.Min(buoyancy, 1f);
            if(maxOutDepth > 0){
                buoyancy *= (1f - Mathf.Clamp01(maxOutDepth / waterPhysics.UpperBuoyancyNeutralizationRange));
            }
        }
        AddBuoyancyAndBuoyancyTorque(rb, buoyancy, gravityDir);
    }

    bool DepthCast (Collider col, Vector3 targetPos, Vector3 gravityDir, out float depth) {
        var colSize = col.bounds.size.magnitude;
        var rl = 4f * colSize;
        var ro = targetPos - (gravityDir * 2f * colSize);
        var rd = gravityDir;
        if(col.Raycast(new Ray(ro, rd), out var hit, rl)){
            depth = (hit.point - targetPos).magnitude;
            // depth *= Mathf.Sign(Vector3.Dot(gravityDir, hit.normal));
            // depth = Mathf.Max(0, depth);
            return true;
        }
        depth = float.NaN;
        return false;
    }

    bool IsValid (out string message) {
        if(!ValidateMajorComponents(out message)){
            return false;
        }
        return ValidateTriggers(out message);

        bool ValidateMajorComponents (out string outputMsg) {
            bool physicsOk = (waterPhysics != null);
            bool fogOk = (fog != null);
            bool colsOk = CollidersNotNull;
            if(!physicsOk || !fogOk || !colsOk){
                outputMsg = string.Empty;
                if(!physicsOk){
                    outputMsg += $"No {(nameof(WaterPhysics))} assigned on {nameof(WaterBody)} \"{gameObject.name}\"!\n";
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
        var transformIsOnWaterLayer = transformToCheck.gameObject.layer == Layer.Water;
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
