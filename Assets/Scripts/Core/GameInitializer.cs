using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Collections;
using UnityEngine.SceneManagement;
#endif

public class GameInitializer : MonoBehaviour {

    // TODO no transform.getchild, no icorecomponent
    // just references to things in here that get initialized directly
    // AND even better, this thing will be in the root scene of the finished game, so it'll get its awake either way
    // but for raw starting scenes in the editor, i can just get a editor-guid-asset-reference thing going
    // and do a static GameInitializer.EnsureGameInitialized call in the start of a level's awake

    [SerializeField] bool unloadSceneOnDone = false;

    public static bool gameInitialized { get; private set; }

    static GameInitializer () {
        gameInitialized = false;
    }

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

#if UNITY_EDITOR
    public static IEnumerator EditorForceInitializeCoroutine () {
        yield return null;      // wait one frame
        var activeScene = SceneManager.GetActiveScene();
        var tempScene = SceneManager.CreateScene("Temp");
        SceneManager.SetActiveScene(tempScene);
        var path = AssetDatabase.GUIDToAssetPath("56255569b7329e0468f9e49be4544043");   // << game core components prefab
        var initializerPrefab = AssetDatabase.LoadAssetAtPath<GameInitializer>(path);
        Instantiate(initializerPrefab);
        SceneManager.SetActiveScene(activeScene);
        yield return new WaitForSeconds(1f);
        SceneManager.UnloadSceneAsync(tempScene);
    }
#endif

}
