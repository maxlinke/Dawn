using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomInputSystem;

public class FreeCam : MonoBehaviour {

    const float MIN_SPEED = 5f;
    const float MAX_SPEED = 25f;
    const float ACCEL = 250f;
    const float DRAG = 125f;

    [Header("Components")]
    [SerializeField] Rigidbody rb;
    [SerializeField] SphereCollider col;
    [SerializeField] Camera cam;

    float pitch;
    float pan;

    void Start () {
        pitch = 0f;
        pan = 0f;
    }

    void FixedUpdate () {
        col.radius = cam.nearClipPlane * 1.5f;

        var pc = pitch;
        pitch = 0f;
        ApplyEuler();
        rb.rotation = cam.transform.rotation;
        pan = 0f;
        pitch = pc;
        ApplyEuler();

        // TODO local velocity (when i do velocity volumes for the player as well)
        var dragDeceleration = ClampedDeltaVAcceleration(rb.velocity, Vector3.zero, DRAG, Time.fixedUnscaledDeltaTime);
        rb.velocity += dragDeceleration * Time.fixedUnscaledDeltaTime;

        if(CursorLockManager.CursorIsLocked()){
            var moveInput = cam.transform.TransformDirection(Bind.GetHorizontalMoveInput());
            moveInput += rb.transform.TransformDirection(Bind.GetVerticalMoveInput());
            if(moveInput.sqrMagnitude > 1){
                moveInput = moveInput.normalized;
            }
            var speed = Mathf.Lerp(MIN_SPEED, MAX_SPEED, Bind.WALK_OR_RUN.GetValue()) / Time.timeScale;
            var acceleration = ClampedDeltaVAcceleration(rb.velocity, moveInput * speed, ACCEL * moveInput.magnitude, Time.fixedUnscaledDeltaTime);
            rb.velocity += acceleration * Time.fixedUnscaledDeltaTime;
        }
    }

    void Update () {
        #if UNITY_EDITOR
            if(Input.anyKeyDown){
                CursorLockManager.UpdateLockState();
            }
        #endif
        if(!CursorLockManager.CursorIsLocked() || Time.timeScale == 0f){
            return;
        }
        var viewDelta = Bind.GetViewInput();
        viewDelta *= 60f * Time.unscaledDeltaTime;
        pitch = Mathf.Clamp(pitch + viewDelta.y, -90f, 90f);
        pan = Mathf.Repeat(pan + viewDelta.x, 360f);
        ApplyEuler();
    }

    void ApplyEuler () {
        cam.transform.localEulerAngles = new Vector3(pitch, pan, 0f);
    }

    Vector3 ClampedDeltaVAcceleration (Vector3 currentVelocity, Vector3 targetVelocity, float maxAcceleration, float timeStep) {
        var dV = targetVelocity - currentVelocity;
        var dVAccel = dV / timeStep;
        if(dVAccel.sqrMagnitude > (maxAcceleration * maxAcceleration)){
            return dV.normalized * maxAcceleration;
        }
        return dVAccel;
    }
	
}
