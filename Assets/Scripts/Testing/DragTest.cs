using UnityEngine;

namespace Testing {

    public class DragTest : MonoBehaviour {

        [SerializeField] Rigidbody[] rbs = default;
        [SerializeField] Vector3 startVelocity = default;

        void Start () {
            foreach(var rb in rbs){
                rb.AddForce(startVelocity, ForceMode.VelocityChange);
            }
        }
        
    }

}