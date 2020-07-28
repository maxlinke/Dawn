using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour {

    [SerializeField] Vector3 rotationSpeed;

    void Update () {
        transform.localEulerAngles = rotationSpeed * Time.time;
    }
	
}
