Shader "Custom/PostProcessing"
{
    Properties 
    {
        [HideInInspector]
        _MainTex("Texture", 2D) = "white" {}
    }

    SubShader 
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "..\Includes\PostProcessing.h"

            struct appdata 
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f 
            {
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 viewVector : TEXCOORD2;
                float4 screenPos : TEXCOORD3;
                float4 vertex : SV_POSITION;
            };

            float4 _fogColor;
            float _fogDensity;
            float _fogOffset;
            float _exposure;
            float _temperature;
            float _tint;
            float _contrast;
            float _brightness;
            float _colorFiltering;
            float _saturation;
            float _gamma;

            sampler2D _Effect;
            int _UseEffect;

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;

            v2f vert (appdata v) 
            {
                v2f o;
                o.worldPos = mul(UNITY_MATRIX_M, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1));
                o.viewVector = mul(unity_CameraToWorld, float4(viewVector, 0));
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uv = v.uv;

                return o;
            }

            float4 frag (v2f IN) : SV_Target
            {
                float3 color = tex2D(_MainTex, IN.uv).rgb;
                float4 effect = tex2D(_Effect, IN.uv);
                effect.a *= _UseEffect;

                // color = Fog(color, CalculateViewDistance(Calculate01Depth(_CameraDepthTexture, IN.uv)), _fogColor, _fogDensity, _fogOffset);
                // color = Exposure(color, _exposure);
                // color = WhiteBalance(color, _temperature, _tint);
                // color = Contrast(color, _contrast, _brightness);
                // color = ColorFiltering(color, _colorFiltering);
                // color = Saturation(color, _saturation);
                // color = ToneMap(color);
                // color = Gamma(color, _gamma);

                return float4(saturate(lerp(color, effect.rgb, effect.a)), 1);
            }
            
            ENDCG
        }
    }
}
