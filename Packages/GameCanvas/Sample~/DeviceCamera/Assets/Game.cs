using GameCanvas;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public sealed class Game : GameBase
{
    enum State
    {
        Init,
        Playing,
        NotFound,
        Fail
    }

    GcCameraDevice m_Camera;
    State m_State;
    string m_StateMessage;

    public override void InitGame()
    {
        gc.SetFontSize(36);

        m_State = State.Init;
        m_StateMessage = "初期化中...";

        if (gc.HasUserAuthorizedPermissionCamera)
        {
            PlayCamera();
        }
        else
        {
            gc.RequestUserAuthorizedPermissionCameraAsync(success =>
            {
                if (success)
                {
                    PlayCamera();
                }
                else
                {
                    m_State = State.Fail;
                    m_StateMessage = "Error: no permission";
                }
            });
        }
    }

    public override void DrawGame()
    {
        if (m_State == State.Playing)
        {
            if (!gc.DidUpdateCameraImageThisFrame(m_Camera)) return;

            gc.ClearScreen();
            gc.DrawCameraImage(m_Camera);
            gc.SetColor(gc.ColorBlack);
            gc.DrawString(m_StateMessage, 12, 18);
            gc.SetColor(gc.ColorWhite);
            gc.DrawString(m_StateMessage, 10, 15);
        }
        else
        {
            gc.ClearScreen();
            gc.SetColor(gc.ColorBlack);
            gc.DrawString(m_StateMessage, 10, 15);
        }
    }

    void PlayCamera()
    {
        if (gc.TryGetCameraImageAll(out var devices))
        {
            m_Camera = devices[0];
            gc.PlayCameraImage(m_Camera, new GcResolution(1280, 720, 30), out var size);
            gc.ChangeCanvasSize(size.x, size.y);

            m_State = State.Playing;
            m_StateMessage = $"{m_Camera.DeviceName}\n({size.x}x{size.y})";
            if (gc.IsFlippedCameraImage(m_Camera)) m_StateMessage += "\nFlipped";
            if (gc.TryGetCameraImageRotation(m_Camera, out var deg) && deg != 0f) m_StateMessage += $"\nRotate {deg}";
        }
        else
        {
            m_State = State.NotFound;
            m_StateMessage = "Warn: no camera";
        }
    }
}
