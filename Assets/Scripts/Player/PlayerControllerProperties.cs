using UnityEngine;

[CreateAssetMenu(menuName = "Player Config", fileName = "New Player Config")]
public class PlayerControllerProperties : ScriptableObject {

    [Header("General")]
    [SerializeField] float normalHeight = 1.8f;
    [SerializeField] float crouchHeight = 0.9f;
    [SerializeField] float eyeOffset = -0.15f;
    [SerializeField] float colliderRadius = 0.4f;

    public float NormalHeight => normalHeight;
    public float CrouchHeight => crouchHeight;
    public float EyeOffset => eyeOffset;
    public float ColliderRadius => colliderRadius;

    [Header("Phyics")]
    [SerializeField] float playerMass = 80f;
    // [SerializeField] TODO phyiscs mat etc?

    public float PlayerMass => playerMass;

    [Header("Movement")]
    [SerializeField] float moveSpeedWalk = 8f;
    [SerializeField] float moveSpeedRun = 12f;
    [SerializeField] float moveSpeedCrouch = 4f;
    [SerializeField] float groundAccel = 32f;
    [SerializeField] float airAccel = 32f;
    [SerializeField] float jumpHeight = 1f;
    [SerializeField] float jumpCalcGravity = 29.43f;

    public float MoveSpeedWalk => moveSpeedWalk;
    public float MoveSpeedRun => moveSpeedRun;
    public float MoveSpeedCrouch => moveSpeedCrouch;
    public float GroundAccel => groundAccel;
    public float AirAccel => airAccel;
    public float JumpHeight => jumpHeight;
    public float JumpCalcGravity => jumpCalcGravity;
	
}
