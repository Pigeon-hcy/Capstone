Shader "RogueNoodle/GBPaletteURP"
{
    Properties
    {
        _RenderTexture("RenderTexture", 2D) = "white" {}
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
                // 采样原始颜色
                half4 color = SAMPLE_TEXTURE2D(_RenderTexture, sampler_RenderTexture, IN.uv);

                // fade 效果（应用到完整颜色）
                color.rgb = lerp(color.rgb, float3(0.0, 0.0, 0.0), (1.0 - _Fade));

                return color;
            }

            ENDHLSL
        }
    }
}

