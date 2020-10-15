using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CCTest : MonoBehaviour {

    [SerializeField] CharacterController cc = default;
    [SerializeField] KeyCode moveKey = default;
    [SerializeField] Vector3 moveDelta = default;

    [SerializeField] Transform other;
    [SerializeField] Rigidbody otherRB;
    [SerializeField] Vector3 otherSpeed;

    int collisions = 0;

    void Update () {
        // if(Input.GetKeyDown(moveKey)){
        //     collisions = 0;
        //     var flags = cc.Move(moveDelta);
        //     Debug.Log($"{collisions} hits\n{flags.ToStringImproved()}");
        // }
        if(Input.GetKey(moveKey)){
            if(other != null){
                other.transform.position += otherSpeed * Time.deltaTime;
            }
            cc.Move(moveDelta * Time.deltaTime);
        }
    }

    void FixedUpdate () {
        if(Input.GetKey(moveKey)){
            if(otherRB != null){
                otherRB.MovePosition(otherRB.position + (otherSpeed * Time.deltaTime));
            }
        }
    }

    void OnControllerColliderHit (ControllerColliderHit hit) {
        // collisions++;
        // Debug.DrawRay(hit.point, hit.normal, Color.magenta, 10f, false);
    }
	
}
