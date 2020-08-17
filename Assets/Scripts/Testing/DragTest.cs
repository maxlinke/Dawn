using UnityEngine;

public class DragTest : MonoBehaviour {

    [SerializeField] Rigidbody[] rbs;
    [SerializeField] Vector3 startVelocity;

    void Start () {
        foreach(var rb in rbs){
            rb.AddForce(startVelocity, ForceMode.VelocityChange);
        }
    }
	
}
