#ifndef POSTPROCESSING_HLSL

#define POSTPROCESSING_HLSL

float3 luminance(float3 color) { return dot(color, float3(0.299, 0.587, 0.114)); }
float3 Exposure(float3 color, float exposure) { return max(0, color * exposure); }
float3 Contrast(float3 color, float contrast, float brightness) { return max(0, contrast * (color - 0.5) + 0.5 + brightness); }
float3 Saturation(float3 color, float saturation) { return max(0, lerp(luminance(color), color, saturation)); }
float3 Gamma(float3 color, float gamma) { return max(0, pow(color, gamma)); }
float3 ColorFiltering(float3 color, float3 colorFiltering) { return max(0, color * colorFiltering); }

float3 Fog(float3 color, float viewDistance, float3 fogColor, float density, float offset) 
{
	float fogFactor = (density / sqrt(log(2))) * max(0, viewDistance - offset);
    fogFactor = exp2(-fogFactor * fogFactor);
    return max(0, lerp(fogColor, color, saturate(fogFactor)));
}

float3 WhiteBalance(float3 color, float temperature, float tint) 
{
	float t1 = temperature * 10.0f / 6.0f;
	float t2 = tint * 10.0f / 6.0f;

	float x = 0.31271 - t1 * (t1 < 0 ? 0.1 : 0.05);
	float standardIlluminantY = 2.87 * x - 3 * x * x - 0.27509507;
	float y = standardIlluminantY + t2 * 0.05;

	float3 w1 = float3(0.949237, 1.03542, 1.08728);

	float Y = 1;
	float X = Y * x / y;
	float Z = Y * (1 - x - y) / y;
	float L = 0.7328 * X + 0.4296 * Y - 0.1624 * Z;
	float M = -0.7036 * X + 1.6975 * Y + 0.0061 * Z;
	float S = 0.0030 * X + 0.0136 * Y + 0.9834 * Z;
	float3 w2 = float3(L, M, S);

	float3 balance = float3(w1.x / w2.x, w1.y / w2.y, w1.z / w2.z);

	float3x3 LIN_2_LMS_MAT = {
		3.90405e-1, 5.49941e-1, 8.92632e-3,
		7.08416e-2, 9.63172e-1, 1.35775e-3,
		2.31082e-2, 1.28021e-1, 9.36245e-1
	};

	float3x3 LMS_2_LIN_MAT = {
		2.85847e+0, -1.62879e+0, -2.48910e-2,
		-2.10182e-1,  1.15820e+0,  3.24281e-4,
		-4.18120e-2, -1.18169e-1,  1.06867e+0
	};

	float3 lms = mul(LIN_2_LMS_MAT, color);
	lms *= balance;

	return max(0, mul(LMS_2_LIN_MAT, lms));
}

// Hill ACES
float3 ToneMap(float3 color) 
{
	float3x3 input =
    {
        {0.59719, 0.35458, 0.04823},
        {0.07600, 0.90834, 0.01566},
        {0.02840, 0.13383, 0.83777}
    };

    float3x3 output =
    {
        { 1.60475, -0.53108, -0.07367},
        {-0.10208,  1.10813, -0.00605},
        {-0.00327, -0.07276,  1.07602}
    };

    color = mul(input, color);
    float3 a = color * (color + 0.0245786) - 0.000090537;
    float3 b = color * (0.983729 * color + 0.4329510) + 0.238081;
    return mul(output, a / b);
}

float Calculate01Depth(sampler2D depthTexture, float2 coords) { return Linear01Depth(SAMPLE_DEPTH_TEXTURE(depthTexture, coords)); }
float CalculateViewDistance(float depth) { return _ProjectionParams.z * depth; }

#endif