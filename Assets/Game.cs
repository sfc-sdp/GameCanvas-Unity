
using Sequence = System.Collections.IEnumerator;

/// <summary>
/// ゲームクラス。
/// 学生が編集すべきソースコードです。
/// </summary>
public sealed class Game : GameBase
{
    /// <summary>
    /// ゲームシーケンス
    /// </summary>
    public override Sequence Entry()
    {
        // キャンバスの大きさを設定します
        gc.SetResolution(720, 1280);
        
        // 無限に繰り返します
        while (true)
        {
            // 起動からの経過時間を取得します
            var sec = (int)gc.TimeSinceStartup;

            // 画面を白で塗りつぶします
            gc.ClearScreen();

            // 0番の画像を描画します
            gc.DrawImage(0, 0, 0);

            // 黒の文字を描画します
            gc.SetColor(0, 0, 0);
            gc.SetFontSize(48);
            gc.DrawString("この文字と青空の画像が", 40, 160);
            gc.DrawString("見えていれば成功です", 40, 270);
            gc.DrawRightString($"{sec}s", 630, 10);

            // 次のフレームになるまで待ちます
            yield return gc.WaitForNextFrame;
        }
    }
}
