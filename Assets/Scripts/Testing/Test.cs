using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

    [SerializeField] float drag = 1f;
    [SerializeField] float gravity = 9.81f;
    [SerializeField] float deltaTime = 0.02f;

    void Awake () {
        
    }

    void Update () {
        
    }

    [ContextMenu("Log Terminal Velocity")]
    void LogTerminalVelocity () {
        Debug.Log(GetTerminalVelocity());
    }

    float GetTerminalVelocity () {
        var x = (1f - deltaTime * drag);
        if(x == 1f){
            return float.PositiveInfinity;
        }
        return (gravity * deltaTime * x) / (1 - x);
    }
	
}
