#nullable enable
using GameCanvas;

public sealed class PointerMany : GameBase
{
    sealed class Box
    {
        public float X, Y, OffsetX, OffsetY, OriginalX, OriginalY;
        public int? Id;
    }

    readonly Box[] boxes =
    {
        new Box { X = 100, Y = 450 },
        new Box { X = 400, Y = 450 }
    };

    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);
    }

    public override void UpdateGame()
    {
        for (int i = 0; i < gc.Pointers.Count; i++)
        {
            var p = gc.Pointers[i];
            if (p.Down)
            {
                for (int j = boxes.Length - 1; j >= 0; j--)
                {
                    var b = boxes[j];
                    if (p.StartX < b.X || p.StartX >= b.X + 180 ||
                        p.StartY < b.Y || p.StartY >= b.Y + 180) continue;
                    if (b.Id.HasValue) break;
                    b.Id = p.Id;
                    b.OffsetX = p.StartX - b.X;
                    b.OffsetY = p.StartY - b.Y;
                    b.OriginalX = b.X;
                    b.OriginalY = b.Y;
                    break;
                }
            }

            for (int j = 0; j < boxes.Length; j++)
            {
                var b = boxes[j];
                if (b.Id != p.Id) continue;
                if (p.Held || p.Up)
                {
                    b.X = p.X - b.OffsetX;
                    b.Y = p.Y - b.OffsetY;
                }
                if (p.Cancelled)
                {
                    b.X = b.OriginalX;
                    b.Y = b.OriginalY;
                }
                if (p.Up || p.Cancelled) b.Id = null;
            }
        }
    }

    public override void DrawGame()
    {
        gc.ClearScreen();
        gc.SetRectAnchor(GcAnchor.UpperLeft);
        gc.SetColor(40, 100, 230);
        gc.FillRect(boxes[0].X, boxes[0].Y, 180, 180);
        gc.SetColor(230, 100, 40);
        gc.FillRect(boxes[1].X, boxes[1].Y, 180, 180);
    }
}
