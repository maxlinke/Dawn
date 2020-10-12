using UnityEngine;
using PlayerController;
using CustomInputSystem;

public class RBPlayer : Player {

    [Header("Specific Gameobject Parts")]
    [SerializeField] Transform smoothRotationParent = default;

    [Header("Specific Scripty bits")]
    [SerializeField] RBMovement rbMovement = default;
    [SerializeField] RBView rbView = default;
    [SerializeField] PlayerModel model = default;
    [SerializeField] PlayerHealth health = default;

    [Header("Debug")]
    [SerializeField] KeyCode boostKey = KeyCode.Mouse1;
    [SerializeField] KeyCode slowMoKey = KeyCode.Q;
    [SerializeField] KeyCode gravityFlipKey = KeyCode.G;
    [SerializeField] KeyCode viewLockKey = KeyCode.V;
    [SerializeField] Transform debugViewTarget = default;
    [SerializeField] bool numberKeysDoDebugLogs = false;

    protected override Movement MovementSystem => rbMovement;

    private bool cachedJumpKeyDown = false;

    // TODO more camera init 
    // layers etc
    // for that i need the player config (fov)
    // also some kind of subscription or something to playerconfig
    // because if the fov gets changed or velocity lean disabled, that should actually happen...

    // TODO separate "state loading". init should just set all the important references and init the components
    // so i don't have to destroy the player and spawn a new one if i want to reload a quicksave or something
    
    // TODO serializable playerstate thing (class so null is an option?)
    // so there is a difference between a "fresh" load and a "save" load
    protected override void InitializeComponents () {
        rbView.Initialize(pcProps, this, head);
        rbView.SetHeadOrientation(
            headTilt: 0f, 
            headPan: 0f,
            headRoll: 0f
        ); // should be deserialized or something later on
        rbView.smoothRotationParent = smoothRotationParent;
        rbMovement.Initialize(pcProps, head, modelParent, smoothRotationParent);
        health.Initialize(healthSettings, head);
        health.SetHealth(healthSettings.DefaultHealth);
        health.SetInvulnerable(false);      // TODO commandprompt.godmodeenabled
        model.Initialize(pcProps, rbMovement, head);
        // load the states 
        // set collider height
        rbMovement.UpdateHeadAndModelPosition(instantly: true);
    }

    void Update () {
        if(!initialized){
            return;
        }
        #if UNITY_EDITOR
            DebugInputs();
        #endif
        health.InternalHealthUpdate();
        // TODO what about being dead?
        bool readInput = CursorLockManager.CursorIsLocked();
        // TODO && game paused (timescale == 0f but done somewhere else)
        rbView.Look(GetViewInput(readInput));
        if(rbView.InteractCheck(out var interactable, out var interactDescription)){
            if(Bind.INTERACT.GetKeyDown()){
                interactable.Interact(this);
            }
        }
        rbMovement.UpdateCrouchState(GetCrouchInput(readInput));
        rbMovement.UpdateHeadAndModelPosition(instantly: false);
        rbMovement.AlignWithGravityIfAllowed();
        CacheSingleFrameInputs();
        model.UpdateSpherePositions();

        DebugTools.PlayerControllerDebugUI.ViewInfo = rbView.debugInfo;
        DebugTools.PlayerControllerDebugUI.MovementInfo = rbMovement.debugInfo;
    }

    void FixedUpdate () {
        if(!initialized){
            return;
        }
        health.ClearWaterTriggerList();
        bool readInput = CursorLockManager.CursorIsLocked();
        rbMovement.ApplySubRotation();
        var moveInput = GetMoveInput(readInput);
        if(readInput){
            moveInput.jump |= cachedJumpKeyDown;
        }
        rbMovement.Move(moveInput);
        ResetSingleFrameInputs();
    }

    void CacheSingleFrameInputs () {
        var lastState = rbMovement.lastState;
        if(Time.frameCount != lastState.frame){
            if(!lastState.startedJump && lastState.canJump){
                cachedJumpKeyDown |= Bind.JUMP.GetKeyDown();
            }
        }
    }

    void ResetSingleFrameInputs () {
        cachedJumpKeyDown = false;
    }

    void DebugInputs () {
        if(Input.anyKeyDown){
            CursorLockManager.UpdateLockState();
        }
        if(Input.GetKeyDown(boostKey)){
            rbMovement.Velocity += head.transform.forward * 50f;
        }
        if(Input.GetKeyDown(slowMoKey)){
            if(Time.timeScale == 1f){
                Time.timeScale = Time.fixedDeltaTime;
            }else{
                Time.timeScale = 1f;
            }
        }
        if(Input.GetKeyDown(gravityFlipKey)){
            Physics.gravity = -Physics.gravity;
        }
        if(Input.GetKeyDown(viewLockKey)){
            if(debugViewTarget != null){
                rbView.viewTarget = debugViewTarget;
                rbView.controlMode = View.ControlMode.TARGETED;
            }
        }
        if(Input.GetKeyUp(viewLockKey)){
            rbView.viewTarget = null;
            rbView.controlMode = View.ControlMode.FULL;
        }
        if(numberKeysDoDebugLogs){
            if(Input.GetKeyDown(KeyCode.Alpha1)){
                Debug.Log(Time.frameCount);
            }
            if(Input.GetKeyDown(KeyCode.Alpha2)){
                Debug.LogWarning(Time.frameCount);
            }
            if(Input.GetKeyDown(KeyCode.Alpha3)){
                Debug.LogError(Time.frameCount);
            }
            if(Input.GetKeyDown(KeyCode.Alpha4)){
                Debug.LogException(new System.Exception(Time.frameCount.ToString()));
            }
        }
    }
	
}
