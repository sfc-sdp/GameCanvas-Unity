Shader "Custom/GameCanvas/Draw" {
    Properties{
        _MainTex ("キャンバス", 2D) = "white" {}
        _DrawCalls ("描画命令数 (最多24)", Float) = 0
    }
    SubShader {
        Pass {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;         // キャンバス
            half4 _MainTex_TexelSize;   // キャンバス情報
            fixed     _DrawCalls;       // 描画命令数 (最多24)

            fixed     _Type[24];        // 描画種別 (0: 中抜き円, 1: 円塗り, 2: 中抜き矩形, 3: 矩形塗り)
            fixed4    _Color[24];       // 基本描画色
            float4x4  _Matrix[24];      // マトリックス
            float     _LineWidth[24*2]; // 線の幅 (中抜きのみ)

            fixed4 frag(v2f_img img) : COLOR
            {
                float2 mainXY = img.uv * _MainTex_TexelSize.zw; // 処理するピクセル座標
                fixed4 ret = tex2D(_MainTex, img.uv);

                for (fixed i = 0; i < _DrawCalls; ++i)
                {
                    float2 scaledXY = mul(_Matrix[i], float4(mainXY, 0, 1)); // 対応する画像データのピクセル座標

                    if (_Type[i] == 0)
                    {
                        // 中抜き円
                        float lineWidth = _LineWidth[i*2] * 0.5;
                        float radius = length(scaledXY);
                        if (abs(radius - 1) <= lineWidth)
                        {
                            ret = fixed4(lerp(ret.rgb, _Color[i].rgb, _Color[i].a), 1);
                        }
                    }
                    else if (_Type[i] == 1)
                    {
                        // 円塗りつぶし
                        float radius = length(scaledXY);
                        if (radius <= 1)
                        {
                            ret = fixed4(lerp(ret.rgb, _Color[i].rgb, _Color[i].a), 1);
                        }
                    }
                    else if (_Type[i] == 2)
                    {
                        // 中抜き矩形
                        float lineWidthX = _LineWidth[i * 2] * 0.5;
                        float lineWidthY = _LineWidth[i * 2 + 1] * 0.5;
                        if (( (abs(scaledXY.x) <= lineWidthX || abs(scaledXY.x - 1) <= lineWidthX) && scaledXY.y + lineWidthY >= 0.0 && scaledXY.y - lineWidthY <= 1.0 ) ||
                            ( (abs(scaledXY.y) <= lineWidthY || abs(scaledXY.y - 1) <= lineWidthY) && scaledXY.x + lineWidthX >= 0.0 && scaledXY.x - lineWidthX <= 1.0) )
                        {
                            ret = fixed4(lerp(ret.rgb, _Color[i].rgb, _Color[i].a), 1);
                        }
                    }
                    else if (_Type[i] == 3)
                    {
                        // 矩形塗りつぶし
                        if (scaledXY.x >= 0 && scaledXY.x <= 1 && scaledXY.y >= 0 && scaledXY.y <= 1)
                        {
                            ret = fixed4(lerp(ret.rgb, _Color[i].rgb, _Color[i].a), 1);
                        }
                    }
                }

                return ret;
            }
            ENDCG
        }
    }
}
