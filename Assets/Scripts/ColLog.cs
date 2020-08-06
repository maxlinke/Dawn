using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColLog : MonoBehaviour {

    void Awake () {
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 0;
    }

    void FixedUpdate () {
        Debug.Log($"FIXED UPDATE | {Time.frameCount}");
    }

    void OnCollisionEnter (Collision collision) {
        Debug.Log($"ENTER | {Time.frameCount}");
    }

    void OnCollisionStay (Collision collision) {
        Debug.Log($"STAY | {Time.frameCount}");
    }
	
    // void OnTriggerEnter (Collider other) {
    //     Debug.Log($"ENTER | {Time.frameCount}");
    // }

    // void OnTriggerStay (Collider other) {
    //     Debug.Log($"STAY | {Time.frameCount}");
    // }

}
