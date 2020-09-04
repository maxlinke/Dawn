using UnityEngine;
using PlayerController;
using CustomInputSystem;

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
        fpCamera.cullingMask &= ~LayerMaskUtils.LayerToBitMask(Layer.PlayerControllerAndWorldModel);
    }

    protected Vector3 GetHorizontalLocalSpaceMoveVector () {
        float move = Bind.MOVE_FWD.GetValue() - Bind.MOVE_BWD.GetValue();
        float strafe = Bind.MOVE_RIGHT.GetValue() - Bind.MOVE_LEFT.GetValue();
        var output = new Vector3(strafe, 0, move);
        if(output.sqrMagnitude > 1){
            return output.normalized;
        }
        return output;
    }

    protected Vector3 GetVerticalLocalSpaceMoveVector () {
        var up = Bind.JUMP.GetValue();
        var down = Mathf.Max(Bind.CROUCH_HOLD.GetValue(), Bind.CROUCH_TOGGLE.GetValue());
        var output = new Vector3(0f, up - down, 0f);
        if(output.sqrMagnitude > 1){
            return output.normalized;
        }
        return output;
    }
	
}
