Shader "Minecraft/Chunk"
{
    Properties
    {
        [HideInInspector]
        _MainTex("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags  
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Pass
        {
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "../Includes/Math.h"

            struct appdata 
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f 
            {
                float2 uv : TEXCOORD0;
                float3 position : TEXCOORD1;
                float3 normal : TEXCOORD2;
                float3 viewVector : TEXCOORD3;
                float4 screenPos : TEXCOORD4;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) 
            {
                v2f o;
                o.position = mul(UNITY_MATRIX_M, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1));
                o.viewVector = normalize(_WorldSpaceCameraPos - o.position);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.uv = v.uv;

                return o;
            }

            // Buffer<uint> voxels;
            uint chunkHeight;
            uint chunkSize;

            sampler2D _MainTex;
            float4 _LightColor0;

            // uint GetVoxelIndex(uint3 position) { return position.x + (position.z * chunkSize) + (position.y * chunkSize * chunkSize); }
            // uint GetVoxelType(uint3 position) { return voxels[GetVoxelIndex(position)] & 0xffff; }
            // uint GetVoxelLight(uint3 position) { return (voxels[GetVoxelIndex(position)] >> 16) & 0xf; }

            float4 frag (v2f IN) : SV_Target
            {
                float4 color = tex2D(_MainTex, IN.uv);

                if (color.a <= 0) { clip(-1); }

                // return color;
                float value = saturate(dot(_WorldSpaceLightPos0.xyz, IN.normal));
                value = saturate(value + 0.4);

                color.rgb *= value;
                // return float4(saturate((dot(_WorldSpaceLightPos0.xyz, IN.normal).xxxx * 0.3) + (color * 0.6)).rgb, color.a);

                uint3 voxelPosition = uint3(
                    floor(IN.position.x - 0.001),
                    floor(IN.position.y - 0.001),
                    floor(IN.position.z - 0.001)
                );


                // float light = (float)GetVoxelLight(voxelPosition) / 15.0;
                // color.rgb *= light;
                return saturate(color);
            }

            ENDCG
        }
    }
}
