Shader "Custom/Image Effects/Tint"{

	Properties{
		[HideInInspector] _MainTex ("Texture", 2D) = "white" {}
		_TintColor ("Tint Color", Color) = (1,1,1,1)
	}

	SubShader{

		Cull Off
		ZWrite Off
		ZTest Always

		Tags { "RenderType"="Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _TintColor;

			struct appdata{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert (appdata v){
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target{
				fixed4 tex = tex2D(_MainTex, i.uv);
				fixed4 col = tex * _TintColor;
				return col;
			}
			ENDCG
		}
	}
}
