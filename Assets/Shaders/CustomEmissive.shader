Shader "Custom/Emissive" {

    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Emission (RGB)", 2D) = "white" {}
    }

    CustomEditor "ShaderEditors.CustomEmissiveEditor"

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
            o.Emission = c.rgb;
            o.Albedo = fixed3(0,0,0);
            o.Alpha = c.a;
        }
		
        ENDCG
    }
    FallBack "Diffuse"
}
