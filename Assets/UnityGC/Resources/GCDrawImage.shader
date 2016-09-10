Shader "Custom/GameCanvas/DrawImage" {
	Properties{
		_MainTex ("キャンバス", 2D) = "white" {}
		_ImageTex("画像データ", 2D) = "white" {}
		_Color("パレットカラー", Color) = (1, 1, 1, 1)
		_Clip("切り抜き範囲 (left, top, right, bottom)", Vector) = (0, 0, 0, 0)
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex, _ImageTex;
			half4 _MainTex_TexelSize, _ImageTex_TexelSize;
			fixed4 _Color;
			float4 _Clip;
			float4x4 _Matrix;

			fixed4 frag(v2f_img i) : COLOR
			{
				float2 pm = i.uv * _MainTex_TexelSize.zw;              // 処理するピクセル座標
				float2 pi = mul(_Matrix, float4(pm, 0, 1)) + _Clip.xy; // 対応する画像データのピクセル座標
				float2 si = _ImageTex_TexelSize.zw - _Clip.zw;         // 右下の画像ピクセル座標

				if (pi.x >= 0 && pi.y >= 0 && pi.x < si.x && pi.y < si.y)
				{
					float2 uv = float2(pi.x * _ImageTex_TexelSize.x, 1 - pi.y * _ImageTex_TexelSize.y);
					fixed4 c = tex2D(_ImageTex, uv);
					if (c.a != 1)
						return fixed4(lerp(tex2D(_MainTex, i.uv).rgb, c.rgb, c.a), 1);
					else
						return c;
				}
				else
				{
					return tex2D(_MainTex, i.uv);
				}
			}
			ENDCG
		}
	}
}
