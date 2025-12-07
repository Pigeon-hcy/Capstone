Shader "RogueNoodle/GBPaletteURP"
{
    Properties
    {
        _RenderTexture("RenderTexture", 2D) = "white" {}
        _Palette("Palette", 2D) = "white" {}
        _Fade("Fade", Range(0, 5)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite Off
            ZTest Always

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_RenderTexture);
            SAMPLER(sampler_RenderTexture);

            TEXTURE2D(_Palette);
            SAMPLER(sampler_Palette);

            float _Fade;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;   // Same as surface shader's i.uv_texcoord
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Sample grayscale from RenderTexture
                float gray = SAMPLE_TEXTURE2D(_RenderTexture, sampler_RenderTexture, IN.uv).r;

                // Fade to black (same logic)
                float lerped = lerp(gray, 0.0, 1.0 - _Fade);

                // Palette lookup using grayscale for both axes
                float2 paletteUV = float2(lerped, lerped);

                float3 col = SAMPLE_TEXTURE2D(_Palette, sampler_Palette, paletteUV).rgb;

                return half4(col, 1.0);
            }

            ENDHLSL
        }
    }
}
