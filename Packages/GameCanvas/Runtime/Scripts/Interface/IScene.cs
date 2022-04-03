/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2022 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
#nullable enable
namespace GameCanvas
{
    public interface IScene
    {
        /// <summary>
        /// シーンの描画処理
        /// </summary>
        /// <remarks>
        /// 毎フレーム（計算より後に）GameCanvasにより自動的に呼び出されます
        /// </remarks>
        void DrawScene();

        /// <summary>
        /// シーンの開始処理
        /// </summary>
        /// <remarks>
        /// シーン遷移時にGameCanvasにより自動的に呼び出されます
        /// </remarks>
        /// <param name="state"><see cref="ISceneManagementEx.ChangeScene"/>を通じて渡される任意の値。未設定の場合はnull</param>
        void EnterScene(object state);

        /// <summary>
        /// シーンの終了処理
        /// </summary>
        /// <remarks>
        /// シーン遷移時にGameCanvasにより自動的に呼び出されます
        /// </remarks>
        void LeaveScene();

        /// <summary>
        /// シーンの中断処理
        /// </summary>
        /// <remarks>
        /// アプリが一時停止する直前にGameCanvasにより自動的に呼び出されます
        /// </remarks>
        void PauseScene();

        /// <summary>
        /// シーンの再開処理
        /// </summary>
        /// <remarks>
        /// アプリが再開する直前にGameCanvasにより自動的に呼び出されます
        /// </remarks>
        void ResumeScene();

        /// <summary>
        /// シーンの計算処理
        /// </summary>
        /// <remarks>
        /// 毎フレーム（描画より前に）GameCanvasにより自動的に呼び出されます
        /// </remarks>
        void UpdateScene();
    }
}
