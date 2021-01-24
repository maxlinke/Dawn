using UnityEngine;
using SceneLoading;

public class Level : MonoBehaviour {

    // this thing shouldn't need the id
    // scriptable object that has an editor reference to the scene asset and serializes the name?
    // also make a game init option in here

#if UNITY_EDITOR
    [Header("Editor")]
    [SerializeField] bool ensureGameInitialized = true;
#endif

    [Header("Settings")]
    [SerializeField] SceneID id = default;
    [SerializeField] FogSettings fogSettings = FogSettings.Default;

    [Header("Player")]
    [SerializeField] Player playerPrefab = default;
    [SerializeField] Transform playerSpawn = default;

    public static Level current { get; private set; }

    public SceneID ID => id;
    public FogSettings FogSettings => fogSettings;

    bool initialized = false;

    void Awake () {
        if(current != null){
            Debug.LogError("Singleton violation, aborting!");
            return;
        }
        current = this;
#if UNITY_EDITOR
        if(ensureGameInitialized){
            if(!GameInitializer.gameInitialized){
                StartCoroutine(GameInitializer.EditorForceInitializeCoroutine());
            }
        }
#endif
        CheckID();
        FirstTimeInit();
    }

    void OnDestroy () {
        if(current == this){
            current = null;
        }
    }

    void CheckID () {
        int idIndex = (int)id;
        int realIndex = this.gameObject.scene.buildIndex;
        if(idIndex != realIndex){
            Debug.LogWarning($"{nameof(Level)} is tagged as \"{id}\" (index {idIndex}) but scene has index {realIndex}!");
        }
    }

    void FirstTimeInit () {
        if(initialized){
            Debug.LogWarning("Already initialized, aborting!");
            return;
        }
        fogSettings.Apply();
        if(playerPrefab != null){
            var player = Instantiate(playerPrefab);
            player.transform.SetAsFirstSibling();
            player.transform.position = playerSpawn.position;
            player.transform.rotation = playerSpawn.rotation;
            player.Initialize();    // TODO player state here
        }
        initialized = true;
    }
	
}
