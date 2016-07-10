using System;
using GameCanvas;

public class Game : GameBase
{
    override public void Start()
    {
        //
    }

    override public void Calc()
    {
        //
    }

    override public void Draw()
    {
        // 画面を白で塗りつぶします
        gc.ClearScreen();

        // 2番の画像を描画します
        gc.DrawImage(2, 0, 0);

        // 赤い長方形（塗り）を描画します
        gc.SetColor(255, 0, 0);
        gc.FillRect(100, 100, 120, 120);

        // 黒い長方形（線）を描画します
        gc.SetColor(0, 0, 0);
        gc.DrawRect(40, 40, 440, 280);

        // 青い直線を描画します
        gc.SetColor(0, 0, 255);
        gc.DrawLine(60, 380, 220, 10);

        // 緑の円を描画します
        gc.SetColor(0, 64, 0);
        gc.DrawCircle(320, 240, 200);
    }

    override public void Pause()
    {
        //
    }

    override public void Final()
    {
        //
    }
}
