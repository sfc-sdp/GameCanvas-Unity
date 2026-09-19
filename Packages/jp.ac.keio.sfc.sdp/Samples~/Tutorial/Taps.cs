#nullable enable
using GameCanvas;

public sealed class Taps : GameBase
{
    int count;
    float lastX, lastY;

    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);
    }

    public override void UpdateGame()
    {
        for (int i = 0; i < gc.Taps.Count; i++)
        {
            var p = gc.Taps[i];
            lastX = p.X;
            lastY = p.Y;
            count++;
        }
    }

    public override void DrawGame()
    {
        gc.ClearScreen();
        gc.SetColor(0, 0, 0);
        gc.SetFontSize(32);
        gc.DrawString($"タップ {count} 回", 40, 40);
        if (count > 0)
        {
            gc.SetRectAnchor(GcAnchor.MiddleCenter);
            gc.SetColor(40, 100, 230);
            gc.FillRect(lastX, lastY, 40, 40);
        }
    }
}
