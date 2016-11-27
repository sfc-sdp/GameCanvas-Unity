Shader "Custom/GameCanvas/DrawString" {
	Properties{
		_MainTex ("キャンバス", 2D) = "white" {}
		_CharTex ("ビットマップフォント", 2D) = "white" {}
		_Color("パレットカラー", Color) = (1, 1, 1, 1)
		_TextLength("文字長 (最長128)", Float) = 0
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex, _CharTex;
			half4 _MainTex_TexelSize, _CharTex_TexelSize;
			fixed4 _Color;
			float4x4 _Matrix;
			half _TextLength;
			half _Text[128];

			fixed4 frag(v2f_img i) : COLOR
			{
				float2 pb = i.uv * _MainTex_TexelSize.zw;   // 処理するピクセル座標
				float2 pa = mul(_Matrix, float4(pb, 0, 1)); // 対応するビットマップフォントのピクセル座標
				
				if (pa.x >= 0 && pa.x <= 10 * _TextLength && pa.y >= 0 && pa.y <= 11)
				{
					float index = floor(pa.x * 0.1);
					float n = _Text[index];

					float sx = floor(pa.x - 10 * index);
					float sy = floor(pa.y);
					float py = 11 * floor(n * 0.04) + pa.y;
					float px = 10 * fmod(n, 25) + (pa.x - 10 * index);
					float2 uv = float2(px * _CharTex_TexelSize.x, 1 - py * _CharTex_TexelSize.y);

					if (tex2D(_CharTex, uv).r != 0)
					{
						if (_Color.a != 1)
							return fixed4(lerp(tex2D(_MainTex, i.uv).rgb, _Color.rgb, _Color.a), 1);
						else
							return _Color;
					}
				}

				return tex2D(_MainTex, i.uv);
			}
			ENDCG
		}
	}
}
