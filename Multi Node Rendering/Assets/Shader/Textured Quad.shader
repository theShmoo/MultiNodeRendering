Shader "Custom/TexturedQuadShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 texcoords: TEXCOORD;
			};

			struct v2f
			{				
				float4 vertex : SV_POSITION;
				float2 texcoords : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4x4 _matrix;		

			fixed4 frag (v2f_img i) : COLOR
			{
				
				float4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
	}
}
