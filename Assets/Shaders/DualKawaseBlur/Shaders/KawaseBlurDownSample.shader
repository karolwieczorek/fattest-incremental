Shader "KawaseBlur/KawaseBlurDownSample"
{
    Properties {
        _MainTex ("Source", 2D) = "white" {}
        _TexelSize ("Texel Size", Vector) = (1, 1, 0, 0)
    }
    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        Pass {
//            ZTest Always Cull Off ZWrite Off
            Cull Off ZWrite Off ZTest Always
            HLSLPROGRAM
            // #pragma vertex vert
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _TexelSize;
            
            half4 frag (v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                float2 offset = _TexelSize.xy / 16.0;

                half3 col = (half3)0.0h;
                
                col += tex2D(_MainTex, uv) * 0.5;
                col += tex2D(_MainTex, uv + offset * float2(-1, 1)) * 0.125;
                col += tex2D(_MainTex, uv + offset * float2(1, 1)) * 0.125;
                col += tex2D(_MainTex, uv + offset * float2(-1, -1)) * 0.125;
                col += tex2D(_MainTex, uv + offset * float2(1, -1)) * 0.125;

                return half4(col, 1.0h);
            }
            ENDHLSL
        }
    }
}
