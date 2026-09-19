#nullable enable
using System;
using GameCanvas;
using Unity.Mathematics;

public sealed class GeolocationSample : GameBase
{
    const int k_TileSize = 256;
    const int k_CanvasW = k_TileSize * 3;
    const int k_CanvasH = k_TileSize * 3;
    const int k_ZoomLv = 18;

    string m_StateMessage = "画面を押すと位置情報を取ります";
    bool m_HasFix;
    bool m_IsMock;
    int2 m_TileId;
    float2 m_Point;

    public override void InitGame()
    {
        gc.ChangeCanvasSize(k_CanvasW, k_CanvasH);
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
                m_HasFix = false;
                gc.Location.Start();
            }
        }

        if (gc.Location.TryGetSample(out var sample))
        {
            m_HasFix = true;
            m_IsMock = sample.IsMock;
            m_StateMessage = $"緯度 {sample.Latitude:F6}、経度 {sample.Longitude:F6}";
            if (m_IsMock) m_StateMessage += "\n模擬の位置です";
            CalcTileId(sample.Latitude, sample.Longitude, k_ZoomLv, out m_TileId, out m_Point);
            gc.Location.Stop();
        }
        else if (!m_HasFix)
        {
            m_StateMessage = ShowState(gc.Location.Status);
        }
    }

    public override void DrawGame()
    {
        gc.ClearScreen();

        if (m_HasFix)
        {
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    var x = m_TileId.x - 1 + i;
                    var y = m_TileId.y - 1 + j;
                    var url = $"https://cyberjapandata.gsi.go.jp/xyz/std/{k_ZoomLv}/{x}/{y}.png";
                    gc.DrawOnlineImage(url, i * k_TileSize, j * k_TileSize);
                }
            }

            gc.DrawImage("MapPin.png", m_Point.x + k_TileSize, m_Point.y + k_TileSize, anchor: GcAnchor.LowerCenter);
            gc.SetColor(0, 0, 0);
            gc.DrawString("出典：国土地理院", k_CanvasW, k_CanvasH, anchor: GcAnchor.LowerRight);
        }

        gc.SetColor(0, 0, 0);
        gc.DrawString(m_StateMessage, 10, 15);
    }

    /// <summary>緯度経度からタイル座標への変換</summary>
    /// <remarks><see href="https://www.trail-note.net/tech/coordinate/"/></remarks>
    static void CalcTileId(in double lat, in double lng, in int zoom, out int2 tileId, out float2 point)
    {
        const double L = 85.05112878;
        var a = (int)Math.Pow(2, zoom + 7);
        var b = Math.PI / 180.0;
        var x = (int)(a * (lng / 180.0 + 1.0));
        var y = (int)((a / Math.PI) * (-Atanh(Math.Sin(b * lat)) + Atanh(Math.Sin(b * L))));
        tileId = new int2(x / k_TileSize, y / k_TileSize);
        point = new float2(x % k_TileSize, y % k_TileSize);

        static double Atanh(in double value) => 0.5 * Math.Log((1 + value) / (1 - value));
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
