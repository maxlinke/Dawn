using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderUpdateTest : MonoBehaviour {

    // what am i even testing here?
    // if/how it matters when i update the collider
    // test depending on framerate
    // because the fixed updates should still get executed

    [SerializeField] KeyCode testKey = default;
    [SerializeField] CapsuleCollider col = default;
    [SerializeField] Rigidbody rb = default;
    [SerializeField] Vector3 goVelocity = default;
    [SerializeField] Vector3 normalColliderCenter = default;
    [SerializeField] Vector3 goColliderCenter = default;

    bool cachedTestKeyDown = false;
    bool go = false;

    void Start () {
        SetColliderCenter();
    }

    void Update () {
        cachedTestKeyDown = Input.GetKeyDown(testKey);
    }

    void FixedUpdate () {
        if(cachedTestKeyDown){
            go = true;
            rb.velocity = goVelocity;
        }
        cachedTestKeyDown = false;
    }

    void SetColliderCenter () {
        col.center = go ? goColliderCenter : normalColliderCenter;
    }
	
}
