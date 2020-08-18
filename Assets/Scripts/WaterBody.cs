using System.Collections.Generic;
using UnityEngine;

public class WaterBody : MonoBehaviour {

    public const float WaterDrag = 8f;

    bool initialized = false;
    List<Rigidbody> rbs;

    void Start () {
        bool allOK = AllTriggersOnWaterLayer(this.transform);
        if(!allOK){
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
        foreach(var rb in rbs){
            AddDrag(rb);
            AddBuoyancy(rb);
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

    // TODO use rb.drag
    void AddDrag (Rigidbody rb) {
        // var deltaV = -rb.velocity;
        // var dragAccel = deltaV / Time.fixedDeltaTime;
        // if(dragAccel.sqrMagnitude > (WaterDrag * WaterDrag)){
        //     dragAccel = deltaV.normalized * WaterDrag;
        // }
        // rb.velocity += dragAccel * Time.fixedDeltaTime;
    }

    // TODO use rb.drag
    void AddBuoyancy (Rigidbody rb) {

    }
	
}
