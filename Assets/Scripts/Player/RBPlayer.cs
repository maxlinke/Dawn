using UnityEngine;
using PlayerController;

public class RBPlayer : Player {

    [Header("Specific Scripty bits")]
    [SerializeField] RBMovement rbMovement = default;
    [SerializeField] RBView rbView = default;

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
        rbMovement.Initialize(pcProps);
        InitCamera();
    }

    void Update () {
        #if UNITY_EDITOR
            if(Input.anyKeyDown){
                CursorLockManager.UpdateLockState();
            }
            if(Input.GetKeyDown(KeyCode.Mouse1)){
                rbMovement.Velocity += head.transform.forward * 50f;
            }
            if(Input.GetKeyDown(KeyCode.Q)){
                Time.timeScale = 0.05f;
            }
            if(Input.GetKeyUp(KeyCode.Q)){
                Time.timeScale = 1f;
            }
        #endif
        var cursorLocked = CursorLockManager.CursorIsLocked();
        rbView.Look(readInput: cursorLocked);
        rbView.UpdateHeadLocalPosition();
        rbView.InteractCheck(readInput: cursorLocked);
        rbMovement.ExecuteUpdate();
    }

    void FixedUpdate () {
        var cursorLocked = CursorLockManager.CursorIsLocked();
        rbView.RotateBodyInLookDirection();
        rbMovement.Move(readInput: cursorLocked);
    }
	
}
