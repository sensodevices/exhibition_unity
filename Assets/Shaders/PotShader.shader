// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Senso/PotShader"
{
	Properties
	{
		_MainTexture ("Plasma texture", 2D) = "white" {}
		_Radius("Gradient radius", Range (0, 1)) = 0.3
		_Speed ("Speed", float) = 1
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
			float _XOffset;
			float _YOffset;
			float _XSines;
			float _YSines;
			float _Size;
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
				o.pos = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float offsetX = (cos(_Time.y * _Speed) + 1.0f) * 0.3f;
				float offsetY = (sin(_Time.y * _Speed) + 1.0f) * 0.3f;
				float2 coord = float2((i.texcoord.x - offsetX) / _Radius, (i.texcoord.y - offsetY) / _Radius); 
				fixed gradColor = (coord.x * coord.x + coord.y * coord.y);
				
				float K = (_Time.x) % 2.0f;
				if (K > 1.0f) K = 2.0f - K;
				
				fixed4 aColor = fixed4((1.0f - K), K, gradColor, 1.0f);
				fixed4 textureColor = tex2D(_MainTexture, float2(i.texcoord.x + _Time.x * 1.5f * _Speed, i.texcoord.y));

				return aColor * textureColor;
			}

			ENDCG
		}
	}
}
