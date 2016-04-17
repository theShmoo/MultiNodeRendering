Shader "Custom/Deferred_Geometry_Pass" {

	Properties {

			vMaterialAmbient("Ambient", range(0,1)) = 0.25
			vMaterialDiffuse("Color", Color) = (1,1,1,1)
			[NoScaleOffset]
			texDiffuseMap("Diffuse", 2D) = "white" {}

			vMaterialSpecular("Color", Color) = (1,1,1,1)
		
			[NoScaleOffset]
			texSpecularMap("Specular", 2D) = "white" {}
		
			sMaterialShininess("Shininess", float) = 1.0
		
			[NoScaleOffset][Normal]
			texNormalMap("Normal Map", 2D) = "bump" {}


			vMaterialEmissive("Emissive Color(Not supported)", Color) = (0,0,0)
		


		}
			CGINCLUDE
			#define UNITY_SETUP_BRDF_INPUT MetallicSetup
			ENDCG

	SubShader 
	{
		Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
			LOD 300

		CGINCLUDE
		#include "Material.cginc"	// Defines Material properties
		#include "UnityCG.cginc"

		//-------------------------------------------------------------------------------------------------
		// Input/output structs
		//-------------------------------------------------------------------------------------------------



		struct PSInput
		{
			float4 position			: SV_POSITION;	// Position in Clip Space
			float4 worldPosition	: TEXCOORD0;	// Position in World Space	
			float2 texcoords		: TEXCOORD1;	// Texture Coordinates
			float3 normal			: TEXCOORD2;	// Normal in World Space
			float3 tangent			: TEXCOORD3;	// Tangent in World Space
			float3 bitangent		: TEXCOORD4;	// Bitangent in World Space
		};


		struct PSOutput
		{		
			float4 worldNormal		: COLOR0;	// Normal in World Space
			float4 diffuseAlbedo	: COLOR1;	// Diffuse Color
			float4 specularAlbedo	: COLOR2;	// Specular Color	
			float4 worldPosition	: COLOR3;	// Position in World Space	
		};


		//-------------------------------------------------------------------------------------------------
		// Vertex Shader
		//-------------------------------------------------------------------------------------------------
		PSInput vert(appdata_tan input)
		{
			PSInput output;
		
			output.position = mul(UNITY_MATRIX_MVP, input.vertex);		// Transform position into clip space		
			//output.position = input.vertex;
			output.worldPosition = mul(_Object2World, input.vertex);	// Transform position into world space
		
			//output.normal = normalize(UnityObjectToWorldNormal(input.normal));	
			output.normal = normalize( mul(input.normal, (float3x3)_Object2World));	// Transform normal into world space
		
			if(bHasNormalMap)
			{
				output.tangent = UnityObjectToWorldDir(input.tangent);	// Transform tagent into world space		
				output.bitangent = cross(output.normal, output.tangent) * input.tangent.w * unity_WorldTransformParams.w; // Compute bitangent
			}
			else
			{
				output.tangent = 0.0f;
				output.bitangent = 0.0f;
			}

			output.texcoords = input.texcoord.xy;


			return output;
		}




		//-------------------------------------------------------------------------------------------------
		// Fragment Shader
		//-------------------------------------------------------------------------------------------------

		PSOutput frag(PSInput input)
		{
			PSOutput output;

			// Get Lighting/Color Information from textures
		
			float3 diffuse = vMaterialDiffuse;
			//if(bHasDiffuseMap)	
			//{
				diffuse *= tex2D(texDiffuseMap, input.texcoords);
			//}


			float3 specular = vMaterialSpecular;
			//if(bHasSpecularMap)
			//{
				specular *= tex2D(texSpecularMap, input.texcoords);
			//}
			

			// Calculate Normal
			float3 worldNormal		= normalize(input.normal);			
			if(bHasNormalMap)
			{
				float3 tangent		= normalize(input.tangent);
				float3 bitangent	= normalize(input.bitangent);
				float3 bumpedNormal	= normalize(tex2D(texNormalMap, input.texcoords) * 0.5f + 0.5f);
				float3x3 tbn		= float3x3(tangent, bitangent, worldNormal);
				worldNormal			= normalize(mul(bumpedNormal, tbn));
			}

		
			// Set output
			output.worldNormal		= float4(worldNormal, 1);		
			output.diffuseAlbedo	= float4(diffuse, vMaterialAmbient);
			output.specularAlbedo	= float4(specular, sMaterialShininess);
			output.worldPosition	= float4(input.worldPosition);

			return output;
		}
		ENDCG

		Pass {
			Name "Geometry Pass"
			Tags { "RenderType"="Opaque" "Queue"="Geometry" }
		
			Cull Back
			ZWrite On
			ZTest Less
		
		

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
