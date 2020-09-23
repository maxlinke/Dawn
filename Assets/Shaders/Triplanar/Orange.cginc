fixed4 _BackgroundColor;
fixed4 _GridTint;
sampler2D _GridTex;
float _GridTexScale;
float4 _GridTexVectorScale;   

#if defined (TINTED)
fixed4 _TintColor;
// fixed4 _SpecColor;   << already declared in UnityLightingCommon.cginc
float _SpecHardness;
#endif

fixed4 GetBaseColor () {
    #if defined (BACKGROUND_ORANGE)
        return fixed4(1, 0.5, 0, 1);
    #elif defined (BACKGROUND_GREY)
        return fixed4(0.5, 0.5, 0.5, 1);
    #else
        return _BackgroundColor;
    #endif
}

#if !defined (CUSTOM_PROGRAM)
struct Input {
    float2 uv_GridTex;
    #if defined (GRIDCOORDS_TRIPLANAR_WORLD) || defined (GRIDCOORDS_TRIPLANAR_OBJECT)
        float3 gridPos;
        float3 gridNormal;
    #endif
};

void orangeVert (inout appdata_full v, out Input o) {
    UNITY_INITIALIZE_OUTPUT(Input, o);
    #if defined (GRIDCOORDS_TRIPLANAR_WORLD)
        o.gridPos = mul(unity_ObjectToWorld, v.vertex).xyz;
        o.gridNormal = UnityObjectToWorldNormal(v.normal);
    #elif defined (GRIDCOORDS_TRIPLANAR_OBJECT)
        o.gridPos = v.vertex.xyz;
        o.gridNormal = v.normal.xyz;
    #endif
}

void orangeSurf (Input IN, inout CustomSurfaceOutput o) {
    fixed4 c = GetBaseColor();
    #if defined (GRIDCOORDS_TRIPLANAR_WORLD)
        fixed4 grid = _GridTint * TriplanarSoftCutoff(_GridTex, IN.gridNormal, IN.gridPos, _GridTexScale, float3(0,0,0));
    #elif defined (GRIDCOORDS_TRIPLANAR_OBJECT)
        float3 vScale = float3(_GridTexVectorScale.x, _GridTexVectorScale.y, _GridTexVectorScale.z) * _GridTexVectorScale.w;
        fixed4 grid = _GridTint * TriplanarSoftCutoff(_GridTex, IN.gridNormal, IN.gridPos, vScale, float3(0,0,0));
    #else
        fixed4 grid = _GridTint * tex2D(_GridTex, IN.uv_GridTex);
    #endif
    c.rgb = lerp(c.rgb, grid.rgb, grid.a);
    #if defined (TINTED)
        c.rgb *= _TintColor.rgb;
        o.SpecCol = _SpecColor;
        o.Hardness = _SpecHardness;
    #endif
    o.Albedo = c.rgb;
    o.Alpha = c.a;
}
#endif