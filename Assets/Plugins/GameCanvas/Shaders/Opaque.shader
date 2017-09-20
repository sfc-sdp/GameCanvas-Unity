//------------------------------------------------------------//
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2017 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
//------------------------------------------------------------//

Shader "GameCanvas/Opaque" {
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_fog

				#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float4 color : COLOR;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					float4 color : COLOR;
				};

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.color = v.color;
					return o;
				}

				fixed4 frag (v2f i) : COLOR
				{
					fixed4 c = i.color;
					clip(c.a - 0.001);
					UNITY_OPAQUE_ALPHA(c.a);
					return c;
				}
			ENDCG
		}
	}
}
