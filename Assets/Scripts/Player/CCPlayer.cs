using UnityEngine;
using PlayerController;

public class CCPlayer : Player {

    [Header("Specific Scripty bits")]
    [SerializeField] CCMovement ccMovement = default;
    [SerializeField] CCView ccView = default;

    protected override Movement MovementSystem => ccMovement;

    // important things:
    // - crouch jump
    // - non binary slope limit (slide a bit before sliding fully)

    // TODO some way of managing the cameras. cameraregistry maybe? (CameraID enum)

    // TODO player config
    // - fov
    // - velocity lean
    // - weapon bob?
    // - what other preferences? idk...
    // not included
    // - inverting controls, that's directly via the bidns
    // - auto run, there is no run

    // TODO gravity alignment

    void Start () {
        ccView.Initialize(pcProps, this, head);
        ccView.SetHeadOrientation(
            headTilt: 0f, 
            headPan: 0f,
            headRoll: 0f
        ); // should be deserialized or something later on
        ccMovement.Initialize(pcProps, head);
        InitCamera();
    }

    void Update () {
        #if UNITY_EDITOR
            if(Input.anyKeyDown){
                CursorLockManager.UpdateLockState();
            }
        #endif
        var cursorLocked = CursorLockManager.CursorIsLocked();
        ccView.Look(readInput: cursorLocked);
        ccMovement.Move(readInput: cursorLocked);
        ccView.UpdateHeadLocalPosition();
        ccView.InteractCheck(readInput: cursorLocked);
    }

    // bad move, this causes me to re-enter triggers i'm already in.
    // public void DoWhileAllCollidersDisabled (System.Action actionToDo) {
    //     var ccEnabled = cc.enabled;
    //     cc.enabled = false;
    //     actionToDo();
    //     cc.enabled = ccEnabled;
    // }
	
}