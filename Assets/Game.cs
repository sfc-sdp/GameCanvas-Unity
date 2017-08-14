// インポート
using GameCanvas;

/// <summary>
/// ゲームクラス。
/// 学生が編集すべきソースコードです。
/// </summary>
public sealed class Game : GameBase
{
    /* 初期化の手順はこちらに */
    public override void initGame() { }

    /* 物体の移動等の更新処理はこちらに */
    public override void updateGame() { }

    /* 画像の描画はこちらに */
    public override void drawGame()
    {
        // 画面を白で塗りつぶします
        gc.clearScreen();

        // ここから、画像を表示する命令を記述していく
        gc.drawImage(0, 0, 0);
        gc.setColor(0, 0, 0);
        gc.drawString("この文字と青空の画像が見えていれば成功です", 60, 220);
    }
}
