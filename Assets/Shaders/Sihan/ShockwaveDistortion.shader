Shader "Custom/ShockwaveDistortion"
{
    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    TEXTURE2D_X(_BlitTexture);
    SAMPLER(sampler_BlitTexture);

    float _Intensity;
    float _Samples;

    struct v2f {
        float4 pos : SV_POSITION;
        float2 uv : TEXCOORD0;
    };

    v2f Vert(uint id : SV_VertexID) {
        v2f o;
        o.uv = float2((id << 1) & 2, id & 2);
        o.pos = float4(o.uv * 2 - 1, 0, 1);
        #if UNITY_UV_STARTS_AT_TOP
            o.uv.y = 1 - o.uv.y;
        #endif
        return o;
    }

    half4 Frag(v2f i) : SV_Target {
        half4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv);
        
        float2 dir = i.uv - 0.5;
        
        float4 accumulation = 0;
        float weightSum = 0;
        
        for (float j = 1; j <= (int)_Samples; j++) {
            float2 ghostUV = i.uv - dir * (j * _Intensity * 0.1);
            
            float weight = 1.0 - (j / _Samples);
            accumulation += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, ghostUV) * weight;
            weightSum += weight;
        }

        return lerp(color, accumulation / weightSum, saturate(_Intensity));
    }
    ENDHLSL

    SubShader {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZTest Always ZWrite Off Cull Off
        Pass {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }
}