Shader "Custom/Triplanar/Orange (UV-Overlay)" {

    Properties {
        _BackgroundColor ("Background Color", Color) = (1,1,1,1)
        _GridTint ("Grid Tint", Color) = (1,1,1,1)
        _GridTex ("Grid Texture", 2D) = "black" {}
        _GridTexScale ("Grid Scale", Range(0, 10)) = 1.0
        _OverlayTint ("Overlay Tint", Color) = (1,1,1,1)
        _OverlayTex ("Overlay Texture", 2D) = "black" {}
    }

    CustomEditor "ShaderEditors.OrangeShaderEditor"
	
    SubShader {
	
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
		
        #pragma surface surf CustomLambert
        #pragma target 3.0
        #pragma shader_feature _ BACKGROUND_ORANGE BACKGROUND_GREY
        #pragma shader_feature GRIDCOORDS_TRIPLANAR_WORLD // can't use object because i'd need additional interpolators and that's not possible with target 3.0
        #define CUSTOM_PROGRAM
        #include "../CustomLighting.cginc"
        #include "TriplanarUtils.cginc"
        #include "Orange.cginc"

        fixed4 _OverlayTint;
        sampler2D _OverlayTex;

        struct Input {
            float2 uv_GridTex;
            float2 uv_OverlayTex;
            float3 worldPos;
            float3 worldNormal;
        };

        void surf (Input IN, inout CustomSurfaceOutput o) {
            fixed4 c = GetBaseColor();
            #ifdef GRIDCOORDS_TRIPLANAR_WORLD
                fixed4 grid = _GridTint * TriplanarSoftCutoff(_GridTex, IN.worldNormal, IN.worldPos, _GridTexScale, float3(0,0,0));
            #else
                fixed4 grid = _GridTint * tex2D(_GridTex, IN.uv_GridTex);
            #endif
            fixed4 overlay = _OverlayTint * tex2D(_OverlayTex, IN.uv_OverlayTex);
            c.rgb = lerp(c.rgb, grid.rgb, grid.a);
            c.rgb = lerp(c.rgb, overlay.rgb, overlay.a);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
		
        ENDCG
    }
    FallBack "Diffuse"
}
