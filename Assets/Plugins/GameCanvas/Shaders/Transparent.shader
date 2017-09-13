// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

//------------------------------------------------------------//
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2017 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
//------------------------------------------------------------//

Shader "GameCanvas/Transparent" {
	Properties {
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_ClipRect ("Clip Rect", Vector) = (-720, -1280, 720, 1280)
	}

	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0

				#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float2 uv: TEXCOORD0;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					float2 uv: TEXCOORD0;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float4 _ClipRect;

				inline float World2dClip (in float2 position, in float4 clipRect)
				{
 					float2 inside = step(clipRect.xy, position.xy) * step(position.xy, clipRect.zw);
 					return inside.x * inside.y;
				}

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					return o;
				}

				fixed4 frag (v2f i) : SV_Target
				{
					fixed4 c = tex2D(_MainTex, i.uv);
					c.a *= World2dClip(i.vertex.xy, _ClipRect);
					clip(c.a - 0.001);
					return c;
				}
			ENDCG
		}
	}
}