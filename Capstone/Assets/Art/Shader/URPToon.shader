Shader "Custom/URPToon"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalStength("Normal stength", Range(0,1)) = 1
       
        _Color("Color", Color) = (1,1,1,1)
        _ShadowColor("ShadowColor", Color) = (1,1,1,1)
        _shadingBands ("ShadingBandsNumber", int) = 3
        _GradientSize ("GradientSize", Range(0,1)) = 0.5
        _TestingOffset("Testing Offset", float) = 0
        _ShadowSmoothingSize("ShadowSmoothness", float) = 0

        _OutlineColor ("Outline Color", Color) = (0,0,0)
        _OutlineOpacity( "Outline Opacity",   Range(0, 1) ) = 0
        _OutlineSizeMultiplier( " Outline Size Multiplier", Range(0, 10) ) = 1



    }

    SubShader
    {
        Tags { "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector" = "True"
         }
        


        Pass
        {

            
            Tags{
                
            "Queue" = "2000"
            }

            HLSLPROGRAM



            // Force depth texture because we need it for almost every nodes
            // TODO: dependency system that triggers this define from position or view direction usage
            #define REQUIRE_DEPTH_TEXTURE
            
        
            // /* WARNING: $splice Could not find named fragment 'PassInstancing' */
             #define SHADERPASS SHADERPASS_DRAWPROCEDURAL
             #define REQUIRE_NORMAL_TEXTURE
             #define REQUIRE_DEPTH_TEXTURE
             #define REQUIRE_OPAQUE_TEXTURE
        

            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _FORWARD_PLUS
            #pragma shader_feature_fragment _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS




            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"


            

             // custom interpolator pre-include
            /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
            
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/Fullscreen/Includes/FullscreenShaderPass.cs.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
          
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GlobalSamplers.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"


            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv: TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };



            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float2 uv: TEXCOORD0;
                float3 normal : TEXCOORD1;
                float4 shadowCoords : TEXCOORD2;
                float shadowDarkness : TEXCOORD3;
                float3 positionWS : TEXCOORD4;
                float4 positionSC : TEXCOORD5;
                half3 tangent : TEXCOORD6;
                half3 bitangent : TEXCOORD7;
                float2 normalUV: TEXCOORD8;


            };

            TEXTURE2D(_OutlineTexture); 
            SAMPLER(sampler_OutlineTexture);

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap); 
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            //TEXTURE2D(_CameraDepthTexture);
            //SAMPLER(sampler_DepthMap);
            //SAMPLER(sampler_NormalMap);
            //TEXTURE2D(normTex);

            float4 _OutlineTexture_TexelSize;

            // Identifier same as the RenderPass
            #define SAMPLE_BLIT(uv) SAMPLE_TEXTURE2D( _OutlineTexture, sampler_LinearClamp, uv )
        
            

            //sampler2D _CameraDepthTexture;


            void Unity_SceneDepth_Raw_float(float4 UV, out float Out)
            {
                Out = SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy);
            }


            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float4 _ShadowColor;
            float _NormalStength;
            float4 _BaseMap_ST;
            half4 _NormalMap_ST;
            int _shadingBands;
            float _GradientSize;
            float _TestingOffset;
            float _ShadowSmoothingSize;

            float3 _OutlineColor;
            float _OutlineOpacity;
    
            float _OutlineSizeMultiplier;

            

            CBUFFER_END

            float3x3 boxBlurKernel = float3x3 (
                    // box
                    0.11, 0.11, 0.11,
                    0.11, 0.11, 0.11,
                    0.11, 0.11, 0.11
                );

            float3x3 gaussianBlurKernel = float3x3 (
                // gaussian
                0.0625, 0.125, 0.0625,
                0.1250, 0.250, 0.1250,
                0.0625, 0.125, 0.0625
            );


            float shadowConvolution(float3 positionWS, float3 normalWS, float3x3 kernel){
                    int steps = 1;

                    float3 camPos = GetCameraPositionWS();

                    float3 viewdirection = normalize(camPos - positionWS);

                    //Real quick, we need to get the tangent and bitangent that we want to move along, to sample the shadow coords.

                    float3 bitangentVector = cross( normalWS, viewdirection);
                    float3 tangentVector = cross( normalWS, bitangentVector);
                    

                   

                    //Down here we attempt convolution, but with shadows!

                    //float2 ts = _MainTex_TexelSize.xy;
                    float result = 0;
                
                    // for(int x = -1; x <= 1; x++) {
                    //     for(int y = -1; y <= 1; y++) {
                    //         //float2 offset = float2(x, y) * ts;

                    //         float3 offsetPos = x*tangentVector*_ShadowSmoothingSize + y*bitangentVector * _ShadowSmoothingSize;

                    //         //float3 sample = tex2D(_MainTex, uv + offset);
                    //         float sample = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS  )); //tex2D(_MainTex, uv + offset);
                    //         //result += sample * kernel[x+1][y+1];
                    //         result += sample * kernel[x+1][y+1];
                    //     }
                    // }

                    float p1 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(-1 *tangentVector + 1 * bitangentVector) * _ShadowSmoothingSize ) );
                    float p2 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(0 *tangentVector + 1 * bitangentVector) * _ShadowSmoothingSize  ));
                    float p3 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(1 *tangentVector + 1 * bitangentVector) * _ShadowSmoothingSize ) );
                    float p4 =  MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(-1 *tangentVector + 0 * bitangentVector) * _ShadowSmoothingSize ) );
                    float p5 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS));
                    float p6 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(1 *tangentVector + 0 * bitangentVector) * _ShadowSmoothingSize ) );
                    float p7 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(-1 *tangentVector + -1 * bitangentVector) * _ShadowSmoothingSize ) );
                    float p8 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(0 *tangentVector + -1 * bitangentVector) * _ShadowSmoothingSize  ));
                    float p9 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(1 *tangentVector + -1 * bitangentVector) * _ShadowSmoothingSize ) );

                    //result = abs( (p1+ (2*p2)+p3)-(p7+(2*p8)+p9) )+ abs( (p3+ (2*p6) +p9 )-(p1+ (2*p4) + p7) );
                    //result = (p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9)/9;
                    result = (p1 * 0.0625 + p2 * 0.125 + p3 * 0.0625 
                            + p4 * 0.1250 + p5 * 0.250 + p6 * 0.1250 
                            + p7 * 0.0625 + p8 * 0.125 + p9 * 0.0625);


                    return result;
            }


            float outlineRaycast(float2 direction, float2 screenUV){
                
            
                float outlineValue = 0;
            
                float2 ts = _OutlineTexture_TexelSize.xy;
            

                for(float i = 0; i < _OutlineSizeMultiplier; i++)
                {
                    // float2 sampleOffset =
                    //     float2 ((blurPixels / _BlitTexture_TexelSize.z) * (i / BLUR_SAMPLES_RANGE), 0);
                    // color += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord + sampleOffset).rgb;
                    
                    float scalingAmount = 1- i/_OutlineSizeMultiplier;

                    float3 sample = SAMPLE_TEXTURE2D( _OutlineTexture, sampler_PointClamp , screenUV + direction * ts * i);

                    float amountToAdd = -dot( sample, float3(1,0,0) )+dot( sample, float3(0,1,0) );

                    outlineValue += amountToAdd * scalingAmount;


                }

                outlineValue = step(0.1, outlineValue);


                return outlineValue;

            }



            float GetOutlineValue ( float2 screenUV ){
                
                float2 ts = _OutlineTexture_TexelSize.xy ;

                float baseValue = SAMPLE_TEXTURE2D( _OutlineTexture, sampler_PointClamp , screenUV );
                
                float p1 = SAMPLE_TEXTURE2D( _OutlineTexture, sampler_PointClamp , screenUV + float2(1,0)*ts*_OutlineSizeMultiplier );
                float p2 = SAMPLE_TEXTURE2D( _OutlineTexture, sampler_PointClamp , screenUV + float2(1,1) * .707 *ts*_OutlineSizeMultiplier );
                float p3 = SAMPLE_TEXTURE2D( _OutlineTexture, sampler_PointClamp , screenUV + float2(0,1)  *ts*_OutlineSizeMultiplier );
                float p4 = SAMPLE_TEXTURE2D( _OutlineTexture, sampler_PointClamp , screenUV + float2(-1,1) * .707 *ts*_OutlineSizeMultiplier );
                float p5 = SAMPLE_TEXTURE2D( _OutlineTexture, sampler_PointClamp , screenUV + float2(-1,0)*ts*_OutlineSizeMultiplier );
                float p6 = SAMPLE_TEXTURE2D( _OutlineTexture, sampler_PointClamp , screenUV + float2(-1,-1) * .707 *ts*_OutlineSizeMultiplier );
                float p7 = SAMPLE_TEXTURE2D( _OutlineTexture, sampler_PointClamp , screenUV + float2(0,-1)*ts*_OutlineSizeMultiplier );
                float p8 = SAMPLE_TEXTURE2D( _OutlineTexture, sampler_PointClamp , screenUV + float2(1,-1) * .707 *ts*_OutlineSizeMultiplier );             

                float outlineValue = saturate(baseValue + p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8);


                //We'll try a horizontal and vertical sweep!

                // float p1 = outlineRaycast( float2(1,0), screenUV );
                // float p3 = outlineRaycast( float2(-1,0), screenUV );
                // float p5 = outlineRaycast( float2(0,1), screenUV );
                // float p7 = outlineRaycast( float2(0,-1), screenUV );
                
                // float outlineValue = saturate(baseValue + p1 + p3 + p5 + p7);

                






                return outlineValue;
            
            
            }


            float shadowGaussianConvolution(float3 positionWS, float3 normalWS, float3x3 kernel){
                    int steps = 1;

                    float3 camPos = GetCameraPositionWS();

                    Light mainLight = GetMainLight();

                    float3 lightDirection = mainLight.direction;


                    float3 viewDirection = normalize(camPos - positionWS);

                    //Real quick, we need to get the tangent and bitangent that we want to move along, to sample the shadow coords.

                    float3 bitangentVector = cross( normalWS, lightDirection);
                    float3 tangentVector = cross( normalWS, bitangentVector);
                    

                   

                    //Down here we attempt convolution, but with shadows!

                    //float2 ts = _MainTex_TexelSize.xy;
                    float result = 0;
                
                    // for(int x = -1; x <= 1; x++) {
                    //     for(int y = -1; y <= 1; y++) {
                    //         //float2 offset = float2(x, y) * ts;

                    //         float3 offsetPos = x*tangentVector*_ShadowSmoothingSize + y*bitangentVector * _ShadowSmoothingSize;

                    //         //float3 sample = tex2D(_MainTex, uv + offset);
                    //         float sample = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS  )); //tex2D(_MainTex, uv + offset);
                    //         //result += sample * kernel[x+1][y+1];
                    //         result += sample * kernel[x+1][y+1];
                    //     }
                    // }

                    
                    
            //         float3x3 gaussianBlurKernel = float3x3 (
            //     // gaussian
            //     0.0625, 0.125, 0.0625,
            //     0.1250, 0.250, 0.1250,
            //     0.0625, 0.125, 0.0625
            // );


                    float p1 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(-1 *tangentVector + 1 * bitangentVector) * _ShadowSmoothingSize ) ) * 0.0625;
                    float p2 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(0 *tangentVector + 1 * bitangentVector) * _ShadowSmoothingSize  )) * 0.1250;
                    float p3 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(1 *tangentVector + 1 * bitangentVector) * _ShadowSmoothingSize ) ) * 0.0625;
                    float p4 =  MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(-1 *tangentVector + 0 * bitangentVector) * _ShadowSmoothingSize ) ) * 0.1250 ;
                    float p5 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS)) * 0.250;
                    float p6 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(1 *tangentVector + 0 * bitangentVector) * _ShadowSmoothingSize ) ) * 0.1250;
                    float p7 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(-1 *tangentVector + -1 * bitangentVector) * _ShadowSmoothingSize ) ) * 0.0625;
                    float p8 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(0 *tangentVector + -1 * bitangentVector) * _ShadowSmoothingSize  ) ) * 0.1250;
                    float p9 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(1 *tangentVector + -1 * bitangentVector) * _ShadowSmoothingSize ) ) * 0.0625;

                    //result = abs( (p1+ (2*p2)+p3)-(p7+(2*p8)+p9) )+ abs( (p3+ (2*p6) +p9 )-(p1+ (2*p4) + p7) );
                    result = (p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9);


                    return result;
            }

            float shadowSteppedGaussianConvolution(float3 positionWS, float3 normalWS, float3 tangentWS){
                    float s = 1;
                    float l = .3 ;

                    float3 camPos = GetCameraPositionWS();

                    Light mainLight = GetMainLight();

                    float3 lightDirection = mainLight.direction;


                    float3 viewDirection = normalize(camPos - positionWS);

                    //Real quick, we need to get the tangent and bitangent that we want to move along, to sample the shadow coords.

                    float3 bitangentVector = cross( normalWS, lightDirection);
                    float3 tangentVector = cross( normalWS, bitangentVector);
                    //float3 tangentVector = tangentWS;
                    

                    //float3 bitangentVector = tangentWS;
                    //float3 tangentVector = cross( normalWS, bitangentVector);
                    //float3 tangentVector = cross( normalWS, bitangentVector);
                    

                   

                    //Down here we attempt convolution, but with shadows!

                    //float2 ts = _MainTex_TexelSize.xy;
                    float result = 0;
                
                    // for(int x = -1; x <= 1; x++) {
                    //     for(int y = -1; y <= 1; y++) {
                    //         //float2 offset = float2(x, y) * ts;

                    //         float3 offsetPos = x*tangentVector*_ShadowSmoothingSize + y*bitangentVector * _ShadowSmoothingSize;

                    //         //float3 sample = tex2D(_MainTex, uv + offset);
                    //         float sample = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS  )); //tex2D(_MainTex, uv + offset);
                    //         //result += sample * kernel[x+1][y+1];
                    //         result += sample * kernel[x+1][y+1];
                    //     }
                    // }

                    
                    
            //         float3x3 gaussianBlurKernel = float3x3 (
            //     // gaussian
            //     0.0625, 0.125, 0.0625,
            //     0.1250, 0.250, 0.1250,
            //     0.0625, 0.125, 0.0625
            // );


                    // float p1 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(-1 *tangentVector + 1 * bitangentVector) * _ShadowSmoothingSize ) ) * 0.0625;
                    // float p2 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(0 *tangentVector + 1 * bitangentVector) * _ShadowSmoothingSize  )) * 0.1250;
                    // float p3 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(1 *tangentVector + 1 * bitangentVector) * _ShadowSmoothingSize ) ) * 0.0625;
                    // float p4 =  MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(-1 *tangentVector + 0 * bitangentVector) * _ShadowSmoothingSize ) ) * 0.1250 ;
                    // float p5 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS)) * 0.250;
                    // float p6 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(1 *tangentVector + 0 * bitangentVector) * _ShadowSmoothingSize ) ) * 0.1250;
                    // float p7 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(-1 *tangentVector + -1 * bitangentVector) * _ShadowSmoothingSize ) ) * 0.0625;
                    // float p8 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(0 *tangentVector + -1 * bitangentVector) * _ShadowSmoothingSize  ) ) * 0.1250;
                    // float p9 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(1 *tangentVector + -1 * bitangentVector) * _ShadowSmoothingSize ) ) * 0.0625;
                    
                    float calculatedSmoothingSize = 0.05;

                    float p1 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(-1 *tangentVector + 1 * bitangentVector) * calculatedSmoothingSize ) );
                    float p2 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(0 *tangentVector + 1 * bitangentVector) * calculatedSmoothingSize  ));
                    float p3 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(1 *tangentVector + 1 * bitangentVector) * calculatedSmoothingSize ) );
                    float p4 =  MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(-1 *tangentVector + 0 * bitangentVector) * calculatedSmoothingSize ) );
                    float p5 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS));
                    float p6 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(1 *tangentVector + 0 * bitangentVector) * calculatedSmoothingSize ) );
                    float p7 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(-1 *tangentVector + -1 * bitangentVector) * calculatedSmoothingSize ) );
                    float p8 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(0 *tangentVector + -1 * bitangentVector) * calculatedSmoothingSize  ) );
                    float p9 = MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS + normalize(1 *tangentVector + -1 * bitangentVector) * calculatedSmoothingSize ) );

                    p1 = smoothstep(l, s, p1);
                    p2 = smoothstep(l, s, p2);
                    p3 = smoothstep(l, s, p3);
                    p4 = smoothstep(l, s, p4);
                    p5 = smoothstep(l, s, p5);
                    p6 = smoothstep(l, s, p6);
                    p7 = smoothstep(l, s, p7);
                    p8 = smoothstep(l, s, p8);
                    p9 = smoothstep(l, s, p9);

                    //result = abs( (p1+ (2*p2)+p3)-(p7+(2*p8)+p9) )+ abs( (p3+ (2*p6) +p9 )-(p1+ (2*p4) + p7) );
                    //result = (p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9);
                    result = (p1 * 0.0625 + p2 * 0.125 + p3 * 0.0625 
                             + p4 * 0.1250 + p5 * 0.250 + p6 * 0.1250 
                             + p7 * 0.0625 + p8 * 0.125 + p9 * 0.0625);


                    //
                    //result = step( 0.8 , result);

                    return result;
                    //return MainLightRealtimeShadow(TransformWorldToShadowCoord( positionWS));
                    //return GetMainLightShadowFade(positionWS);
            }



            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.normalUV = TRANSFORM_TEX(IN.uv, _NormalMap);

                // Get the VertexPositionInputs for the vertex position  
                VertexPositionInputs positions = GetVertexPositionInputs(IN.positionOS.xyz);

                // Convert the vertex position to a position on the shadow map
                float4 shadowCoordinates = TransformWorldToShadowCoord(positions.positionWS.xyz);

                // Pass the shadow coordinates to the fragment shader
                OUT.shadowCoords = shadowCoordinates;

                

                OUT.positionWS = positions.positionWS.xyz;
                
                
                OUT.normal = normalize(mul(IN.normal.xyz, (float3x3)unity_WorldToObject));
                OUT.tangent = normalize(mul(IN.tangent.xyz, (float3x3)unity_WorldToObject));;
                OUT.bitangent = cross(OUT.normal, OUT.tangent) * IN.tangent.w;
                


                //OUT.normal = IN.normal;
                //OUT.normal = normalize(mul(IN.normal.xyz, (float3x3)unity_WorldToObject));
                //OUT.normal = mul(unity_ObjectToWorld, IN.normal) - IN.positionOS ; //UnityObjectToWorldNormal(IN.normal);
                //OUT.normal = normalize(OUT.normal);

               float3 worldPos = mul( (float3x3)unity_ObjectToWorld, ( IN.positionOS ) ) ;

                OUT.positionSC = ComputeScreenPos( TransformObjectToHClip(IN.positionOS.xyz)  );


                OUT.shadowDarkness = shadowConvolution(positions.positionWS.xyz, OUT.normal, gaussianBlurKernel);



                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 texel = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                // return texel * _Color;

                float2 screenUV = IN.positionSC.xy / IN.positionSC.w;
                //float depth = tex2D(_CameraDepthTexture, screenUV);
                
                //depth /= _ProjectionParams.w;

                //float4 depthNormals =  float4( SHADERGRAPH_SAMPLE_SCENE_NORMAL(screenUV)  , Linear01Depth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(screenUV), _ZBufferParams) );

                //float depth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_BaseMap, screenUV);
                //float depth = SampleSceneDepth(IN.uv); //_CameraDepthTexture.sample();
                //float depth = SampleSceneDepth(IN.uv); //_CameraDepthTexture.sample();
               
                //depth /= _ProjectionParams.w;


                // void Unity_SceneDepth_Raw_float(float4 UV, out float Out)
                //  {
                //     Out = SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV.xy);
                //  } 

                //depth = Linear01Depth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(screenUV), _ZBufferParams);
                //depth = (SHADERGRAPH_SAMPLE_SCENE_DEPTH(IN.uv));


                //_CameraDepthTexture.sample();

                //float depth = tex2D(_CameraDepthTexture, screenUV);
                //float depth = depthNormals.w; 
                //float depth = depthNormals.w; 



                float3 color;


                
                
                //normal = normalize(normal);

                //float3 normal = NormalsRenderingShared(_NormalMap, tangentSpaceNormal, IN.tangent.xyz, IN.bitangent.xyz, IN.normal.xyz);



                float3 normal = normalize(IN.normal);
                
                Light mainLight = GetMainLight();

                
               // half3 LightingLambert(half3 lightColor, half3 lightDirection, half3 surfaceNormal);

                



                float3 lightdirection = mainLight.direction;
                float3 lightcolor = mainLight.color; // includes intensity

               // float3 viewdirection = normalize(_worldspacecamerapos.xyz - in.posworld);
                //float3 halfdirection = normalize(viewdirection + lightdirection);

                //half shadowAmount = MainLightRealtimeShadow(IN.shadowCoords);
                half shadowAmount = shadowSteppedGaussianConvolution( IN.positionWS, IN.normal, IN.tangent);


                float ndotl = (dot(normal, lightdirection)+1)/2;
                
                ndotl = min(shadowAmount, ndotl);


                //float diffusefalloff = round( ndotl* (_shadingBands-1) )/(_shadingBands-1);
                float diffusefalloff = floor( ndotl* (_shadingBands) )/(_shadingBands-1);

                //float distanceFromNearestEdge = abs((ndotl-diffusefalloff));
               
                //float amountToOffset = 

                //float diffusefalloffOffset = round( (ndotl + .5  )  * (_shadingBands-1) )/(_shadingBands-1);
                float diffusefalloffOffset = floor( ndotl* (_shadingBands) + .5 )/(_shadingBands-1);

               
                //float distanceFromNearestEdge = ((ndotl+.5-diffusefalloffOffset) * (_shadingBands-1)+.5);
                float distanceFromNearestEdge = (ndotl*(_shadingBands)-floor( ndotl*(_shadingBands) +.5 ) +.5 );  //(x*4-floor(x*4+1/2)+1/2)


                float percentFromNearestEdge = smoothstep( .5 - _GradientSize/2, .5 + _GradientSize/2 , distanceFromNearestEdge);

                float gradientMask = 1-step( _GradientSize/2, abs(distanceFromNearestEdge -.5) );

                float minGradientFalloff = saturate( floor( ndotl* (_shadingBands) - .5)/(_shadingBands-1));

                float maxGradientFalloff = saturate(floor( ndotl* (_shadingBands) + .5)/(_shadingBands-1));

                float gradientFalloff = lerp( minGradientFalloff , maxGradientFalloff  , percentFromNearestEdge );
                 





                //Normal map calculation is down here!

                

                float3 tangentSpaceNormal = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, IN.normalUV));
                //float3 tangentSpaceNormal = ( (SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, IN.normalUV) ) );


                float normalStrength = _NormalStength * _NormalStength * 50;

                //float3 tangentSpaceNormal = UnpackNormal(tex2D(_normalMap, uv));
                tangentSpaceNormal = normalize(lerp(float3(0, 0, 1), tangentSpaceNormal, normalStrength ));// _normalIntensity));
                
                float3x3 tangentToWorld = float3x3 
                (
                    IN.tangent.x, IN.bitangent.x, IN.normal.x,
                    IN.tangent.y, IN.bitangent.y, IN.normal.y,
                    IN.tangent.z, IN.bitangent.z, IN.normal.z
                );




                normal = mul(tangentToWorld, tangentSpaceNormal);


                
                float replaceWithNormalLighting = 1-dot( float3( 0,0,1 ) , tangentSpaceNormal );

                float normalBias = normalStrength/100;

                replaceWithNormalLighting = 1-step(replaceWithNormalLighting,normalBias);

                float normalMapFalloff = (dot(normal, lightdirection)+1)/2 * replaceWithNormalLighting; 
                //float normalMapFalloff = (dot(normal, lightdirection)+1)/2; 
                //float normalMapFalloff = (dot(normal, lightdirection)); 

                normalMapFalloff *= shadowAmount;






                //float3 cellColor = 
                
                float falloffPlusGradients = (1-gradientMask) * diffusefalloff + gradientMask * gradientFalloff;
                //float falloffPlusGradients =  gradientMask * gradientFalloff;

                falloffPlusGradients = lerp( falloffPlusGradients, normalMapFalloff, replaceWithNormalLighting  );

                //falloffPlusGradients += normalMapFalloff;
                //falloffPlusGradients = saturate(falloffPlusGradients);


                //float specularfalloff = max(0, dot(normal, halfdirection));
                //specularfalloff = pow(specularfalloff, _gloss * max_specular_power/ _extraspecularmultiplier  + 0.0001) * _gloss;

                //specularfalloff = floor(specularfalloff * _specularlightsteps)/_specularlightsteps;


                float3 diffuse =  lerp(  _ShadowColor , _Color , falloffPlusGradients) * lightcolor * texel.rgb; // * _surfacecolor;
                //float3 specular = specularfalloff * lightcolor;

                color = diffuse;// + specular + _ambientcolor;







                //half ndotl = (dot(IN.normal, lightdir)+1)/2;
                // //half ndotl = (dot(s.normal, lightdir));
                // //ndotl = //tex2d(_ramptex, fixed2(ndotl, 0.5));
                // ndotl = clamp(ndotl, 0, 1);
        
                // half4 c;

                // float shadeamount = lerp( ndotl, ndotl * atten , ndotl );

                // //c.rgb =  lerp( ndotl, ndotl * atten , ndotl ); //s.albedo * _lightcolor0.rgb * ndotl * atten;
                // //c.rgb = (ndotl * atten);

                // float currentcell = round( shadeamount * _shadingbands )/_shadingbands;

                // float3 calculatedcolor = lerp( _shadowcolor, _color, currentcell );

                // c.rgb = calculatedcolor * _lightcolor0 * s.albedo * 1000; 
                // //c.rgb = s.albedo * 1000;

        
                // c.a = s.alpha;
                // return c;
                
                

                //return float4(1,1,1, 1);
                //return float4(falloffPlusGradients.rrr, 1);
                //return float4(gradientMask.rrr, 1);
                //return float4(gradientFalloff.rrr * gradientMask, 1);
                //return float4(percentFromNearestEdge.rrr * gradientMask, 1);
                //return float4(distanceFromNearestEdge.rrr, 1);
                //return float4(diffusefalloff.rrr, 1);
                //return float4(diffusefalloffOffset.rrr, 1);
                //return float4(color, 1);
                //return float4(depth.rrr, 1);

                //float3 normalTest = SHADERGRAPH_SAMPLE_SCENE_NORMAL( (IN.positionCS.xy * 0.5 + IN.positionCS.w * 0.5).xy ).xyz;
                //float3 normalTest = SHADERGRAPH_SAMPLE_SCENE_NORMAL( GetNormalizedScreenSpaceUV(IN.positionCS.xy) ).xyz;
                
                
                //float3 normalTest = SHADERGRAPH_SAMPLE_SCENE_NORMAL( screenUV ).xyz;
                

                //float3 normalTest = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture,  IN.positionCS.xy);
                
                
                float3 normalTest = SampleSceneNormals(screenUV);



                //float depthTest = Linear01Depth( SHADERGRAPH_SAMPLE_SCENE_DEPTH( GetNormalizedScreenSpaceUV(IN.positionCS.xy) ), _ZBufferParams );
                float depthTest = ( SHADERGRAPH_SAMPLE_SCENE_DEPTH( GetNormalizedScreenSpaceUV(IN.positionCS.xy) )  );

                //return float4( normalTest , 1 );
                //return float4( shadowConvolution( IN.positionWS, IN.normal, boxBlurKernel).rrr, 1);



                float3 rawOutline = SAMPLE_TEXTURE2D( _OutlineTexture, sampler_PointClamp , screenUV );
                
                float outlineMask = GetOutlineValue(screenUV); // SAMPLE_TEXTURE2D( _OutlineTexture, sampler_PointClamp , screenUV );

              

                //float4 outline = SAMPLE_BLIT( screenUV.xy );

                //float4 outline = tex2D( _OutlineTexture, screenUV.xy );
                float3 coloredOutlines = outlineMask *  lerp( color, _OutlineColor, _OutlineOpacity) ;


                //return float4( coloredOutlines.xyz , 1);
                
                return float4( coloredOutlines + color * (1-outlineMask) , 1);
                
                //return float4( tangentSpaceNormal.rgb, 1);
                //return float4( normalMapFalloff.rrr, 1);


            }
            ENDHLSL
        }

        // Pass
        // {
        //     Name "DepthOnly"
        //     Tags
        //     {
        //         "LightMode" = "DepthOnly"
        //     }

        //     // -------------------------------------
        //     // Render State Commands
        //     ZWrite On
        //     ColorMask R
        //     Cull[_Cull]

        //     HLSLPROGRAM
        //     #pragma target 2.0

        //     // -------------------------------------
        //     // Shader Stages
        //     #pragma vertex DepthOnlyVertex
        //     #pragma fragment DepthOnlyFragment

        //     // -------------------------------------
        //     // Material Keywords
        //     #pragma shader_feature_local _ALPHATEST_ON
        //     #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

        //     // -------------------------------------
        //     // Unity defined keywords
        //     #pragma multi_compile _ LOD_FADE_CROSSFADE

        //     //--------------------------------------
        //     // GPU Instancing
        //     #pragma multi_compile_instancing
        //     #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

        //     // -------------------------------------
        //     // Includes
        //     #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
        //     #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
        //     ENDHLSL
        // }

        
        // This pass is used when drawing to a _CameraNormalsTexture texture
        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"

                "Queue" = "1900"
            
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 3.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ LOD_FADE_CROSSFADE

            // -------------------------------------
            // Universal Pipeline keywords
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"
            ENDHLSL
        }

        Pass{
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"  }
     
                ColorMask 0

                HLSLPROGRAM
                #pragma vertex Vertex
                #pragma fragment Fragment
        
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
    
                float3 _LightDirection;

                struct Attributes{
                    float3 positionLS : POSITION;
                    float3 normalLS : NORMAL;
                };


                struct Varyings{
                    float4 positionCS : SV_POSITION;
                };


                float4 GetShadowPositionHClip(Attributes input) {
                    float3 positionWS = TransformObjectToWorld(input.positionLS.xyz);
                    float3 normalWS = TransformObjectToWorldDir(input.normalLS);

                    float invNdotL = 1.0 - saturate(dot(_LightDirection, normalWS));
                    float scale = invNdotL * _ShadowBias.y;

                    // normal bias is negative since we want to apply an inset normal offset
                    positionWS = _LightDirection * _ShadowBias.xxx + positionWS;
                    positionWS = normalWS * scale.xxx + positionWS;
                    float4 positionCS = TransformWorldToHClip(positionWS);

                    #if UNITY_REVERSED_Z
                        positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                    #else
                        positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                    #endif

                    return positionCS;
                }

                Varyings Vertex(Attributes input){
            
                    Varyings output;

                    output.positionCS = GetShadowPositionHClip(input);

                    return output;
        
                }


                half4 Fragment(Varyings v) : SV_Target {
        
            
                    return 0;
                }




             ENDHLSL
        }

    }


}
