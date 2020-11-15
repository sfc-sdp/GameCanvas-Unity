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

namespace GameCanvas
{
    /// <summary>
    /// <see cref="GcRect"/> 拡張クラス
    /// </summary>
    public static class GcRectExtensions
    {
        /// <summary>
        /// 傾き（弧度法）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Degree(this in GcRect self) => math.degrees(self.Radian);
    }
}
