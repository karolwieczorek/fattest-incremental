Shader "KawaseBlur/KawaseBlurBlend"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _BlurTex ("Blurred", 2D) = "black" {}
        _BlendRatio ("Blend Ratio", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _BlurTex;
            float _BlendRatio;

            half4 frag (v2f_img i) : SV_Target
            {
                float3 source = tex2D(_MainTex, i.uv).rgb;
                float3 blur = tex2D(_BlurTex, i.uv).rgb;
                float3 result = lerp(source, blur, _BlendRatio);
                return half4(result, 1.0h);
            }
            ENDHLSL
        }
    }
}
