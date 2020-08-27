using UnityEngine;

[CreateAssetMenu(menuName = "Player Controller Properties", fileName = "New PlayerControllerProperties")]
public class PlayerControllerProperties : ScriptableObject {

    [Header("Dimensions")]
    [SerializeField] float normalHeight = 1.8f;
    [SerializeField] float crouchHeight = 0.9f;
    [SerializeField] float crouchUncrouchTime = 0.25f;
    [SerializeField] float eyeOffset = -0.15f;
    [SerializeField] float colliderRadius = 0.4f;

    public float NormalHeight => normalHeight;
    public float CrouchHeight => crouchHeight;
    public float CrouchUncrouchTime => crouchUncrouchTime;
    public float EyeOffset => eyeOffset;
    public float ColliderRadius => colliderRadius;

    public float HeightChangeSpeed => (normalHeight - crouchHeight) / crouchUncrouchTime;

    [Header("Phyics")]
    [SerializeField] float playerMass = 80f;
    [SerializeField] CollisionDetectionMode collisionDetection = CollisionDetectionMode.ContinuousDynamic;
    [SerializeField] PhysicMaterial physicMaterial = null;
    [SerializeField] float footRBNonSolidMass = 160f;
    [SerializeField] float footRBSolidMass = 400f;
    [SerializeField] float gravityTurnDegreesPerSecond = 360f;

    public float PlayerMass => playerMass;
    public CollisionDetectionMode CollisionDetection => collisionDetection;
    public PhysicMaterial PhysicMaterial => physicMaterial;
    public float FootRBNonSolidMass => footRBNonSolidMass;
    public float FootRBSolidMass => footRBSolidMass;
    public float GravityTurnDegreesPerSecond => gravityTurnDegreesPerSecond;

    [Header("Camera")]
    [SerializeField] float nearClipDist = 0.15f;
    [SerializeField] float farClipDist = 1000f;

    public float NearClipDist => nearClipDist;
    public float FarClipDist => farClipDist;

    [Header("Movement")]
    [SerializeField] float hardSlopeLimit = 60f;
    [SerializeField] float moveSpeedRun = 12f;
    [SerializeField] float moveSpeedWalk = 6f;
    [SerializeField] float moveSpeedCrouch = 6f;
    [SerializeField] float groundAccel = 64f;
    [SerializeField] float groundDrag = 32f;
    [SerializeField] float slopeAccel = 32f;
    [SerializeField] float slopeDrag = 12f;
    [SerializeField] float airAccel = 24f;
    [SerializeField] float airDrag = 4f;
    [SerializeField] float waterAccel = 24f;
    [SerializeField] float waterDrag = 0f;
    [SerializeField] float jumpHeight = 1f;
    [SerializeField] float jumpCalcGravity = 29.43f;
	
    public float HardSlopeLimit => hardSlopeLimit;
    public float MoveSpeedRun => moveSpeedRun;
    public float MoveSpeedWalk => moveSpeedWalk;
    public float MoveSpeedCrouch => moveSpeedCrouch;
    public float GroundAccel => groundAccel;
    public float GroundDrag => groundDrag;
    public float SlopeAccel => slopeAccel;
    public float SlopeDrag => slopeDrag;
    public float AirAccel => airAccel;
    public float AirDrag => airDrag;
    public float WaterAccel => waterAccel;
    public float WaterDrag => waterDrag;
    public float JumpHeight => jumpHeight;
    public float JumpCalcGravity => jumpCalcGravity;

    public float MinAccel => Mathf.Min(groundAccel, Mathf.Min(slopeAccel, airAccel));
    public float MinDrag => Mathf.Min(groundDrag, Mathf.Min(slopeDrag, airDrag));

    [Header("Dodge Move")]
    [SerializeField] float dodgeSpeed = 12f;
    [SerializeField] float dodgeStartTime = 0.125f;
    [SerializeField] float dodgeEndTime = 0.125f;
    [SerializeField] float dodgeDuration = 0.5f;

    public float DodgeSpeed => dodgeSpeed;
    public float DodgeStartTime => dodgeStartTime;
    public float DodgeEndTime => dodgeEndTime;
    public float DodgeDuration => dodgeDuration;

    [Header("Interaction")]
    [SerializeField] float interactRange = 1.5f;

    public float InteractRange => interactRange;

}
