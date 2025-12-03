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
        Tags { "RenderPipeline"="UniversalRenderPipeline" "RenderType"="Opaque" }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }

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
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 screenPos  : TEXCOORD0;   // <-- proper screen position
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // normal object → clip transform
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);

                // Unity macro that handles projection flips
                OUT.screenPos = ComputeScreenPos(OUT.positionCS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // proper screen UV (handles Metal / Vulkan flip)
                float2 uv = IN.screenPos.xy / IN.screenPos.w;

                // grayscale sample
                float gray = SAMPLE_TEXTURE2D(_RenderTexture, sampler_RenderTexture, uv).r;

                // fade
                float lerped = lerp(gray, 0.0, (1.0 - _Fade));

                // palette lookup
                float3 color = SAMPLE_TEXTURE2D(_Palette, sampler_Palette, float2(lerped, lerped)).rgb;

                return half4(color, 1.0);
            }

            ENDHLSL
        }
    }
}


