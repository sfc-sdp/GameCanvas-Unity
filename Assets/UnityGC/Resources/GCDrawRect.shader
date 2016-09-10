Shader "Custom/GameCanvas/DrawRect" {
	Properties{
		_MainTex ("キャンバス", 2D) = "white" {}
		_Color ("パレットカラー", Color) = (0, 0, 0, 1)
		_IsFill ("塗りつぶすかどうか", Float) = 1
		_LineWidth("線の太さ", Float) = 1
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
			float4x4 _Matrix;

			fixed4 frag(v2f_img i) : COLOR
			{
				float2 pb = i.uv * _MainTex_TexelSize.zw;   // 処理するピクセル座標
				float2 pa = mul(_Matrix, float4(pb, 0, 1)); // 対応する画像データのピクセル座標

				half inside = pa.x >= 0 && pa.x <= 1 && pa.y >= 0 && pa.y <= 1;
				if ( inside && (_IsFill || ( !_IsFill && (abs(pa.x) <= _MainTex_TexelSize.x || abs(pa.x - 1) <= _MainTex_TexelSize.x)
								                      || (abs(pa.y) <= _MainTex_TexelSize.y || abs(pa.y - 1) <= _MainTex_TexelSize.y) )) )
				{
					if (_Color.a != 1)
						return fixed4(lerp(tex2D(_MainTex, i.uv).rgb, _Color.rgb, _Color.a), 1);
					else
						return _Color;
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
