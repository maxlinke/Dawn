using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CCTest : MonoBehaviour {

    [SerializeField] CharacterController cc = default;

    int c = 0;

    void Update () {
        if(Input.GetKeyDown(KeyCode.Q)){
            var r = cc.Move(Vector3.down);
            Debug.Log(r.ToStringImproved() + "\n\n" + c);
        }else if(Input.GetKeyDown(KeyCode.E)){
            var r = cc.Move(Vector3.up);
            Debug.Log(r.ToStringImproved() + "\n\n" + c);
        }
        c = 0;
    }

    void OnControllerColliderHit (ControllerColliderHit hit) {
        Debug.DrawRay(hit.point, hit.normal, Color.magenta, 4f, false);
        c++;
    }
	
}
