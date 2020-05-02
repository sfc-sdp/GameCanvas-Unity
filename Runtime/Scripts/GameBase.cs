/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using System.Collections;

public abstract class GameBase : GameCanvas.BehaviourBase
{
    /// <summary>
    /// 初期化処理
    /// </summary>
    public override void InitGame() { }

    /// <summary>
    /// 更新処理
    /// </summary>
    public override void UpdateGame() { }

    /// <summary>
    /// 描画処理
    /// </summary>
    public override void DrawGame() { }

    /// <summary>
    /// ゲームシーケンス
    /// </summary>
    public override IEnumerator Entry()
    {
        yield break;
    }
}
