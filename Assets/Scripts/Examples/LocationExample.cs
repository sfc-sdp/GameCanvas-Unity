/*------------------------------------------------------------*/
/// <summary>GameCanvas Location Example</summary>
/// <author>Seibe TAKAHASHI</author>
/// <remarks>
/// (c) 2015-2016 Smart Device Programming.
/// This software is released under the MIT License.
/// http://opensource.org/licenses/mit-license.php
/// </remarks>
/*------------------------------------------------------------*/
using GameCanvas;

public class LocationExample : GameBase
{
    float lat, lng;

    override public void Start()
    {
        // GPSを有効化します
        gc.StartLocationService();

        lat = 35.685410f;
        lng = 139.752842f;
    }

    public override void Calc()
    {
        // 位置情報が有効かどうか
        if (gc.isLocationEnabled && gc.isRunningLocaltionService)
        {
            lat = gc.lastLocationLatitude;
            lng = gc.lastLocationLongitude;
        }
    }

    override public void Draw()
    {
        // 画面を白で塗りつぶします
        gc.ClearScreen();

        // 地図を描画します
        var url = string.Format(
            "http://maps.googleapis.com/maps/api/staticmap?center={0},{1}&zoom=15&format=png&sensor=false&size={2}x{3}&maptype=roadmap&markers={0},{1}",
            lat, lng, gc.screenWidth, gc.screenHeight
        );
        gc.DrawImageFromNet(url, 0, 0);
        
        if (!gc.isLocationEnabled || !gc.isRunningLocaltionService)
        {
            // エラー表示
            gc.DrawString(10, 10, "Not running location service.");
        }
        else
        {
            // 緯度経度表示
            gc.DrawString(10, 10, lat.ToString());
            gc.DrawString(10, 50, lng.ToString());
        }
    }
}
