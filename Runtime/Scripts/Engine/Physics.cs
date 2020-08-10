/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace GameCanvas.Engine
{
    public sealed class Physics
    {
        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        /// <summary>
        /// 矩形同士の接触判定
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HitTest(in Rect rect1, in Rect rect2) => rect1.Overlaps(rect2);

        /// <summary>
        /// 矩形同士の接触判定
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HitTest(in Box box1, in Box box2) => box1.Overlaps(box2);

        /// <summary>
        /// 矩形と点の接触判定
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HitTest(in Rect rect, in float2 point) => rect.Contains(point);

        /// <summary>
        /// 矩形と点の接触判定
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HitTest(in Box box, in float2 point) => box.Contains(point);

        /// <summary>
        /// 円同士の接触判定
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HitTest(in Circle circle1, in Circle circle2) => circle1.Overlaps(circle2);

        /// <summary>
        /// 円と点の接触判定
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HitTest(in Circle circle, in float2 point) => circle.Contains(point);

        /// <summary>
        /// 線分同士の交差判定
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CrossTest(in Segment a, in Segment b) => a.Intersects(b);

        /// <summary>
        /// 線分同士の交差判定
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CrossTest(in Segment a, in Segment b, out float2 intersection)
            => a.Intersects(b, out intersection);

        /// <summary>
        /// 線分と直線の交差判定
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CrossTest(in Segment a, in Line b) => a.Intersects(b);

        /// <summary>
        /// 線分と直線の交差判定
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CrossTest(in Segment a, in Line b, out float2 intersection)
            => a.Intersects(b, out intersection);

        /// <summary>
        /// 直線同士の交差判定
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CrossTest(in Line a, in Line b) => a.Intersects(b);

        /// <summary>
        /// 直線同士の交差判定
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CrossTest(in Line a, in Line b, out float2 intersection)
            => a.Intersects(b, out intersection);

        /// <summary>
        /// 連続衝突検出
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SweepTest(in Box target, in float2 point, in float2 delta, out HitResult hit)
            => target.SweepTest(point, delta, out hit);

        /// <summary>
        /// 連続衝突検出
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SweepTest(in Box target, in Box rect, in float2 delta, out HitResult hit)
            => target.SweepTest(rect, delta, out hit);

        [System.Obsolete]
        public bool CheckHitImage(Resource res, in int imageId1, in int x1, in int y1, in int imageId2, in int x2, in int y2)
        {
            var img1 = res.GetImg(imageId1);
            var img2 = res.GetImg(imageId2);
            if (img1.Data == null || img2.Data == null) return false;

            var rect1 = img1.Data.rect;
            rect1.position = new float2(x1, y1);
            var rect2 = img2.Data.rect;
            rect1.position = new float2(x2, y2);
            return HitTest(rect1, rect2);
        }

        #endregion
    }
}
