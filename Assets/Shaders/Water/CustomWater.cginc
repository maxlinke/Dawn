fixed4 _Color;
sampler2D _MainTex;
fixed4 _EmissionColor;
sampler2D _EmissionTex;

sampler2D _FlowTex;
float4 _FlowTex_ST;
float _FlowSpeed;
float _FlowDistortion;
float4 _VertexOffsetDir;

struct Input {
    float2 uv_MainTex;
    #if defined(_EMISSIVE)
        float2 uv_EmissionTex;
    #endif
    float2 flowUV;
};

void vert (inout appdata_full v, out Input o) {
    UNITY_INITIALIZE_OUTPUT(Input, o);
    o.flowUV = TRANSFORM_TEX(v.texcoord, _FlowTex);
    #if defined(_OFFSET_VERTICES)  
        fixed4 flowTex = tex2Dlod(_FlowTex, float4(o.flowUV, 0, 1)) * 2 * UNITY_PI;
        float t = _Time.y * _FlowSpeed;
        float f = sin(t + flowTex.r + flowTex.g + flowTex.b);
        float3 offset = f * _VertexOffsetDir.xyz * _VertexOffsetDir.w;
        v.vertex.xyz += offset;
    #endif
}

void surf (Input IN, inout CustomSurfaceOutput o) {
    fixed4 f = tex2D (_FlowTex, IN.flowUV) * 2 * UNITY_PI;
    float t = _Time.y * _FlowSpeed;
    float2 dUV = sin(t + f.xy) * _FlowDistortion;
    fixed4 c = tex2D (_MainTex, dUV + IN.uv_MainTex) * _Color;
    o.Albedo = c.rgb;
    o.Alpha = c.a;
    #if defined(_EMISSIVE)
        fixed4 e = tex2D (_EmissionTex, dUV + IN.uv_EmissionTex) * _EmissionColor;
        o.Emission = e.rgb;
    #endif
}