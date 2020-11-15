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

namespace GameCanvas
{
    public struct GcRect : IPrimitive<GcRect>
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        /// <summary>
        /// 位置
        /// </summary>
        public float2 Position;
        /// <summary>
        /// 大きさ
        /// </summary>
        public float2 Size;
        /// <summary>
        /// 回転（弧度法）
        /// </summary>
        public float Radian;

        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="size">大きさ</param>
        /// <param name="radian">回転（弧度法）</param>
        public GcRect(in float2 position, in float2 size, in float radian = 0f)
        {
            Position = position;
            Size = size;
            Radian = radian;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="width">横幅</param>
        /// <param name="height">縦幅</param>
        /// <param name="radian">回転（弧度法）</param>
        public GcRect(in float2 position, in float width, in float height, in float radian = 0f)
        {
            Position = position;
            Size = new float2(width, height);
            Radian = radian;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="width">横幅</param>
        /// <param name="height">縦幅</param>
        /// <param name="radian">回転（弧度法）</param>
        public GcRect(in float x, in float y, in float width, in float height, in float radian = 0f)
        {
            Position = new float2(x, y);
            Size = new float2(width, height);
            Radian = radian;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="rect">矩形</param>
        /// <param name="radian">回転（弧度法）</param>
        public GcRect(in Rect rect, in float radian = 0f)
        {
            Position = rect.position;
            Size = rect.size;
            Radian = radian;
        }

        public static explicit operator GcRect(Rect rect) => new GcRect(rect);

        public static bool operator !=(GcRect lh, GcRect rh) => !lh.Equals(rh);

        public static bool operator ==(GcRect lh, GcRect rh) => lh.Equals(rh);

        public bool Equals(GcRect other)
            => Position.Equals(other.Position) && Size.Equals(other.Size) && Radian.Equals(other.Radian);

        public override bool Equals(object obj) => (obj is GcRect other) && Equals(other);

        public override int GetHashCode()
            => Position.GetHashCode() ^ Size.GetHashCode() ^ Radian.GetHashCode();

        public override string ToString()
            => $"{nameof(GcRect)}: {{ x: {Position.x}, y: {Position.y}, w: {Size.x}, h: {Size.y}, angle: {this.Degree()} }}";

        #endregion
    }
}
