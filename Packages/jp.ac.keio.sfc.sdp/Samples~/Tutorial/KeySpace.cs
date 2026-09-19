#nullable enable
using GameCanvas;

public sealed class KeySpace : GameBase
{
    float y = 600;

    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);
    }

    public override void UpdateGame()
    {
        var space = gc.Key(GcKey.Space);
        if (space.Down) y -= 80;
        y += 2;
        if (y > 600) y = 600;
        if (space.Cancelled) y = 600;
    }

    public override void DrawGame()
    {
        gc.ClearScreen();
        gc.SetRectAnchor(GcAnchor.UpperLeft);
        gc.SetColor(40, 100, 230);
        gc.FillRect(270, y, 180, 180);
        gc.SetColor(0, 0, 0);
        gc.SetFontSize(32);
        var space = gc.Key(GcKey.Space);
        gc.DrawString($"Space Held={space.Held} / {space.Duration:0.00}s", 40, 40);
    }
}
