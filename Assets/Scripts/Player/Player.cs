using UnityEngine;
using PlayerController;
using CustomInputSystem;

public abstract class Player : MonoBehaviour {

    [Header("Properties")]
    [SerializeField] protected Properties pcProps = default;
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

    protected Vector3 GetViewInput (bool readInput) {
        if(!readInput){
            return Vector2.zero;
        }
        return Bind.GetViewInput();
    }

    protected Movement.CrouchControlInput GetCrouchInput (bool readInput) {
        if(!readInput){
            return Movement.CrouchControlInput.None;
        }
        Movement.CrouchControlInput output;
        output.toggleCrouch = Bind.CROUCH_TOGGLE.GetKeyDown();
        output.crouchHold = Bind.CROUCH_HOLD.GetKey();
        output.crouchHoldRelease = Bind.CROUCH_HOLD.GetKeyUp();
        return output;
    }

    protected Movement.MoveInput GetMoveInput (bool readInput) {
        if(!readInput){
            return RBMovement.MoveInput.None;
        }
        Movement.MoveInput output;
        output.horizontalInput = Bind.GetHorizontalMoveInput();
        output.verticalInput = Bind.GetVerticalMoveInput();
        output.run = 1f - Mathf.Clamp01(Bind.WALK_OR_RUN.GetValue());   // TODO make (1f - x) optional because "Auto Run"
        output.jump = Bind.JUMP.GetKeyDown();
        output.waterExitJump = Bind.JUMP.GetKey();
        return output;
    }
	
}
