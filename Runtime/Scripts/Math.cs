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
    public static class Math
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        private static System.Random s_Random;

        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        /// <summary>
        /// 乱数の種の設定
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetSeed(in int seed)
        {
            s_Random = new System.Random(seed);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Random(in int min, in int max) => s_Random.Next(min, max + 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Random() => UnityEngine.Random.value;

        /// <summary>
        /// 計算誤差を考慮したゼロ判定
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AlmostZero(in float value) => (value >= -math.EPSILON && value <= math.EPSILON);

        /// <summary>
        /// 計算誤差を考慮した同値判定
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AlmostSame(in float a, in float b) => (a >= b - math.EPSILON && a <= b + math.EPSILON);

        /// <summary>
        /// 四捨五入
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Round(in double value) => (int)System.Math.Round(value);

        /// <summary>
        /// 絶対値
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Abs(in float value) => math.abs(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(in float value, in float min, in float max)
            => math.clamp(value, min, max);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Min(in float a, in float b) => math.min(a, b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Max(in float a, in float b) => math.max(a, b);

        /// <summary>
        /// 平方根
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sqrt(in float value) => Mathf.Sqrt(value);

        /// <summary>
        /// コサイン
        /// </summary>
        /// <param name="degree">角度（度数法）</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cos(in float degree) => math.cos(math.radians(degree));

        /// <summary>
        /// サイン
        /// </summary>
        /// <param name="degree">角度（度数法）</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sin(in float degree) => math.sin(math.radians(degree));

        /// <summary>
        /// ベクトルの角度
        /// </summary>
        /// <param name="v">ベクトル</param>
        /// <returns>ベクトルの角度（度数法）</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Atan2(in float2 v) => math.degrees(math.atan2(v.y, v.x));

        /// <summary>
        /// ベクトルの転回
        /// </summary>
        /// <param name="v">回転前のベクトル</param>
        /// <param name="degree">回転量（度数法）</param>
        /// <returns>回転後のベクトル</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Rotate(in float2 v, in float degree)
            => AffinTransform.FromRotate(math.radians(degree)).Mul(v);

        /// <summary>
        /// ベクトルの内積
        /// </summary>
        /// <param name="a">ベクトルA</param>
        /// <param name="b">ベクトルB</param>
        /// <returns>内積</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(in float2 a, in float2 b) => a.x * b.x + a.y * b.y;

        /// <summary>
        /// ベクトルの外積
        /// </summary>
        /// <param name="a">ベクトルA</param>
        /// <param name="b">ベクトルB</param>
        /// <returns>外積</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cross(in float2 a, in float2 b) => a.x * b.y - a.y * b.x;

        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        internal static void ResetSeed()
        {
            s_Random = new System.Random();
        }

        #endregion
    }
}
