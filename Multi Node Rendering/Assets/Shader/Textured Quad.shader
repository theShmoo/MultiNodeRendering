Shader "Custom/Deferred_Light_Pass" {

	Properties {
		[NoScaleOffset]
		_MainTex ("Color map", 2D) = "white" {}		
	}

	SubShader 
	{
		Tags{ "LightMode" = "ForwardBase" }
		LOD 300

		CGINCLUDE
		#include "UnityCG.cginc"

		struct VSInput {
			float4 position		: POSITION;
		};

		struct VSOutput
		{
			float4 position		: SV_POSITION;
			float4 screen_pos	: TEXCOORD0;
		};

		struct PSOutput
		{
			half3 color	: COLOR0;
		};

		//-------------------------------------------------------------------------------------------------
		// Textures
		//-------------------------------------------------------------------------------------------------
		sampler2D _MainTex;		


		//-------------------------------------------------------------------------------------------------
		// Vertex Shader
		//-------------------------------------------------------------------------------------------------
		VSOutput vert(VSInput input)
		{
			VSOutput output;

			output.position = input.position;
			output.screen_pos = input.position;

			return output;
		}

		//-------------------------------------------------------------------------------------------------
		// Fragment Shader
		//-------------------------------------------------------------------------------------------------

		PSOutput frag(VSOutput input)
		{
			float2 texcoords = (input.screen_pos.xy / input.screen_pos.w + 1.0) * 0.5;
			#if UNITY_UV_STARTS_AT_TOP
				texcoords.y = 1.0 - texcoords.y;
			#endif

			PSOutput output;

			output.color = tex2D(_MainTex, texcoords).rgb;
			return output;
		}
		ENDCG

		Pass {
			Name "Textured Quad"
		
			Cull Off
			ZWrite Off
			Blend One One // Additive

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#ifdef SHADER_API_OPENGL
				#pragma glsl
			#endif
			ENDCG
		}
	}
}
