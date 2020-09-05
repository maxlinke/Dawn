Shader "Custom/Emissive" {

    Properties {
        [HDR] _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Emission (RGB)", 2D) = "white" {}

        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Culling", Int) = 2
        [Enum(Off, 0, On, 1)] _ZWrite ("ZWrite", Int) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Int) = 4

        [Enum(UnityEngine.Rendering.BlendMode)] _BlendSrc ("Blend mode Source", Int) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendDst ("Blend mode Destination", Int) = 0
    }

    CustomEditor "ShaderEditors.CustomEmissiveEditor"

    SubShader {
	
        Tags { "RenderType"="Opaque" }
        LOD 200

        Cull [_Cull]
        ZWrite [_ZWrite]
        ZTest [_ZTest]

        Blend [_BlendSrc] [_BlendDst]

        CGPROGRAM
		
        #pragma surface surf CustomLambert fullforwardshadows
        #pragma target 3.0
        #include "CustomLighting.cginc"

        fixed4 _Color;
        sampler2D _MainTex;

        struct Input {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout CustomSurfaceOutput o) {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Emission = c.rgb;
            o.Albedo = fixed3(0,0,0);
            o.Alpha = c.a;
        }
		
        ENDCG
    }
    FallBack "Diffuse"
}
