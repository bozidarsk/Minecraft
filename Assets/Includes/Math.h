#ifndef MATH_HLSL

#define MATH_HLSL

float PI = 3.1415926535897932384626433832795;
float TAU = 6.283185307179586476925286766559;
float E = 2.7182818284590452353602874713527;
float goldenRatio = 1.6180339887498948482045868343657;
float DEG2RAD = 0.01745329251994329576923690768489;
float RAD2DEG = 57.295779513082320876798154814105;

float InverseLerp(float a, float b, float x) { return (x - a) / (b - a); }

float2 RaySphere(float3 center, float radius, float3 rayOrigin, float3 rayDir) // returns float2(distance to sphere, distance inside sphere)
{
    const float a = 1;
    float3 offset = rayOrigin - center;
    float b = 2 * dot(offset, rayDir);
    float c = dot(offset, offset) - radius * radius;

    float disciminant = b * b - 4 * a * c;

    if (disciminant > 0) 
    {
        float s = sqrt(disciminant);
        float dstToSphereNear = max(0, (-b - s) / (2 * a));
        float dstToShpereFar = (-b + s) / (2 * a);

        if (dstToShpereFar >= 0) 
        {
            return float2(dstToSphereNear, dstToShpereFar - dstToSphereNear);
        }
    }

    return float2(-1, 0);
}

float2 Rotate(float2 origin, float2 p, float angle) 
{
    float2 trig = float2(sin(angle), cos(angle));
    float x = origin.x + ((p.x - origin.x) * trig.y) + ((p.y - origin.y) * trig.x);
    float y = origin.y + ((p.x - origin.x) * trig.x) + ((p.y - origin.y) * trig.y);

    return float2(x, y);
}

float3 SpreadSpherePoints(float3 a) 
{
    a = normalize(a);

    float x2 = a.x * a.x;
    float y2 = a.y * a.y;
    float z2 = a.z * a.z;
    float x = a.x * sqrt(1 - (y2 + z2) / 2 + (y2 * z2) / 3);
    float y = a.y * sqrt(1 - (z2 + x2) / 2 + (z2 * x2) / 3);
    float z = a.z * sqrt(1 - (x2 + y2) / 2 + (x2 * y2) / 3);

    return float3(x, y, z);
}

float3 MidPoint(float3 a, float3 b) { return float3((a.x + b.x) / 2, (a.y + b.y) / 2, (a.z + b.z) / 2); }
float2 MidPoint(float2 a, float2 b) { return float2((a.x + b.x) / 2, (a.y + b.y) / 2); }
float3 MovePoint(float3 a, float3 b, float distance) { return a + (distance * normalize(b - a)); }
float2 MovePoint(float2 a, float2 b, float distance) { return a + (distance * normalize(b - a)); }
float3 MovePoint01(float3 a, float3 b, float distance) { return lerp(a, b, distance); }
float2 MovePoint01(float2 a, float2 b, float distance) { return lerp(a, b, distance); }

bool IsPrime(int num)
{
    if (num < 1) { return false; }
    if (num == 1) { return true; }
 
    for (int i = 2; i <= sqrt(num); i++) 
    {
        if (num % i == 0) { return false; }
    }
 
    return true;
}

#endif