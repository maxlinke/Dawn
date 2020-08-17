using UnityEngine;

namespace Testing {

    public class ForceModeTest : MonoBehaviour {

        [System.Serializable]
        public struct Thing {
            public Rigidbody rb;
            public ForceMode fm;
        }

        [SerializeField] Thing[] things;
        [SerializeField] Vector3 vector;

        void Start () {
            foreach(var thing in things){
                thing.rb.AddForce(vector, thing.fm);
            }   
        }
        
    }

}