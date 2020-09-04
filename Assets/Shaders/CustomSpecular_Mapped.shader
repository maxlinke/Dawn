Shader "Custom/Specular (Mapped)" {

    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _SpecTex ("Specular Color (RGB), Hardness (A)", 2D) = "grey" {}
    }
	
    SubShader {
	
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
		
        #pragma surface surf CustomBlinnPhong fullforwardshadows
        #pragma target 3.0
        #include "CustomLighting.cginc"

        fixed4 _Color;
        sampler2D _MainTex;
        sampler2D _SpecTex;

        struct Input {
            float2 uv_MainTex;
            float2 uv_SpecTex;
        };

        void surf (Input IN, inout CustomSurfaceOutput o) {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
            fixed4 s = tex2D (_SpecTex, IN.uv_SpecTex);
            o.SpecCol = s.rgb;
            o.Hardness = s.a;
        }
		
        ENDCG
    }
    FallBack "Diffuse"
}
