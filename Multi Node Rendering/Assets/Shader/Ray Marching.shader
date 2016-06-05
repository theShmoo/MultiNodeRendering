Shader "Custom/Ray Marching/Ray Marching" 
{	
	CGINCLUDE
	
	#include "UnityCG.cginc"
	#pragma target 5.0
	
	float _Opacity;
	float4 _TextureSize;
	sampler2D _BackTex;
	sampler2D _FrontTex;
	sampler3D _VolumeTex;	
	
	#define STEP_CNT 512
	#define STEP_SIZE 1 / 512

	/*#define STEP_CNT 128
	#define STEP_SIZE 1 / 1024*/
	
	float sampleVolume(float3 p)
	{		
		return tex3Dlod(_VolumeTex, float4(p, 0)).a;
	}

	float4 frag_blend(v2f_img i) : COLOR
	{
	
		float3 rayEndPos = tex2D(_BackTex, i.uv).xyz;
		float3 rayStartPos = tex2D(_FrontTex, i.uv).xyz;
		if(rayEndPos.x == 0.0 && rayEndPos.y == 0.0 && rayEndPos.z == 0.0)
			discard;
		//if(rayEndPos.x == rayStartPos.x && rayEndPos.y == rayStartPos.y && rayEndPos.z == rayStartPos.z)
		//	discard;
				
		float3 rayDir = normalize(rayEndPos - rayStartPos);
		float3 rayStep = rayDir * STEP_SIZE;
		
		float4 finalColor = 0;
		float3 rayCurrentPos = rayStartPos;
		float4 colorMask = float4(1, 1, 1, 1);

		for(int k = 0; k < STEP_CNT; k++)
		{
			float intensity = sampleVolume(rayCurrentPos);
			rayCurrentPos += rayStep;

			colorMask = (intensity < 0.35) ? float4(1, 1, 0, 0.05): float4(0, 1, 1,1);
			float4 currentColor = float4(intensity * colorMask.rgb * 2, intensity * pow(_Opacity, 2) * colorMask.a);

	        // Standard blending
			currentColor.rgb *= currentColor.a;
			finalColor = (1.0f - finalColor.a) * currentColor + finalColor;
		}

    	return finalColor;
	}
		
	//**** Second Pass ****//

	float3 getNormal(float3 position, float3 dataStep)
	{
		float dx = sampleVolume(position + float3(dataStep.x, 0, 0)) - sampleVolume(position + float3(-dataStep.y, 0, 0));
		float dy = sampleVolume(position + float3(0, dataStep.y, 0)) - sampleVolume(position + float3(0, -dataStep.y, 0));
		float dz = sampleVolume(position + float3(0, 0, dataStep.z)) - sampleVolume(position + float3(0, 0, -dataStep.z));

		return normalize(float3(dx, dy, dz));
	}

	float4 frag_iso(v2f_img i) : COLOR
	{
		float3 rayEndPos = tex2D(_BackTex, i.uv).xyz;		
		float3 rayStartPos = tex2D(_FrontTex, i.uv).xyz;
			
		float3 rayDir = normalize(rayEndPos - rayStartPos);
		float3 rayStep = rayDir * STEP_SIZE;
		
		float intensity = 0;		
		float threshold = _Opacity;
		float3 rayCurrentPos = rayStartPos;
		
		// Linear search
		for (int k = 0; k < STEP_CNT; k++)
		{
			intensity = sampleVolume(rayCurrentPos);
			if (intensity > threshold) break;
			rayCurrentPos += rayStep;
		}

		if (intensity < threshold) discard;
		
		rayStep *= 0.5;
		rayCurrentPos -= rayStep;

		// Binary search
		for (uint j = 0; j < 4; j++)
		{
			rayStep *= 0.5;
			intensity = sampleVolume(rayCurrentPos);
			rayCurrentPos += (intensity >= threshold) ? -rayStep : rayStep;
		}

		float3 texelSize = 1.0 / _TextureSize;
		float3 normal = getNormal(rayCurrentPos, texelSize);
		float ndotl = pow(max(0.0, dot(normal, rayDir)), 1);

		return float4(1,1,1,1) * ndotl;
	}

	ENDCG
	
Subshader
{
	ZTest Always Cull Off ZWrite Off
	Fog { Mode off }
		
	Pass 
	{
		CGPROGRAM
		#pragma vertex vert_img
		#pragma fragment frag_blend
		ENDCG
	}	

	Pass
	{
		CGPROGRAM
		#pragma vertex vert_img
		#pragma fragment frag_iso
		ENDCG
	}	
}

Fallback off
	
} // shader