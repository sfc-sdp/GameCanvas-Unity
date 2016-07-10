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

        gc.SetColor(255, 0, 0);
        gc.FillRect(100, 100, 120, 120);

        gc.SetColor(0, 0, 255);
        gc.DrawLine(60, 380, 220, 10);

        gc.SetColor(0, 64, 0);
        gc.DrawCircle(320, 240, 200);

        // ここから、画像を表示する命令を記述していく
        //gc.drawImage(0, 0, 0);
        //gc.setColor(0, 0, 0);
        //gc.drawString("この文字と青空の画像が見えていれば成功です", 60, 220);
    }

    public override void Pause()
    {
        //
    }

    override public void Final()
    {
        //
    }
}
