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
    int time = 600;
    int score = 0;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public override void InitGame()
    {
        // キャンバスの大きさを設定します
        gc.ChangeCanvasSize(360, 640);
    }

    /// <summary>
    /// 動きなどの更新処理
    /// </summary>
    public override void UpdateGame()
    {time -= 1;

if (time >= 0)
{
    score += gc.PointerBeginCount;
}

if (gc.GetPointerDuration(0) >= 2)
{
    time = 600;
    score = 0;
}
    }

    /// <summary>
    /// 描画の処理
    /// </summary>
    public override void DrawGame()
    {
        gc.ClearScreen();

if(time >= 0 ){
  gc.DrawString("time:"+time,60,0);
}
else {
  gc.DrawString("finished!!",60,0);
}

gc.DrawString("score:"+score,60,60);
    }
}
