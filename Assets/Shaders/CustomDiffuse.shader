Shader "Custom/Diffuse" {

    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }

    SubShader {
	
        Tags { "RenderType"="Opaque" }
        LOD 200

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
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
		
        ENDCG
    }
    FallBack "Diffuse"
}
