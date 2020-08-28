using UnityEngine;
using PlayerController;

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

    // TODO more camera init 
    // layers etc
    // for that i need the player config (fov)
    // also some kind of subscription or something to playerconfig
    // because if the fov gets changed or velocity lean disabled, that should actually happen...

    void Start () {
        rbView.Initialize(pcProps, this, head);
        rbView.SetHeadOrientation(
            headTilt: 0f, 
            headPan: 0f,
            headRoll: 0f
        ); // should be deserialized or something later on
        rbView.smoothRotationParent = smoothRotationParent;
        rbMovement.Initialize(pcProps, head, modelParent);
        rbMovement.smoothRotationParent = smoothRotationParent;
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
        rbView.Look(readInput: cursorLocked);
        rbView.InteractCheck(readInput: cursorLocked);
        rbMovement.UpdateCrouchState(readInput: cursorLocked);
        rbMovement.UpdateHeadAndModelPosition(instantly: false);
        rbMovement.AlignWithGravityIfAllowed(timeStep: Time.deltaTime);
        rbMovement.CacheSingleFrameInputs();
    }

    void FixedUpdate () {
        var cursorLocked = CursorLockManager.CursorIsLocked();
        // rbView.RotateBodyInLookDirection();
        // update collider height
        // -> does NOT update head position. that happens in update
        // rbMovement.UpdateCrouchState(readInput: cursorLocked);
        // rbMovement.SetPositionToColliderBottom();
        rbMovement.ApplySubRotation();
        rbMovement.Move(readInput: cursorLocked);
    }
	
}
