using UnityEngine;

public class PMTest : MonoBehaviour {

    [SerializeField] Rigidbody[] rbs = default;
    [SerializeField] Vector3 rbForce = default;
    [SerializeField] Collider scriptedDefaultPMTarget = default;

    void Start () {
        var defaultPM = new PhysicMaterial();
        var log = $"{nameof(PhysicMaterial)}";
        log += $"\nbounceCombine: {defaultPM.bounceCombine}";
        log += $"\nbounciness: {defaultPM.bounciness}";
        log += $"\ndynamicFriction: {defaultPM.dynamicFriction}";
        log += $"\nfrictionCombine: {defaultPM.frictionCombine}";
        log += $"\nstaticFriction: {defaultPM.staticFriction}";
        Debug.Log(log);
        scriptedDefaultPMTarget.sharedMaterial = defaultPM;
        foreach(var rb in rbs){
            rb.AddForce(rbForce);
        }
    }
	
}
