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
    /// 2次元アフィン変換
    /// </summary>
    public static class AffinTransform
    {
        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        /// <summary>
        /// 単位行列（なにも移動・回転・拡縮を行わない場合の変換行列）
        /// </summary>
        public static float2x3 Identity
            => new float2x3(1f, 0f, 0f, 0f, 1f, 0f);

        /// <summary>
        /// アフィン行列から移動成分を取り出す
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 GetTranslate(in this float2x3 mtx)
            => mtx.c2;

        /// <summary>
        /// アフィン行列から拡縮成分を取り出す
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 CalcScale(in this float2x3 mtx)
            => new float2(
                math.sqrt(mtx.c0.x * mtx.c0.x + mtx.c1.x * mtx.c1.x),
                math.sqrt(mtx.c0.y * mtx.c0.y + mtx.c1.y * mtx.c1.y)
            );

        /// <summary>
        /// アフィン行列から回転成分を取り出す
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CalcRotate(in this float2x3 mtx)
            => -math.atan2(mtx.c1.x, mtx.c0.x);

        /// <summary>
        /// アフィン行列からせん断成分を取り出す
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 GetShear(in this float2x3 mtx)
            => new float2(mtx.c1.x, mtx.c0.y);

        /// <summary>
        /// アフィン行列から移動・回転・拡縮成分を取り出す
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CalcTRS(in this float2x3 mtx, out float2 translate, out float rotate, out float2 scale)
        {
            translate = mtx.c2;
            rotate = -math.atan2(mtx.c1.x, mtx.c0.x);
            scale = new float2(mtx.c0.x, mtx.c1.y) / math.cos(rotate);
        }

        /// <summary>
        /// 行列とベクトルの積を求める
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Mul(in this float2x3 mtx, in float2 vec)
        {
            return new float2(
                mtx.c0.x * vec.x + mtx.c1.x * vec.y + mtx.c2.x,
                mtx.c0.y * vec.x + mtx.c1.y * vec.y + mtx.c2.y
            );
        }

        /// <summary>
        /// 行列同士の積を求める
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2x3 Mul(in this float2x3 lhs, in float2x3 rhs)
        {
            return new float2x3(
                lhs.c0.x * rhs.c0.x + lhs.c1.x * rhs.c0.y,
                lhs.c0.x * rhs.c1.x + lhs.c1.x * rhs.c1.y,
                lhs.c0.x * rhs.c2.x + lhs.c1.x * rhs.c2.y + lhs.c2.x,
                lhs.c0.y * rhs.c0.x + lhs.c1.y * rhs.c0.y,
                lhs.c0.y * rhs.c1.x + lhs.c1.y * rhs.c1.y,
                lhs.c0.y * rhs.c2.x + lhs.c1.y * rhs.c2.y + lhs.c2.y
            );
        }

        /// <summary>
        /// 移動成分からアフィン行列を作る
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2x3 FromTranslate(in float2 translate)
        {
            return new float2x3(
                1f, 0f, translate.x,
                0f, 1f, translate.y
            );
        }

        /// <summary>
        /// 拡縮成分からアフィン行列を作る
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2x3 FromScale(in float2 scale)
        {
            return new float2x3(
                scale.x, 0f, 0f,
                0f, scale.y, 0f
            );
        }

        /// <summary>
        /// 回転成分からアフィン行列を作る
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2x3 FromRotate(in float radian)
        {
            var s = math.sin(radian);
            var c = math.cos(radian);

            return new float2x3(
                c, -s, 0f,
                s, c, 0f
            );
        }

        /// <summary>
        /// <paramref name="ahchor"/>を中心とした回転成分からアフィン行列を作る
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2x3 FromRotate(in float2 ahchor, in float radian)
        {
            return FromTranslate(ahchor).Mul(FromRotate(radian)).Mul(FromTranslate(-ahchor));
        }

        /// <summary>
        /// せん断成分からアフィン行列を作る
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2x3 FromShear(in float2 shear)
        {
            return new float2x3(
                1f, shear.x, 0f,
                shear.y, 1f, 0f
            );
        }

        /// <summary>
        /// 移動・回転・拡縮成分からアフィン行列を作る
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2x3 FromTRS(in float2 translate, in float rotate, in float2 scale)
        {
            var s = math.sin(rotate);
            var c = math.cos(rotate);

            return new float2x3(
                scale.x * c, scale.y * -s, translate.x,
                scale.x * s, scale.y * c, translate.y
            );
        }

        #endregion
    }
}
