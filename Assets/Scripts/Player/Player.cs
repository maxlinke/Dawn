using UnityEngine;
using PlayerUtils;
using CustomInputSystem;

public class Player : MonoBehaviour {

    [Header("Scripty bits")]
    [SerializeField] PlayerControllerProperties pcProps = default;
    [SerializeField] Movement movementSystem = default;
    [SerializeField] View viewSystem = default;

    [Header("GameObject Parts")]
    [SerializeField] Transform head = default;

    public float Height => movementSystem.Height;
    public Vector3 Velocity => movementSystem.Velocity;

    public Vector3 HeadPos => head.transform.position;
    public Vector3 FootPos => transform.position;
    public Vector3 CenterPos => movementSystem.WorldCenter;

    public MoveControl modeControl;
    public ViewControl viewControl;

    public enum MoveControl {
        FULL,
        BLOCK_INPUT,
        ANCHORED
    }

    public enum ViewControl {
        FULL,
        BLOCK_INPUT,
        TARGETED
    }

    // TODO fuck cc, rb is where it's at
    // first rb view
    // then simple movement
    // then build upon that (if necessary)

    // important things:
    // - crouch jump
    // - non binary slope limit (slide a bit before sliding fully)
    // - screw the mass-screwery. the player weighs what he weighs. if anything gets pushed too easily, adjust its mass.
    // - i don't like the velocityComesFromMove stuff. try to not do that.

    // TODO walk and run OR always "run" and dodge? the second sounds a lot more fun

    // TODO some way of managing the cameras. cameraregistry maybe? (CameraID enum)

    void Start () {

    }

    void Update () {
        #if UNITY_EDITOR
            if(Input.anyKeyDown){
                CursorLockManager.UpdateLockState();
            }
        #endif
    }

    Vector2 GetViewInput () {
        var dx = Bind.LOOK_RIGHT.GetValue() - Bind.LOOK_LEFT.GetValue();
        var dy = Bind.LOOK_DOWN.GetValue() - Bind.LOOK_UP.GetValue();
        return new Vector2(dx, dy);
    }

    Vector3 GetLocalSpaceMoveInput () {
        float move = Bind.MOVE_FWD.GetValue() - Bind.MOVE_BWD.GetValue();
        float strafe = Bind.MOVE_RIGHT.GetValue() - Bind.MOVE_BWD.GetValue();
        return new Vector3(strafe, 0, move);
    }
	
}