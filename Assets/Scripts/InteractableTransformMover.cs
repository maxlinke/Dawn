using UnityEngine;

public class InteractableTransformMover : MonoBehaviour, IInteractable {

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
        startPos = transform.position;
        resetTime = float.PositiveInfinity;
    }

    void Update () {
        if(!isMoving){
            if(Time.time > resetTime){
                transform.position = startPos;
                resetTime = float.PositiveInfinity;
            }
            return;
        }
        if((transform.position - startPos).magnitude >= moveDist){
            isMoving = false;
            resetTime = Time.time + resetDelay;
            return;
        }
        if(accelerated){
            velocity += moveDir.normalized * moveAcceleration * Time.deltaTime;
        }
        transform.position += velocity * Time.deltaTime;
    }
	
}
