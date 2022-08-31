Shader "Minecraft/Player"
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

            sampler2D _MainTex;
            float4 _LightColor0;

            float4 frag (v2f IN) : SV_Target
            {
                float4 color = tex2D(_MainTex, IN.uv);

                // return float4(saturate((dot(_WorldSpaceLightPos0.xyz, IN.normal).xxxx * 0.3) + (_LightColor0 * 0.6)).rgb, 1);

                return color;
            }

            ENDCG
        }
    }
}
