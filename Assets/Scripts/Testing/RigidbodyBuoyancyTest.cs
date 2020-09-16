using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyBuoyancyTest : MonoBehaviour {

    [SerializeField] float velocity = 0f;
    [SerializeField] float peakVelocity = 0f;

    Rigidbody rb;

    void Start () {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate () {
        if(rb != null){
            velocity = rb.velocity.magnitude;
            peakVelocity = Mathf.Max(peakVelocity, velocity);
        }
    }
	
}
