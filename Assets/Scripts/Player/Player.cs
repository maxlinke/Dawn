using UnityEngine;
using PlayerUtils;
using CustomInputSystem;

public class Player : MonoBehaviour {

    [Header("Scripty bits")]
    [SerializeField] PlayerConfig config = default;
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

    // go full beans on charactercontroller
    // this script itself gathers the input and structifies them
    // this script calls the appropriate movement (and view) modes
    // so that a bot can do the same with its bot-brain
    // how to handle collisions and that stuff?
    // dedicated start- and end-movement methods? start reads the collision data and stores it for use in update, clears the old data beforehand etc?
    // what about crouching etc?
    // TODO ensure same jump height/distance independent of framerate. one version favors low framerates, the other punishes them. 

    // TODO where do i put the cursor.lockmode thingy?

    void Start () {
        // movementSystem.Initialize(config, this);
        viewSystem.Initialize(config, this, head, 0); // TODO take a good long look at the stuff from fpstest2. pretty sure copying it and modifying the parts i want is smarter than writing it all over again...
    }

    // TODO some way of managing the cameras. cameraregistry maybe?

    // because of the option of targeted looking, this should get executed after pretty much all other updates. also makes sense with aiming n stuff
    // so move player to after the default execution in the script execution order?
    // or even move the view and shoot stuff into lateupdate
    void Update () {
        // viewSystem.ExecuteUpdate();
        // movementSystem.ExecuteUpdate();
        viewSystem.Look(GetViewInput());
        // viewSystem.UpdateHeadLocalPosition();
    }

    // block movement, block movement and parent to, unblock movement (automatically unparents)
    // block view etc...
    // no gravity for complete block etc

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