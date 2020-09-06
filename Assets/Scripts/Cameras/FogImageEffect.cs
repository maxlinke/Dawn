using UnityEngine;

public class FogImageEffect : MonoBehaviour {

    const string fogEffectShaderName = "Custom/Image Effects/FogImageEffect";

    static Material fogEffectMaterial;

    Camera cam;

    [ContextMenu(nameof(LogRenderingPaths))]
    void LogRenderingPaths () {
        Debug.Log("Set: " + cam.renderingPath.ToString() + "\nActual: " + cam.actualRenderingPath.ToString());
    }

    [ContextMenu(nameof(LogDepthTextureMode))]
    void LogDepthTextureMode () {
        Debug.Log(cam.depthTextureMode);
    }

    [ContextMenu(nameof(SetDepthTextureModeToDepth))]
    void SetDepthTextureModeToDepth () {
        cam.depthTextureMode = DepthTextureMode.Depth;
    }

    void Awake () {
        cam = GetComponent<Camera>();
        if(fogEffectMaterial == null){
            fogEffectMaterial = new Material(Shader.Find(fogEffectShaderName));
            fogEffectMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    [ImageEffectOpaque]
    void OnRenderImage (RenderTexture src, RenderTexture dst) {
        if(cam.actualRenderingPath == RenderingPath.DeferredShading){
            Graphics.Blit(src, dst, fogEffectMaterial);
        }else{
            Graphics.Blit(src, dst);
        }
    }

}
