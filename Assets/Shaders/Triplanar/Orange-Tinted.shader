Shader "Custom/Triplanar/Orange (Tinted)" {

    Properties {
        _BackgroundColor ("Background Color", Color) = (1,1,1,1)
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _SpecColor ("Specular Color", Color) = (0.5,0.5,0.5,1)
        _SpecHardness ("Specular Hardness", Range(0, 1)) = 0.5
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
		
        #pragma surface orangeSurf CustomBlinnPhong vertex:orangeVert
        #pragma target 3.0
        #pragma shader_feature _ BACKGROUND_ORANGE BACKGROUND_GREY
        #pragma shader_feature _ GRIDCOORDS_TRIPLANAR_WORLD GRIDCOORDS_TRIPLANAR_OBJECT
        #define TINTED
        #include "../CustomLighting.cginc"
        #include "TriplanarUtils.cginc"
        #include "Orange.cginc"

        ENDCG
    }
    FallBack "Diffuse"
}
