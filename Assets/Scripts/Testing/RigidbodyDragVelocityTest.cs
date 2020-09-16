using UnityEngine;

public class RigidbodyDragVelocityTest : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] Vector3 initialVelocity = Vector3.zero;
    [SerializeField] float testLength = 10f;

    [Header("Output")]
    [SerializeField] float velocity = 0f;
    [SerializeField] float peakVelocity = 0f;
    [SerializeField] float lastDelta = 0f;

    Rigidbody rb;
    Vector3 startPos;
    float testEndTime;
    float lastVelocity;

    void Start () {
        rb = GetComponent<Rigidbody>();
        if(rb != null){
            startPos = rb.position;
            testEndTime = Time.time + testLength;
            rb.velocity = initialVelocity;
        }
    }

    void FixedUpdate () {
        if(rb != null){
            if(!rb.isKinematic){
                velocity = rb.velocity.magnitude;
                peakVelocity = Mathf.Max(peakVelocity, velocity);
                lastDelta = velocity - lastVelocity;
                lastVelocity = velocity;
                if(Time.time > testEndTime){
                    transform.position = startPos;
                    rb.velocity = Vector3.zero;
                    rb.isKinematic = true;
                }
            }
        }
    }
	
}
