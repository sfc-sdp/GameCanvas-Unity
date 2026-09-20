/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2026 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
#nullable enable
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
        internal float Radian;

        /// <summary>時計回りの回転角度（度数法）。</summary>
        public float Rotation
        {
            readonly get => math.degrees(Radian);
            set => Radian = math.radians(value);
        }

        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public GcRect(in float2 position, in float2 size) : this(position, size, 0f) { }
        public GcRect(in float2 position, in float width, in float height) : this(position, width, height, 0f) { }
        public GcRect(in float x, in float y, in float width, in float height) : this(x, y, width, height, 0f) { }
        public GcRect(in Rect rect) : this(rect, 0f) { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="size">大きさ</param>
        /// <param name="radian">回転（弧度法）</param>
        internal GcRect(in float2 position, in float2 size, in float radian)
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
        internal GcRect(in float2 position, in float width, in float height, in float radian)
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
        internal GcRect(in float x, in float y, in float width, in float height, in float radian)
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
        internal GcRect(in Rect rect, in float radian)
        {
            Position = rect.position;
            Size = rect.size;
            Radian = radian;
        }

        /// <summary>
        /// 度数法で回転を指定して GcRect を生成します
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="size">大きさ</param>
        /// <param name="degree">回転（度数法）</param>
        public static GcRect FromDegrees(in float2 position, in float2 size, in float degree)
            => new(position, size, math.radians(degree));

        /// <summary>
        /// 度数法で回転を指定して GcRect を生成します
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="width">横幅</param>
        /// <param name="height">縦幅</param>
        /// <param name="degree">回転（度数法）</param>
        public static GcRect FromDegrees(in float x, in float y, in float width, in float height, in float degree)
            => new(new float2(x, y), new float2(width, height), math.radians(degree));

        /// <summary>左上を基準とする矩形の内部を判定する。回転を含み、右端と下端は含まない。
        /// 描画状態も使う場合はgc.Containsを使う。</summary>
        public readonly bool Contains(in GcPoint point) => GcHitTest.Contains(this, point, GcAnchor.UpperLeft, GcAffine.Identity);
        public readonly bool Contains(float x, float y) => Contains(new GcPoint(x, y));
        /// <summary>矩形の基準点のX座標。</summary>
        public float X { readonly get => Position.x; set => Position.x = value; }
        /// <summary>矩形の基準点のY座標。</summary>
        public float Y { readonly get => Position.y; set => Position.y = value; }
        public float Width { readonly get => Size.x; set => Size.x = value; }
        public float Height { readonly get => Size.y; set => Size.y = value; }

        public static explicit operator GcRect(Rect rect) => new(rect);

        public static bool operator !=(GcRect lh, GcRect rh) => !lh.Equals(rh);

        public static bool operator ==(GcRect lh, GcRect rh) => lh.Equals(rh);

        public readonly bool Equals(GcRect other)
            => Position.Equals(other.Position) && Size.Equals(other.Size) && Radian.Equals(other.Radian);

        public override readonly bool Equals(object obj) => (obj is GcRect other) && Equals(other);

        public override readonly int GetHashCode()
            => Position.GetHashCode() ^ Size.GetHashCode() ^ Radian.GetHashCode();

        public override readonly string ToString()
            => $"{nameof(GcRect)}: {{ x: {Position.x}, y: {Position.y}, w: {Size.x}, h: {Size.y}, angle: {Rotation} }}";

        #endregion
    }
}
