#nullable enable
using GameCanvas;

public sealed class AccelerationSample : GameBase
{
    float x, y, vx, vy;
    string message = "画面を押すと加速度計を開始します";

    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);
        gc.SetFontSize(36);
        gc.SetRectAnchor(GcAnchor.MiddleCenter);
        gc.SetStringAnchor(GcAnchor.UpperLeft);
        x = gc.CanvasWidth * 0.5f;
        y = gc.CanvasHeight * 0.5f;
        vx = 0;
        vy = 0;
        message = "画面を押すと加速度計を開始します";
    }

    public override void UpdateGame()
    {
        if (gc.Pointer.Down)
        {
            var state = gc.Acceleration.Status;
            if (state == GcAccelerationState.Running || state == GcAccelerationState.Waiting)
            {
                gc.Acceleration.Stop();
            }
            else
            {
                gc.Acceleration.Start();
            }
        }

        if (gc.Acceleration.HasValue)
        {
            var dt = gc.TimeSincePrevFrame;
            vx += gc.Acceleration.X * 400f * dt;
            vy += gc.Acceleration.Y * 400f * dt;
            var decay = (float)System.Math.Pow(0.98, dt * 60);
            vx *= decay;
            vy *= decay;
            x += vx * dt;
            y += vy * dt;
            x = gc.Repeat(x, gc.CanvasWidth);
            y = gc.Repeat(y, gc.CanvasHeight);

            var last = gc.Acceleration.Last;
            var events = gc.Acceleration.Events;
            message = $"x {last.X:F2}\ny {last.Y:F2}\nz {last.Z:F2}\n標本 {events.Count}";
            for (int i = 0; i < events.Count; i++)
            {
                if (i >= 3)
                {
                    message += $"\nほか {events.Count - 3} 件";
                    break;
                }
                var e = events[i];
                message += $"\n[{i}] {e.Time:F3} dt {e.DeltaTime:F3}";
            }
        }
        else
        {
            vx = 0;
            vy = 0;
            message = ShowState(gc.Acceleration.Status);
        }
    }

    public override void DrawGame()
    {
        gc.ClearScreen();
        gc.SetColor(255, 255, 255);
        gc.DrawImage("BallRed.png", x, y);
        gc.SetColor(0, 0, 0);
        gc.DrawString(message, 40, 80);
    }

    static string ShowState(GcAccelerationState state) => state switch
    {
        GcAccelerationState.Waiting => "加速度の標本を待っています\n画面を押すと停止します",
        GcAccelerationState.Unsupported => "この環境では加速度計を使えません",
        GcAccelerationState.Failed => "加速度計が切れました\n画面を押すと再試行できます",
        GcAccelerationState.Stopped => "停止中です。画面を押すと再開できます",
        GcAccelerationState.Idle => "画面を押すと加速度計を開始します",
        _ => "加速度計を確認中です"
    };
}
