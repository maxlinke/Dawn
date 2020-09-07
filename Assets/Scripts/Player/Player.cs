using UnityEngine;
using PlayerController;

public abstract class Player : MonoBehaviour {

    [Header("Properties")]
    [SerializeField] protected PlayerControllerProperties pcProps = default;
    [SerializeField] protected PlayerHealthSettings healthSettings = default;
    [SerializeField] protected bool selfInit = false;

    [Header("GameObject Parts")]
    [SerializeField] protected Transform head = default;
    [SerializeField] protected Transform modelParent = default;
    [SerializeField] protected Camera fpCamera = default;

    public static Player Instance { get; private set; }

    public float Height => MovementSystem.LocalColliderHeight * transform.lossyScale.Average();
    public Vector3 Velocity => MovementSystem.Velocity;

    public Vector3 HeadPos => head.transform.position;
    public Vector3 CenterPos => MovementSystem.WorldCenterPos;

    protected abstract Movement MovementSystem { get; }

    private bool m_initialized = false;
    public bool initialized { 
        get => m_initialized; 
        private set => m_initialized = value;
    }

    protected virtual void Start () {
        if(Instance != null){
            Debug.LogError($"Singleton violation, instance of {nameof(Player)} is not null!");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        if(selfInit){
            Debug.LogWarning($"{nameof(Player)} is self initializing!");
            Initialize();
        }
    }

    protected virtual void OnDestroy () {
        if(Instance == this){
            Instance = null;
        }
    }

    public void Initialize () {
        if(initialized){
            Debug.LogWarning($"{nameof(Player)} is already initialized, aborting!");
            return;
        }
        InitCamera();
        InitializeComponents();
        initialized = true;
    }

    protected abstract void InitializeComponents ();

    // TODO also third person camera
    void InitCamera () {
        fpCamera.orthographic = false;
        fpCamera.nearClipPlane = pcProps.NearClipDist;
        fpCamera.farClipPlane = pcProps.FarClipDist;
        fpCamera.cullingMask &= ~LayerMaskUtils.LayerToBitMask(Layer.PlayerControllerAndWorldModel);
    }
	
}
