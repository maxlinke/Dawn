using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Testing {

    public class RigidbodyTorqueAdder : MonoBehaviour {

        [SerializeField] Vector3 torque = default;
        [SerializeField] bool scaleWithMass = default;
        [SerializeField] bool addRelative = default;

        Rigidbody rb;

        void Awake () {
            rb = GetComponent<Rigidbody>();
        }

        public void DoTorque () {
            if(rb == null){
                return;
            }
            var multiplier = (scaleWithMass ? rb.mass : 1f);
            if(addRelative){
                rb.AddRelativeTorque(torque * multiplier, ForceMode.Acceleration);
            }else{
                rb.AddTorque(torque * multiplier, ForceMode.Acceleration);
            }
        }
        
    }

    [CustomEditor(typeof(RigidbodyTorqueAdder))]
    public class RigidbodyTorqueAdderEditor : Editor {

        public override void OnInspectorGUI () {
            base.OnInspectorGUI();
            if(EditorApplication.isPlaying){
                GUILayout.Space(10f);
                if(GUILayout.Button("Do Torque")){
                    (target as RigidbodyTorqueAdder).DoTorque();
                }
            }
        }

    }

}