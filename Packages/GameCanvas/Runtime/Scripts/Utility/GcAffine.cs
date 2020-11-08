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
    public static class GcAffine
    {
        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        /// <summary>
        /// 単位行列（なにも移動・回転・拡縮を行わない場合の変換行列）
        /// </summary>
        public static readonly float2x3 Identity = new float2x3(1f, 0f, 0f, 0f, 1f, 0f);

        #endregion

        //----------------------------------------------------------
        #region 公開関数（取り出し）
        //----------------------------------------------------------

        /// <summary>
        /// 変換行列から回転成分を取り出します
        /// </summary>
        /// <param name="mtx">変換行列</param>[
        /// <returns>回転量（弧度法）</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CalcRotate(this in float2x3 mtx)
            => -math.atan2(mtx.c1.x, mtx.c0.x);

        /// <summary>
        /// 変換行列から拡縮成分を取り出します
        /// </summary>
        /// <param name="mtx">変換行列</param>
        /// <returns>拡縮率</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 CalcScale(this in float2x3 mtx)
            => new float2(
                math.sqrt(mtx.c0.x * mtx.c0.x + mtx.c1.x * mtx.c1.x),
                math.sqrt(mtx.c0.y * mtx.c0.y + mtx.c1.y * mtx.c1.y)
            );

        /// <summary>
        /// 変換行列から移動・回転・拡縮成分を取り出します
        /// </summary>
        /// <param name="mtx">変換行列</param>
        /// <param name="translate">移動量</param>
        /// <param name="rotate">回転量（弧度法）</param>
        /// <param name="scale">拡縮率</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CalcTRS(this in float2x3 mtx, out float2 translate, out float rotate, out float2 scale)
        {
            translate = mtx.c2;
            rotate = -math.atan2(mtx.c1.x, mtx.c0.x);
            scale = new float2(mtx.c0.x, mtx.c1.y) / math.cos(rotate);
        }

        /// <summary>
        /// 変換行列からせん断成分を取り出します
        /// </summary>
        /// <param name="mtx">変換行列</param>
        /// <returns>せん断係数</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 GetShear(this in float2x3 mtx)
            => new float2(mtx.c1.x, mtx.c0.y);

        /// <summary>
        /// 変換行列から移動成分を取り出します
        /// </summary>
        /// <param name="mtx">変換行列</param>
        /// <returns>移動量</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 GetTranslate(this in float2x3 mtx)
            => mtx.c2;

        #endregion

        //----------------------------------------------------------
        #region 公開関数（作成）
        //----------------------------------------------------------

        /// <summary>
        /// 回転成分から変換行列を作ります
        /// </summary>
        /// <param name="radian">回転量（弧度法）</param>
        /// <returns>変換行列</returns>
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
        /// <paramref name="ahchor"/>を中心とした回転成分から変換行列を作ります
        /// </summary>
        /// <param name="ahchor">回転中心</param>
        /// <param name="radian">回転量（弧度法）</param>
        /// <returns>変換行列</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2x3 FromRotate(in float2 ahchor, in float radian)
        {
            return FromTranslate(ahchor).Mul(FromRotate(radian)).Mul(FromTranslate(-ahchor));
        }

        /// <summary>
        /// 拡縮成分から変換行列を作ります
        /// </summary>
        /// <param name="scale">拡縮率</param>
        /// <returns>変換行列</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2x3 FromScale(in float scale)
        {
            return new float2x3(
                scale, 0f, 0f,
                0f, scale, 0f
            );
        }

        /// <summary>
        /// 拡縮成分から変換行列を作ります
        /// </summary>
        /// <param name="scale">拡縮率</param>
        /// <returns>変換行列</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2x3 FromScale(in float2 scale)
        {
            return new float2x3(
                scale.x, 0f, 0f,
                0f, scale.y, 0f
            );
        }

        /// <summary>
        /// せん断成分から変換行列を作ります
        /// </summary>
        /// <param name="shear">せん断係数</param>
        /// <returns>変換行列</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2x3 FromShear(in float2 shear)
        {
            return new float2x3(
                1f, shear.x, 0f,
                shear.y, 1f, 0f
            );
        }

        /// <summary>
        /// 移動成分から変換行列を作ります
        /// </summary>
        /// <param name="translate">移動量</param>
        /// <returns>変換行列</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2x3 FromTranslate(in float2 translate)
        {
            return new float2x3(
                1f, 0f, translate.x,
                0f, 1f, translate.y
            );
        }

        /// <summary>
        /// 移動・回転・拡縮成分から変換行列を作ります
        /// </summary>
        /// <param name="translate">移動量</param>
        /// <param name="rotate">回転量（弧度法）</param>
        /// <param name="scale">拡縮率</param>
        /// <returns>変換行列</returns>
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

        /// <summary>
        /// 移動・拡縮成分から変換行列を作ります
        /// </summary>
        /// <param name="translate">移動量</param>
        /// <param name="scale">拡縮率</param>
        /// <returns>変換行列</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2x3 FromTS(in float2 translate, in float2 scale)
        {
            return new float2x3(
                scale.x, 0f, translate.x,
                0f, scale.y, translate.y
            );
        }

        #endregion

        //----------------------------------------------------------
        #region 公開関数（変換）
        //----------------------------------------------------------

        /// <summary>
        /// 行列とベクトルの積を求めます
        /// </summary>
        /// <param name="mtx">変換前の行列</param>
        /// <param name="vec">ベクトル</param>
        /// <returns>行列とベクトルの積</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Mul(this in float2x3 mtx, in float2 vec)
        {
            return new float2(
                mtx.c0.x * vec.x + mtx.c1.x * vec.y + mtx.c2.x,
                mtx.c0.y * vec.x + mtx.c1.y * vec.y + mtx.c2.y
            );
        }

        /// <summary>
        /// 行列同士の積を求めます
        /// </summary>
        /// <param name="lhs">左辺</param>
        /// <param name="rhs">右辺</param>
        /// <returns>行列同士の積</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2x3 Mul(this in float2x3 lhs, in float2x3 rhs)
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
        /// 2次元変換行列を3次元変換行列に変換します
        /// </summary>
        /// <param name="mtx">2次元変換行列</param>
        /// <returns>3次元変換行列</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4x4 ToFloat4x4(this in float2x3 mtx)
        {
            return new float4x4(
                mtx.c0.x, mtx.c1.x, 0, mtx.c2.x,
                mtx.c0.y, mtx.c1.y, 0, mtx.c2.y,
                0, 0, 1, 0,
                0, 0, 0, 1
            );
        }

        /// <summary>
        /// ベクトルを回転させます
        /// </summary>
        /// <param name="vec">回転前のベクトル</param>
        /// <param name="radian">回転量（弧度法）</param>
        /// <returns>回転後のベクトル</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Rotate(this in float2 vec, in float radian)
        {
            var s = math.sin(radian);
            var c = math.cos(radian);
            return new float2(vec.x * c - vec.y * s, vec.x * s + vec.y * c);
        }

        #endregion
    }
}
