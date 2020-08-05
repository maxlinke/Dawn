﻿using UnityEngine;
using PlayerController;
using CustomInputSystem;

public class Player : MonoBehaviour {

    [Header("Scripty bits")]
    [SerializeField] PlayerControllerProperties pcProps = default;
    [SerializeField] Movement movementSystem = default;
    [SerializeField] View viewSystem = default;

    [Header("GameObject Parts")]
    [SerializeField] Rigidbody rb = default;
    [SerializeField] CapsuleCollider col = default;
    [SerializeField] Transform head = default;

    public float Height => movementSystem.Height;
    public Vector3 Velocity => movementSystem.Velocity;

    public Vector3 HeadPos => head.transform.position;
    public Vector3 FootPos => transform.position;
    public Vector3 CenterPos => movementSystem.WorldCenter;

    public MoveControl modeControl;
    public ViewControl viewControl;

    public Transform viewTarget;    // temp

    public enum MoveControl {
        FULL,
        BLOCK_INPUT,
        ANCHORED
    }

    public enum ViewControl {
        FULL,
        BLOCK_INPUT,
        TARGETED
    }

    // TODO fuck cc, rb is where it's at
    // first rb view
    // then simple movement
    // then build upon that (if necessary)

    // important things:
    // - crouch jump
    // - non binary slope limit (slide a bit before sliding fully)
    // - screw the mass-screwery. the player weighs what he weighs. if anything gets pushed too easily, adjust its mass.
    // - i don't like the velocityComesFromMove stuff. try to not do that.

    // TODO walk and run OR always "run" and dodge? the second sounds a lot more fun

    // TODO some way of managing the cameras. cameraregistry maybe? (CameraID enum)

    void Start () {
        viewSystem.Initialize(pcProps, this, rb, head);
        viewSystem.SetHeadOrientation(
            headTilt: 0f, 
            headPan: 0f,
            headRoll: 0f
        ); // should be deserialized or something later on
    }

    void Update () {
        #if UNITY_EDITOR
            if(Input.anyKeyDown){
                CursorLockManager.UpdateLockState();
            }
        #endif
        if((viewControl == ViewControl.FULL) && !CursorLockManager.CursorIsUnlocked()){
            viewSystem.Look(GetViewInput());
        }else if((viewControl == ViewControl.TARGETED) && viewTarget != null){
            viewSystem.LookAt(viewTarget);
        }else{
            // nothing
        }
        // i think that checking the cursor lock mode is ONE good way to see if i should gather input at all. like totally. 
        // the enums on top are valid too tho...
    }

    void FixedUpdate () {
        viewSystem.MatchRBRotationToHead();
        Vector3 moveInput;
        if(!CursorLockManager.CursorIsUnlocked()){
            moveInput = GetLocalSpaceMoveInput();
        }else{
            moveInput = Vector3.zero;
        }
    }

    Vector2 GetViewInput () {
        var dx = Bind.LOOK_RIGHT.GetValue() - Bind.LOOK_LEFT.GetValue();
        var dy = Bind.LOOK_DOWN.GetValue() - Bind.LOOK_UP.GetValue();
        return new Vector2(dx, dy);
    }

    Vector3 GetLocalSpaceMoveInput () {
        float move = Bind.MOVE_FWD.GetValue() - Bind.MOVE_BWD.GetValue();
        float strafe = Bind.MOVE_RIGHT.GetValue() - Bind.MOVE_BWD.GetValue();
        return new Vector3(strafe, 0, move);
    }
	
}