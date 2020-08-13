using UnityEngine;
using PlayerController;

public class RBPlayer : Player {

    [Header("Specific Scripty bits")]
    [SerializeField] RBMovement rbMovement = default;
    [SerializeField] RBView rbView = default;

    protected override Movement MovementSystem => rbMovement;

    // TODO view init should also init camera
    // clipping distances, layers etc
    // for that i need the player config (fov)

    void Start () {
        rbView.Initialize(pcProps, this, head);
        rbView.SetHeadOrientation(
            headTilt: 0f, 
            headPan: 0f,
            headRoll: 0f
        ); // should be deserialized or something later on
        rbMovement.Initialize(pcProps);
    }

    void Update () {
        #if UNITY_EDITOR
            if(Input.anyKeyDown){
                CursorLockManager.UpdateLockState();
            }
        #endif
        var cursorLocked = CursorLockManager.CursorIsLocked();
        rbView.Look(readInput: cursorLocked);
        rbView.UpdateHeadLocalPosition();
        rbView.InteractCheck(readInput: cursorLocked);
    }

    void FixedUpdate () {
        var cursorLocked = CursorLockManager.CursorIsLocked();
        rbView.RotateBodyInLookDirection();
        rbMovement.Move(readInput: cursorLocked);
    }
	
}
