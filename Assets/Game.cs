#nullable enable
using GameCanvas;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ゲームクラス。
/// 学生が編集すべきソースコードです。
/// </summary>
public sealed class Game : GameBase
{
    // 変数の宣言
    int sec = 0;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public override void InitGame()
    {
        // キャンバスの大きさを設定します
        gc.ChangeCanvasSize(720, 1280);
    }

    /// <summary>
    /// 動きなどの更新処理
    /// </summary>
    public override void UpdateGame()
    {
        // 起動からの経過時間を取得します
        sec = (int)gc.TimeSinceStartup;
    }

    /// <summary>
    /// 描画の処理
    /// </summary>
    public override void DrawGame()
    {
        // 画面を白で塗りつぶします
        gc.ClearScreen();

        // 青空の画像を描画します
        gc.DrawImage(GcImage.BlueSky, 0, 0);

        // 黒の文字を描画します
        gc.SetColor(0, 0, 0);
        gc.SetFontSize(48);
        gc.SetStringAnchor(GcAnchor.UpperLeft);
        gc.DrawString("この文字と青空の画像が", 40, 160);
        gc.DrawString("見えていれば成功です", 40, 270);
        gc.SetStringAnchor(GcAnchor.UpperRight);
        gc.DrawString($"{sec}s", 630, 10);
    }
}
