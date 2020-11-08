using GameCanvas;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public sealed class Game : GameBase
{
    struct Ball
    {
        public float2 Point;
        public float2 Speed;
    }

    Ball m_Ball;
    Color m_Color;
    string m_DebugText;

    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);
        gc.SetFontSize(36);
        gc.SetRectAnchor(GcAnchor.MiddleCenter);

        m_Ball = new Ball
        {
            Point = gc.CanvasCenter,
            Speed = float2.zero
        };
    }

    public override void UpdateGame()
    {
        var accel = float2.zero;
        foreach (var e in gc.AccelerationEvents)
        {
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

        gc.DrawImage(GcImage.BallRed, m_Ball.Point);

        gc.SetColor(m_Color);
        gc.DrawString(m_DebugText, 20, 20);
    }
}
