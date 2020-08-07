using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

    [SerializeField] Rigidbody target;
    [SerializeField] Vector3 vOutput;
    [SerializeField] float fOutput;

    void Awake () {
        
    }

    void Update () {
        vOutput = target.velocity;
        fOutput = target.velocity.magnitude;
        // if(Input.GetKeyDown(KeyCode.Alpha1)){
        //     Debug.Log("asdf");
        // }
        // if(Input.GetKeyDown(KeyCode.Alpha2)){
        //     Debug.LogWarning("asdf");
        // }
        // if(Input.GetKeyDown(KeyCode.Alpha3)){
        //     Debug.LogError("asdf");
        // }
        // if(Input.GetKeyDown(KeyCode.Alpha4)){
        //     throw new System.Exception("asdf");
        // }
    }
	
}
