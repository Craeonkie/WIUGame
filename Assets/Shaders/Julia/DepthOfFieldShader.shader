Shader "Hidden/Custom/DepthOfFieldShader"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        float _bokehRadius;

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


        // PASS 0: Circle of Confusion Pass
        Pass
        {
            Name "CircleOfConfusion"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert;
            #pragma fragment Frag;

            TEXTURE2D_X(_BlitTexture);
            TEXTURE2D_X_FLOAT(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);
            SAMPLER(sampler_BlitTexture);            

            float _focusDistance;
            float _focusRange;

            half Frag(v2f i) : SV_TARGET
            {
                float rawDepth = SAMPLE_TEXTURE2D_X_LOD(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv, 0).r;
                float depth = LinearEyeDepth(rawDepth, _ZBufferParams);

                // Calculate how far the pixel is from the focus distance and normalize it based on the focus range
                float coc = (depth - _focusDistance) / _focusRange;

                // Get blur weight and scale by radius of blur (bokeh)
                coc = clamp(coc, -1, 1) * _bokehRadius;
                return coc;
            }
            
            ENDHLSL
        }

        // PASS 1: PreFilter Pass (Downsample here)
        Pass
        {
            Name "PreFilter"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert;
            #pragma fragment Frag;

            TEXTURE2D_X(_BlitTexture);
            TEXTURE2D_X(_CoCTexture);
            SAMPLER(sampler_BlitTexture);    
            float4 _BlitTexture_TexelSize;

            // Reduce flicker in bright areas (anti-bloom/fireflies)
            half Weigh (half3 c) {
				return 1 / (1 + max(max(c.r, c.g), c.b));
			}

            half4 Frag(v2f i) : SV_TARGET
            {
                float4 o = _BlitTexture_TexelSize.xyxy * float2(-0.5, 0.5).xxyy;
                
                // Sample from four high-resolution texels corresponding to the low-resolution texel and average them
                half3 s0 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv + o.xy).rgb;
                half3 s1 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv + o.zy).rgb;
                half3 s2 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv + o.xw).rgb;
                half3 s3 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv + o.zw).rgb;
                               
                // Weigh each colour
                half w0 = Weigh(s0);
				half w1 = Weigh(s1);
				half w2 = Weigh(s2);
				half w3 = Weigh(s3);

                // Average the weighted colours
                half3 color = s0 * w0 + s1 * w1 + s2 * w2 + s3 * w3;
				color /= max(w0 + w1 + w2 + w3, 0.00001);

                // Sample from four high-resolution texels corresponding to the low-resolution texel and average them
                half coc0 = SAMPLE_TEXTURE2D_X(_CoCTexture, sampler_BlitTexture, i.uv + o.xy).r;
                half coc1 = SAMPLE_TEXTURE2D_X(_CoCTexture, sampler_BlitTexture, i.uv + o.zy).r;
                half coc2 = SAMPLE_TEXTURE2D_X(_CoCTexture, sampler_BlitTexture, i.uv + o.xw).r;
                half coc3 = SAMPLE_TEXTURE2D_X(_CoCTexture, sampler_BlitTexture, i.uv + o.zw).r;

                half cocMin = min(min(min(coc0, coc1), coc2), coc3);
				half cocMax = max(max(max(coc0, coc1), coc2), coc3);
				half coc = cocMax >= -cocMin ? cocMax : cocMin;

                return half4(color, coc);
            }
            
            ENDHLSL
        }
        
        // PASS 2: Bokeh PASS
        Pass
        {
            Name "Bokeh"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert;
            #pragma fragment Frag;

            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);
            float4 _BlitTexture_TexelSize;

            static const int kernelSampleCount = 22;
            // unity's kernels, sample a disc for better offsets
			static const float2 kernel[kernelSampleCount] = {
				float2(0, 0),
				float2(0.53333336, 0),
				float2(0.3325279, 0.4169768),
				float2(-0.11867785, 0.5199616),
				float2(-0.48051673, 0.2314047),
				float2(-0.48051673, -0.23140468),
				float2(-0.11867763, -0.51996166),
				float2(0.33252785, -0.4169769),
				float2(1, 0),
				float2(0.90096885, 0.43388376),
				float2(0.6234898, 0.7818315),
				float2(0.22252098, 0.9749279),
				float2(-0.22252095, 0.9749279),
				float2(-0.62349, 0.7818314),
				float2(-0.90096885, 0.43388382),
				float2(-1, 0),
				float2(-0.90096885, -0.43388376),
				float2(-0.6234896, -0.7818316),
				float2(-0.22252055, -0.974928),
				float2(0.2225215, -0.9749278),
				float2(0.6234897, -0.7818316),
				float2(0.90096885, -0.43388376),
			};

            // Smoothing samples
            half Weigh (half coc, half radius) {
				return saturate((coc - radius + 2) / 2);
			}

            half4 Frag(v2f i) : SV_TARGET
            {
                half coc = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv).a;

                half3 bgColor = 0;
                half3 fgColor = 0;
				half bgWeight = 0;
                half fgWeight = 0;

                for (int k = 0; k < kernelSampleCount; k++) {
					float2 o = kernel[k] * _bokehRadius;
                    half radius = length(o);
					o *= _BlitTexture_TexelSize.xy;
                    half4 s = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv + o);

                    half bgw = Weigh(max(0, min(s.a, coc)), radius);
					bgColor += s.rgb * bgw;
					bgWeight += bgw;

                    half fgw = Weigh(-s.a, radius);
					fgColor += s.rgb * fgw;
					fgWeight += fgw;
				}

                bgColor *= 1.0 / (bgWeight + (bgWeight == 0));
                fgColor *= 1 / (fgWeight + (fgWeight == 0));

                // Use PI to boost the overall foreground 
                half bgfg =  min(1, fgWeight * PI / kernelSampleCount);

                half3 color = lerp(bgColor, fgColor, bgfg);

				return half4(color, bgfg);
            }
            
            ENDHLSL
        }

        // PASS 3: PostFilter
        Pass
        {
            Name "PostFilter"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert;
            #pragma fragment Frag;

            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);
            float4 _BlitTexture_TexelSize;

            half4 Frag(v2f i) : SV_TARGET
            {
                float3 color = 0;

                // Simple gaussian blur tent filter
                float4 o = _BlitTexture_TexelSize.xyxy * float2(-0.5, 0.5).xxyy;
                half4 s = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv + o.xy) +
                            SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv + o.zy) +
                            SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv + o.xw) +
                            SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv + o.zw);
                    

                return s * 0.25;
            }
            
            ENDHLSL
        }

        // Pass 4: Combining
        Pass 
        {
            Name "Combine"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert;
            #pragma fragment Frag;

            TEXTURE2D_X(_BlitTexture);
            TEXTURE2D_X(_DoFTexture);
            TEXTURE2D_X(_CoCTexture);
            SAMPLER(sampler_BlitTexture);
            float4 _BlitTexture_TexelSize;

            half4 Frag(v2f i) : SV_TARGET
            {
                // Blend the out of focus texture and the original textures together based on the CoC
                half4 source = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv);
                half coc = SAMPLE_TEXTURE2D_X(_CoCTexture, sampler_BlitTexture, i.uv).r;
                half4 dof = SAMPLE_TEXTURE2D_X(_DoFTexture, sampler_BlitTexture, i.uv);
                
                half dofStrength = smoothstep(0.1, 1, abs(coc));
                half3 colour = lerp(source.rgb, dof.rgb, dofStrength + dof.a - dofStrength * dof.a);
                return half4(colour, source.a);
            }
            
            ENDHLSL
        }
    }
    FallBack Off
}
