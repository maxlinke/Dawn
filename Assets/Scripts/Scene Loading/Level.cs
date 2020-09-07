using UnityEngine;

public class Level : MonoBehaviour {

    [SerializeField] FogSettings fogSettings = FogSettings.Default;

    public static Level current { get; private set; }

    public FogSettings FogSettings => fogSettings;

    void Awake () {
        if(current != null){
            Debug.LogError("Singleton violation! Aborting!");
            return;
        }
        current = this;
        fogSettings.Apply();
    }

    void OnDestroy () {
        if(current == this){
            current = null;
        }
    }
	
}
