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
    public readonly struct HitResult : System.IEquatable<HitResult>
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        /// <summary>
        /// 衝突点
        /// </summary>
        public readonly float2 HitPosition;
        /// <summary>
        /// 衝突点の法線
        /// </summary>
        public readonly float2 HitNormal;
        /// <summary>
        /// めり込み量の逆ベクトル
        /// </summary>
        public readonly float2 SinkVecInv;
        /// <summary>
        /// めり込まないよう良しなに修正された移動量ベクトル
        /// </summary>
        public readonly float2 ModifiedDelta;
        /// <summary>
        /// めり込む直前のオブジェクト座標
        /// </summary>
        public readonly float2 SweepPosition;
        /// <summary>
        /// 移動量に対して、めり込む直前までの移動量の割合。
        /// 移動量の指定のないメソッドでは常に1となる。
        /// </summary>
        public readonly float SweepRatio;

        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public static bool operator ==(HitResult lh, HitResult rh) => lh.Equals(rh);

        public static bool operator !=(HitResult lh, HitResult rh) => !lh.Equals(rh);

        // todo

        public bool Equals(HitResult other)
            => HitPosition.Equals(other.HitPosition)
            && SinkVecInv.Equals(other.SinkVecInv)
            && HitNormal.Equals(other.HitNormal)
            && SweepRatio.Equals(other.SweepRatio);

        public override bool Equals(object obj) => (obj is HitResult other) && Equals(other);

        public override int GetHashCode()
            => HitPosition.GetHashCode() ^ SinkVecInv.GetHashCode()
            ^ HitNormal.GetHashCode() ^ SweepRatio.GetHashCode();

        public override string ToString()
            => $"SweepResult: {{ Pos: {HitPosition}, Nrm: {HitNormal}, Dlt: {SinkVecInv} ({SweepRatio:0.00}) }}";

        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal HitResult(in float2 pos, in float2 nrm, in float2 inv, in float2 mod)
        {
            HitPosition = pos;
            HitNormal = nrm;
            SinkVecInv = inv;
            ModifiedDelta = mod;
            SweepPosition = pos;
            SweepRatio = 1f;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal HitResult(in float2 hitPos, in float2 hitNrm, in float2 inv, in float2 mod, in float2 pos, in float rt)
        {
            HitPosition = hitPos;
            HitNormal = hitNrm;
            SinkVecInv = inv;
            ModifiedDelta = mod;
            SweepPosition = pos;
            SweepRatio = rt;
        }

        #endregion
    }
}
