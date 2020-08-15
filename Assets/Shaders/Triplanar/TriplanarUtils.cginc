fixed4 TriplanarBlend (sampler2D texSrc, float3 blend, float3 position, float3 textureScale, float3 textureOffset) {
    fixed4 cx = tex2D(texSrc, (position.zy * textureScale.zy) + textureOffset.zy);
    fixed4 cy = tex2D(texSrc, (position.xz * textureScale.xz) + textureOffset.xz);
    fixed4 cz = tex2D(texSrc, (position.xy * textureScale.xy) + textureOffset.xy);
    return blend.x * cx + blend.y * cy + blend.z * cz;
}

fixed4 TriplanarNoCutoff (sampler2D texSrc, float3 normal, float3 position, float3 textureScale, float3 textureOffset) {
    half3 blend = abs(normal);
    blend /= dot(blend, float3(1,1,1));
    return TriplanarBlend(texSrc, blend, position, textureScale, textureOffset);
}

fixed4 TriplanarSoftCutoff (sampler2D texSrc, float3 normal, float3 position, float3 textureScale, float3 textureOffset) {
    half3 blend = normal * normal;
    blend *= blend;
    blend /= dot(blend, float3(1,1,1));
    return TriplanarBlend(texSrc, blend, position, textureScale, textureOffset);
}