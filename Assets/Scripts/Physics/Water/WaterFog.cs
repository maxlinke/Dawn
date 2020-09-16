using UnityEngine;

[CreateAssetMenu(menuName = "Water/Fog Settings", fileName = "New WaterFog")]
public class WaterFog : ScriptableObject {

    [Header("Camera Overlay")]
    [SerializeField] Color overlayColor = Color.clear;
    [SerializeField] Color multiplyColor = Color.white;

    [Header("Fog")]
    [SerializeField] bool overrideFog = true;
    [SerializeField] FogSettings fogSettings = FogSettings.Default;

    public Color OverlayColor => overlayColor;
    public Color MultiplyColor => multiplyColor;

    public bool OverrideFog => overrideFog;
    public FogSettings FogSettings => fogSettings;
	
}