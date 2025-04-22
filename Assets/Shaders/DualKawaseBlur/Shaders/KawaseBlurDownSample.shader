Shader "KawaseBlur/KawaseBlurDownSample"
{
    Properties {
        _MainTex ("Source", 2D) = "white" {}
        _TexelSize ("Texel Size", Vector) = (1, 1, 0, 0)
    }
    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        Pass {
            ZTest Always Cull Off ZWrite Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _TexelSize;

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata_base v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                // float2 uvv = float2(i.uv);
                // return tex2D(_MainTex, uvv);
                // float2 halfPixel = 0.5f * _TexelSize;
                // float2 uv = float2(i.uv) * _TexelSize + halfPixel;
                //
                // half3 col = 0;
                // col += tex2D(_MainTex, uv + float2(0.0f, 0.0f)) * 0.5;
                // col += tex2D(_MainTex, uv + float2(-1.0f,  1.0f) * halfPixel) * 0.125;
                // col += tex2D(_MainTex, uv + float2( 1.0f,  1.0f) * halfPixel) * 0.125;
                // col += tex2D(_MainTex, uv + float2(-1.0f, -1.0f) * halfPixel) * 0.125;
                // col += tex2D(_MainTex, uv + float2( 1.0f, -1.0f) * halfPixel) * 0.125;
                
                // _TargetTexture[id.xy] = float4(color, 1.0f);


                
                float2 uv = i.uv;
                // float2 offset = float2(0.01, 0.01);
                // float2 offset = float2(1 / 400, 1 / 300);
                float2 offset = _TexelSize.xy;
                
                float3 col = 0;
                col += tex2D(_MainTex, uv) * 0.5;
                col += tex2D(_MainTex, uv + offset * float2(-1, 1)) * 0.125;
                col += tex2D(_MainTex, uv + offset * float2(1, 1)) * 0.125;
                col += tex2D(_MainTex, uv + offset * float2(-1, -1)) * 0.125;
                col += tex2D(_MainTex, uv + offset * float2(1, -1)) * 0.125;

                return float4(col, 1);
            }
            ENDHLSL
        }
    }
}
