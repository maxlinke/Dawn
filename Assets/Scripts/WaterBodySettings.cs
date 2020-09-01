using UnityEngine;

[CreateAssetMenu(menuName = "Water Body Settings", fileName = "New WaterBodySettings")]
public class WaterBodySettings : ScriptableObject {

    const string dragNormalizerTip = "Rigidbody drag will be divided by this value to serve as a lerp value between min and max water drag";
    const string velocityNormalizerTip = "Rigidbody's velocity will be divided by this to serve as multiplier for drag";
    const string buoyancyTip = "Multiplied with gravity strength";
    const string buoyancyDepthTip = "Depth at which buoyancy will be lerped towards 1 at the surface";

    [Header("Fog")]
    [SerializeField] FogSettings fogSettings = FogSettings.Default;

    public FogSettings FogSettings => fogSettings;

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
    [SerializeField, Tooltip(buoyancyDepthTip)] float buoyancyNeutralizationDepth = 1f;

    public float MinBuoyancy => minBuoyancy;
    public float StandardBuoyancy => standardBuoyancy;
    public float MinBuoyancyRBDrag => minBuoyancyRBDrag;
    public float StandardBuoyancyRBDrag => standardBuoyancyRBDrag;
    public float BuoyancyNeutralizationDepth => buoyancyNeutralizationDepth;

}