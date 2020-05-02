//------------------------------------------------------------//
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
//------------------------------------------------------------//
Shader "GameCanvas/TransparentImage" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Multiply", Color) = (1, 1, 1, 1)
		_ClipRect ("Clip Rect", Vector) = (0, 0, 1, 1)
	}

	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 100

		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0

				#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float4 color : COLOR;
					float2 uv : TEXCOORD0;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					float4 color : COLOR;
					float2 uv : TEXCOORD0;
					float2 screen : TEXCOORD1;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				fixed4 _Color;
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
					o.color = v.color;
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					o.screen = ComputeScreenPos(o.vertex);
					return o;
				}

				fixed4 frag (v2f i) : SV_Target
				{
					fixed4 c = tex2D(_MainTex, i.uv) * _Color;
					c.a *= World2dClip(i.screen.xy, _ClipRect);
					clip(c.a - 0.001);
					return fixed4(c.rgb + i.color.rgb, c.a);
				}
			ENDCG
		}
	}
}
