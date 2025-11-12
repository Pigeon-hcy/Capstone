Shader "Custom/GaussianBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0, 10)) = 1.0
        _BlurIntensity ("Blur Intensity", Range(0, 1)) = 1.0
        _SampleDistance ("Sample Distance", Range(0.1, 5)) = 1.0
        _Iterations ("Blur Iterations", Range(1, 5)) = 1
        _DownSample ("Down Sample", Range(1, 4)) = 1
        [Toggle] _UseHighQuality ("High Quality (13 Samples)", Float) = 0
        _Tint ("Tint Color", Color) = (1, 1, 1, 1)
        _Brightness ("Brightness", Range(0, 2)) = 1.0
        _Saturation ("Saturation", Range(0, 1)) = 1.0
        _ColorTemperature ("Color Temperature", Range(-1, 1)) = 0
        [Toggle] _EnableFog ("Enable Fog", Float) = 1
        _FogColor ("Fog Color", Color) = (0.5, 0.5, 0.5, 1)
        _FogDistance ("Fog Distance", Range(0.1, 10)) = 1.0
        _AerialPerspective ("Aerial Perspective", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        CGINCLUDE
        #include "UnityCG.cginc"
        
        sampler2D _MainTex;
        float4 _MainTex_ST;
        float4 _MainTex_TexelSize;
        float _BlurSize;
        float _BlurIntensity;
        float _SampleDistance;
        float _Iterations;
        float _DownSample;
        float _UseHighQuality;
        float4 _Tint;
        float _Brightness;
        float _Saturation;
        float _ColorTemperature;
        float _EnableFog;
        float4 _FogColor;
        float _FogDistance;
        float _AerialPerspective;
        
        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };
        
        struct v2f
        {
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
        };
        
        v2f vert(appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            return o;
        }
        
        ENDCG
        

        Pass
        {
            Name "HorizontalBlur"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float4 color = float4(0, 0, 0, 0);
                float4 originalColor = tex2D(_MainTex, uv);
                
                if (_UseHighQuality > 0.5)
                {
                    // High Quality: 13 samples
                    float weights[13] = {0.0222, 0.0378, 0.0578, 0.0792, 0.0963, 0.1075, 0.1096, 0.1075, 0.0963, 0.0792, 0.0578, 0.0378, 0.0222};
                    for (int j = 0; j < 13; j++)
                    {
                        float offset = (j - 6) * _MainTex_TexelSize.x * _BlurSize * _SampleDistance * _DownSample;
                        color += tex2D(_MainTex, uv + float2(offset, 0)) * weights[j];
                    }
                }
                else
                {
                    // Standard Quality: 9 samples
                    float weights[9] = {0.05, 0.09, 0.12, 0.15, 0.18, 0.15, 0.12, 0.09, 0.05};
                    for (int j = 0; j < 9; j++)
                    {
                        float offset = (j - 4) * _MainTex_TexelSize.x * _BlurSize * _SampleDistance * _DownSample;
                        color += tex2D(_MainTex, uv + float2(offset, 0)) * weights[j];
                    }
                }
                
                // Apply blur intensity (blend between original and blurred)
                color = lerp(originalColor, color, _BlurIntensity);
                
                // Apply tint and brightness
                color *= _Tint * _Brightness;
                
                // Apply saturation
                float luminance = dot(color.rgb, float3(0.299, 0.587, 0.114));
                float3 grayscale = float3(luminance, luminance, luminance);
                color.rgb = lerp(grayscale, color.rgb, _Saturation);
                
                // Apply color temperature
                // Positive values = warm (more red/yellow), Negative values = cool (more blue)
                color.r *= 1.0 + _ColorTemperature * 0.3;
                color.b *= 1.0 - _ColorTemperature * 0.3;
                
                // Apply fog effects (if enabled)
                if (_EnableFog > 0.5)
                {
                    // Calculate depth (distance from center)
                    float2 centerOffset = uv - float2(0.5, 0.5);
                    float depth = length(centerOffset);  // 0 at center, increases outward
                    
                    // Apply aerial perspective (darken distant objects)
                    float darkening = 1.0 - (depth * _AerialPerspective);
                    color.rgb *= darkening;
                    
                    // Apply fog effect (based on distance from center)
                    float fogAmount = saturate(depth / _FogDistance);
                    color.rgb = lerp(color.rgb, _FogColor.rgb, fogAmount);
                }
                
                return color;
            }
            ENDCG
        }
        

        Pass
        {
            Name "VerticalBlur"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float4 color = float4(0, 0, 0, 0);
                float4 originalColor = tex2D(_MainTex, uv);
                
                if (_UseHighQuality > 0.5)
                {
                    // High Quality: 13 samples
                    float weights[13] = {0.0222, 0.0378, 0.0578, 0.0792, 0.0963, 0.1075, 0.1096, 0.1075, 0.0963, 0.0792, 0.0578, 0.0378, 0.0222};
                    for (int j = 0; j < 13; j++)
                    {
                        float offset = (j - 6) * _MainTex_TexelSize.y * _BlurSize * _SampleDistance * _DownSample;
                        color += tex2D(_MainTex, uv + float2(0, offset)) * weights[j];
                    }
                }
                else
                {
                    // Standard Quality: 9 samples
                    float weights[9] = {0.05, 0.09, 0.12, 0.15, 0.18, 0.15, 0.12, 0.09, 0.05};
                    for (int j = 0; j < 9; j++)
                    {
                        float offset = (j - 4) * _MainTex_TexelSize.y * _BlurSize * _SampleDistance * _DownSample;
                        color += tex2D(_MainTex, uv + float2(0, offset)) * weights[j];
                    }
                }
                
                // Apply blur intensity (blend between original and blurred)
                color = lerp(originalColor, color, _BlurIntensity);
                
                // Apply tint and brightness
                color *= _Tint * _Brightness;
                
                // Apply saturation
                float luminance = dot(color.rgb, float3(0.299, 0.587, 0.114));
                float3 grayscale = float3(luminance, luminance, luminance);
                color.rgb = lerp(grayscale, color.rgb, _Saturation);
                
                // Apply color temperature
                // Positive values = warm (more red/yellow), Negative values = cool (more blue)
                color.r *= 1.0 + _ColorTemperature * 0.3;
                color.b *= 1.0 - _ColorTemperature * 0.3;
                
                // Apply fog effects (if enabled)
                if (_EnableFog > 0.5)
                {
                    // Calculate depth (distance from center)
                    float2 centerOffset = uv - float2(0.5, 0.5);
                    float depth = length(centerOffset);  // 0 at center, increases outward
                    
                    // Apply aerial perspective (darken distant objects)
                    float darkening = 1.0 - (depth * _AerialPerspective);
                    color.rgb *= darkening;
                    
                    // Apply fog effect (based on distance from center)
                    float fogAmount = saturate(depth / _FogDistance);
                    color.rgb = lerp(color.rgb, _FogColor.rgb, fogAmount);
                }
                
                return color;
            }
            ENDCG
        }
    }
    
    FallBack "Diffuse"
}
