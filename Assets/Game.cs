
/// <summary>
/// ゲームクラス。
/// 学生が編集すべきソースコードです。
/// </summary>
public sealed class Game : GameBase
{
    // ここに変数の宣言を書きます
    int time = 0;

    /// <summary>
    /// この関数は起動時に１回だけ呼ばれます。
    /// ここに初期化処理を書きます。
    /// </summary>
    public override void InitGame()
    {
        // キャンバスの大きさを設定します
        gc.SetResolution(720, 1280);
    }

    /// <summary>
    /// この関数は毎フレーム呼ばれます。1フレームは1/30秒です。設定で変更もできます。
    /// ここに動きなどの更新処理を書きます。
    /// </summary>
    public override void UpdateGame()
    {
        // 起動からの経過時間を取得します
        time = (int)gc.TimeSinceStartup;
    }

    /// <summary>
    /// この関数は毎フレーム呼ばれます。ここに描画の処理をまとめます。
    /// 奥に描きたいものは最初に、手前に描きたいものは最後に記述します。
    /// </summary>
    public override void DrawGame()
    {
        // 画面を白で塗りつぶします
        gc.ClearScreen();

        // 0番の画像を描画します
        gc.DrawImage(0, 0, 0);

        // 黒の文字を描画します
        gc.SetColor(0, 0, 0);
        gc.SetFontSize(50);
        gc.DrawString("この文字と青空の画像が", 40, 160);
        gc.DrawString("見えていれば成功です", 40, 280);
        gc.DrawRightString($"{time}s", 630, 10);
    }
}
