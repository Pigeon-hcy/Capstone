Shader "RogueNoodle/URPNaturalColor"
{
    Properties
    {
        _RenderTexture("RenderTexture", 2D) = "white" {}
        _Fade("Fade", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Overlay"
            "IsEmissive" = "true"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite Off
            ZTest Always

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_RenderTexture);
            SAMPLER(sampler_RenderTexture);

            float _Fade;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Sample original color
                float3 col = SAMPLE_TEXTURE2D(
                    _RenderTexture,
                    sampler_RenderTexture,
                    IN.uv
                ).rgb;

                // Fade to black
                col *= _Fade;

                return half4(col, 1.0);
            }

            ENDHLSL
        }
    }
}


