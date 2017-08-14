/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2017 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/

public abstract class GameBase : GameCanvas.Behaviour
{
    /// <summary>
    /// 初期化処理
    /// </summary>
    public override void initGame() { }

    /// <summary>
    /// 更新処理
    /// </summary>
    public override void updateGame() { }

    /// <summary>
    /// 描画処理
    /// </summary>
    public override void drawGame() { }
}
