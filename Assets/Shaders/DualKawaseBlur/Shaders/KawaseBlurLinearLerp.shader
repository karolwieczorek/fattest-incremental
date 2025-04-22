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
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _BlurTex;
            float _BlendRatio;

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
                float3 source = tex2D(_MainTex, i.uv).rgb;
                float3 blur = tex2D(_BlurTex, i.uv).rgb;
                // return fixed4(source, 1.0f);

                float3 result = lerp(source, blur, _BlendRatio);
                return fixed4(result, 1.0f);
            }
            ENDHLSL
        }
    }
}
