using GameCanvas;

public class Game : GameBase
{
    public void Start()
    {
        //
    }

    public void Update()
    {
        //
    }

    public void Draw()
    {
        // 画面を白で塗りつぶします
        gc.clearScreen();

        // ここから、画像を表示する命令を記述していく
        gc.drawImage(0, 0, 0);
        gc.setColor(0, 0, 0);
        gc.drawString("この文字と青空の画像が見えていれば成功です", 60, 220);
    }

    public void OnDestroy()
    {
        //
    }
}
