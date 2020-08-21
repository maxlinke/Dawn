using UnityEngine;

namespace Testing {

    public class PMTest : MonoBehaviour {

        [SerializeField] KeyCode addForceKey = default;
        [SerializeField] Rigidbody[] rbs = default;
        [SerializeField] Vector3 rbForce = default;
        [SerializeField] Collider scriptedDefaultPMTarget = default;
        [SerializeField] Collider customPMTarget = default;
        [SerializeField] float customStaticFriction = 0.6f;
        [SerializeField] float customDynamicFriction = 0.6f;
        [SerializeField] float customBounciness = 0f;
        [SerializeField] PhysicMaterialCombine customFrictionCombine = PhysicMaterialCombine.Average;
        [SerializeField] PhysicMaterialCombine customBounceCombine = PhysicMaterialCombine.Average;

        void Start () {
            var defaultPM = new PhysicMaterial();
            Debug.Log($"default: \n{GetPMLog(defaultPM)}");
            scriptedDefaultPMTarget.sharedMaterial = defaultPM;
            var customPM = new PhysicMaterial();
            customPM.staticFriction = customStaticFriction;
            customPM.dynamicFriction = customDynamicFriction;
            customPM.bounciness = customBounciness;
            customPM.frictionCombine = customFrictionCombine;
            customPM.bounceCombine = customBounceCombine;
            Debug.Log($"custom: \n{GetPMLog(customPM)}");
            customPMTarget.sharedMaterial = customPM;
            PushRBs();
            
            string GetPMLog (PhysicMaterial inputPM) {
                var outputLog = string.Empty;
                outputLog += $"\nbounceCombine: {inputPM.bounceCombine}";
                outputLog += $"\nbounciness: {inputPM.bounciness}";
                outputLog += $"\ndynamicFriction: {inputPM.dynamicFriction}";
                outputLog += $"\nfrictionCombine: {inputPM.frictionCombine}";
                outputLog += $"\nstaticFriction: {inputPM.staticFriction}";
                return outputLog;
            }
        }


        void Update () {
            if(Input.GetKeyDown(addForceKey)){
                PushRBs();
            }
        }

        void PushRBs () {
            foreach(var rb in rbs){
                rb.AddForce(rbForce);
            }
        }
        
    }

}