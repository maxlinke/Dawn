Shader "Custom/Diffuse (Normalmap)" {

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

        [Enum(UnityEngine.Rendering.BlendMode)] _BlendSrc ("Blend mode Source", Int) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendDst ("Blend mode Destination", Int) = 0
    }

    CustomEditor "ShaderEditors.DefaultCustomLMEditor"

    SubShader {
	
        Tags { "RenderType"="Opaque" }
        LOD 200

        Cull [_Cull]
        ZWrite [_ZWrite]
        ZTest [_ZTest]

        Blend [_BlendSrc] [_BlendDst]

        CGPROGRAM
		
        #pragma surface surf CustomLambert fullforwardshadows
        #pragma shader_feature _EMISSIVE
        #pragma target 3.0
        #include "CustomLighting.cginc"

        fixed4 _Color;
        sampler2D _MainTex;
        sampler2D _BumpMap;
        fixed4 _EmissionColor;
        sampler2D _EmissionTex;

        struct Input {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            #if defined(_EMISSIVE)
                float2 uv_EmissionTex;
            #endif
        };

        void surf (Input IN, inout CustomSurfaceOutput o) {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
            #if defined(_EMISSIVE)
                fixed4 e = tex2D (_EmissionTex, IN.uv_EmissionTex) * _EmissionColor;
                o.Emission = e.rgb;
            #endif
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
        }
		
        ENDCG
    }
    FallBack "Diffuse"
}
