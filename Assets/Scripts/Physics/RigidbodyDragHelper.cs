using UnityEngine;

// [CreateAssetMenu(fileName = "Rigidbody Drag Helper", menuName = "Rigidbody Drag Helper")]
public class RigidbodyDragHelper : ScriptableObject {

    public const string u_g = "m/s²";
    public const string u_vt = "m/s";

    public const string u_d = "";
    public const string u_dt = "s";

    public const string u_m = "kg";
    public const string u_da = "kg/m³";
    public const string u_cD = "";
    public const string u_a = "m²";
    public const string u_r = "m";

    const float inf = float.PositiveInfinity;

    [Header("General")]
    [SerializeField, RangedUnit(u_g,  0, inf)] float gravity = 9.81f;
    [SerializeField, RangedUnit(u_vt, 0, inf)] float terminalVelocity = float.PositiveInfinity;

    [Header("Unity Physics")]
    [SerializeField, RangedUnit(u_d, 0, inf)]       float drag = 0f;
    [SerializeField, RangedUnit(u_dt, 0.0001f, 1f)] float deltaTime = 0.02f;

    [Header("Real Physics")]
    [SerializeField, RangedUnit(u_m, 0.0000001f, inf)] float mass = 1f;
    [SerializeField, RangedUnit(u_da, 0, inf)]       float airDensity = 1.27f;
    [SerializeField, RangedUnit(u_cD, 0, inf)]       float dragCoefficient = 0.47f;
    [SerializeField, RangedUnit(u_a, 0, inf)]        float area = 0.012f;

    public float Gravity => gravity;
    public float TerminalVelocity => terminalVelocity;

    public float Drag => drag;
    public float DeltaTime => deltaTime;

    public float Mass => mass;
    public float AirDensity => airDensity;
    public float DragCoefficient => dragCoefficient;
    public float Area => area;

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

    public static float CalculateRealTerminalVelocity (float mass, float gravity, float airDensity, float dragCoefficient, float area) {
        float denominator = airDensity * dragCoefficient * area;
        if(denominator == 0f){
            return float.PositiveInfinity;
        }
        return Mathf.Sqrt((2f * mass * gravity) / denominator);
    }

    public static float CalculateRealDragCoefficient (float mass, float gravity, float airDensity, float terminalVelocity, float area) {
        float denominator = airDensity * terminalVelocity * terminalVelocity * area;
        if(denominator == 0f){
            return float.PositiveInfinity;
        }
        return (2f * mass * gravity) / denominator;
    }

    public static float CalculateRealArea (float mass, float gravity, float airDensity, float dragCoefficient, float terminalVelocity) {
        float denominator = airDensity * terminalVelocity * terminalVelocity * dragCoefficient;
        if(denominator == 0f){
            return float.PositiveInfinity;
        }
        return (2f * mass * gravity) / denominator;
    }
	
}