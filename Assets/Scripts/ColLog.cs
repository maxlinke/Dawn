using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColLog : MonoBehaviour {

    Rigidbody rb;
    bool started = false;

    void Awake () {
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 0;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.Sleep();
    }

    void FixedUpdate () {
        // Debug.Log($"FIXED UPDATE | {Time.frameCount}");
        if(Input.GetKey(KeyCode.Q)){
            started = true;
            rb.WakeUp();
        }
        if(!started){
            rb.Sleep();
        }else{
            if(Input.GetKey(KeyCode.E)){
                rb.velocity += Vector3.up * 10f;
                Debug.Log($" > JUMP < | {Time.frameCount}");
            }else{
                rb.velocity += Physics.gravity * Time.fixedDeltaTime;
            }
        }
    }

    void OnCollisionEnter (Collision collision) {
        Debug.Log(collision.collider.attachedRigidbody);
    }

    // void OnCollisionEnter (Collision collision) {
    //     Debug.Log($"ENTER | {Time.frameCount}");
    // }

    // void OnCollisionStay (Collision collision) {
    //     Debug.Log($"STAY | {Time.frameCount}");
    // }

    // void OnCollisionExit (Collision collision) {
    //     Debug.Log($"EXIT | {Time.frameCount}");
    // }
	
    // void OnTriggerEnter (Collider other) {
    //     Debug.Log($"ENTER | {Time.frameCount}");
    // }

    // void OnTriggerStay (Collider other) {
    //     Debug.Log($"STAY | {Time.frameCount}");
    // }

}
