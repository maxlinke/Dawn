using UnityEngine;
using PlayerController;
using CustomInputSystem;

public class CCPlayer : Player {

    [Header("Specific Scripty bits")]
    [SerializeField] CCMovement ccMovement = default;
    [SerializeField] CCView ccView = default;

    protected override Movement MovementSystem => ccMovement;

    // brought back from the dead
    // plan: NO PHYSICS (maybe pushing small physics objects but that's it!)
    // moving platforms via parenting maybe (need moving platforms first tho)
    // this one will be for fast stuff, the rb one for the slower more realistic stuff
    // up slopes: set velocity FLAT
    // down slopes: use regular ground move vector


    protected override void InitializeComponents () {
        ccView.Initialize(pcProps, this, head);
        ccView.SetHeadOrientation(
            headTilt: 0f, 
            headPan: 0f,
            headRoll: 0f
        ); // should be deserialized or something later on
        ccMovement.Initialize(pcProps, head, modelParent);
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
        var cursorLocked = CursorLockManager.CursorIsLocked();
        ccView.Look(GetViewInput(readInput: cursorLocked));
        ccMovement.Move(GetMoveInput(readInput: cursorLocked));
        ccView.UpdateHeadLocalPosition();
        if(ccView.InteractCheck(out var interactable, out var interactDescription)){
            if(Bind.INTERACT.GetKeyDown()){
                interactable.Interact(this);
            }
        }
    }

}