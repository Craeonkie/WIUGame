Shader "Hidden/Custom/J_OutlineShader"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }

        Pass 
        {
            Name "Outline"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            // This is what Blitter.BlitTexture binds in URP RenderGraph
            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            float _outlineThickness;
            float4 _outlineColour;

            struct appdata
            {
                uint vertexID : SV_VertexID;
            };

            struct v2f
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            struct ScharrOperators
            {
                float3x3 x;
                float3x3 y;
            };

            ScharrOperators GetEdgeDetectionKernels()
            {
                ScharrOperators kernels;
                kernels.x = float3x3(-3, 0, 3, -10, 0, 10, -3, 0, 3);
                kernels.y = float3x3(-3, -10, -3, 0, 0, 0, 3, 10, 3);
                return kernels;
            }

            void DepthBasedOutlines(float2 screenUV, float2 px, out float outlines)
            {
                outlines = 0;
    
                // Conditional Compilation
                #if defined(UNITY_DECLARE_DEPTH_TEXTURE_INCLUDED)
                ScharrOperators kernels = GetEdgeDetectionKernels();
                // Specify our horizontal and vertical gradients, this is actually delX and delY i think?
                float gx = 0;
                float gy = 0;
    
                // Evaluate the depth value for all adjacent pixels in a 3x3 grid with our current pixel as the middle
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        // We skip i == 0 and j == 0 because both are multiplied by 0, making calculation redundant
                        if (i == 0 && j == 0)
                            continue;
            
                        float2 offset = float2(i, j) * px;
                        float d = SampleSceneDepth(screenUV + offset);
                        gx += d * kernels.x[i + 1][j + 1]; // Col, row
                        gy += d * kernels.y[i + 1][j + 1]; // Col, row
                    }
                }

                // Pythagoras
                float g = sqrt(gx * gx + gy * gy);

                outlines = step(0.2, g); // If g is less than 0.2, return 0, else, return 1
                #endif
            }

            void NormalBasedOutlines(float2 screenUV, float2 px, out float outlines)
            {
                outlines = 0;
                #if defined(UNITY_DECLARE_NORMALS_TEXTURE_INCLUDED)
                ScharrOperators kernels = GetEdgeDetectionKernels();
                float gx = 0;
                float gy = 0;
    
                float3 cn = SampleSceneNormals(screenUV);
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (i == 0 && j == 0)
                            continue;
            
                        float2 offset = float2(i, j) * px;
                        float3 n = SampleSceneNormals(screenUV + offset);
    
                        // We'll use the dot product of our current normal of the pixel against the new normal that was offset in order to get a float value to add to our gradient
                        float dp = dot(cn, n);
                        gx += dp * kernels.x[i + 1][j + 1];
                        gy += dp * kernels.y[i + 1][j + 1];
                    }
                }

                float g = sqrt(gx * gx + gy * gy);
                outlines = step(2, g);
                #endif
            }

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
                // Get pixel size
                float2 texelSize = _outlineThickness / _ScreenParams.xy;

                float depthOutlines = 0;
                float normalOutlines = 0;

                // Get outlines based on depth and normal
                DepthBasedOutlines(i.uv, texelSize, depthOutlines);
                NormalBasedOutlines(i.uv, texelSize, normalOutlines);

                float outlines = normalOutlines + ((1.0f - normalOutlines) * depthOutlines);
                
                float4 screenCol = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv);

                // Lerp between the scene color and the outline color based on outline intensity
                return lerp(screenCol, _outlineColour, outlines);
            }

            ENDHLSL
        }
    }
}
