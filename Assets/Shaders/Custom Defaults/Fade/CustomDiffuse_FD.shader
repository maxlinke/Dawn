Shader "Custom/Defaults/Fade/Diffuse" {

    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}

        [Toggle(_EMISSIVE)] _Emissive ("Enable Emission", Int) = 0
        [HDR] _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _EmissionTex ("Emission Color (RGB)", 2D) = "white" {}

        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Culling", Int) = 2
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Int) = 4
    }

    CustomEditor "ShaderEditors.DefaultCustomLMEditor"
	
    SubShader {
	
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        LOD 200

        Cull [_Cull]
        ZWrite Off
        ZTest [_ZTest]

        CGPROGRAM
		
        #pragma surface surf CustomLambert fullforwardshadows alpha:fade
        #pragma shader_feature _EMISSIVE
        #pragma target 3.0
        #include "../../CustomLighting.cginc"
        #include "../CustomLightingDefaults.cginc"        
		
        ENDCG
    }
}
