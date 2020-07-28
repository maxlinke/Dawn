using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

    void Start () {

    }

    void Update () {
        // if(CustomInputSystem.InputSystem.Bind.PAUSE_CANCEL_ETC.GetKeyDown()){
        //     Debug.Log("it works");
        // }
        if(CustomInputSystem.InputSystem.Bind.PAUSE_CANCEL_ETC.GetValue() >= 0.5f){
            Debug.Log("it works");
        }
    }
	
}
