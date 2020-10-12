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

    public interface IPhysicsEx : IPhysics { }
}
