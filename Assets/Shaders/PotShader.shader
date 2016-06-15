Shader "Senso/PotShader"
{
	Properties
	{
		_MainTexture ("Plasma texture", 2D) = "white" {}
		_Radius("Gradient radius", Range (0, 1)) = 0.5
		_Speed ("Speed", float) = 25
	}
	SubShader
	{
		Pass {
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTexture;
			fixed _Radius;
			fixed _Speed;
			static const float PI = 3.14159265f;

			struct appdata 
			{
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float2 texcoord: TEXCOORD0;
			};

			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = v.texcoord;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float offsetX = (cos(_Time.y * _Speed) + 1.0f) * 0.3f;
				float offsetY = (sin(_Time.y * _Speed) + 1.0f) * 0.3f;

				float2 coord = float2((i.texcoord.x - offsetX) / _Radius, (i.texcoord.y - offsetY) / _Radius); 
				fixed gradColor = (coord.x * coord.x + coord.y * coord.y);

				fixed4 aColor = fixed4(1.0f, 1.0f, gradColor, 1.0f);
				return aColor * tex2D(_MainTexture, float2(i.texcoord.x + _Time.x * 1.5f * _Speed, i.texcoord.y));
			}

			ENDCG
		}
	}
}
