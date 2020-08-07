using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicRBMover : MonoBehaviour {

    [SerializeField] Rigidbody rb;
    [SerializeField] float moveOffset;
    [SerializeField] float moveSpeed;

    Vector3 initPos;

    void Start () {
        rb.isKinematic = true;
        initPos = transform.position;
    }

    void FixedUpdate () {
        var angle = Mathf.Repeat(Time.fixedTime * moveSpeed, 360f);
        var newPos = initPos + new Vector3(Mathf.Sin(angle) * moveOffset, 0f, Mathf.Cos(angle) * moveOffset);
        rb.MovePosition(newPos);
    }
	
}
