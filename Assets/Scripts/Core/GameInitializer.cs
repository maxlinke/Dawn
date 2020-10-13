using System.Collections.Generic;
using UnityEngine;

public class GameInitializer : MonoBehaviour {

    private static bool gameInitialized = false;

    void Awake () {
        if(gameInitialized){
            Destroy(this.gameObject);
            return;
        }
        InitializeCoreComponents();
        gameInitialized = true;
        DontDestroyOnLoad(this.gameObject);
    }

    void InitializeCoreComponents () {
        var coreComponents = new List<ICoreComponent>();
        int childCount = this.transform.childCount;
        for(int i=0; i<childCount; i++){
            ICoreComponent cc = this.transform.GetChild(i).GetComponent<ICoreComponent>();
            if(cc != null){
                coreComponents.Add(cc);
            }
        }
        foreach(var coreComponent in coreComponents){
            coreComponent.InitializeCoreComponent(coreComponents);
        }
    }
	
}
