//MAKE SURE TO USE FLOAT4. DONT FORGET THE 4444444!!!!
Shader "Custom/Skybox"
{
    Properties
    {
        [Header(Sky color)]
        [HDR]_ColorTop("Color top", Color) = (1,1,1,1)
        [HDR]_ColorMiddle("Color middle", Color) = (1,1,1,1)
        [HDR]_ColorBottom("Color bottom", Color) = (1,1,1,1)

        _MiddleSmoothness("Middle smoothness", Range(0.0,1.0)) = 1
        _MiddleOffset("Middle offset", float) = 0
        _TopSmoothness("Top smoothness", Range(0.0, 1.0)) = 1
        _TopOffset("Top offset", float) = 0

        [Header(Sun)]
        _SunSize("Sun size", Range(0.0, 1.0)) = 0.1
        [HDR]_SunColor("Sun color", Color) = (1,1,1,1)

        [Header(Clouds)]
        [HDR]_CloudsColor("Clouds color", Color) = (1,1,1,1)
        _CloudsTexture("Clouds texture", 2D) = "black" {}
        _CloudsThreshold("Clouds threshold", Range(0.0, 1.0)) = 0
        _CloudsSmoothness("Clouds smoothness", Range(0.0, 1.0)) = 0.1
        _SunCloudIntensity("Sun behind clouds intensity", Range(0, 1)) = 0
        _PanningSpeedX("Panning speed X", float) = 0
        _PanningSpeedY("Panning speed Y", float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" }

        Cull Off
        ZWrite Off
        ZTest LEqual

        Pass
        {
            HLSLPROGRAM
            #include "UnityCG.cginc"
            #pragma vertex MyVertexShader
            #pragma fragment MyFragmentShader

            struct vertexData
            {
                float4 position : POSITION;
                float3 dir      : TEXCOORD0;
            };

            struct vertex2Fragment
            {
                float4 position : SV_POSITION;
                float3 dir      : TEXCOORD0;
            };

            float4 _ColorBottom;
            float4 _ColorMiddle;
            float4 _ColorTop;

            float _MiddleSmoothness;
            float _MiddleOffset;
            float _TopSmoothness;
            float _TopOffset;

            float4 _SunColor;
            float  _SunSize;

            sampler2D _CloudsTexture;
            float4 _CloudsTexture_ST;
            float4 _CloudsColor;
            float _CloudsSmoothness;
            float _CloudsThreshold;
            float _SunCloudIntensity;
            float _PanningSpeedX;
            float _PanningSpeedY;

            vertex2Fragment MyVertexShader(vertexData vd)
            {
                vertex2Fragment v2f;
                v2f.position = UnityObjectToClipPos(vd.position);
                v2f.dir = vd.dir;
                return v2f;
            }

            float4 MyFragmentShader(vertex2Fragment v2f) : SV_TARGET
            {
                float3 d = normalize(v2f.dir);

                float2 uv = float2(atan2(d.x, d.z) / UNITY_TWO_PI, asin(d.y) / UNITY_HALF_PI);

                float middleThreshold = smoothstep(0.0, 0.5 - (1.0 - _MiddleSmoothness) * 0.5, d.y - _MiddleOffset);
                float topThreshold    = smoothstep(0.5, 1.0 - (1.0 - _TopSmoothness)   * 0.5, d.y - _TopOffset);

                float4 col = lerp(_ColorBottom, _ColorMiddle, middleThreshold);
                col = lerp(col, _ColorTop, topThreshold);

                //clouds
                float cloudsThreshold = d.y - _CloudsThreshold;
                float cloudsTex = tex2D(
                    _CloudsTexture,
                    uv * _CloudsTexture_ST.xy + _CloudsTexture_ST.zw + float2(_PanningSpeedX, _PanningSpeedY) * _Time.y
                ).r;

                float clouds = smoothstep(cloudsThreshold, cloudsThreshold + _CloudsSmoothness, cloudsTex);

                // sun
                float sunSDF = distance(d.xyz, _WorldSpaceLightPos0.xyz);
                float sun = max(clouds * _CloudsColor.a, smoothstep(0.0, _SunSize, sunSDF));

                //cloud shading 
                float cloudShading =
                    smoothstep(cloudsThreshold, _CloudsSmoothness + cloudsThreshold + 0.1, cloudsTex) -
                    smoothstep(_CloudsSmoothness + cloudsThreshold + 0.1, _CloudsSmoothness + cloudsThreshold + 0.4, cloudsTex);

                clouds = lerp(clouds, cloudShading, 0.5) * middleThreshold * _CloudsColor.a;

                float silverLining =
                    (smoothstep(cloudsThreshold, cloudsThreshold + _CloudsSmoothness, cloudsTex) -
                     smoothstep(cloudsThreshold + 0.02, cloudsThreshold + _CloudsSmoothness + 0.02, cloudsTex));

                silverLining *= smoothstep(_SunSize * 3.0, 0.0, sunSDF) * _CloudsColor.a;

                col = lerp(_SunColor, col, sun);

                float4 cloudsCol = lerp(
                    _CloudsColor,
                    _CloudsColor + _SunColor,
                    cloudShading * smoothstep(0.3, 0.0, sunSDF) * _SunCloudIntensity
                );

                col = lerp(col, cloudsCol, clouds);
                col += silverLining * _SunColor;
                return col;
            }
            ENDHLSL
            
        }
    }
}
