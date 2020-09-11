using UnityEngine;

[CreateAssetMenu(menuName = "Water/Phyics Settings", fileName = "New WaterPhyicsSettings")]
public class WaterPhyicsSettings : ScriptableObject {

    const string dragNormalizerTip = "Rigidbody drag will be divided by this value to serve as a lerp value between min and max water drag";
    const string velocityNormalizerTip = "Rigidbody's velocity will be divided by this to serve as multiplier for drag";
    const string buoyancyTypeTip = "Better buoyancy requires the water surface(s) and gravity to be aligned along one of the cardinal axes (X, Y or Z)";
    const string buoyancyTip = "Multiplied with gravity strength";
    const string buoyancyDepthTip = "Depth at which buoyancy will be lerped towards 1 at the surface";

    [Header("Drag")]
    [SerializeField]                                 float minWaterDrag = 2f;
    [SerializeField]                                 float maxWaterDrag = 20f;
    [SerializeField, Tooltip(dragNormalizerTip)]     float waterDragRBDragNormalizer = 1f;
    [SerializeField, Tooltip(velocityNormalizerTip)] float waterDragRBVelocityNormalizer = 4f;

    public float MinWaterDrag => minWaterDrag;
    public float MaxWaterDrag => maxWaterDrag;
    public float WaterDragRBDragNormalizer => waterDragRBDragNormalizer;
    public float WaterDragRBVelocityNormalizer => waterDragRBVelocityNormalizer;

    [Header("Buoyancy")]
    [SerializeField, Tooltip(buoyancyTip)]      float minBuoyancy = 0f;
    [SerializeField, Tooltip(buoyancyTip)]      float standardBuoyancy = 1f;
    [SerializeField]                            float minBuoyancyRBDrag = 1f;
    [SerializeField]                            float standardBuoyancyRBDrag = 2f;
    [SerializeField, Tooltip(buoyancyTypeTip)]  bool useBetterBuoyancy = true;
    [SerializeField, Tooltip(buoyancyDepthTip)] float simpleBuoyancyNeutralizationRange = 1f;

    public bool UseBetterBuoyancy => useBetterBuoyancy;
    public float MinBuoyancy => minBuoyancy;
    public float StandardBuoyancy => standardBuoyancy;
    public float MinBuoyancyRBDrag => minBuoyancyRBDrag;
    public float StandardBuoyancyRBDrag => standardBuoyancyRBDrag;
    public float SimpleBuoyancyNeutralizationRange => simpleBuoyancyNeutralizationRange;

}