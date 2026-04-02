Shader "Hidden/Custom/BloomShader"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        struct appdata {
            uint vertexID : SV_VertexID;
        };

        struct v2f {
            float4 positionHCS : SV_POSITION;
            float2 uv          : TEXCOORD0;
        };

        v2f Vert(appdata input) 
        {
            v2f o;
            float2 uv = float2((input.vertexID << 1) & 2, input.vertexID & 2);
            o.uv = uv;
            o.positionHCS = float4(uv * 2.0 - 1.0, 0.0, 1.0);

            #if UNITY_UV_STARTS_AT_TOP
                o.uv.y = 1.0 - o.uv.y;
            #endif

            return o;
        }
        ENDHLSL

        // First pass: Extracting bright areas
        Pass
        {
            Name "Bloom"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert;
            #pragma fragment Frag;
        
            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            float _threshold; 

            half4 Frag(v2f i) : SV_TARGET
            {
                float4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv);
        
                // Get luminance / brightness
                float luminance = dot(col.rgb, float3(0.2126, 0.7152, 0.0722));
        
                // Get the soft knee (prevent sharp cutoffs)
                float _softKnee = 0.5;
                float knee = _threshold * _softKnee + 1e-5;

                float soft = saturate((luminance - _threshold + knee) / (2.0 * knee));
                
                // Get bloom weight (if the luminance of the material is less than the threshold, return max btwn 0 and soft (prevent sharp cuts))
                float bloomWeight = max(soft, step(_threshold, luminance));

                return float4(col.rgb * bloomWeight, 1.0);
            }

            ENDHLSL
        }

        // Second pass: Combining original scene with scene in original texture
        Pass
        {
            Name "BloomCombine"
            ZTest Always 
            ZWrite Off 
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragCombine

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_X(_BlitTexture);
            TEXTURE2D_X(_BloomTex);

            float _exposure;
            float _threshold;

            half4 FragCombine(v2f i) : SV_Target
            {
                const float gamma = 2.2;

                // Sample the original scene
                float3 scene = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, i.uv).rgb;
        
                // Sample the blurred bloom (automatically upsampled if smaller)
                float3 bloom = SAMPLE_TEXTURE2D_X(_BloomTex, sampler_LinearClamp, i.uv).rgb;
        
                scene += bloom;
                float3 result = result / (1.0 + result);

                result = pow(result, float3(1.0 / gamma, 1.0 / gamma, 1.0 / gamma));

                // Return additive
                return float4((scene += bloom).xyz, 1.0);
            }
            ENDHLSL
        }
    }

    Fallback Off
}