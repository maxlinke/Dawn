using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASDFRaycaster : MonoBehaviour {

    [SerializeField] Collider target = default;
    [SerializeField] Vector3 direction = default;
    [SerializeField] string debugOutput = default;

    void Update () {
        if(target == null){
            debugOutput = "target is null";
            return;
        }
        if(target.Raycast(new Ray(transform.position, direction), out var hit, Mathf.Infinity)){
            debugOutput = hit.distance.ToString();
        }else{
            debugOutput = "no hit";
        }
    }
	
}
