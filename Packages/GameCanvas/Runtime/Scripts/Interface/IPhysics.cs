/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using Unity.Mathematics;

namespace GameCanvas
{
    public interface IPhysics
    {
        /// <summary>
        /// <see cref="GcLine"/> と <see cref="GcLine"/> の交差判定
        /// </summary>
        bool CrossTest(in GcLine a, in GcLine b);

        /// <summary>
        /// <see cref="GcLine"/> と <see cref="GcLine"/> の交差判定
        /// </summary>
        bool CrossTest(in GcLine a, in GcLine b, out float2 intersection);

        /// <summary>
        /// <see cref="GcAABB"/> と <see cref="GcAABB"/> の接触判定
        /// </summary>
        bool HitTest(in GcAABB a, in GcAABB b);

        /// <summary>
        /// <see cref="GcAABB"/> と 点の接触判定
        /// </summary>
        bool HitTest(in GcAABB aabb, in float2 point);

        /// <summary>
        /// <see cref="GcCircle"/> と <see cref="GcCircle"/> の接触判定
        /// </summary>
        bool HitTest(in GcCircle circle1, in GcCircle circle2);

        /// <summary>
        /// <see cref="GcCircle"/> と 点の接触判定
        /// </summary>
        bool HitTest(in GcCircle circle, in float2 point);

        /// <summary>
        /// 移動する点と静的な矩形の連続衝突検出
        /// </summary>
        bool SweepTest(in GcAABB @static, in float2 @dynamic, in float2 delta, out GcSweepResult hit);

        /// <summary>
        /// 移動する矩形と静的な矩形の連続衝突検出
        /// </summary>
        bool SweepTest(in GcAABB @static, in GcAABB @dynamic, in float2 delta, out GcSweepResult hit);
    }

    public interface IPhysicsEx : IPhysics
    {
        /// <summary>
        /// 円と円の接触判定
        /// </summary>
        /// <param name="x1">円1の中心X座標</param>
        /// <param name="y1">円1の中心Y座標</param>
        /// <param name="r1">円1の半径</param>
        /// <param name="x2">円2の中心X座標</param>
        /// <param name="y2">円2の中心Y座標</param>
        /// <param name="r2">円2の半径</param>
        /// <returns>接触しているかどうか</returns>
        [System.Obsolete("Use to `HitTest`  instead.")]
        bool CheckHitCircle(in float x1, in float y1, in float r1, in float x2, in float y2, in float r2);

        /// <summary>
        /// 矩形と矩形の接触判定
        /// </summary>
        /// <param name="x1">矩形1の左上X座標</param>
        /// <param name="y1">矩形1の左上Y座標</param>
        /// <param name="w1">矩形1の横幅</param>
        /// <param name="h1">矩形1の縦幅</param>
        /// <param name="x2">矩形2の左上X座標</param>
        /// <param name="y2">矩形2の左上Y座標</param>
        /// <param name="w2">矩形2の横幅</param>
        /// <param name="h2">矩形2の縦幅</param>
        /// <returns>接触しているかどうか</returns>
        [System.Obsolete("Use to `HitTest`  instead.")]
        bool CheckHitRect(in float x1, in float y1, in float w1, in float h1, in float x2, in float y2, in float w2, in float h2);
    }
}
