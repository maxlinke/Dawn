using UnityEngine;

// [CreateAssetMenu(fileName = "Rigidbody Mass Helper", menuName = "Rigidbody Mass Helper")]
public class RigidbodyMassHelper : ScriptableObject {

    const float inf = float.PositiveInfinity;

    public enum Shape {
        Box,
        Sphere
    }

    [Header("Material")]
    // [SerializeField, RangedUnit("kg", 0f, inf)]          float mass = 1f;
    [SerializeField, RangedUnit("g/cm³", 0f, inf)]       float materialDensity = 1f;
    [SerializeField, RangedUnit("%", 0.01f, 100f, true)] float percentSolid = 100f;

    [Header("Size")]
    [SerializeField] Shape shape = Shape.Sphere;
    [SerializeField, RangedUnit("m", 0.0001f, inf)] float size = 1f;
    // [SerializeField, RangedUnit("m²", 0.0001f, inf)] float area = 3.14f;
    // [SerializeField, RangedUnit("m³", 0.0001f, inf)] float volume = 4.19f;

    public float MaterialDensity => materialDensity;
    public float PercentSolid => percentSolid;
    public Shape Shapeeeeeeee => shape;
    public float Size => size;

    public static float CalculateCircleArea (float radius) {
        return Mathf.PI * radius * radius;
    }

    public static float CalculateSphereVolume (float radius) {
        return (4f / 3f) * Mathf.PI * radius * radius * radius;
    }

    public static float CalculateSquareArea (float length) {
        return length * length;
    }

    public static float CalculateCubeVolume (float length) {
        return length * length * length;
    }

    // assuming density in g/cm³
    public static float CalculateMass (float volume, float density) {
        return density * 1000f * volume;
    }
	
}