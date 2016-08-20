using System;
using GameCanvas;
using UnityEngine;

public class Game : GameBase
{
    private int frame;

    override public void Start()
    {
        frame = 0;
    }

    override public void Calc()
    {
        ++frame;

        if (frame > 50) frame = 0;
    }

    override public void Draw()
    {
        // 画面を白で塗りつぶします
        gc.ClearScreen();

        // 2番の画像を描画します
        gc.DrawImage(2, 0, 0);

        // 0番の画像を描画します
        gc.DrawImage(0, 400, 160);

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
        gc.DrawCircle(320, 240, 150 + frame);

        // タッチ位置に赤い円を描画します
        if (gc.isTouch)
        {
            gc.SetColor(255, 0, 0);
            gc.DrawCircle(gc.touchX, gc.touchY, 10);
        }
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
