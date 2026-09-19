#nullable enable
using GameCanvas;
using Unity.Mathematics;
using UnityEngine;

public sealed class AccelerationSample : GameBase
{
    struct Ball
    {
        public float2 Point;
        public float2 Speed;
    }

    Ball m_Ball;
    Color m_Color;
    string m_DebugText = "";
    bool m_Supported;

    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);
        gc.SetFontSize(36);
        gc.SetRectAnchor(GcAnchor.MiddleCenter);
        m_Supported = gc.IsAccelerometerSupported;
        if (m_Supported) gc.IsAccelerometerEnabled = true;

        m_Ball = new Ball
        {
            Point = gc.CanvasCenter,
            Speed = float2.zero
        };
        m_Color = gc.ColorBlack;
        if (!m_Supported) m_DebugText = "この端末では加速度計を使えません";
    }

    public override void UpdateGame()
    {
        if (!m_Supported) return;

        var accel = float2.zero;
        var events = gc.AccelerationEvents;
        for (int i = 0; i < events.Length; i++)
        {
            var e = events[i];
            accel += new float2(e.Acceleration.x, e.Acceleration.y) * e.DeltaTime;
        }
        m_Ball.Speed += accel * 20;

        m_Ball.Point += m_Ball.Speed;
        m_Ball.Point.x = gc.Repeat(m_Ball.Point.x, gc.CanvasWidth);
        m_Ball.Point.y = gc.Repeat(m_Ball.Point.y, gc.CanvasHeight);

        m_Ball.Speed *= 0.9f;

        m_Color = gc.DidUpdateAccelerationThisFrame
            ? gc.ColorBlack
            : gc.ColorGray;

        var last = gc.LastAccelerationEvent;
        m_DebugText = $" x: {last.Acceleration.x:+0.00;-0.00;0}\n y: {last.Acceleration.y:+0.00;-0.00;0}\n z: {last.Acceleration.z:+0.00;-0.00;0}\ndt: {last.DeltaTime:0.000}";
    }

    public override void DrawGame()
    {
        gc.ClearScreen();
        gc.DrawImage("BallRed.png", m_Ball.Point.x, m_Ball.Point.y);
        gc.SetColor(m_Color);
        gc.DrawString(m_DebugText, 20, 20);
    }
}
