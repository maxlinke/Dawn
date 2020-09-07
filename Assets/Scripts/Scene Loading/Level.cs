using UnityEngine;
using SceneLoading;

public class Level : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] SceneID id = default;
    [SerializeField] FogSettings fogSettings = FogSettings.Default;

    [Header("Player")]
    [SerializeField] Player playerPrefab = default;
    [SerializeField] Transform playerSpawn = default;

    public static Level current { get; private set; }

    public SceneID ID => id;
    public FogSettings FogSettings => fogSettings;

    void Awake () {
        if(current != null){
            Debug.LogError("Singleton violation! Aborting!");
            return;
        }
        current = this;
        if(SceneLoader.instance == null){
            Init();
        }
    }

    void OnDestroy () {
        if(current == this){
            current = null;
        }
    }

    // TODO level state here
    public void Init () {
        fogSettings.Apply();
        if(playerPrefab != null){
            var player = Instantiate(playerPrefab);
            player.transform.SetAsFirstSibling();
            player.transform.position = playerSpawn.position;
            player.transform.rotation = playerSpawn.rotation;
            player.Initialize();    // TODO player state here
        }
    }

    // ???
    public void LoadSave () {
        
    }
	
}
