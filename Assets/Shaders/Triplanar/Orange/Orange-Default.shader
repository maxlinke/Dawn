Shader "Custom/Triplanar/Orange (World)" {

    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "black" {}
        _TexScale ("Tex Scale", float) = 1.0
        [Toggle(COLOR_ORANGE)] _ToggleColorOrange ("Orange", Int) = 1
    }
	
    SubShader {
	
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
		
        #pragma surface surf CustomLambert 
        #pragma target 3.0
        #pragma shader_feature COLOR_ORANGE
        #include "../../CustomLighting.cginc"
        #include "../TriplanarUtils.cginc"
        #include "OrangeUtils.cginc"

        fixed4 _Color;
        sampler2D _MainTex;
        float _TexScale;

        struct Input {
            float2 uv_MainTex;
            float3 worldPos;
            float3 worldNormal;
        };

        void vert (inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			o.worldNormal = UnityObjectToWorldNormal(v.normal);
        }

        void surf (Input IN, inout CustomSurfaceOutput o) {
            fixed4 tex = _Color * TriplanarSoftCutoff(_MainTex, IN.worldNormal, IN.worldPos, _TexScale, float3(0,0,0));
            #ifdef COLOR_ORANGE
            fixed4 c = _Orange_Orange;
            #else
            fixed4 c = _Orange_Grey;
            #endif
            c.rgb = lerp(c.rgb, tex.rgb, tex.a);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
		
        ENDCG
    }
    FallBack "Diffuse"
}
