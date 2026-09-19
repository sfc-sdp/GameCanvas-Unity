#nullable enable
using GameCanvas;

/// <summary>
/// ゲームクラス。
/// 学生が編集すべきソースコードです。
/// </summary>
public sealed class Game : GameBase
{
    int sec = 0;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);
    }

    /// <summary>
    /// 動きなどの更新処理
    /// </summary>
    public override void UpdateGame()
    {
        sec = (int)gc.TimeSinceStartup;
    }

    /// <summary>
    /// 描画の処理
    /// </summary>
    public override void DrawGame()
    {
        gc.ClearScreen();
        gc.DrawImage("BlueSky.png", 0, 0);
        gc.SetColor(0, 0, 0);
        gc.SetFontSize(48);
        gc.DrawString("この文字と青空の画像が", 40, 160);
        gc.DrawString("見えていれば成功です", 40, 270);
        gc.DrawString($"{sec}s", 630, 10, anchor: GcAnchor.UpperRight);
    }
}
