Shader "Custom/GameCanvas/DrawCircle" {
	Properties{
		_MainTex ("キャンバス", 2D) = "white" {}
		_Color ("パレットカラー", Color) = (0, 0, 0, 1)
		_IsFill ("塗りつぶすかどうか", Float) = 1
		_LineWidth ("線の太さ / 直径", Float) = 1
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			half4 _MainTex_TexelSize;
			fixed4 _Color;
			fixed _IsFill;
			float _LineWidth;
			float4x4 _Matrix;

			fixed4 frag(v2f_img i) : COLOR
			{
				float2 pb = i.uv * _MainTex_TexelSize.zw;   // 処理するピクセル座標
				float2 pa = mul(_Matrix, float4(pb, 0, 1)); // 対応する画像データのピクセル座標

				float d = length(pa);
				if ((_IsFill && d <= 1)  || (!_IsFill && abs(d - 1) <= _LineWidth))
					return _Color;
				else
					return tex2D(_MainTex, i.uv);
			}
			ENDCG
		}
	}
}
