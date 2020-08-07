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
    [SerializeField] float slopeLimit = 60f;
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float moveSpeedCrouch = 4f;
    [SerializeField] float groundAccel = 32f;
    [SerializeField] float groundDrag = 16f;
    [SerializeField] float airAccel = 32f;
    [SerializeField] float airDrag = 8f;
    [SerializeField] float jumpHeight = 1f;
    [SerializeField] float jumpCalcGravity = 29.43f;
	
    public float SlopeLimit => slopeLimit;
    public float MoveSpeed => moveSpeed;
    public float MoveSpeedCrouch => moveSpeedCrouch;
    public float GroundAccel => groundAccel;
    public float GroundDrag => groundDrag;
    public float AirAccel => airAccel;
    public float AirDrag => airDrag;
    public float JumpHeight => jumpHeight;
    public float JumpCalcGravity => jumpCalcGravity;

    [Header("Dodge Move")]
    [SerializeField] float dodgeSpeed = 12f;
    [SerializeField] float dodgeStartTime = 0.125f;
    [SerializeField] float dodgeEndTime = 0.125f;
    [SerializeField] float dodgeDuration = 0.5f;

    public float DodgeSpeed => dodgeSpeed;
    public float DodgeStartTime => dodgeStartTime;
    public float DodgeEndTime => dodgeEndTime;
    public float DodgeDuration => dodgeDuration;

}
