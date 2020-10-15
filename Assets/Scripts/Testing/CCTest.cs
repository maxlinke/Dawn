using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CCTest : MonoBehaviour {

    [Header("CC")]
    [SerializeField] CharacterController cc = default;
    [SerializeField] KeyCode moveKey = default;
    [SerializeField] float moveSpeed = default;

    [Header("Parent Transform")]
    [SerializeField] Transform other = default;
    [SerializeField] KeyCode otherMoveKey = default;
    [SerializeField] float otherSpeed = default;

    [Header("Parent RB")]
    [SerializeField] Rigidbody otherRB = default;
    [SerializeField] KeyCode rbMoveKey = default;
    [SerializeField] float rbSpeed = default;

    void Update () {
        if(Input.GetKey(moveKey)){
            cc.Move(MoveVec() * moveSpeed * Time.deltaTime);
        }
        if(Input.GetKey(otherMoveKey)){
            if(other != null){
                other.transform.position += MoveVec () * otherSpeed * Time.deltaTime;
            }
        }
    }

    void FixedUpdate () {
        if(Input.GetKey(rbMoveKey)){
            if(otherRB != null){
                otherRB.MovePosition(otherRB.position + (MoveVec() * rbSpeed * Time.deltaTime));
            }
        }
    }

    void OnControllerColliderHit (ControllerColliderHit hit) {
        // Debug.DrawRay(hit.point, hit.normal, Color.magenta, 10f, false);
    }

    Vector3 MoveVec () {
        Vector3 output = Vector3.zero;
        if(Input.GetKey(KeyCode.W)){
            output += Vector3.forward;
        }
        if(Input.GetKey(KeyCode.S)){
            output += Vector3.back;
        }
        if(Input.GetKey(KeyCode.A)){
            output += Vector3.left;
        }
        if(Input.GetKey(KeyCode.D)){
            output += Vector3.right;
        }
        return output;
    }
	
}
