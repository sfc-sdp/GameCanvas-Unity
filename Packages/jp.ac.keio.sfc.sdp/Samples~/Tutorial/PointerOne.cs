#nullable enable
using GameCanvas;

public sealed class PointerOne : GameBase
{
    float x = 100, y = 300;
    float offsetX, offsetY, originalX, originalY;
    int? dragId;

    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);
    }

    public override void UpdateGame()
    {
        var p = gc.Pointer;
        if (p.Down && p.StartX >= x && p.StartX < x + 180 &&
                      p.StartY >= y && p.StartY < y + 180)
        {
            dragId = p.Id;
            offsetX = p.StartX - x;
            offsetY = p.StartY - y;
            originalX = x;
            originalY = y;
        }
        if (dragId != p.Id) return;
        if (p.Held || p.Up)
        {
            x = p.X - offsetX;
            y = p.Y - offsetY;
        }
        if (p.Cancelled)
        {
            x = originalX;
            y = originalY;
        }
        if (p.Up || p.Cancelled) dragId = null;
    }

    public override void DrawGame()
    {
        gc.ClearScreen();
        gc.SetRectAnchor(GcAnchor.UpperLeft);
        gc.SetColor(40, 100, 230);
        gc.FillRect(x, y, 180, 180);
    }
}
