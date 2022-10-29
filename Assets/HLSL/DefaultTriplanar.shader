Shader "Minecraft/DefaultTriplanar"
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
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Assets/Includes/Triplanar.h"

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

            sampler2D _MainTex;
            int _UseUvs;

            float4 frag (v2f IN) : SV_Target
            {
                float4 color = (_UseUvs) ? tex2D(_MainTex, IN.uv) : Triplanar(IN.position, IN.normal, 1, _MainTex);

                float sunLight = saturate(dot(_WorldSpaceLightPos0.xyz, IN.normal));
                sunLight = saturate(sunLight + 0.4);

                color.rgb *= sunLight;
                return saturate(color);
            }

            ENDCG
        }
    }
}
