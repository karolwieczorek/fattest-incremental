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
//            ZTest Always Cull Off ZWrite Off
            Cull Off ZWrite Off ZTest Always

            HLSLPROGRAM
            // #pragma vertex vert
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _TexelSize; // x = 1/width, y = 1/height

            half4 frag (v2f_img i) : SV_Target
            {
                // Get the size of one pixel in UV space (texel size)
                // Equivalent to 'onePixel' in the compute shader
                float2 onePixel = _TexelSize.xy;

                // Get the UV coordinate for the current pixel center
                // Equivalent to 'uv' in the compute shader's context
                float2 uv = i.uv;

                // Define blur weights (use half precision like the original)
                const half weight = (half)(1.0 / 12.0);
                const half weight1 = (half)(1.0 / 6.0);

                // Accumulator for the final color (use half precision)
                half3 color = (half3)0.0h;

                // Sample the source texture (_MainTex) at offset locations
                // tex2D(sampler, uv) samples the texture
                // .rgb gets the color channels

                // Diagonal neighbors (higher weight)
                color += tex2D(_MainTex, uv + onePixel * float2(-1.0f, +1.0f)).rgb * weight1;
                color += tex2D(_MainTex, uv + onePixel * float2(+1.0f, +1.0f)).rgb * weight1;
                color += tex2D(_MainTex, uv + onePixel * float2(-1.0f, -1.0f)).rgb * weight1;
                color += tex2D(_MainTex, uv + onePixel * float2(+1.0f, -1.0f)).rgb * weight1;

                // Cardinal neighbors (further out, lower weight)
                color += tex2D(_MainTex, uv + onePixel * float2(+0.0f, +2.0f)).rgb * weight;
                color += tex2D(_MainTex, uv + onePixel * float2(+0.0f, -2.0f)).rgb * weight;
                color += tex2D(_MainTex, uv + onePixel * float2(-2.0f, +0.0f)).rgb * weight;
                color += tex2D(_MainTex, uv + onePixel * float2(+2.0f, +0.0f)).rgb * weight;

                // Return the final color with alpha = 1.0
                // Equivalent to '_TargetTexture[id.xy] = half4(color, 1.0f);'
                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }
}
