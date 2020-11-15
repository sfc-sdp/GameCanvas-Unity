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
    /// <summary>
    /// 衝突点情報
    /// </summary>
    public readonly struct GcHitResult : System.IEquatable<GcHitResult>
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        /// <summary>
        /// 衝突点の法線
        /// </summary>
        public readonly float2 HitNormal;
        /// <summary>
        /// 衝突点
        /// </summary>
        public readonly float2 HitPoint;
        /// <summary>
        /// めり込み量の逆ベクトル
        /// </summary>
        public readonly float2 SinkVecInv;

        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public static bool operator !=(GcHitResult lh, GcHitResult rh) => !lh.Equals(rh);

        public static bool operator ==(GcHitResult lh, GcHitResult rh) => lh.Equals(rh);

        public bool Equals(GcHitResult other)
            => HitPoint.Equals(other.HitPoint)
            && HitNormal.Equals(other.HitNormal)
            && SinkVecInv.Equals(other.SinkVecInv);

        public override bool Equals(object obj) => (obj is GcHitResult other) && Equals(other);

        public override int GetHashCode()
            => HitPoint.GetHashCode()
            ^ SinkVecInv.GetHashCode()
            ^ HitNormal.GetHashCode();

        public override string ToString()
            => $"{nameof(GcHitResult)}: {{ Pos: {HitPoint}, Nrm: {HitNormal}, Inv: {SinkVecInv} }}";

        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal GcHitResult(in float2 pos, in float2 nrm, in float2 inv)
        {
            HitPoint = pos;
            HitNormal = nrm;
            SinkVecInv = inv;
        }

        #endregion
    }
}
