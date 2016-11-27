/*------------------------------------------------------------*/
/// <summary>GameCanvas Particle Example</summary>
/// <author>Seibe TAKAHASHI</author>
/// <remarks>
/// (c) 2015-2016 Smart Device Programming.
/// This software is released under the MIT License.
/// http://opensource.org/licenses/mit-license.php
/// </remarks>
/*------------------------------------------------------------*/
using GameCanvas;

public class ParticleExample : GameBase
{
    int degree;

    override public void Start()
    {
        // ゲーム解像度を 320x320 に設定します
        gc.SetResolution(320, 320);

        // 回転角を0にリセットします
        degree = 0;
    }

    override public void Calc()
    {
        // 回転角を3度ずつ増やします
        degree += 3;

        // 360度以上になったら巻き戻します
        if (degree >= 360)
        {
            degree -= 360;
        }
    }

    override public void Draw()
    {
        // 画面を白で塗りつぶします
        gc.ClearScreen();

        // 背景（中抜きされた円）を描画します
        int circleX, circleY;
        for (var x = 0; x < 11; ++x)
        {
            for (var y = 0; y < 11; ++y)
            {
                circleX = x * 32;
                circleY = y * 32;

                gc.SetColor(0.9f, 0.9f, 0.9f);
                gc.DrawCircle(circleX, circleY, 20);
            }
        }

        // パーティクル（塗りつぶされた円）を描画します
        for (var x = 0; x < 11; ++x)
        {
            for (var y = 0; y < 11; ++y)
            {
                var localDegree = degree + 30 * (x + y);
                circleX = x * 32 + (int)(20 * gc.Cos(localDegree));
                circleY = y * 32 + (int)(20 * gc.Sin(localDegree));

                var lightness = (localDegree % 360) / 1080f;
                gc.SetColor(lightness, lightness, lightness);
                gc.FillCircle(circleX, circleY, 6);
            }
        }
    }
}
