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

    FogSettings airFogSettings; // TODO get from level
    bool fogOverridden;         // TODO get rid of, compare with level fog settings
    bool isUnderWater;

    void Awake () {
        cam = GetComponent<Camera>();
        if(waterFXShader == null){
            waterFXShader = Shader.Find(waterFXShaderName);
            overlayID = Shader.PropertyToID(OverlayPropName);
            tintID = Shader.PropertyToID(TintPropName);
        }
        waterFXMat = new Material(waterFXShader);
        waterFXMat.hideFlags = HideFlags.HideAndDontSave;
        airFogSettings = FogSettings.GetCurrent();
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
                    fogOverridden = true;
                }
            }
        }else{
            waterFXMat.SetColor(overlayID, Color.clear);
            waterFXMat.SetColor(tintID, Color.white);
            if(fogOverridden){
                airFogSettings.Apply();
                fogOverridden = false;
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
