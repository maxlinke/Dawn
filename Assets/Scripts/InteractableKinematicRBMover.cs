using UnityEngine;

public class InteractableKinematicRBMover : MonoBehaviour, IInteractable {

    [SerializeField] Rigidbody rb = default;
    [SerializeField] Vector3 moveDir = default;
    [SerializeField] float moveDist = default;
    [SerializeField] float moveSpeed = default;
    [SerializeField] bool accelerated = default;
    [SerializeField] float moveAcceleration = default;
    [SerializeField] float resetDelay = default;

    bool isMoving = false;
    Vector3 startPos;
    Vector3 velocity;
    float resetTime;

    public bool CanBeInteractedWith => !isMoving && float.IsPositiveInfinity(resetTime);
    public string InteractionDescription => "Start moving";

    public void Interact (object interactor) {
        if(!CanBeInteractedWith){
            return;
        }
        velocity = accelerated ? Vector3.zero : moveDir.normalized * moveSpeed;
        isMoving = true;
    }

    void Start () {
        if(rb == null){
            rb = GetComponent<Rigidbody>();
        }
        if(rb == null){
            Debug.LogError("No rigidbody!");
            return;
        }
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        startPos = rb.position;
        resetTime = float.PositiveInfinity;
    }

    void FixedUpdate () {
        if(!isMoving){
            if(Time.time > resetTime){
                rb.MovePosition(startPos);
                resetTime = float.PositiveInfinity;
            }
            return;
        }
        if((rb.position - startPos).magnitude >= moveDist){
            isMoving = false;
            resetTime = Time.time + resetDelay;
            return;
        }
        if(accelerated){
            velocity += moveDir.normalized * moveAcceleration * Time.fixedDeltaTime;
        }
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }
	
}
