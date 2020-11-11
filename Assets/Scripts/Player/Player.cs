using UnityEngine;
using PlayerController;
using CustomInputSystem;

public abstract class Player : MonoBehaviour {

    public static Player Instance { get; private set; }

    public float Height => MovementSystem.LocalColliderHeight * transform.lossyScale.Average();
    public Vector3 Velocity => MovementSystem.WorldVelocity;

    protected abstract Camera FirstPersonCamera { get; }
    protected abstract Properties Props { get; }
    protected abstract Movement MovementSystem { get; }
    protected abstract bool SelfInit { get; }

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
        if(SelfInit){
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
        InitFirstPersonCamera();
        InitializeComponents();
        initialized = true;
    }

    protected abstract void InitializeComponents ();

    // TODO also third person camera
    void InitFirstPersonCamera () {
        FirstPersonCamera.orthographic = false;
        FirstPersonCamera.nearClipPlane = Props.NearClipDist;
        FirstPersonCamera.farClipPlane = Props.FarClipDist;
        FirstPersonCamera.cullingMask &= ~Layer.PlayerHitboxesAndThirdPersonModel.mask;
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
        output.crouch = Bind.CROUCH_HOLD.GetKey();
        output.uncrouch = Bind.CROUCH_HOLD.GetKeyUp();
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
