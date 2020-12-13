using GameCanvas;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public sealed class Game : GameBase
{
    enum State
    {
        Init,
        Running,
        Success,
        Fail
    }

    const int k_TileSize = 256;
    const int k_CanvasW = k_TileSize * 3;
    const int k_CanvasH = k_TileSize * 3;
    const int k_ZoomLv = 18;

    private State m_State;
    private string m_StateMessage;
    private int2 m_TileId;
    private float2 m_Point;

    public override void InitGame()
    {
        gc.ChangeCanvasSize(k_CanvasW, k_CanvasH);
        gc.SetRectAnchor(GcAnchor.UpperLeft);
        gc.SetFontSize(36);

#if UNITY_EDITOR
        var lat = 35.38818385259218f;
        var lng = 139.42752479544862f;
        m_State = State.Success;
        m_StateMessage = $"lat: {lat:0.000000}, lng: {lng:0.000000}";
        CalcTileId(lat, lng, k_ZoomLv, out m_TileId, out m_Point);
#else
        m_State = State.Init;
        m_StateMessage = "初期化中...";

        if (gc.HasUserAuthorizedPermissionGeolocation)
        {
            StartGeolocationService();
        }
        else
        {
            gc.RequestUserAuthorizedPermissionGeolocationAsync(success =>
            {
                if (success)
                {
                    StartGeolocationService();
                }
                else
                {
                    m_State = State.Fail;
                    m_StateMessage = "Error: no permission";
                }
            });
        }
#endif // UNITY_EDITOR
    }

    public override void UpdateGame()
    {
        if (m_State == State.Running)
        {
            switch (gc.GeolocationStatus)
            {
                case LocationServiceStatus.Stopped:
                    m_State = State.Fail;
                    m_StateMessage = "Error: geolocation service stopped";
                    break;

                case LocationServiceStatus.Running:
                    if (gc.TryGetGeolocationEvent(out var e))
                    {
                        m_State = State.Success;
                        m_StateMessage = $"lat: {e.Latitude:0.000000}, lng: {e.Longitude:0.000000}";
                        CalcTileId(e.Latitude, e.Longitude, k_ZoomLv, out m_TileId, out m_Point);
                        gc.StopGeolocationService();
                    }
                    break;

                case LocationServiceStatus.Failed:
                    m_State = State.Fail;
                    m_StateMessage = "Error: geolocation service failed";
                    break;
            }
        }
    }

    public override void DrawGame()
    {
        gc.ClearScreen();

        if (m_State == State.Success)
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

            using (gc.StyleScope)
            {
                gc.SetRectAnchor(GcAnchor.LowerCenter);
                gc.DrawImage(GcImage.MapPin, m_Point + new float2(k_TileSize, k_TileSize));

                gc.SetStringAnchor(GcAnchor.LowerRight);
                gc.DrawString("出典：国土地理院", k_CanvasW, k_CanvasH);
            }
        }

        gc.DrawString(m_StateMessage, 10, 15);
    }

    private void StartGeolocationService()
    {
        m_State = State.Running;
        m_StateMessage = "計測中...";

        gc.StartGeolocationService();
    }

    /// <summary>緯度経度からタイル座標への変換</summary>
    /// <remarks><see href="https://www.trail-note.net/tech/coordinate/"/></remarks>
    private static void CalcTileId(in float lat, in float lng, in int zoom, out int2 tileId, out float2 point)
    {
        const double L = 85.05112878;
        var a = (int)math.pow(2, zoom + 7);
        var b = math.PI_DBL / 180;
        var x = (int)(a * (lng / 180 + 1));
        var y = (int)((a / math.PI) * (-Atanh(math.sin(b * lat)) + Atanh(math.sin(b * L))));
        tileId = new int2(x / k_TileSize, y / k_TileSize);
        point = new float2(x % k_TileSize, y % k_TileSize);
    }

    private static double Atanh(in double x) => 0.5 * math.log((1 + x) / (1 - x));
}
