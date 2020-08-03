using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomInputSystem;

public class TESTPLAYERYO : MonoBehaviour {

    [SerializeField] PlayerConfig config;
    [SerializeField] CharacterController cc;
    [SerializeField] Transform head;

    List<ControllerColliderHit> ccHits;
    float headPitch;

    Vector3 velocity;

    void Start () {
        ccHits = new List<ControllerColliderHit>();
        headPitch = 0f;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update () {
        if(Input.GetKeyDown(KeyCode.Mouse0)){
            Cursor.lockState = CursorLockMode.Locked;
        }
        // if(Cursor.lockState != CursorLockMode.Locked){
        //     return; // probably not the best move, should rather still execute all that stuff but have the inputs zeroed
        // }
        if(Input.GetKey(KeyCode.Q)){
            Time.timeScale = 0.01f;
        }else{
            Time.timeScale = 1f;
        }
        Move();
        Look();
        head.localPosition = new Vector3(0f, cc.height + config.EyeOffset, 0f);
        // don't forget to orient with gravity
    }

    void Move () {
        var localMoveInput = GetMoveInput();
        var worldMoveInput = transform.TransformDirection(localMoveInput);
        var surfacePoint = GetSurfacePoint();
        if(surfacePoint != null){
            Debug.DrawRay(surfacePoint.point, surfacePoint.normal * 0.1f, Color.blue, 0f, false);
            velocity += GetGroundAcceleration(worldMoveInput, surfacePoint.normal) * Time.deltaTime;
            if(Bind.JUMP.GetKeyDown()){
                velocity += transform.up * Mathf.Sqrt(2f * config.JumpCalcGravity * config.JumpHeight);     // TODO instead of adding, set the "local velocity y". feels more gamey than this
            }
        }else{
            velocity += GetAirAcceleration(worldMoveInput) * Time.deltaTime;    // stops me from freefall
        }
        Debug.DrawRay(transform.TransformPoint(cc.center), velocity.normalized, Color.green, 0f, false);
        velocity += Physics.gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }

    Vector3 GetGroundAcceleration (Vector3 worldMoveInput, Vector3 groundNormal) {
        return ClampedDeltaVAcceleration(velocity, GroundMoveVector(worldMoveInput, groundNormal) * config.MoveSpeed, config.MoveAccel);
    }

    Vector3 GetAirAcceleration (Vector3 worldMoveInput) {
        // TODO drag ~ lack of input (less here than on ground)
        return ClampedDeltaVAcceleration(Vector3.ProjectOnPlane(velocity, transform.up), worldMoveInput * config.AirSpeed, config.AirAccel * worldMoveInput.magnitude);
    }

    Vector3 GetMoveInput () {
        var move = Bind.MOVE_FWD.GetValue() - Bind.MOVE_BWD.GetValue();
        var strafe = Bind.MOVE_RIGHT.GetValue() - Bind.MOVE_LEFT.GetValue();
        var localMoveDir = new Vector3(strafe, 0, move);
        if(localMoveDir.sqrMagnitude > 1){
            localMoveDir = localMoveDir.normalized;
        }
        return localMoveDir;
    }

    void Look () {
        var viewDelta = GetViewDelta() * 60f * Time.deltaTime;
        headPitch = Mathf.Clamp(headPitch + viewDelta.y, -90f, 90f);
        head.localRotation = Quaternion.Euler(headPitch, 0f, 0f);
        transform.Rotate(new Vector3(0f, viewDelta.x, 0f), Space.Self);
    }

    Vector2 GetViewDelta () {
        var dx = Bind.LOOK_RIGHT.GetValue() - Bind.LOOK_LEFT.GetValue();
        var dy = Bind.LOOK_DOWN.GetValue() - Bind.LOOK_UP.GetValue();
        return new Vector2(dx, dy);
    }

    void OnControllerColliderHit (ControllerColliderHit hit) {
        ccHits.Add(hit);
        if(Vector3.Dot(velocity.normalized, hit.normal) > 0){
            velocity = Vector3.ProjectOnPlane(velocity, hit.normal);    // does this get me stuck when jumping into the floating quad platform?
        }
    }

    ControllerColliderHit GetSurfacePoint () {
        ControllerColliderHit output = null;
        float maxDot = Mathf.NegativeInfinity;
        foreach(var hit in ccHits){
            if(hit.collider == null){   // TODO use the UOBject destroyed flag of whatever
                continue;               // thing got destroyed before this call came
            }
            var dot = Vector3.Dot(hit.normal, transform.up);
            if(dot > maxDot){
                maxDot = dot;
                output = hit;
            }
        }
        ccHits.Clear();
        return output;
    }

    Vector3 GroundMoveVector (Vector3 worldMoveInput, Vector3 groundNormal) {
        return ProjectOnPlaneAlongVector(worldMoveInput, groundNormal, transform.up);
    }

    Vector3 ProjectOnPlaneAlongVector (Vector3 vector, Vector3 normal, Vector3 projectVector) {
        float x = Vector3.Dot(normal, vector) / Vector3.Dot(normal, projectVector);
        return (vector - (x * projectVector));
    }

    Vector3 ClampedDeltaVAcceleration (Vector3 currentVelocity, Vector3 targetVelocity, float maxAcceleration) {
        var dV = targetVelocity - currentVelocity;
        var dVAccel = dV / Time.deltaTime;
        if(dVAccel.sqrMagnitude > (maxAcceleration * maxAcceleration)){
            return dV.normalized * maxAcceleration;
        }
        return dVAccel;
    }
	
}
