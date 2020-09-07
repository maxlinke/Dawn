using UnityEngine;

public class CameraWaterEffects : MonoBehaviour {

    const string waterFXShaderName = "Custom/Image Effects/Color Overlay and Tint";
    const string OverlayPropName = "_OverlayColor";
    const string TintPropName = "_TintColor";

    static Shader waterFXShader;
    static int overlayID;
    static int tintID;

    Camera cam;
    Material waterFXMat;

    FogSettings m_initFogSettings;
    FogSettings airFogSettings { get {
        var lvl = Level.current;
        return (lvl != null ? lvl.FogSettings : m_initFogSettings);
    } }
    bool isUnderWater;

    void Start () {
        cam = GetComponent<Camera>();
        if(waterFXShader == null){
            waterFXShader = Shader.Find(waterFXShaderName);
            overlayID = Shader.PropertyToID(OverlayPropName);
            tintID = Shader.PropertyToID(TintPropName);
        }
        waterFXMat = new Material(waterFXShader);
        waterFXMat.hideFlags = HideFlags.HideAndDontSave;
        m_initFogSettings = FogSettings.GetCurrent();
    }

    void OnPreRender () {
        isUnderWater = WaterBody.IsInAnyWaterBody(cam.transform.position, out WaterBody waterBody);
        if(isUnderWater){
            var waterFog = waterBody.Fog;
            waterFXMat.SetColor(overlayID, waterFog.OverlayColor);
            waterFXMat.SetColor(tintID, waterFog.MultiplyColor);
            if(waterFog.OverrideFog){
                if(!waterFog.FogSettings.Equals(FogSettings.GetCurrent())){
                    waterFog.FogSettings.Apply();
                }
            }
        }else{
            waterFXMat.SetColor(overlayID, Color.clear);
            waterFXMat.SetColor(tintID, Color.white);
            if(!FogSettings.GetCurrent().Equals(airFogSettings)){
                airFogSettings.Apply();
            }
        }
    }

    // TODO FXAA
    // when fxaa is implemented, only do blit(src, dst) when no other blits happened!
    void OnRenderImage (RenderTexture src, RenderTexture dst) {
        if(isUnderWater){
            Graphics.Blit(src, dst, waterFXMat);
        }else{
            Graphics.Blit(src, dst);
        }
    }
	
}
