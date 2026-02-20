Shader "Custom/radiusShader"
{
    Properties
    {
		_radCutOff("Radius Cutoff", Range(0,1))=0.5
		_mainTexture("Texture", 2D) = "white" {}
		_color ("Color",Color) = (1,1,1,1)
    }

    SubShader
    {
		Tags{"Queue" = "Transparent" "RenderType"= "Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
		Pass{
			HLSLPROGRAM //to start coding unity shader language
			#include "UnityCG.cginc"
			#pragma vertex MyVertexShader
			#pragma fragment MyFragmentShader

			uniform float _radCutOff;
			uniform sampler2D _mainTexture;
			uniform float4 _mainTexture_ST;
			uniform float4 _color;

			struct vertexData{
				float4 position:POSITION;
				float2 uv: TEXCOORD0;
			};
			struct vertex2Fragment{
				float4 position: SV_POSITION;
				float2 uv: TEXCOORD0;

			};

			vertex2Fragment MyVertexShader (vertexData vd){
				vertex2Fragment v2f;
				v2f.position =UnityObjectToClipPos(vd.position);
				v2f.uv = vd.uv;
				v2f.uv = TRANSFORM_TEX(vd.uv,_mainTexture);
				return v2f;
			}

			float4 MyFragmentShader (vertex2Fragment v2f):SV_TARGET{
                float2 centeredUV = v2f.uv - float2(0.5, 0.5);
                float dist = length(centeredUV);

                // Discard pixels outside the circle
                // if(dist > _radCutOff)
                //     discard;

                // Sample the texture
				float alpha = saturate(1.0 - (dist - _radCutOff)/0.05);
                float4 tex = tex2D(_mainTexture, v2f.uv);
                return float4(tex.rgb * _color.rgb, tex.a * alpha * _color.a);			}

			ENDHLSL//to end the code IMPORTANT
		}
    }
}
