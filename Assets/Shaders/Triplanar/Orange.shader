Shader "Custom/Triplanar/Orange" {

    Properties {
        _GridTint ("Grid Tint", Color) = (1,1,1,1)
        _GridTex ("Grid Texture", 2D) = "black" {}
        _GridTexScale ("Grid Scale", Range(0, 10)) = 1.0
        _GridTexVectorScale ("Grid Scale", Vector) = (1,1,1,1)
    }

    CustomEditor "ShaderEditors.OrangeShaderEditor"
	
    SubShader {
	
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
		
        #pragma surface surf CustomLambert vertex:vert
        #pragma target 3.0
        #pragma shader_feature BACKGROUND_ORANGE
        #pragma shader_feature _ GRIDCOORDS_TRIPLANAR_WORLD GRIDCOORDS_TRIPLANAR_OBJECT
        #include "../CustomLighting.cginc"
        #include "TriplanarUtils.cginc"

        fixed4 _GridTint;
        sampler2D _GridTex;
        float _GridTexScale;
        float4 _GridTexVectorScale;

        struct Input {
            float2 uv_GridTex;
            #if defined (GRIDCOORDS_TRIPLANAR_WORLD) || defined (GRIDCOORDS_TRIPLANAR_OBJECT)
                float3 gridPos;
                float3 gridNormal;
            #endif
        };

        void vert (inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            #if defined (GRIDCOORDS_TRIPLANAR_WORLD)
                o.gridPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.gridNormal = UnityObjectToWorldNormal(v.normal);
            #elif defined (GRIDCOORDS_TRIPLANAR_OBJECT)
                o.gridPos = v.vertex.xyz;
                o.gridNormal = v.normal.xyz;
            #endif
        }

        void surf (Input IN, inout CustomSurfaceOutput o) {
            #ifdef BACKGROUND_ORANGE
                fixed4 c = fixed4(1, 0.5, 0, 1);
            #else
                fixed4 c = fixed4(0.5, 0.5, 0.5, 1);
            #endif
            #if defined (GRIDCOORDS_TRIPLANAR_WORLD)
                fixed4 grid = _GridTint * TriplanarSoftCutoff(_GridTex, IN.gridNormal, IN.gridPos, _GridTexScale, float3(0,0,0));
            #elif defined (GRIDCOORDS_TRIPLANAR_OBJECT)
                float3 vScale = float3(_GridTexVectorScale.x, _GridTexVectorScale.y, _GridTexVectorScale.z) * _GridTexVectorScale.w;
                fixed4 grid = _GridTint * TriplanarSoftCutoff(_GridTex, IN.gridNormal, IN.gridPos, vScale, float3(0,0,0));
            #else
                fixed4 grid = _GridTint * tex2D(_GridTex, IN.uv_GridTex);
            #endif
            c.rgb = lerp(c.rgb, grid.rgb, grid.a);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
		
        ENDCG
    }
    FallBack "Diffuse"
}
