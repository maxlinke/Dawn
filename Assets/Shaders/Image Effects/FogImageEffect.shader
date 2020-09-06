Shader "Custom/Image Effects/FogImageEffect" {

	Properties {
		[HideInInspector] _MainTex ("Texture", 2D) = "white" {}
	}

	SubShader {

		Tags { "RenderType"="Opaque" }
		LOD 100

        Cull Off
        ZTest Always
        ZWrite Off

		Pass {

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

            #pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
            sampler2D _CameraDepthTexture;
			
			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
                fixed3 col = tex2D(_MainTex, i.uv).rgb;
                #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
                    float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv));
                    float viewDist = depth * _ProjectionParams.z;
                    UNITY_CALC_FOG_FACTOR_RAW(viewDist);
                    unityFogFactor = saturate(unityFogFactor);
                    if(depth == 1){
                        unityFogFactor = 1;
                    }
                    col = lerp(unity_FogColor.rgb, col, unityFogFactor);
                #endif
                return fixed4(col, 1);
			}
			ENDCG
		}
	}
}
