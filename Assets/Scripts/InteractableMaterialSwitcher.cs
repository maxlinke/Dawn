using UnityEngine;

public class InteractableMaterialSwitcher : MonoBehaviour, IInteractable {

    [SerializeField] MeshRenderer mr = default;
    [SerializeField] Material[] materials = default;

    public bool CanBeInteractedWith => true;
    public string InteractionDescription => "Change Color";

    public void Interact (object interactor) {
        NextColor();
    }

    void NextColor () {
        var current = mr.sharedMaterial;
        int currentMatIndex = -1;
        for(int i=0; i<materials.Length; i++){
            if(materials[i] == current){
                currentMatIndex = i;
                break;
            }
        }
        currentMatIndex = (currentMatIndex + 1) % materials.Length;
        mr.sharedMaterial = materials[currentMatIndex];
    }
	
}
