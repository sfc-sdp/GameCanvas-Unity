
namespace GameCanvas.Tests
{
    public sealed class Geolocation : GameBase
    {
        float lat;
        float lng;
        string text;
        string url;

        public override void InitGame()
        {
            gc.StartGeolocationService();

            lat = 35.685410f;
            lng = 139.752842f;
            text = "取得中";
            var w = gc.CanvasWidth / 2;
            var h = gc.CanvasHeight / 2;
            url = string.Format("https://maps.googleapis.com/maps/api/staticmap?center={0},{1}&zoom=15&format=png&sensor=false&size={2}x{3}&scale=2&maptype=roadmap&markers={0},{1}", lat, lng, w, h);
        }

        public override void UpdateGame()
        {
            if (!gc.HasGeolocationPermission)
            {
                text = "位置情報サービスが無効です";
            }

            if (gc.HasGeolocationUpdate)
            {
                lat = gc.GeolocationLastLatitude;
                lng = gc.GeolocationLastLongitude;
                text = string.Format("緯度: {0}\n経度: {1}", lat, lng);
                var w = gc.CanvasWidth / 2;
                var h = gc.CanvasHeight / 2;
                url = string.Format("http://maps.googleapis.com/maps/api/staticmap?center={0},{1}&zoom=15&format=png&sensor=false&size={2}x{3}&scale=2&maptype=roadmap&markers={0},{1}", lat, lng, w, h);
            }
        }
        
        public override void DrawGame()
        {
            gc.ClearScreen();
            gc.DrawOnlineImage(url, 0, 0);
            gc.DrawString(text, 15, 15);
        }
    }
}
