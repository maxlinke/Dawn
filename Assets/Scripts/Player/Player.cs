using UnityEngine;
using PlayerController;

public class Player : MonoBehaviour {

    [Header("Scripty bits")]
    [SerializeField] PlayerControllerProperties pcProps = default;
    [SerializeField] Movement movementSystem = default;
    [SerializeField] View viewSystem = default;

    [Header("GameObject Parts")]
    [SerializeField] CharacterController cc = default;
    [SerializeField] Transform head = default;

    public float Height => movementSystem.Height;
    public Vector3 Velocity => movementSystem.Velocity;

    public Vector3 HeadPos => head.transform.position;
    public Vector3 FootPos => transform.position;
    public Vector3 CenterPos => cc.transform.TransformPoint(cc.center);

    // TODO do kinematic rigidbodies only update their collision per fixedupdate?

    // TODO revert back to charactercontroller
    // - TODO test if collision info comes immediately after cc.Move or after update...
    //   - immediately after move
    // - TODO does the capsule also collide with the ground when jumping, causing double the jump? implement jump via += first before going to y = value
    //   - no, it doesn't thankfully.

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

    void Start () {
        viewSystem.Initialize(pcProps, this, cc, head);
        viewSystem.SetHeadOrientation(
            headTilt: 0f, 
            headPan: 0f,
            headRoll: 0f
        ); // should be deserialized or something later on
        movementSystem.Initialize(pcProps, this, cc);
    }

    void Update () {
        #if UNITY_EDITOR
            if(Input.anyKeyDown){
                CursorLockManager.UpdateLockState();
            }
        #endif
        var cursorLocked = CursorLockManager.CursorIsLocked();
        viewSystem.Look(readInput: cursorLocked);
        movementSystem.Move(readInput: cursorLocked);
        viewSystem.UpdateHeadLocalPosition();
        viewSystem.InteractCheck(readInput: cursorLocked);
    }

    // bad move, this causes me to re-enter triggers i'm already in.
    // public void DoWhileAllCollidersDisabled (System.Action actionToDo) {
    //     var ccEnabled = cc.enabled;
    //     cc.enabled = false;
    //     actionToDo();
    //     cc.enabled = ccEnabled;
    // }
	
}