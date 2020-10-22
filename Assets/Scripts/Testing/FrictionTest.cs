using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrictionTest : MonoBehaviour {

    [Header("Push")]
    [SerializeField] Vector3 pushVelocity = default;
    [SerializeField] KeyCode pushKey = KeyCode.Q;
    [SerializeField] bool pushOnStart = false;

    [Header("Ground")]
    [SerializeField] Transform groundParent = default;
    [SerializeField] float groundStaticFrictionMin = 0.6f;
    [SerializeField] float groundStaticFrictionMax = 0.6f;
    [SerializeField] float groundDynamicFrictionMin = 0.6f;
    [SerializeField] float groundDynamicFrictionMax = 0.6f;

    [Header("Object")]
    [SerializeField] float objectStaticFrictionMin = 0.6f;
    [SerializeField] float objectStaticFrictionMax = 0.6f;
    [SerializeField] float objectDynamicFrictionMin = 0.6f;
    [SerializeField] float objectDynamicFrictionMax = 0.6f;
    [SerializeField] float objectMassMin = 1f;
    [SerializeField] float objectMassMax = 1f;

    // test varying ground friction (static/dynamic)
    // test varying object friction (static/dynamic)
    // test varying object mass

    Rigidbody[] rbs;

    void Start () {
        var cc = groundParent.childCount;
        rbs = new Rigidbody[cc];
        for(int i=0; i<cc; i++){
            var lerp = ((float)i) / (cc - 1);
            var p = groundParent.GetChild(i);
            var newGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newGO.transform.position = groundParent.transform.position + Vector3.up + (Vector3.right * p.localPosition.x);
            var newRB = newGO.AddComponent<Rigidbody>();
            newRB.mass = Mathf.Lerp(objectMassMin, objectMassMax, lerp);
            rbs[i] = newRB;
            var rbPM = new PhysicMaterial();
            rbPM.staticFriction = Mathf.Lerp(objectStaticFrictionMin, objectStaticFrictionMax, lerp);
            rbPM.dynamicFriction = Mathf.Lerp(objectDynamicFrictionMin, objectDynamicFrictionMax, lerp);
            newGO.GetComponent<Collider>().sharedMaterial = rbPM;
            var groundPM = new PhysicMaterial();
            groundPM.staticFriction = Mathf.Lerp(groundStaticFrictionMin, groundStaticFrictionMax, lerp);
            groundPM.dynamicFriction = Mathf.Lerp(groundDynamicFrictionMin, groundDynamicFrictionMax, lerp);
            p.GetComponent<Collider>().sharedMaterial = groundPM;
        }
        if(pushOnStart){
            Push();
        }
    }

    void Update () {
        if(Input.GetKeyDown(pushKey)){
            Push();
        }
    }

    void Push () {
        foreach(var rb in rbs){
            rb.velocity += pushVelocity;
        }
    }
	
}
