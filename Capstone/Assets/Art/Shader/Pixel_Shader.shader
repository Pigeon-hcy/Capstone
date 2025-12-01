Shader "Unlit/Pixel_Shader"
{
   Properties {
       _resolution("resolution", Int) = 128
       _ColorSteps("Color Steps", Int) = 8
       _Brightness("Brightness", Range(0.5, 2.0)) = 1.2
       _Saturation("Saturation", Range(0.0, 2.0)) = 1.3
       _Contrast("Contrast", Range(0.5, 2.0)) = 1.1
       _Gamma("Gamma", Range(0.5, 2.0)) = 1.0
    }
    SubShader {
        Tags { "RenderPipeline"="UniversalPipeline" }
        
        ZWrite Off
        Cull Off
        ZTest Always
        
        Pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            int _resolution;
            int _ColorSteps;
            float _Brightness;
            float _Saturation;
            float _Contrast;
            float _Gamma;
            CBUFFER_END

            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);
            
            struct MeshData {
                uint vertexID : SV_VertexID;
            };
            
            struct Interpolators {
                float4 posCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Interpolators vert (MeshData v) {
                Interpolators o;
                o.posCS = GetFullScreenTriangleVertexPosition(v.vertexID);
                o.uv    = GetFullScreenTriangleTexCoord   (v.vertexID);
                return o;
            }
            
            float4 frag (Interpolators i) : SV_Target {
                float2 uv = i.uv;
                float3 color = 0;

                float aspect = _ScreenParams.x / _ScreenParams.y;
                uv.x *= aspect;
                
                uv = floor(uv * _resolution) / _resolution;

                uv.x /= aspect;
                
                // 采样原始颜色
                float3 originalColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv).rgb;
                
                // 亮度增强
                color = originalColor * _Brightness;
                
                // 对比度增强
                color = (color - 0.5) * _Contrast + 0.5;
                
                // 饱和度增强
                float luminance = dot(color, float3(0.299, 0.587, 0.114));
                color = lerp(float3(luminance, luminance, luminance), color, _Saturation);
                
                // Gamma 校正
                color = pow(saturate(color), 1.0 / _Gamma);
                
                // 颜色量化：将相近的颜色合并成一个颜色
                // 将颜色值量化到指定的步数，相近的颜色会被合并
                color = floor(color * _ColorSteps) / _ColorSteps;
                
                return float4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
