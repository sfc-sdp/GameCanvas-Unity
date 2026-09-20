#nullable enable
using System;
using GameCanvas;

public sealed class GeolocationSample : GameBase
{
    const int k_TileSize = 256;
    const int k_CanvasW = k_TileSize * 3;
    const int k_CanvasH = k_TileSize * 3;
    const int k_ZoomLv = 18;

    string m_StateMessage = "画面を押すと位置情報を取ります";
    bool m_HasFix;
    bool m_IsMock;
    int m_TileX;
    int m_TileY;
    float m_PointX;
    float m_PointY;
    readonly GcImageRequest?[] m_Tiles = new GcImageRequest?[9];

    public override void InitGame()
    {
        gc.ChangeCanvasSize(k_CanvasW, k_CanvasH);
        gc.SetFontSize(36);
        gc.SetBackgroundColor(255, 255, 255);
        ClearTiles();
        m_HasFix = false;
        m_IsMock = false;
        m_StateMessage = "画面を押すと位置情報を取ります";
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
                ClearTiles();
                gc.Location.Start();
            }
        }

        if (gc.Location.TryGetSample(out var sample))
        {
            m_HasFix = true;
            m_IsMock = sample.IsMock;
            m_StateMessage = $"緯度 {sample.Latitude:F6}、経度 {sample.Longitude:F6}";
            if (m_IsMock) m_StateMessage += "\n模擬の位置です";
            CalcTileId(sample.Latitude, sample.Longitude, k_ZoomLv, out m_TileX, out m_TileY, out m_PointX, out m_PointY);
            gc.Location.Stop();
            UpdateTiles();
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
            gc.SetColor(255, 255, 255);
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    var tile = m_Tiles[i + j * 3];
                    if (tile == null || tile.Status != GcRequestState.Succeeded) continue;
                    gc.DrawImage(tile, i * k_TileSize, j * k_TileSize);
                }
            }

            using (gc.StyleScope)
            {
                gc.SetRectAnchor(GcAnchor.LowerCenter);
                gc.DrawImage("MapPin.png", m_PointX + k_TileSize, m_PointY + k_TileSize);
                gc.SetColor(0, 0, 0);
                gc.SetStringAnchor(GcAnchor.LowerRight);
                gc.DrawString("出典：国土地理院", k_CanvasW, k_CanvasH);
            }
        }

        gc.SetColor(0, 0, 0);
        gc.DrawString(m_StateMessage, 10, 15);
    }

    void UpdateTiles()
    {
        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                var index = i + j * 3;
                var x = m_TileX - 1 + i;
                var y = m_TileY - 1 + j;
                var url = $"https://cyberjapandata.gsi.go.jp/xyz/std/{k_ZoomLv}/{x}/{y}.png";
                var current = m_Tiles[index];
                if (current != null && current.Url != url)
                {
                    current.Dispose();
                    m_Tiles[index] = null;
                }
                if (m_Tiles[index] == null)
                {
                    m_Tiles[index] = gc.Network.GetImage(url);
                }
            }
        }
    }

    void ClearTiles()
    {
        for (var i = 0; i < m_Tiles.Length; i++)
        {
            m_Tiles[i]?.Dispose();
            m_Tiles[i] = null;
        }
    }

    /// <summary>緯度経度からタイル座標への変換</summary>
    /// <remarks><see href="https://www.trail-note.net/tech/coordinate/"/></remarks>
    static void CalcTileId(in double lat, in double lng, in int zoom, out int tileX, out int tileY, out float pointX, out float pointY)
    {
        const double L = 85.05112878;
        var a = (int)Math.Pow(2, zoom + 7);
        var b = Math.PI / 180.0;
        var x = (int)(a * (lng / 180.0 + 1.0));
        var y = (int)((a / Math.PI) * (-Atanh(Math.Sin(b * lat)) + Atanh(Math.Sin(b * L))));
        tileX = x / k_TileSize;
        tileY = y / k_TileSize;
        pointX = x % k_TileSize;
        pointY = y % k_TileSize;

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
