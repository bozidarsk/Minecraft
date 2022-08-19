#ifndef TRIPLANAR_HLSL

#define TRIPLANAR_HLSL

float4 Triplanar(float3 position, float3 normal, float triplanarScale, sampler2D _tex) 
{
    float2 uvX = position.zy * triplanarScale;
    float2 uvY = position.xz * triplanarScale;
    float2 uvZ = position.xy * triplanarScale;

    float4 colX = tex2D(_tex, uvX);
    float4 colY = tex2D(_tex, uvY);
    float4 colZ = tex2D(_tex, uvZ);
    
    float3 blendWeight = normal * normal;
    blendWeight /= dot(blendWeight, 1);
    
    return colX * blendWeight.x + colY * blendWeight.y + colZ * blendWeight.z;
}

float3 TriplanarNormal(float3 position, float3 normal, float triplanarScale, float triplanarSharpness, sampler2D normalMap) 
{
    float3 normalX = UnpackNormal(tex2D(normalMap, position.zy * triplanarScale));
    float3 normalY = UnpackNormal(tex2D(normalMap, position.xz * triplanarScale));
    float3 normalZ = UnpackNormal(tex2D(normalMap, position.xy * triplanarScale));

    normalX = float3(normalX.xy + normal.zy, normalX.z * normal.x);
    normalY = float3(normalY.xy + normal.xz, normalY.z * normal.y);
    normalZ = float3(normalZ.xy + normal.xy, normalZ.z * normal.y);

    float3 weight = pow(abs(normal), triplanarSharpness);
    weight /= dot(weight, 1);

    return normalize(normalX.zyx * weight.x + normalY.xzy * weight.y + normalZ.xyz * weight.z);
}

float3 WaveOnSphere(float3 vertex, float3 normal, float waveScale, float waveSpeed, float waveFrequency) 
{
    float phi = atan2(vertex.y, vertex.x);
    float theta = acos(normal.z);
    float waveA = sin(_Time * waveSpeed + theta * waveFrequency);
    float waveB = sin(_Time * waveSpeed + phi * waveFrequency);
    float waveAmplitude = (waveA + waveB) * waveScale;
    vertex += float3(normal * waveAmplitude);
    return vertex;
}

#endif