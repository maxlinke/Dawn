Shader "Custom/Defaults/Diffuse (Normalmap)" {

    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}

        [Normal] _BumpMap ("Normal Map", 2D) = "bump" {}

        [Toggle(_EMISSIVE)] _Emissive ("Enable Emission", Int) = 0
        [HDR] _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _EmissionTex ("Emission Color (RGB)", 2D) = "white" {}

        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Culling", Int) = 2
        [Enum(Off, 0, On, 1)] _ZWrite ("ZWrite", Int) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Int) = 4
    }

    CustomEditor "ShaderEditors.DefaultCustomLMEditor"

    SubShader {
	
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        Cull [_Cull]
        ZWrite [_ZWrite]
        ZTest [_ZTest]

        CGPROGRAM
		
        #pragma surface surf CustomLambert fullforwardshadows
        #pragma shader_feature _EMISSIVE
        #pragma target 3.0
        #include "../CustomLighting.cginc"
        #define _NORMALMAP
        #include "CustomLightingDefaults.cginc"
		
        ENDCG
    }
    FallBack "Diffuse"
}
