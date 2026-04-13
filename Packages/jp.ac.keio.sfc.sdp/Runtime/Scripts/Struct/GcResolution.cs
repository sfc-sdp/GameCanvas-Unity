/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2024 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
#nullable enable
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace GameCanvas
{
    /// <summary>
    /// 解像度とリフレッシュレート
    /// </summary>
    public readonly struct GcResolution : System.IEquatable<GcResolution>
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        /// <summary>
        /// リフレッシュレート（1秒間の更新回数）
        /// </summary>
        public readonly RefreshRate RefreshRate;

        /// <summary>
        /// 解像度
        /// </summary>
        public readonly int2 Size;
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GcResolution(int width, int height, in RefreshRate refreshRate)
        {
            Size = new int2(width, height);
            RefreshRate = refreshRate;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GcResolution(in int2 size, in RefreshRate refreshRate)
        {
            Size = size;
            RefreshRate = refreshRate;
        }

        /// <summary>
        /// 縦幅
        /// </summary>
        public int Height
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Size.y;
        }

        /// <summary>
        /// 横幅
        /// </summary>
        public int Width
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Size.x;
        }

        public static explicit operator GcResolution(Resolution src) => new(src.width, src.height, src.refreshRateRatio);

        public static explicit operator Resolution(GcResolution src) => new() { width = src.Size.x, height = src.Size.y, refreshRateRatio = src.RefreshRate };

        public static bool operator !=(GcResolution lh, GcResolution rh) => !lh.Equals(rh);

        public static bool operator ==(GcResolution lh, GcResolution rh) => lh.Equals(rh);

        public bool Equals(GcResolution other)
            => Size.Equals(other.Size)
            && RefreshRate.Equals(other.RefreshRate);

        public override bool Equals(object obj) => (obj is GcResolution other) && Equals(other);

        public override int GetHashCode() => System.HashCode.Combine(Size, RefreshRate);

        public override string ToString()
            => $"{nameof(GcResolution)}: {{ Size: {Size.x}x{Size.y}, RefrashRate: {RefreshRate.value:0.00}Hz }}";
        #endregion
    }
}
