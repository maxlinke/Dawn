using UnityEngine;

public class SimpleTriggerLogger : MonoBehaviour {

    void OnTriggerEnter (Collider otherCollider) {
        Debug.Log($"Collider \"{otherCollider.name}\" entered trigger \"{this.name}\"");
    }
	
}
