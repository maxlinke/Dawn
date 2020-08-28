using UnityEngine;
using PlayerController;

public abstract class Player : MonoBehaviour {

    [Header("Properties")]
    [SerializeField] protected PlayerControllerProperties pcProps = default;

    [Header("GameObject Parts")]
    [SerializeField] protected Transform head = default;
    [SerializeField] protected Transform modelParent = default;
    [SerializeField] protected Camera fpCamera = default;

    public float Height => MovementSystem.LocalColliderHeight * transform.lossyScale.Average();
    public Vector3 Velocity => MovementSystem.Velocity;

    public Vector3 HeadPos => head.transform.position;
    public Vector3 CenterPos => MovementSystem.WorldCenterPos;

    protected abstract Movement MovementSystem { get; }

    protected void InitCamera () {
        fpCamera.orthographic = false;
        fpCamera.nearClipPlane = pcProps.NearClipDist;
        fpCamera.farClipPlane = pcProps.FarClipDist;
        fpCamera.cullingMask &= ~LayerMaskUtils.CreateDirectMask(Layer.PlayerControllerAndWorldModel);
    }
	
}
