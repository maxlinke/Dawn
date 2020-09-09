Shader "Custom/Defaults/AlphaTest/Diffuse (Normalmap)" {

    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _AlphaCutoff ("Cutoff", Range(0, 1)) = 0.5

        [Normal] _BumpMap ("Normal Map", 2D) = "bump" {}

        [Toggle(_EMISSIVE)] _Emissive ("Enable Emission", Int) = 0
        [HDR] _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _EmissionTex ("Emission Color (RGB)", 2D) = "white" {}

        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Culling", Int) = 2
        [Enum(Off, 0, On, 1)] _ZWrite ("ZWrite", Int) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Int) = 4

        [Enum(UnityEngine.Rendering.BlendMode)] _BlendSrc ("Blend mode Source", Int) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendDst ("Blend mode Destination", Int) = 0
    }

    CustomEditor "ShaderEditors.DefaultCustomLMEditor"
	
    SubShader {
	
        Tags { "RenderType"="Opaque" "Queue" = "AlphaTest" }
        LOD 200

        Cull [_Cull]
        ZWrite [_ZWrite]
        ZTest [_ZTest]

        Blend [_BlendSrc] [_BlendDst]

        CGPROGRAM
		
        #pragma surface surf CustomLambert fullforwardshadows alphatest:_AlphaCutoff addshadow
        #pragma shader_feature _EMISSIVE
        #pragma target 3.0
        #include "../../CustomLighting.cginc"
        #define _NORMALMAP
        #include "../CustomLightingDefaults.cginc"        
		
        ENDCG
    }
    FallBack "Diffuse"
}
