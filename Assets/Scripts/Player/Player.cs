using UnityEngine;
using PlayerController;

public class Player : MonoBehaviour {

    [Header("Scripty bits")]
    [SerializeField] PlayerControllerProperties pcProps = default;
    [SerializeField] Movement movementSystem = default;
    [SerializeField] View viewSystem = default;

    [Header("GameObject Parts")]
    [SerializeField] Rigidbody rb = default;
    [SerializeField] CapsuleCollider col = default;
    [SerializeField] Transform head = default;

    public float Height => movementSystem.Height;
    public Vector3 Velocity => movementSystem.Velocity;

    public Vector3 HeadPos => head.transform.position;
    public Vector3 FootPos => transform.position;
    public Vector3 CenterPos => movementSystem.WorldCenter;

    // important things:
    // - crouch jump
    // - non binary slope limit (slide a bit before sliding fully)
    // - screw the mass-screwery. the player weighs what he weighs. if anything gets pushed too easily, adjust its mass.
    // - i don't like the velocityComesFromMove stuff. try to not do that.

    // TODO walk and run OR always "run" and dodge? the second sounds a lot more fun

    // TODO some way of managing the cameras. cameraregistry maybe? (CameraID enum)

    Vector3 DEBUG_LAST_POS;

    void Start () {
        viewSystem.Initialize(pcProps, this, rb, head);
        viewSystem.SetHeadOrientation(
            headTilt: 0f, 
            headPan: 0f,
            headRoll: 0f
        ); // should be deserialized or something later on
        movementSystem.Initialize(pcProps, this, rb, col);
        DEBUG_LAST_POS = transform.position;
    }

    void Update () {
        #if UNITY_EDITOR
            if(Input.anyKeyDown){
                CursorLockManager.UpdateLockState();
            }
            if(Input.GetKeyDown(KeyCode.Mouse1)){
                rb.velocity += head.transform.forward * 50f;
            }
        #endif
        viewSystem.Look(readInput: CursorLockManager.CursorIsLocked());
        viewSystem.UpdateHeadLocalPosition();
    }

    void FixedUpdate () {
        viewSystem.MatchRBRotationToHead();
        movementSystem.Move(readInput: CursorLockManager.CursorIsLocked());
        var DEBUG_CURRENT_POS = transform.position;
        Debug.DrawLine(DEBUG_LAST_POS, DEBUG_CURRENT_POS, Color.red, 4f);
        DEBUG_LAST_POS = DEBUG_CURRENT_POS;
    }
	
}