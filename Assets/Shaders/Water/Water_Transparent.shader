Shader "Custom/Water (Transparent)" {

    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}

        [Toggle(_EMISSIVE)] _Emissive ("Enable Emission", Int) = 0
        [HDR] _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _EmissionTex ("Emission Color (RGB)", 2D) = "white" {}

        _FlowTex ("Flow Map", 2D) = "grey" {}
        _FlowSpeed ("Flow Speed", Float) = 1
        _FlowDistortion ("Flow Distortion", Float) = 1
        [Toggle(_OFFSET_VERTICES)] _OffsetVertices ("Offset Vertices", Int) = 0
        _VertexOffsetDir ("Vertex Offset Vector", Vector) = (0,1,0,1)

        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Culling", Int) = 2
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Int) = 4

        [Enum(UnityEngine.Rendering.BlendMode)] _BlendSrc ("Blend mode Source", Int) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendDst ("Blend mode Destination", Int) = 10
    }

    CustomEditor "ShaderEditors.CustomWaterEditor"
	
    SubShader {
	
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        Cull [_Cull]
        ZWrite Off
        ZTest [_ZTest]

        Blend [_BlendSrc] [_BlendDst]

        CGPROGRAM
		
        #pragma surface surf CustomLambert vertex:vert keepalpha
        #pragma shader_feature _OFFSET_VERTICES
        #pragma shader_feature _EMISSIVE
        #pragma target 3.0
        #include "../CustomLighting.cginc"
        #include "CustomWater.cginc"        
		
        ENDCG
    }
}
