using UnityEngine;
using PlayerUtils;

public class Player : MonoBehaviour {

    [SerializeField] Transform head = default;
    [SerializeField] PlayerConfig config = default;
    [SerializeField] Inputs inputSystem = default;
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