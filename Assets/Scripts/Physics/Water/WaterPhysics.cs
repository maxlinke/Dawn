using UnityEngine;

[CreateAssetMenu(menuName = "Water/Physics Settings", fileName = "New WaterPhyicsSettings")]
public class WaterPhysics : ScriptableObject {

    const string viscosityTip = "Analogous to Rigidbody drag, applied to the velocities of all Rigidbodies in the water.";
    const string buoyancyTypeTip = "Simple buoyancy is meant to be cheaper to compute but less \"accurate\" than the alternative.";

    const float objectDragCoefficient = 0.47f;

    public const float DEFAULT_DENSITY = 1f;
    public const float DEFAULT_VISCOSITY = 1f;

    public const float DEFAULT_BUOYANCY_LIMIT = 20f;
    public const bool DEFAULT_SIMPLE_BUOYANCY = false;
    public const float DEFAULT_SIMPLE_BUOYANCY_NEUTRALIZATION_RANGE = 1f;

    public const float DEFAULT_GRAVITY = 9.81f;
    public const float DEFAULT_AIR_DENSITY = 1.27f;
    public const float DEFAULT_FIXED_DELTA_TIME = 0.02f;

    [Header("Properties")]
    [SerializeField, Unit("g/cm³")]                   float m_density = DEFAULT_DENSITY;
    [SerializeField, Unit(""), Tooltip(viscosityTip)] float m_viscosity = DEFAULT_VISCOSITY;

    [Header("Buoyancy")]
    [SerializeField]                            float m_buoyancyLimit = DEFAULT_BUOYANCY_LIMIT;
    [SerializeField, Tooltip(buoyancyTypeTip)]  bool  m_useSimpleBuoyancy = DEFAULT_SIMPLE_BUOYANCY;
    [SerializeField] float m_lowerBuoyancyNeutralizationRange = DEFAULT_SIMPLE_BUOYANCY_NEUTRALIZATION_RANGE;
    [SerializeField] float m_upperBuoyancyNeutralizationRange = DEFAULT_SIMPLE_BUOYANCY_NEUTRALIZATION_RANGE;

    [Header("Object Density Calculation (Edit at your own risk)")]
    [SerializeField, Unit("m/s²")]  float m_densityCalcGravity = DEFAULT_GRAVITY;
    [SerializeField, Unit("kg/m³")] float m_densityCalcAirDensity = DEFAULT_AIR_DENSITY;
    [SerializeField, Unit("s")]     float m_densityCalcFixedDeltaTime = DEFAULT_FIXED_DELTA_TIME;

    public float Density => m_density;
    public float Viscosity => m_viscosity;

    public float BuoyancyLimit => m_buoyancyLimit;
    public bool UseSimpleBuoyancy => m_useSimpleBuoyancy;
    public float LowerBuoyancyNeutralizationRange => m_lowerBuoyancyNeutralizationRange;
    public float UpperBuoyancyNeutralizationRange => m_upperBuoyancyNeutralizationRange;

    public float UnclampedBuoyancyFromDensity (float density) {
        return Mathf.Sqrt(m_density / density);
    }

    public float BuoyancyFromDensity (float density) {
        return Mathf.Min(m_buoyancyLimit, Mathf.Sqrt(m_density / density));
    }

    public float ApproxDensity (Rigidbody rb) {
        return ApproxDensity(rb.mass, rb.drag, m_densityCalcGravity, m_densityCalcAirDensity, m_densityCalcFixedDeltaTime);
    }

    public float ApproxDensity (float mass, float drag) {
        return ApproxDensity(mass, drag, m_densityCalcGravity, m_densityCalcAirDensity, m_densityCalcFixedDeltaTime);
    }

    public float GetApproxDensityNormalizer () {
        return GetApproxDensityNormalizer(m_densityCalcGravity, m_densityCalcAirDensity);
    }

    public float FastApproxDensity (Rigidbody rb) {
        return FastApproxDensity(rb.mass, rb.drag, m_densityCalcFixedDeltaTime);
    }

    public float FastApproxDensity (float mass, float drag) {
        return FastApproxDensity(mass, drag, m_densityCalcFixedDeltaTime);
    }

    ///<summary> Approximates a density in g/cm³ given a Rigidbody's mass and drag.
    /// Additional paramers are the gravity, air density and fixed timestep used to calculate the drag.
    /// The Rigidbody is assumed to be a sphere with a drag coefficient of 0.47.
    /// Note that the fixedDeltaTime parameter is not necessarily the same as the current Time.fixedDeltaTime!
    /// <param name="mass">The mass of the Rigidbody</param>
    /// <param name="drag">The drag of the Rigidbody</param>
    /// <param name="gravity">The gravity used to calculate the Rigidbody's drag</param>
    /// <param name="airDensity">The airDensity used to calculate the Rigidbody's drag</param>
    /// <param name="fixedDeltaTime">The fixedDeltaTime used to calculate the Rigidbody's drag</param>
    ///</summary>
    public static float ApproxDensity (float mass, float drag, float gravity = DEFAULT_GRAVITY, float airDensity = DEFAULT_AIR_DENSITY, float fixedDeltaTime = DEFAULT_FIXED_DELTA_TIME) {
        if(drag == 0f){
            return float.PositiveInfinity;
        }
        var d = (1f / drag) - fixedDeltaTime;
        var area = (2f * mass) / (gravity * airDensity * objectDragCoefficient * d * d);
        var radius = Mathf.Sqrt(area / Mathf.PI);
        var volume = (4f / 3f) * Mathf.PI * radius * radius * radius;
        var density = mass / volume;
        return density / 1000f;
    }

    /// <summary> Gives the normalizer needed to make a proper density in g/cm³ from the fast approximation.
    /// Simply multiply it with the result of the fast approximation.
    /// <param name="gravity">The gravity used to calculate the Rigidbody's drag</param>
    /// <param name="airDensity">The airDensity used to calculate the Rigidbody's drag</param>
    /// </summary>
    public static float GetApproxDensityNormalizer (float gravity, float airDensity) {
        var dynConsts = Mathf.Sqrt(2f / (gravity * airDensity * objectDragCoefficient));
        var statConsts = 4f / (3f * Mathf.Sqrt(Mathf.PI));
        return 1f / (1000f * statConsts * dynConsts * dynConsts * dynConsts);
    }

    /// <summary> Approximates a density that will need to be normalized to have a real meaning.
    /// Note that the fixedDeltaTime parameter is not necessarily the same as the current Time.fixedDeltaTime!
    /// <param name="mass">The mass of the Rigidbody</param>
    /// <param name="drag">The drag of the Rigidbody</param>
    /// <param name="fixedDeltaTime">The fixedDeltaTime used to calculate the Rigidbody's drag</param>
    ///</summary>
    public static float FastApproxDensity (float mass, float drag, float fixedDeltaTime) {
        if(drag == 0f){
            return float.PositiveInfinity;
        }
        var d = (1f / drag) - fixedDeltaTime;
        var radius = Mathf.Sqrt(mass / (d * d));
        return mass / (radius * radius * radius);
    }

}