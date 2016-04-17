	Shader "Custom/SimpleTexture" {

SubShader {

	Tags { "RenderType"="Opaque" }
	// No culling or depth			
	ZTest Off
	ZWrite Off 
	Cull Off 

		
	CGINCLUDE	
	#include "UnityCG.cginc"

	//-------------------------------------------------------------------------------------------------
	// Input/output structs
	//-------------------------------------------------------------------------------------------------
	struct VSInput
	{
		float4 position		: POSITION;
	};

	struct VSOutput
	{
		float4 position		: SV_POSITION;
		float4 screen_pos	: TEXCOORD0;
	};

	struct PSOutput
	{
		float4 color	: COLOR0;
		float4 color2	: COLOR1;
		float4 color3	: COLOR2;
		float4 color4	: COLOR3;
	};

	//-------------------------------------------------------------------------------------------------
	// Texture
	//-------------------------------------------------------------------------------------------------
	sampler2D _MainTex;

	//-------------------------------------------------------------------------------------------------
	// Vertex shader entry point
	//-------------------------------------------------------------------------------------------------
	VSOutput vert (VSInput input)
	{
		VSOutput output;

		output.position =  input.position;
		output.screen_pos = input.position;

		return output;
	}
	
			
			
	//-------------------------------------------------------------------------------------------------
	// Pixel shader entry point
	//-------------------------------------------------------------------------------------------------
	PSOutput frag (VSOutput input)
	{
				
		float2 texcoords = (input.screen_pos.xy / input.screen_pos.w + 1.0) * 0.5;
		 #if UNITY_UV_STARTS_AT_TOP
        texcoords.y = 1.0 - texcoords.y;
		 #endif
		
		PSOutput output;
		output.color = tex2D(_MainTex, texcoords);
		output.color2 = float4(1, 0, 0, 1);
		output.color3 = float4(0, 1, 0, 1);
		output.color4 = float4(0, 0, 1, 1);
		return output;
	}

ENDCG

	Pass {
		Name "Full Screen Quad"
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag        
        ENDCG
    }
}

}
