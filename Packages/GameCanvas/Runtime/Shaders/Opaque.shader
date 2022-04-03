//------------------------------------------------------------//
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2022 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
//------------------------------------------------------------//
Shader "GameCanvas/Opaque" {
	Properties {
		_Color ("Color", Color) = (0, 0, 0, 0)
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100
		Cull Off

		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0

				#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
				};

				fixed4 _Color;

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					return o;
				}

				fixed4 frag (v2f i) : COLOR
				{
					clip(_Color.a - 0.001);
					UNITY_OPAQUE_ALPHA(_Color.a);
					return _Color;
				}
			ENDCG
		}
	}
}
