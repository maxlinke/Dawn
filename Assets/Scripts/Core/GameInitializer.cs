using System.Collections.Generic;
using UnityEngine;

public class GameInitializer : MonoBehaviour {

    [SerializeField] GameObject[] objects = default;

    private static bool gameInitialized = false;

    void Awake () {
        if(gameInitialized){
            Destroy(this.gameObject);
            return;
        }
        var core = SpawnAllAndGetCoreComponents();
        gameInitialized = true;
        Destroy(this.gameObject);
    }

    IEnumerable<ICoreComponent> SpawnAllAndGetCoreComponents() {
        List<ICoreComponent> output = new List<ICoreComponent>();
        for(int i=0; i<objects.Length; i++){
            GameObject prefab = objects[i];
            if(prefab == null){
                Debug.LogWarning($"Null entry at index {i} in {nameof(GameInitializer)}.{nameof(objects)}!");
                continue;
            }
            GameObject instance = Instantiate(prefab);
            DontDestroyOnLoad(instance);
            ICoreComponent cc = instance.GetComponent<ICoreComponent>();
            if(cc != null){
                output.Add(cc);
            }
        }
        return output;
    }

    void InitializeCoreComponents (IEnumerable<ICoreComponent> coreComponents) {
        foreach(var coreComponent in coreComponents){
            coreComponent.Initialize(coreComponents);
        }
    }
	
}
