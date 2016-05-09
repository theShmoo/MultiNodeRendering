Shader "Custom/Ray Marching/Render Position" {

	CGINCLUDE
		#pragma exclude_renderers xbox360
		#include "UnityCG.cginc"

		struct v2f {
			float4 pos : POSITION;
			float3 localPos : TEXCOORD0;
		};
		
		float4 _VolumeScale;
		float4x4 _MVPMatrix;

		v2f vert(appdata_base v) 
		{
			v2f o;
			o.pos = mul(_MVPMatrix, v.vertex);
			// Apparently the y coordinate has to be flipped
			o.pos.y = -o.pos.y;
			o.localPos = v.vertex.xyz + 0.5;
			return o;
		}

		half4 frag(v2f i) : COLOR 
		{ 
			return float4(i.localPos, 1);
		}
		
	ENDCG

	Subshader 
	{ 	
		Tags {"RenderType"="Volume"}
		Fog { Mode Off }
		
		Pass 
		{	
			Cull Back			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}

		Pass 
		{	
			Cull Front
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
}
