Shader "Custom/Deferred_Light_Pass" {

	Properties {
		// currently no properties
		[NoScaleOffset]
		_WorldPositionMap ("world position map", 2D) = "white" {}
		[NoScaleOffset]
		_ColorMap ("color map", 2D) = "white" {}
		[NoScaleOffset]
		_SpecularMap ("specular map", 2D) = "white" {}
		[NoScaleOffset]
		_NormalMap ("normal map", 2D) = "white" {}
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
		sampler2D _WorldPositionMap;
		sampler2D _ColorMap;
		sampler2D _SpecularMap;
		sampler2D _NormalMap;

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

			// get parameters from the maps
			half3 diffuse = tex2D(_ColorMap, texcoords).rgb;
			half3 specular = tex2D(_SpecularMap, texcoords).rgb;
			half3 worldPosition = tex2D(_WorldPositionMap, texcoords).rgb;
			half3 normal = tex2D(_NormalMap, texcoords).rgb;
			normal = normalize(normal);

			// calculate lightning
			// TODO: insert light pos
			half3 lightDir = normalize(-worldPosition); //lightpos - worldpos
			// TODO insert light color
			half3 lightColor = half3(1, 1, 1);
			// TODO insert ambient color
			half3 ambientLight = lightColor * half3(0.5, 0.5, 0.5);

			// standard phong
			half3 diffuseLight = lightColor * dot(normal.rgb, lightDir.rgb);

			// TODO inser camera position
			half3 cameraPos = half3(1, 1, 1);
			half3 eyeDirection = normalize(-worldPosition);
			half3 lightReflect = normalize(reflect(-lightDir, normal));
			float cosAngle = max(0.0, dot(eyeDirection, lightReflect));
			float SpecularFactor = pow(cosAngle, 3) * 0.8;
			half3 specularLight = specular * max(SpecularFactor, 0.0);


			output.color = (ambientLight + diffuseLight) * diffuse + specularLight;
			return output;
		}
		ENDCG

		Pass {
			Name "Light Pass"
		
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
