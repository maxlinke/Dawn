using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FogImageEffect : MonoBehaviour {

    const string fogEffectShaderName = "Custom/Image Effects/FogImageEffect";

    static Material fogEffectMaterial;

    Camera cam;
    bool m_appliedEffect;
    public bool AppliedEffect => m_appliedEffect;

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
        var fogOn = RenderSettings.fog;
        var deferred = cam.actualRenderingPath == RenderingPath.DeferredShading;
        if(fogOn && deferred){
            Graphics.Blit(src, dst, fogEffectMaterial);
            m_appliedEffect = true;
        }else{
            Graphics.Blit(src, dst);
            m_appliedEffect = false;
        }
    }

}

#if UNITY_EDITOR

[CustomEditor(typeof(FogImageEffect))]
public class FogImageEffectEditor : Editor {

    public override void OnInspectorGUI () {
        DrawDefaultInspector();
        if(EditorApplication.isPlaying){
            var applied = ((FogImageEffect)target).AppliedEffect;
            EditorGUILayout.LabelField(applied ? "Active" : "Not active");
        }
    }

}

#endif
