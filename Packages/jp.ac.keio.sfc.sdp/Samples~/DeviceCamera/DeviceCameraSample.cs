#nullable enable
using GameCanvas;

public sealed class DeviceCameraSample : GameBase
{
    string message = "画面を押すとカメラを開始します";

    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);
        gc.SetFontSize(36);
    }

    public override void UpdateGame()
    {
        if (gc.Pointer.Down)
        {
            var state = gc.Camera.Status;
            if (state == GcCameraState.Running ||
                state == GcCameraState.RequestingPermission ||
                state == GcCameraState.Waiting)
            {
                gc.Camera.Stop();
            }
            else
            {
                gc.Camera.Start();
            }
        }

        if (gc.Camera.Status == GcCameraState.Running)
        {
            message = $"{gc.Camera.Width}x{gc.Camera.Height}";
            var device = gc.Camera.Device;
            if (device != null) message = $"{device.DeviceName}\n{message}";
            message += "\n画面を押すと停止します";
        }
        else
        {
            message = ShowState(gc.Camera.Status);
        }
    }

    public override void DrawGame()
    {
        gc.ClearScreen();
        if (gc.Camera.Status == GcCameraState.Running)
        {
            gc.SetColor(255, 255, 255);
            var scale = 680f / gc.Camera.Width;
            if (gc.Camera.Height * scale > 980f)
            {
                scale = 980f / gc.Camera.Height;
            }
            gc.DrawCamera(20, 240, gc.Camera.Width * scale, gc.Camera.Height * scale);
        }
        gc.SetColor(0, 0, 0);
        gc.DrawString(message, 40, 80);
    }

    static string ShowState(GcCameraState state) => state switch
    {
        GcCameraState.RequestingPermission => "カメラの許可を確認中です\n画面を押すと取り消します",
        GcCameraState.Waiting => "カメラ映像を待っています\n画面を押すと取り消します",
        GcCameraState.NotGranted => "カメラが許可されていません\n画面を押すと再試行できます",
        GcCameraState.NoDevice => "カメラがありません",
        GcCameraState.TimedOut => "時間内にカメラを開始できませんでした\n画面を押すと再試行できます",
        GcCameraState.Unsupported => "この環境ではカメラを使えません",
        GcCameraState.Failed => "カメラの開始に失敗しました\n画面を押すと再試行できます",
        GcCameraState.Stopped => "停止中です。画面を押すと再開できます",
        GcCameraState.Idle => "画面を押すとカメラを開始します",
        _ => "カメラを確認中です"
    };
}
