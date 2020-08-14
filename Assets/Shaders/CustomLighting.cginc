struct CustomSurfaceOutput{
	fixed3 Albedo;
	fixed3 SpecCol;
	fixed3 Normal;
	fixed3 Emission;
	fixed Hardness;
	fixed Alpha;
	fixed Roughness;
    fixed Occlusion;
};

half3 LambertDiffuse (UnityLight light, half3 normal) {
    half3 diff = saturate(dot(light.dir, normal)) * light.color;
    return diff;
}

half3 BlinnPhongSpecular (UnityLight light, half3 viewDir, half3 normal, half hardness) {
    half3 halfVec = normalize(light.dir + viewDir);
    half3 spec = pow(saturate(dot(normal, halfVec)), hardness * 128.0) * light.color;
    return spec;
}

inline half4 LightingCustomLambert (CustomSurfaceOutput s, half3 viewDir, UnityGI gi) {     //viewdir is unused but without it, the deferred version doesn't compile...
	s.Normal = normalize(s.Normal);
	half3 diff = LambertDiffuse(gi.light, s.Normal) + (gi.indirect.diffuse * s.Occlusion);
	half4 c;
	c.rgb = (diff * s.Albedo);
	c.a = s.Alpha;
	return c;
}

inline half4 LightingCustomLambert_Deferred (CustomSurfaceOutput s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2) {
    UnityStandardData data;
    data.diffuseColor   = s.Albedo;
    data.occlusion      = s.Occlusion;
    data.specularColor  = 0;
    data.smoothness     = 0;
    data.normalWorld    = s.Normal;

    UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

    half4 emission = half4(s.Emission, 1);

    #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
        emission.rgb += s.Albedo * gi.indirect.diffuse * s.Occlusion;
    #endif

    return emission;
}

inline void LightingCustomLambert_GI (CustomSurfaceOutput s, UnityGIInput data, inout UnityGI gi) {
    gi = UnityGlobalIllumination(data, 1.0, s.Normal);
}

inline half4 LightingCustomBlinnPhong (CustomSurfaceOutput s, half3 viewDir, UnityGI gi) {
	s.Normal = normalize(s.Normal);

	half3 diff = LambertDiffuse(gi.light, s.Normal) + (gi.indirect.diffuse * s.Occlusion);
	half3 spec = BlinnPhongSpecular(gi.light, viewDir, s.Normal, s.Hardness) + gi.indirect.specular;

	half4 c;
	c.rgb = ((diff * s.Albedo) + (spec * s.SpecCol));
	c.a = s.Alpha;
	return c;
}

inline half4 LightingCustomBlinnPhong_Deferred (CustomSurfaceOutput s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2) {
    UnityStandardData data;
    data.diffuseColor   = s.Albedo;
    data.occlusion      = s.Occlusion;
    data.specularColor  = s.SpecCol.rgb;
    data.smoothness     = 1.0 - s.Hardness;
    data.normalWorld    = s.Normal;

    UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

    half4 emission = half4(s.Emission, 1);

    #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
        emission.rgb += s.Albedo * gi.indirect.diffuse * s.Occlusion;
    #endif

    return emission;
}

inline void LightingCustomBlinnPhong_GI (CustomSurfaceOutput s, UnityGIInput data, inout UnityGI gi) {
    gi = UnityGlobalIllumination(data, 1.0, s.Normal);
}

//this won't get a deferred variant because then everything deferred would have to be rendered like this
inline half4 LightingCustomOrenNayer (CustomSurfaceOutput s, half3 viewDir, inout UnityGI gi) {
    half3 lightDir = gi.light.dir;
    s.Normal = normalize(s.Normal);

	half nDotL = dot(s.Normal, lightDir);
	half nDotV = dot(s.Normal, viewDir);
	half lDotV = dot(lightDir, viewDir);

	half roughSQ = s.Roughness * s.Roughness;
	half3 orenNayerFraction = roughSQ / (roughSQ + half3(0.33, 0.13, 0.09));
	half3 orenNayer = half3(1,0,0) + half3(-0.5, 0.17, 0.45) * orenNayerFraction;
	half orenNayerS = lDotV - nDotL * nDotV;
	orenNayerS /= lerp(max(nDotL, nDotV), 1, step(orenNayerS, 0));

	half3 someFactor = orenNayer.x;
	someFactor += s.Albedo * orenNayer.y;
	someFactor += orenNayer.z * orenNayerS;	

    half3 diff = (saturate(nDotL) * someFactor * gi.light.color) + gi.indirect.diffuse;

	half4 c;
    c.rgb = s.Albedo * diff;
	c.a = s.Alpha;
	return c;
}

inline void LightingCustomOrenNayer_GI (CustomSurfaceOutput s, UnityGIInput data, inout UnityGI gi) {
    gi = UnityGlobalIllumination(data, 1.0, s.Normal);
}

//almost the same, but a bit darker and i dont like that. gotta do some more experimenting
//mostly taken from https://github.com/glslify/glsl-diffuse-oren-nayar/blob/master/index.glsl

//inline half4 LightingCustomOrenNayer (CustomSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
//	lightDir = normalize(lightDir);
//	viewDir = normalize(viewDir);
//	s.Normal = normalize(s.Normal);
//
//	half lDotV = dot(lightDir, viewDir);
//	half nDotV = dot(s.Normal, viewDir);
//	half nDotL = dot(s.Normal, lightDir);
//
//	half vS = lDotV - nDotL * nDotV;
//	half vT = lerp(1.0, max(nDotL, nDotV), step(0.0, vS));
//
//	half roughSQ = s.Roughness * s.Roughness;
////	half3 vA = 1.0 + roughSQ * (s.Albedo / (roughSQ + 0.13) + 0.5 / (roughSQ + 0.33));	//<- this line ruins everything...
//	half vA = 1.0 - 0.5 * roughSQ / (roughSQ + 0.33);										//<- my fix (à la wikipedia...)
//	half vB = 0.45 * roughSQ / (roughSQ + 0.09);
//	half3 orenNayerFactor = (vA + vB * vS / vT);
//
//	half4 c;
//	c.rgb = s.Albedo * saturate(nDotL) * orenNayerFactor * atten * _LightColor0.rgb;
//	c.a = s.Alpha;
//	return c;
//}
