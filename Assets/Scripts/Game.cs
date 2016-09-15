using GameCanvas;

public class Game : GameBase
{
    override public void Start()
    {
        // ここに初期化処理を書きます
    }

    override public void Calc()
    {
        // ここに更新処理を書きます
    }

    override public void Draw()
    {
        // 画面を白で塗りつぶします
        gc.ClearScreen();

        // ここから、画像を表示する命令を記述していく
        gc.DrawImage(0, 0, 0);
        gc.DrawString(60, 60, "この モジ と Blue Sky が");
        gc.DrawString(60, 86, "みえていれば せいこう です ↑↑");
    }
}
