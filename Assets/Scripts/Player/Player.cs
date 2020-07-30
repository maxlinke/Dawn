using UnityEngine;
using PlayerUtils;
using CustomInputSystem;

public class Player : MonoBehaviour {

    [SerializeField] Transform head = default;
    [SerializeField] PlayerConfig config = default;
    [SerializeField] Inputs inputs = default;           // do i really want this?
    [SerializeField] Movement movementSystem = default;
    [SerializeField] View viewSystem = default;

    public float Height => movementSystem.Height;
    public Vector3 Velocity => movementSystem.Velocity;

    void Start () {
        // movementSystem.Initialize(config, this);
    }

    // TODO some way of managing the cameras. cameraregistry maybe?

    void Update () {
        // inputSystem.ExecuteUpdate();
        // movementSystem.ExecuteUpdate();     // movement, crouch, jump
        // viewSystem.ExecuteUpdate();         // head rotation and position (because crouch)

        // super quick test that confirmed that i had to scale the mouse axis by 1/(60*Time.unscaledDeltaTime)
        var dx = Bind.LOOK_RIGHT.GetValue() - Bind.LOOK_LEFT.GetValue();
        var dy = Bind.LOOK_DOWN.GetValue() - Bind.LOOK_UP.GetValue();
        transform.Rotate(new Vector3(0f, dx * 60f * Time.deltaTime, 0f));
        head.transform.Rotate(new Vector3(dy * 60f * Time.deltaTime, 0f, 0f));
    }

    void FixedUpdate () {
        // movementSystem.ExecuteFixedUpdate();
        // viewSystem.Execut
    }

    // 

    // block movement, block movement and parent to, unblock movement (automatically unparents)
    // block view etc...
    // no gravity for complete block etc
	
}