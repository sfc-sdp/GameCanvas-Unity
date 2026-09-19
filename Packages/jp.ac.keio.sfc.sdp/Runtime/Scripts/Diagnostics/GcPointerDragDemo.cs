#nullable enable
using UnityEngine;

namespace GameCanvas.Diagnostics
{
    /// <summary>添字で2本の指を処理する入力診断。通常の描画設定は変えない。</summary>
    internal sealed class GcPointerDragDemo
    {
        sealed class Box
        {
            public float X, Y, OffsetX, OffsetY, StartX, StartY;
            public int? Id;
        }
        readonly Box[] boxes = { new() { X = 100, Y = 450 }, new() { X = 400, Y = 450 } };
        string lastEvent = "四角形をそれぞれドラッグしてください";
        internal void Reset()
        {
            for (int i = 0; i < boxes.Length; i++)
            {
                var b = boxes[i];
                if (b.Id.HasValue) { b.X = b.StartX; b.Y = b.StartY; b.Id = null; }
            }
        }
        internal void Update(IGameCanvas gc)
        {
            for (int i = 0; i < gc.Pointers.Count; i++)
            {
                var p = gc.Pointers[i];
                if (p.Down)
                    for (int j = boxes.Length - 1; j >= 0; j--)
                    {
                        var b = boxes[j];
                        if (p.StartX < b.X || p.StartX >= b.X + 180 || p.StartY < b.Y || p.StartY >= b.Y + 180) continue;
                        if (b.Id.HasValue) break;
                        b.Id = p.Id; b.OffsetX = p.StartX - b.X; b.OffsetY = p.StartY - b.Y;
                        b.StartX = b.X; b.StartY = b.Y; break;
                    }
                for (int j = 0; j < boxes.Length; j++)
                {
                    var b = boxes[j];
                    if (b.Id != p.Id) continue;
                    if (p.Held || p.Up) { b.X = p.X - b.OffsetX; b.Y = p.Y - b.OffsetY; }
                    if (p.Cancelled) { b.X = b.StartX; b.Y = b.StartY; }
                    if (p.Up || p.Cancelled)
                    {
                        Debug.Log($"GC_POINTER_BOX index={j} id={p.Id} x={b.X:F1} y={b.Y:F1} cancelled={p.Cancelled}");
                        b.Id = null;
                    }
                }
            }
            for (int i = 0; i < gc.PointerEvents.Count; i++)
            {
                var e = gc.PointerEvents[i];
                if (e.Phase != GcPointerEventPhase.Begin && e.Phase != GcPointerEventPhase.End && e.Phase != GcPointerEventPhase.Cancelled) continue;
                lastEvent = $"{e.Kind} ID {e.Id} / {e.Phase} / ({e.X:F0}, {e.Y:F0})";
                Debug.Log($"GC_POINTER_EVENT frame={e.Frame} id={e.Id} kind={e.Kind} phase={e.Phase} x={e.X:F1} y={e.Y:F1}");
                for (int j = 0; j < gc.Pointers.Count; j++)
                {
                    var p = gc.Pointers[j];
                    Debug.Log($"GC_POINTER_STATE id={p.Id} kind={p.Kind} held={p.Held} present={p.Present}");
                }
            }
        }
        internal void Draw(IGameCanvas gc)
        {
            gc.SetRectAnchor(GcAnchor.UpperLeft);
            for (int j = 0; j < boxes.Length; j++)
            {
                gc.SetColor(j == 0 ? (byte)40 : (byte)230, (byte)100, j == 0 ? (byte)230 : (byte)40);
                gc.FillRect(boxes[j].X, boxes[j].Y, 180, 180);
            }
            gc.SetColor(0, 0, 0); gc.SetFontSize(26); gc.SetStringAnchor(GcAnchor.UpperLeft);
            gc.DrawString(lastEvent, 28, 280);
            gc.DrawString($"Pointers.Count = {gc.Pointers.Count} / 代表ID = {gc.Pointer.Id}", 28, 330);
            gc.DrawString("2本の指は別々の四角形を動かします", 28, 900);
            gc.DrawString("中断した操作は元の位置へ戻ります", 28, 950);
        }
    }
}
