using GameCanvas;

public class CompassExample : GameBase
{
    override public void Start()
    {
        // 地磁気センサーを有効化します
        gc.isCompassEnabled = true;
    }

    override public void Draw()
    {
        // 画面を白で塗りつぶします
        gc.ClearScreen();

        // 背景画像を描画します
        gc.SetColor(1f, 1f, 1f);
        gc.DrawImage(2, 0, 0);

        // ボールを描画します
        gc.DrawRotatedImage(0, 308, 228, gc.compass, 12, 12);
    }
}
