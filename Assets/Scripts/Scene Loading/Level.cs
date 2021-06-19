using UnityEngine;
using SceneLoading;

public class Level : MonoBehaviour {

    // this thing shouldn't need the id
    // scriptable object that has an editor reference to the scene asset and serializes the name?
    // also make a game init option in here

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
        GameInitializer.EnsureGameInitialized();

        // TODO this can go, i'll do stuff straight via the build-list of scenes...
        CheckID();

        // TODO if i make the gameinit thing do it's "slower" stuff asynchronously, i can put this in the start-ienumerator
        // as well as all other stuff that'd like the game to be properly initialized by the time it runs
        // maybe use the loading screen here for that, if i can... 
        // i'll still have to rip out the loading screen from the scene loader
        // it can USE the loading screen but it shouldn't BE the loading screen
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
