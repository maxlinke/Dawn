using System.Collections.Generic;
using UnityEngine;

public class GameInitializer : MonoBehaviour {

    [SerializeField] bool unloadSceneOnDone = false;

    private static bool gameInitialized = false;

    void Awake () {
        if(gameInitialized){
            Destroy(this.gameObject);
            return;
        }
        InitializeChildren();
        gameInitialized = true;
    }

    void Start () {
        if(unloadSceneOnDone && transform.childCount == 0){
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(this.gameObject.scene);
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
