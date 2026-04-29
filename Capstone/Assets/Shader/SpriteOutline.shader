Shader "Custom/SpriteOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        _OutlineThickness ("Outline Thickness", Range(0, 10)) = 1
        _OutlineEnabled ("Outline Enabled", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _OutlineColor;
                float _OutlineThickness;
                float _OutlineEnabled;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 col = mainTex * input.color;

                if (_OutlineEnabled > 0.5 && _OutlineThickness > 0)
                {
                    float2 texelSize = _MainTex_TexelSize.xy * _OutlineThickness;

                    // 8-directional sampling
                    half alphaU  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(0, texelSize.y)).a;
                    half alphaD  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(0, -texelSize.y)).a;
                    half alphaL  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(-texelSize.x, 0)).a;
                    half alphaR  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(texelSize.x, 0)).a;
                    half alphaUL = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(-texelSize.x, texelSize.y)).a;
                    half alphaUR = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(texelSize.x, texelSize.y)).a;
                    half alphaDL = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(-texelSize.x, -texelSize.y)).a;
                    half alphaDR = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(texelSize.x, -texelSize.y)).a;

                    half maxNeighborAlpha = max(max(max(alphaU, alphaD), max(alphaL, alphaR)),
                                                max(max(alphaUL, alphaUR), max(alphaDL, alphaDR)));

                    // Current pixel is transparent but a neighbor is opaque → outline pixel
                    if (mainTex.a < 0.1 && maxNeighborAlpha > 0.1)
                    {
                        col = half4(_OutlineColor.rgb * _OutlineColor.a, _OutlineColor.a);
                    }
                }

                col.rgb *= col.a;
                return col;
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
