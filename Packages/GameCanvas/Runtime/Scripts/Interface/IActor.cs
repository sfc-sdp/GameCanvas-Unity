/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
namespace GameCanvas
{
    public interface IActor
    {
        /// <summary>
        /// アクターの処理優先度
        /// </summary>
        /// <remarks>
        /// - 初期値は0（無指定）<br />
        /// - アクターは、シーン内でこの値が小さい順に処理が回ってきます<br />
        /// - 同じ値だった場合は、シーンに登録した順序で実行されます<br />
        /// - 負の値を設定した場合は、そのシーンの処理よりも前に実行されます
        /// </remarks>
        int Priority { get; set; }

        /// <summary>
        /// アクターの計算処理
        /// </summary>
        /// <remarks>
        /// 毎フレーム（描画より前に）GameCanvasにより自動的に呼び出されます
        /// </remarks>
        void Update();

        /// <summary>
        /// アクターの描画処理
        /// </summary>
        /// <remarks>
        /// 毎フレーム（計算より後に）GameCanvasにより自動的に呼び出されます
        /// </remarks>
        void Draw();

        /// <summary>
        /// アクターの描画後処理
        /// </summary>
        /// <remarks>
        /// 毎フレーム（描画より後に）GameCanvasにより自動的に呼び出されます
        /// </remarks>
        void AfterDraw();
    }
}
