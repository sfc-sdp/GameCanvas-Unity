Shader "Custom/GameCanvas/Init" {
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			fixed4 frag(v2f_img i) : COLOR
			{
				return 1;
			}
			ENDCG
		}
	}
}
