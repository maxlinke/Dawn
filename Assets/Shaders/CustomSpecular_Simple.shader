Shader "Custom/Specular (Simple)" {

    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _SpecColor ("Specular Color", Color) = (0.5,0.5,0.5,1)
        _SpecHard ("Specular Hardness", Range(0, 1)) = 0.5
    }
	
    SubShader {
	
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
		
        #pragma surface surf CustomBlinnPhong fullforwardshadows
        #pragma target 3.0
        #include "CustomLighting.cginc"

        fixed4 _Color;
        // fixed4 _SpecColor;   << already declared in UnityLightingCommon.cginc
        float _SpecHard;
        sampler2D _MainTex;

        struct Input {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout CustomSurfaceOutput o) {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
            o.SpecCol = _SpecColor;
            o.Hardness = _SpecHard;
        }
		
        ENDCG
    }
    FallBack "Diffuse"
}
