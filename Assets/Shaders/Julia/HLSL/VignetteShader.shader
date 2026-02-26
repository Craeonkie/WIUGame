Shader "Hidden/Custom/VignetteShader"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "Vignette"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // This is what Blitter.BlitTexture binds in URP RenderGraph
            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            float _radius;
            float _feather;
            float4 _tintColour;
            float _invertColour;

            struct appdata
            {
                uint vertexID : SV_VertexID;
            };

            struct v2f
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata input)
            {
                v2f o;

                // Fullscreen triangle UV from VertexID (0, 1, 2)
                float2 uv = float2((input.vertexID << 1) & 2, input.vertexID & 2);
                o.uv = uv;
                o.positionHCS = float4(uv * 2.0f - 1.0f, 0.0f, 1.0f);

                #if UNITY_UV_STARTS_AT_TOP
                    o.uv.y = 1.0f - o.uv.y;
                #endif

                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // Sample the camera colour
                float4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv);

                // Get aspect ratio
                float aspect = _ScreenSize.x / _ScreenSize.y;

                float2 newUV = i.uv * 2 - 1;
                newUV.x *= aspect;
                
                float circle = length(newUV);
                // Create a mask cutoff based on the feather 
                float mask = 1 - smoothstep(_radius, _radius + _feather, circle);
                float invertMask = 1 - mask;

                float3 displayColour = col.rgb * mask;
                float3 vignetteColour;
                
                if (_invertColour)
                    vignetteColour = (1 - col.rgb) * invertMask * _tintColour;
                else
                    vignetteColour = col.rgb * _tintColour * invertMask;

                return float4(displayColour + vignetteColour, 1);
            }

            ENDHLSL
        }
    }
}
