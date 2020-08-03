using UnityEngine;

[CreateAssetMenu(menuName = "Player Config", fileName = "New Player Config")]
public class PlayerConfig : ScriptableObject {

    [Header("General")]
    [SerializeField] float normalHeight = 1.8f;
    [SerializeField] float crouchHeight = 0.9f;
    [SerializeField] float eyeOffset = -0.15f;
    [SerializeField] float colliderRadius = 0.4f;

    public float NormalHeight => normalHeight;
    public float CrouchHeight => crouchHeight;
    public float EyeOffset => eyeOffset;
    public float ColliderRadius => colliderRadius;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 12f;
    [SerializeField] float moveAccel = 32f;
    [SerializeField] float airSpeed = 12f;
    [SerializeField] float airAccel = 32f;
    [SerializeField] float jumpHeight = 1f;
    [SerializeField] float jumpCalcGravity = 29.43f;

    public float MoveSpeed => moveSpeed;
    public float MoveAccel => moveAccel;
    public float AirSpeed => airSpeed;
    public float AirAccel => airAccel;
    public float JumpHeight => jumpHeight;
    public float JumpCalcGravity => jumpCalcGravity;
	
}
