Shader "Hidden/RGBSplitShader"
{
    Properties
    {
        _Offset ("Offset", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "PassType"="FullScreen" }

        Pass
        {
            Name "FullScreen Pass"   // ✅ 必须有这个名字
            ZTest Always ZWrite Off Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);
            float _Offset;

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings  { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
                return o;
            }

            float4 frag (Varyings i) : SV_Target
            {
                return float4(1,0,0,1); // 先纯红测试
            }
            ENDHLSL
        }
    }
}
