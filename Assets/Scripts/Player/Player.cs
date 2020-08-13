using UnityEngine;
using PlayerController;

public abstract class Player : MonoBehaviour {

    [Header("Properties")]
    [SerializeField] protected PlayerControllerProperties pcProps = default;

    [Header("GameObject Parts")]
    [SerializeField] protected Transform head = default;

    public float Height => MovementSystem.Height;
    public Vector3 Velocity => MovementSystem.Velocity;

    public Vector3 HeadPos => head.transform.position;
    public Vector3 FootPos => MovementSystem.WorldFootPos;
    public Vector3 CenterPos => MovementSystem.WorldCenterPos;

    protected abstract Movement MovementSystem { get; }
	
}
