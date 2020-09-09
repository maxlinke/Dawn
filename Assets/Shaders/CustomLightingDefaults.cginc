fixed4 _Color;
sampler2D _MainTex;

// fixed4 _SpecColor;   << already declared in UnityLightingCommon.cginc
sampler2D _SpecTex;
float _SpecHardness;

sampler2D _BumpMap;

fixed4 _EmissionColor;
sampler2D _EmissionTex;

struct Input {
    float2 uv_MainTex;
    #if defined(_EMISSIVE)
        float2 uv_EmissionTex;
    #endif
    #if defined(_NORMALMAP)
        float2 uv_BumpMap;
    #endif
    #if defined(_SPECULARMAP)
        float2 uv_SpecTex;
    #endif
};

inline void Specular (Input IN, inout CustomSurfaceOutput o) {
    #if defined(_SPECULAR)
        fixed4 s;
        #if defined(_SPECULARMAP)
            s = tex2D (_SpecTex, IN.uv_SpecTex);
        #else
            s = fixed4(1,1,1,1);
        #endif            
        o.SpecCol = s.rgb * _SpecColor.rgb;
        o.Hardness = s.a * _SpecHardness;
    #endif
}

inline void Emission (Input IN, inout CustomSurfaceOutput o) {
    #if defined(_EMISSIVE)
        fixed4 e = tex2D (_EmissionTex, IN.uv_EmissionTex) * _EmissionColor;
        o.Emission = e.rgb;
    #endif
}

inline void Normals (Input IN, inout CustomSurfaceOutput o) {
    #if defined(_NORMALMAP)
        o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
    #endif
}

void surf (Input IN, inout CustomSurfaceOutput o) {
    fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
    o.Albedo = c.rgb;
    o.Alpha = c.a;
    Specular(IN, o);
    Emission(IN, o);
    Normals(IN, o);
}