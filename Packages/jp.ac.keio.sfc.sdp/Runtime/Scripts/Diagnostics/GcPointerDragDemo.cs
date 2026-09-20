#nullable enable
using UnityEngine;

namespace GameCanvas.Diagnostics
{
    /// <summary>添字で2本の指を処理する入力診断。通常の描画設定は変えない。</summary>
    internal sealed class GcPointerDragDemo
    {
        sealed class Box
        {
            public GcRect Rect;
            public readonly GcDrag Drag = new();
        }
        readonly Box[] boxes = { new() { Rect = new(100, 450, 180, 180) }, new() { Rect = new(400, 450, 180, 180) } };
        string lastEvent = "四角形をそれぞれドラッグしてください";
        internal void Reset()
        {
            for (int i = 0; i < boxes.Length; i++) boxes[i].Drag.Cancel(ref boxes[i].Rect);
        }
        internal void Update(IGameCanvas gc)
        {
            gc.SetRectAnchor(GcAnchor.UpperLeft);
            for (int i = 0; i < boxes.Length; i++)
            {
                var box = boxes[i];
                gc.Drag(box.Drag, ref box.Rect);
                if (box.Drag.Ended || box.Drag.Cancelled)
                    Debug.Log($"GC_POINTER_BOX index={i} x={box.Rect.X:F1} y={box.Rect.Y:F1} cancelled={box.Drag.Cancelled}");
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
                gc.FillRect(boxes[j].Rect);
            }
            gc.SetColor(0, 0, 0); gc.SetFontSize(26); gc.SetStringAnchor(GcAnchor.UpperLeft);
            gc.DrawString(lastEvent, 28, 280);
            gc.DrawString($"Pointers.Count = {gc.Pointers.Count} / 代表ID = {gc.Pointer.Id}", 28, 330);
            gc.DrawString("2本の指は別々の四角形を動かします", 28, 900);
            gc.DrawString("中断した操作は元の位置へ戻ります", 28, 950);
        }
    }
}
