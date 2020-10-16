using UnityEngine;
using PlayerController;
using CustomInputSystem;

public class CCPlayer : Player {

    [Header("Properties")]
    [SerializeField] Properties props = default;
    // [SerializeField] PlayerHealthSettings healthSettings = default;
    [SerializeField] bool selfInit = false;

    [Header("GameObject Parts")]
    [SerializeField] Transform head = default;
    [SerializeField] Transform modelParent = default;
    [SerializeField] Camera firstPersonCamera = default;

    [Header("Script Components")]
    [SerializeField] CCMovement ccMovement = default;
    [SerializeField] CCView ccView = default;

    protected override Camera FirstPersonCamera => firstPersonCamera;
    protected override Properties Props => props;
    protected override Movement MovementSystem => ccMovement;
    protected override bool SelfInit => selfInit;

    // brought back from the dead
    // plan: NO PHYSICS (maybe pushing small physics objects but that's it!)
    // moving platforms via parenting maybe (need moving platforms first tho)
    // this one will be for fast stuff, the rb one for the slower more realistic stuff
    // up slopes: set velocity FLAT
    // down slopes: use regular ground move vector

    protected override void InitializeComponents () {
        ccView.Initialize(props, this, head);
        ccView.SetHeadOrientation(
            headTilt: 0f, 
            headPan: 0f,
            headRoll: 0f
        ); // should be deserialized or something later on
        ccMovement.Initialize(props, head, modelParent);
    }

    void Update () {
        if(!initialized){
            return;
        }
        #if UNITY_EDITOR
            if(Input.anyKeyDown){
                CursorLockManager.UpdateLockState();
            }
        #endif
        bool readInput = CursorLockManager.CursorIsLocked();   // TODO && !paused
        ccView.Look(GetViewInput(readInput));
        ccMovement.UpdateCrouchState(GetCrouchInput(readInput), ccMovement.lastState);
        ccMovement.Move(GetMoveInput(readInput));
        ccView.UpdateHeadLocalPosition();
        if(ccView.InteractCheck(out var interactable, out var interactDescription)){
            if(Bind.INTERACT.GetKeyDown()){
                interactable.Interact(this);
            }
        }
    }

}