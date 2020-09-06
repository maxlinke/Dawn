using UnityEngine;
using PlayerController;
using CustomInputSystem;

public class RBPlayer : Player {

    [Header("Specific Gameobject Parts")]
    [SerializeField] Transform smoothRotationParent = default;

    [Header("Specific Scripty bits")]
    [SerializeField] RBMovement rbMovement = default;
    [SerializeField] RBView rbView = default;

    [Header("Debug")]
    [SerializeField] KeyCode boostKey = KeyCode.Mouse1;
    [SerializeField] KeyCode slowMoKey = KeyCode.Q;
    [SerializeField] KeyCode gravityFlipKey = KeyCode.G;
    [SerializeField] KeyCode viewLockKey = KeyCode.V;
    [SerializeField] Transform debugViewTarget = default;

    protected override Movement MovementSystem => rbMovement;

    private bool cachedJumpKeyDown = false;

    // TODO more camera init 
    // layers etc
    // for that i need the player config (fov)
    // also some kind of subscription or something to playerconfig
    // because if the fov gets changed or velocity lean disabled, that should actually happen...

    void Start () {
        if(!IsValidSingleton()){
            return;
        }
        rbView.Initialize(pcProps, this, head);
        rbView.SetHeadOrientation(
            headTilt: 0f, 
            headPan: 0f,
            headRoll: 0f
        ); // should be deserialized or something later on
        rbView.smoothRotationParent = smoothRotationParent;
        rbMovement.Initialize(pcProps, head, modelParent, smoothRotationParent);
        // load the states 
        // set collider height
        rbMovement.UpdateHeadAndModelPosition(instantly: true);
        InitCamera();
    }

    void Update () {
        #if UNITY_EDITOR
            if(Input.anyKeyDown){
                CursorLockManager.UpdateLockState();
            }
            if(Input.GetKeyDown(boostKey)){
                rbMovement.Velocity += head.transform.forward * 50f;
            }
            if(Input.GetKeyDown(slowMoKey)){
                if(Time.timeScale == 1f){
                    Time.timeScale = 0.05f;
                }else{
                    Time.timeScale = 1f;
                }
            }
            if(Input.GetKeyDown(gravityFlipKey)){
                Physics.gravity = -Physics.gravity;
            }
            if(Input.GetKeyDown(viewLockKey)){
                rbView.viewTarget = debugViewTarget;
                rbView.controlMode = View.ControlMode.TARGETED;
            }
            if(Input.GetKeyUp(viewLockKey)){
                rbView.viewTarget = null;
                rbView.controlMode = View.ControlMode.FULL;
            }
        #endif
        var cursorLocked = CursorLockManager.CursorIsLocked();
        rbView.Look(cursorLocked ? Bind.GetViewInput() : Vector2.zero);
        if(rbView.InteractCheck(out var interactable, out var interactDescription)){
            if(Bind.INTERACT.GetKeyDown()){
                interactable.Interact(this);
            }
        }
        rbMovement.UpdateCrouchState(GetCrouchInput(readInput: cursorLocked));
        rbMovement.UpdateHeadAndModelPosition(instantly: false);
        rbMovement.AlignWithGravityIfAllowed(timeStep: Time.deltaTime);
        CacheSingleFrameInputs();

        DebugTools.PlayerControllerDebugUI.ViewInfo = rbView.debugInfo;
        DebugTools.PlayerControllerDebugUI.MovementInfo = rbMovement.debugInfo;
    }

    void FixedUpdate () {
        var cursorLocked = CursorLockManager.CursorIsLocked();
        rbMovement.ApplySubRotation();
        rbMovement.Move(GetMoveInput(readInput: cursorLocked));
        ResetSingleFrameInputs();
    }

    Movement.CrouchControlInput GetCrouchInput (bool readInput) {
        if(!readInput){
            return Movement.CrouchControlInput.None;
        }
        Movement.CrouchControlInput output;
        output.toggleCrouch = Bind.CROUCH_TOGGLE.GetKeyDown();
        output.crouchHold = Bind.CROUCH_HOLD.GetKey();
        output.crouchHoldRelease = Bind.CROUCH_HOLD.GetKeyUp();
        return output;
    }

    RBMovement.MoveInput GetMoveInput (bool readInput) {
        if(!readInput){
            return RBMovement.MoveInput.None;
        }
        RBMovement.MoveInput output;
        output.horizontalInput = Bind.GetHorizontalMoveInput();
        output.verticalInput = Bind.GetVerticalMoveInput();
        output.run = 1f - Mathf.Clamp01(Bind.WALK_OR_RUN.GetValue());   // TODO make (1f - x) optional because "Auto Run"
        output.jump = Bind.JUMP.GetKeyDown() || cachedJumpKeyDown;
        output.waterExitJump = Bind.JUMP.GetKey();
        return output;
    }

    void CacheSingleFrameInputs () {
        var lastState = rbMovement.lastState;
        if(Time.frameCount != lastState.frame){
            if(!lastState.jumped && lastState.canJump){
                cachedJumpKeyDown |= Bind.JUMP.GetKeyDown();
            }
        }
    }

    void ResetSingleFrameInputs () {
        cachedJumpKeyDown = false;
    }
	
}
