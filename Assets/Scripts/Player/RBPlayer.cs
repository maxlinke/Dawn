using UnityEngine;
using PlayerController;

public class RBPlayer : Player {

    [Header("Specific Scripty bits")]
    [SerializeField] RBMovement rbMovement = default;
    [SerializeField] RBView rbView = default;

    [Header("Debug")]
    [SerializeField] KeyCode boostKey = KeyCode.Mouse1;
    [SerializeField] KeyCode slowMoKey = KeyCode.Q;
    [SerializeField] KeyCode gravityFlipKey = KeyCode.G;

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
        rbMovement.Initialize(pcProps, head);
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
                Time.timeScale = 0.05f;
            }
            if(Input.GetKeyUp(slowMoKey)){
                Time.timeScale = 1f;
            }
            if(Input.GetKeyDown(gravityFlipKey)){
                Physics.gravity = -Physics.gravity;
            }
        #endif
        var cursorLocked = CursorLockManager.CursorIsLocked();
        rbView.Look(readInput: cursorLocked);
        // rbView.UpdateHeadLocalPosition();    // <- move responsibility to movement
        rbView.InteractCheck(readInput: cursorLocked);
        rbMovement.CacheJumpInputIfNeeded();
    }

    void FixedUpdate () {
        var cursorLocked = CursorLockManager.CursorIsLocked();
        rbView.RotateBodyInLookDirection();
        // update collider height
        // -> does NOT update head position. that happens in update
        // rbMovement.SetPositionToColliderBottom();
        rbMovement.Move(readInput: cursorLocked);
    }
	
}
