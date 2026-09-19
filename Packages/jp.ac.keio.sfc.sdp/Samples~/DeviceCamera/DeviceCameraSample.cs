#nullable enable
using GameCanvas;

public sealed class DeviceCameraSample : GameBase
{
    GcCameraDevice? camera;
    bool asking;
    bool playing;
    int requestId;
    string message = "画面を押すとカメラを開始します";

    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);
        gc.SetFontSize(36);
    }

    public override void UpdateGame()
    {
        if (asking || playing || !gc.Pointer.Down) return;
        asking = true;
        var id = ++requestId;
        if (gc.HasUserAuthorizedPermissionCamera)
        {
            Play(id);
            return;
        }
        gc.RequestUserAuthorizedPermissionCameraAsync(ok =>
        {
            if (id != requestId) return;
            if (ok) Play(id);
            else
            {
                asking = false;
                message = "カメラが許可されていません。画面を押すと再試行できます";
            }
        });
    }

    void Play(int id)
    {
        if (id != requestId) return;
        asking = false;
        if (!gc.TryGetCameraImage(out var device))
        {
            camera = null;
            playing = false;
            message = "カメラがありません";
            return;
        }
        camera = device;
        if (!gc.PlayCameraImage(device, out var size))
        {
            gc.StopCameraImage(device);
            camera = null;
            playing = false;
            message = "カメラを開始できませんでした。画面を押すと再試行できます";
            return;
        }
        gc.ChangeCanvasSize(size.x, size.y);
        playing = true;
        message = $"{device.DeviceName}\n({size.x}x{size.y})";
        if (gc.IsFlippedCameraImage(device)) message += "\nFlipped";
        if (gc.TryGetCameraImageRotation(device, out var rotation) && rotation != 0f) message += $"\nrotation {rotation}";
    }

    public override void DrawGame()
    {
        gc.ClearScreen();
        if (playing && camera != null) gc.DrawCameraImage(camera);
        if (playing)
        {
            gc.SetColor(gc.ColorBlack);
            gc.DrawString(message, 12, 18);
            gc.SetColor(gc.ColorWhite);
            gc.DrawString(message, 10, 15);
        }
        else
        {
            gc.SetColor(gc.ColorBlack);
            gc.DrawString(message, 10, 15);
        }
    }

    public override void PauseGame()
    {
        requestId++;
        if (camera != null) gc.StopCameraImage(camera);
        playing = false;
        asking = false;
        message = "中断しました。画面を押すと再開できます";
    }
}
