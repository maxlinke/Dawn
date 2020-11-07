using System.Collections.Generic;
using UnityEngine;

public class GameInitializer : MonoBehaviour {

    [SerializeField] bool unloadSceneOnDone = false;

    private static bool gameInitialized = false;

    void Awake () {
        if(gameInitialized){
            Debug.LogWarning($"Game is already initialized, self destructing! (Scene \"{this.gameObject.scene.name}\")");
            Destroy(this.gameObject);
            return;
        }
        InitializeChildren();
        gameInitialized = true;
    }

    void Start () {
        int cc = transform.childCount;
        if(cc == 0){
            if(unloadSceneOnDone){
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(this.gameObject.scene);
            }else{
                Destroy(this.gameObject);
            }
        }else{
            Debug.LogWarning($"{nameof(GameInitializer)} still has {cc} children left (should be 0)! Something went wrong!");
        }
    }

    void InitializeChildren () {
        var coreComponents = new List<ICoreComponent>();
        while(this.transform.childCount > 0){
            var child = this.transform.GetChild(0).gameObject;
            child.transform.SetParent(null);
            ICoreComponent cc = child.GetComponent<ICoreComponent>();
            if(cc != null){
                coreComponents.Add(cc);
            }
            DontDestroyOnLoad(child);
        }
        foreach(var coreComponent in coreComponents){
            coreComponent.InitializeCoreComponent(coreComponents);
        }
    }
	
}
