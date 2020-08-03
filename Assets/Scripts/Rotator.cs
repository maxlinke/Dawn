using UnityEngine;

public class Rotator : MonoBehaviour {

    [SerializeField] Vector3 rotationSpeed = default;

    void Update () {
        transform.localEulerAngles = rotationSpeed * Time.time;
    }
	
}
