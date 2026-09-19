#nullable enable
using GameCanvas;

public sealed class LocationTap : GameBase
{
    string message = "画面を押すと位置情報を取ります";

    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);
        gc.SetFontSize(36);
    }

    public override void UpdateGame()
    {
        if (gc.Pointer.Down)
        {
            var s = gc.Location.Status;
            if (s == GcLocationState.Idle || s == GcLocationState.Stopped ||
                s == GcLocationState.Failed || s == GcLocationState.TimedOut ||
                s == GcLocationState.NotGranted || s == GcLocationState.Disabled ||
                s == GcLocationState.Unsupported)
            {
                gc.Location.Start();
            }
        }

        if (gc.Location.TryGetSample(out var sample))
        {
            message = $"緯度 {sample.Latitude:F5}\n経度 {sample.Longitude:F5}";
            if (sample.IsMock) message += "\n模擬の位置です";
        }
        else
        {
            message = ShowState(gc.Location.Status);
        }
    }

    public override void DrawGame()
    {
        gc.ClearScreen();
        gc.SetColor(0, 0, 0);
        gc.DrawString(message, 40, 80);
    }

    static string ShowState(GcLocationState state) => state switch
    {
        GcLocationState.RequestingPermission => "位置情報の許可を確認中です",
        GcLocationState.Waiting => "位置情報を取得中です",
        GcLocationState.NotGranted => "位置情報が許可されていません",
        GcLocationState.Disabled => "端末の位置情報がオフです",
        GcLocationState.TimedOut => "時間内に位置情報を取得できませんでした",
        GcLocationState.Unsupported => "この環境では位置情報を使えません",
        GcLocationState.Failed => "位置情報の取得に失敗しました",
        GcLocationState.Stopped => "停止中です。画面を押すと再開できます",
        GcLocationState.Idle => "画面を押してください",
        _ => "位置情報を確認中です"
    };
}
