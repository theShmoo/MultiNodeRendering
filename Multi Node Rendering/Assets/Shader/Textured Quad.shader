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
			#pragma vertex vert
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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = v.vertex;
				o.texcoords = v.texcoords;
				return o;
			}			

			fixed4 frag (v2f i) : SV_Target
			{
				
				fixed4 col = tex2D(_MainTex, i.texcoords);
				// just invert the colors
				//col = 1 - col;
				return col;
			}
			ENDCG
		}
	}
}
