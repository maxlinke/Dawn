using UnityEngine;

// [CreateAssetMenu(fileName = "Rigidbody Drag Helper", menuName = "Rigidbody Drag Helper")]
public class RigidbodyDragHelper : ScriptableObject {

    [SerializeField] float drag = 0f;
    [SerializeField] float gravity = 9.81f;
    [SerializeField] float deltaTime = 0.02f;
    [SerializeField] float terminalVelocity = float.PositiveInfinity;

    public float Drag => drag;
    public float Gravity => gravity;
    public float DeltaTime => deltaTime;
    public float TerminalVelocity => terminalVelocity;

    public static float CalculateTerminalVelocity (Rigidbody rb) {
        return CalculateTerminalVelocity(rb.drag, Physics.gravity.magnitude, Time.fixedDeltaTime);
    }

    public static float CalculateTerminalVelocity (float drag, float gravity, float deltaTime) {
        if(drag == 0f){
            return float.PositiveInfinity;
        }
        var x = 1f - deltaTime * drag;
        return (gravity * x) / drag;
    }

    public static float CalculateDrag (float terminalVelocity, float gravity, float deltaTime) {
        if(float.IsInfinity(terminalVelocity)){
            return 0f;
        }
        return gravity / (terminalVelocity + gravity * deltaTime);
    }
	
}