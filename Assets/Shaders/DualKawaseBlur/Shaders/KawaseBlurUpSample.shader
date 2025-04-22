Shader "KawaseBlur/KawaseBlurUpSample"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _TexelSize ("Texel Size", Vector) = (1,1,0,0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _TexelSize; // x = 1/width, y = 1/height

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                // return tex2D(_MainTex, uv);
                float2 offset = _TexelSize.xy;
                // float2 offset = float2(0.01, 0.01);
                // float2 offset = float2(0.002083333, 0.003333333);

                const float weight = 1.0 / 12.0;
                const float weight1 = 1.0 / 6.0;

                float3 col = 0;
                col += tex2D(_MainTex, uv + offset * float2(-1.0f, +1.0f)).rgb * weight1;
                col += tex2D(_MainTex, uv + offset * float2(+1.0f, +1.0f)).rgb * weight1;
                col += tex2D(_MainTex, uv + offset * float2(-1.0f, -1.0f)).rgb * weight1;
                col += tex2D(_MainTex, uv + offset * float2(+1.0f, -1.0f)).rgb * weight1;

                col += tex2D(_MainTex, uv + offset * float2(0.0f, +2.0f)).rgb * weight;
                col += tex2D(_MainTex, uv + offset * float2(0.0f, -2.0f)).rgb * weight;
                col += tex2D(_MainTex, uv + offset * float2(-2.0f, 0.0f)).rgb * weight;
                col += tex2D(_MainTex, uv + offset * float2(+2.0f, 0.0f)).rgb * weight;

                return fixed4(col, 1.0f);
            }
            ENDHLSL
        }
    }
}
