using UnityEngine;

// [CreateAssetMenu(fileName = "Rigidbody Mass Helper", menuName = "Rigidbody Mass Helper")]
public class RigidbodyMassHelper : ScriptableObject {

    const float inf = float.PositiveInfinity;

    public enum Shape {
        Box,
        Sphere,
        Cylinder
    }

    [Header("Material")]
    [SerializeField, RangedUnit("g/cm³", 0f, inf)]       float materialDensity = 1f;
    [SerializeField, RangedUnit("%", 0.01f, 100f, true)] float percentSolid = 100f;

    [Header("Size")]
    [SerializeField] Shape shape = Shape.Sphere;
    [SerializeField, RangedUnit("m", 0.0001f, inf)] float diameter = 1f;
    [SerializeField, RangedUnit("m", 0.0001f, inf)] float length = 1f;

    public float MaterialDensity => materialDensity;
    public float PercentSolid => percentSolid;
    public Shape DaShape => shape;
    public float Diameter => diameter;
    public float Length => length;
	
}