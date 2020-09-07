using UnityEngine;
using PlayerController;

public abstract class Player : MonoBehaviour {

    [Header("Properties")]
    [SerializeField] protected PlayerControllerProperties pcProps = default;
    [SerializeField] protected PlayerHealthSettings healthSettings = default;

    [Header("GameObject Parts")]
    [SerializeField] protected Transform head = default;
    [SerializeField] protected Transform modelParent = default;
    [SerializeField] protected Camera fpCamera = default;

    public static Player Instance { get; private set; }

    public float Height => MovementSystem.LocalColliderHeight * transform.lossyScale.Average();
    public Vector3 Velocity => MovementSystem.Velocity;

    public Vector3 HeadPos => head.transform.position;
    public Vector3 CenterPos => MovementSystem.WorldCenterPos;

    protected abstract Movement MovementSystem { get; }

    protected bool IsValidSingleton () {
        if(Instance != null){
            Debug.LogError($"Singleton violation, instance of {nameof(Player)} is not null!");
            Destroy(this.gameObject);
            return false;
        }
        Instance = this;
        return true;
    }

    protected virtual void OnDestroy () {
        if(Instance == this){
            Instance = null;
        }
    }

    protected void InitCamera () {
        fpCamera.orthographic = false;
        fpCamera.nearClipPlane = pcProps.NearClipDist;
        fpCamera.farClipPlane = pcProps.FarClipDist;
        fpCamera.cullingMask &= ~LayerMaskUtils.LayerToBitMask(Layer.PlayerControllerAndWorldModel);
    }
	
}
