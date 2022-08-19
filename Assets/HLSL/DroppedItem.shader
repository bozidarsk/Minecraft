Shader "Custom/DroppedItem"
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

                float speed = 2;
                float movement = 0.2;
                float rotation = 0.7;
                float2 trig = float2(sin(_Time.y * rotation), cos(_Time.y * rotation));

                v.vertex.y += sin(_Time.y * speed) * movement;
                v.vertex.xz = (v.vertex.x * float2(trig.y, trig.x)) + (v.vertex.z * float2(-trig.x, trig.y));

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

            float4 frag (v2f IN) : SV_Target
            {
                float4 color = tex2D(_MainTex, IN.uv);
                return color;
            }

            ENDCG
        }
    }
}
