Shader "Skybox/8-Bit Skybox"
{
    Properties
    {
        _Tint ("Tint Color", Color) = (0.5, 0.5, 0.5, 1)
        [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
        _Rotation ("Rotation", Range(0, 360)) = 0
        [NoScaleOffset] _Tex ("Cubemap (HDR)", Cube) = "grey" {}
        _PixelSize ("Pixel Size", Range(1, 256)) = 8
        _ColorDepth ("Color Depth", Range(2, 32)) = 8
        _DitherStrength ("Dither Strength", Range(0, 1)) = 0.5
        _Grayscale ("Grayscale", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            samplerCUBE _Tex;
            half4 _Tex_HDR;
            half4 _Tint;
            half _Exposure;
            float _Rotation;
            float _PixelSize;
            float _ColorDepth;
            float _DitherStrength;
            float _Grayscale;

            struct appdata_t
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 texcoord : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // 8x8 Bayer matrix for ordered dithering (GBA-style)
            float BayerMatrix8x8(float2 screenPos)
            {
                // 8x8 Bayer matrix values (0-63 normalized to 0-1)
                const float bayerMatrix[64] = {
                     0, 32,  8, 40,  2, 34, 10, 42,
                    48, 16, 56, 24, 50, 18, 58, 26,
                    12, 44,  4, 36, 14, 46,  6, 38,
                    60, 28, 52, 20, 62, 30, 54, 22,
                     3, 35, 11, 43,  1, 33,  9, 41,
                    51, 19, 59, 27, 49, 17, 57, 25,
                    15, 47,  7, 39, 13, 45,  5, 37,
                    63, 31, 55, 23, 61, 29, 53, 21
                };
                
                int2 pos = int2(screenPos.x, screenPos.y) % 8;
                int index = pos.y * 8 + pos.x;
                return bayerMatrix[index] / 64.0;
            }
            
            float3 RotateAroundYInDegrees(float3 vertex, float degrees)
            {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float3(mul(m, vertex.xz), vertex.y).xzy;
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);
                o.vertex = UnityObjectToClipPos(rotated);
                o.texcoord = v.vertex.xyz;
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // Sample the cubemap
                float3 texcoord = i.texcoord;
                
                // 8-bit pixelation effect
                texcoord = round(texcoord * _PixelSize) / _PixelSize;
                
                half4 tex = texCUBE(_Tex, texcoord);
                half3 c = DecodeHDR(tex, _Tex_HDR);
                c *= _Exposure;
                
                // Convert to grayscale (using luminance formula for better perception)
                float luminance = dot(c, float3(0.299, 0.587, 0.114));
                c = lerp(c, float3(luminance, luminance, luminance), _Grayscale);
                
                // Apply tint color (for recoloring the grayscale image)
                c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
                
                // Get screen position for dithering
                float2 screenPos = i.screenPos.xy / i.screenPos.w * _ScreenParams.xy;
                
                // Apply dithering before quantization (GBA-style)
                float ditherValue = BayerMatrix8x8(screenPos);
                float ditherOffset = (ditherValue - 0.5) * _DitherStrength / _ColorDepth;
                c += ditherOffset;
                
                // 8-bit color quantization (reduce color depth)
                c = floor(c * _ColorDepth) / _ColorDepth;
                
                // Clamp to valid range
                c = saturate(c);
                
                return half4(c, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}
