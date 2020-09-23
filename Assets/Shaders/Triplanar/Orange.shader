﻿Shader "Custom/Triplanar/Orange" {

    Properties {
        _BackgroundColor ("Background Color", Color) = (1,1,1,1)
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
		
        #pragma surface orangeSurf CustomLambert vertex:orangeVert
        #pragma target 3.0
        #pragma shader_feature _ BACKGROUND_ORANGE BACKGROUND_GREY
        #pragma shader_feature _ GRIDCOORDS_TRIPLANAR_WORLD GRIDCOORDS_TRIPLANAR_OBJECT
        #include "../CustomLighting.cginc"
        #include "TriplanarUtils.cginc"
        #include "Orange.cginc"
		
        ENDCG
    }
    FallBack "Diffuse"
}
