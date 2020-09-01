using System.Collections.Generic;
using UnityEngine;

public class WaterBody : MonoBehaviour {

    [SerializeField] WaterPhyicsSettings physics = default;
    [SerializeField] WaterFog fog = default;
    [SerializeField] Collider[] cols = default;
    // TODO optional bobbing?

    bool initialized = false;
    List<Rigidbody> rbs;

    public WaterFog Fog => fog;

    void Start () {
        bool physicsOk = (physics != null);
        bool fogOk = (fog != null);
        bool colsOk = (cols != null && cols.Length > 0);
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
            Debug.DrawRay(transform.position, Vector3.up * 100f, Color.red, float.PositiveInfinity);
            return;
        }
        bool layersOk = AllTriggersOnWaterLayer(this.transform);
        if(!layersOk){
            Debug.LogWarning($"{nameof(WaterBody)} \"{gameObject.name}\" is not 100% on the layer \"{Layer.Water.name}\". Check the children?", this.gameObject);
            Debug.DrawRay(transform.position, Vector3.up * 100f, Color.red, float.PositiveInfinity);
        }
        rbs = new List<Rigidbody>();
        initialized = true;

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
	
}
