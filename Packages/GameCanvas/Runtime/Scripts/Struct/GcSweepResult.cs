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
    /// 衝突情報
    /// </summary>
    public readonly struct GcSweepResult : System.IEquatable<GcSweepResult>
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        /// <summary>
        /// 進行ベクトル（SweepTest関数のdelta引数に与えられた値）
        /// </summary>
        public readonly float2 ForwardVector;
        /// <summary>
        /// 衝突点の法線を表す単位ベクトル
        /// </summary>
        public readonly float2 HitNormal;
        /// <summary>
        /// 衝突点
        /// </summary>
        public readonly float2 HitPoint;
        /// <summary>
        /// 衝突時（めり込む直前）のオブジェクト座標
        /// </summary>
        public readonly float2 PositionOnHit;
        /// <summary>
        /// 移動量に対して、めり込む直前までの移動量の割合
        /// </summary>
        /// <remarks>
        /// 初期位置で既にめり込んでいた場合、0よりも小さい値になります
        /// </remarks>
        public readonly float SweepRatioOnHit;
        /// <summary>
        /// 初期位置から衝突点（めり込む直前）までの移動量ベクトル
        /// </summary>
        /// <remarks>
        /// 初期位置で既にめり込んでいた場合、<see cref="ForwardVector"/> * <see cref="SweepRatioOnHit"/> と異なる値になります
        /// </remarks>
        public readonly float2 VectorToHit;
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public static bool operator !=(GcSweepResult lh, GcSweepResult rh) => !lh.Equals(rh);

        public static bool operator ==(GcSweepResult lh, GcSweepResult rh) => lh.Equals(rh);

        /// <summary>
        /// 反射を考慮した移動後の座標を計算します
        /// </summary>
        /// <param name="dir">反射方向を表す単位ベクトル</param>
        /// <param name="pos">反射を考慮した移動後の座標</param>
        /// <param name="coefficient">反射係数</param>
        public void CalcReflect(out float2 dir, out float2 pos, in float coefficient = 1f)
        {
            if (SweepRatioOnHit < 0f)
            {
                dir = math.normalizesafe(VectorToHit);
                pos = PositionOnHit - VectorToHit + ForwardVector * -coefficient;
            }
            else
            {
                dir = math.normalizesafe(ForwardVector - 2 * GcMath.Dot(ForwardVector, HitNormal) * HitNormal);
                pos = PositionOnHit + dir * math.length(ForwardVector - VectorToHit) * coefficient;
            }
        }

        /// <summary>
        /// 反射方向を計算します
        /// </summary>
        /// <returns>反射方向を表す単位ベクトル</returns>
        public float2 CalcReflectDir()
        {
            if (SweepRatioOnHit < 0f) return math.normalizesafe(VectorToHit);
            return math.normalizesafe(ForwardVector - 2 * GcMath.Dot(ForwardVector, HitNormal) * HitNormal);
        }

        /// <summary>
        /// 反射を考慮した移動後の座標を計算します
        /// </summary>
        /// <param name="coefficient">反射係数</param>
        /// <returns>反射を考慮した移動後の座標</returns>
        public float2 CalcReflectPoint(in float coefficient = 1f)
        {
            return (SweepRatioOnHit < 0)
                ? PositionOnHit - VectorToHit + ForwardVector * -coefficient
                : PositionOnHit + CalcReflectDir() * math.length(ForwardVector - VectorToHit) * coefficient;
        }

        /// <summary>
        /// 壁ずりを考慮した移動後の座標を計算します
        /// </summary>
        /// <param name="dir">壁ずり方向を表す単位ベクトル</param>
        /// <param name="pos">壁ずりを考慮した移動後の座標</param>
        public void CalcWallScratch(out float2 dir, out float2 pos)
        {
            dir = math.normalizesafe(ForwardVector - GcMath.Dot(ForwardVector, HitNormal) * HitNormal);
            pos = PositionOnHit + GcMath.Dot(ForwardVector - VectorToHit, dir) * dir;
        }

        /// <summary>
        /// 壁ずり方向を計算します
        /// </summary>
        /// <returns>壁ずり方向を表す単位ベクトル</returns>
        public float2 CalcWallScratchDir()
            => math.normalizesafe(ForwardVector - GcMath.Dot(ForwardVector, HitNormal) * HitNormal);

        /// <summary>
        /// 壁ずりを考慮した移動後の座標を計算します
        /// </summary>
        /// <returns>壁ずりを考慮した移動後の座標</returns>
        public float2 CalcWallScratchPoint()
        {
            var dir = CalcWallScratchDir();
            return PositionOnHit + GcMath.Dot(ForwardVector - VectorToHit, dir) * dir;
        }

        public bool Equals(GcSweepResult other)
            => ForwardVector.Equals(other.ForwardVector)
            && HitNormal.Equals(other.HitNormal)
            && HitPoint.Equals(other.HitPoint)
            && PositionOnHit.Equals(other.PositionOnHit)
            && SweepRatioOnHit.Equals(other.SweepRatioOnHit)
            && VectorToHit.Equals(other.VectorToHit);

        public override bool Equals(object obj)
            => (obj is GcHitResult other) && Equals(other);

        public override int GetHashCode()
            => ForwardVector.GetHashCode() ^ HitNormal.GetHashCode()
            ^ HitPoint.GetHashCode() ^ PositionOnHit.GetHashCode()
            ^ SweepRatioOnHit.GetHashCode() ^ VectorToHit.GetHashCode();

        public override string ToString()
            => $"{nameof(GcSweepResult)}: {{ Pos: {HitPoint}, Nrm: {HitNormal}, Dlt: {VectorToHit}, Ratio: ({SweepRatioOnHit:0.00}) }}";
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal GcSweepResult(in float2 fwd, in GcHitResult hit, in float2 toHit, in float2 onHit, in float ratio)
        {
            ForwardVector = fwd;
            HitPoint = hit.HitPoint;
            HitNormal = hit.HitNormal;
            VectorToHit = toHit;
            PositionOnHit = onHit;
            SweepRatioOnHit = ratio;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal GcSweepResult(in float2 fwd, in float2 hitPoint, in float2 hitNormal, in float2 toHit, in float2 onHit, in float ratio)
        {
            ForwardVector = fwd;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
            VectorToHit = toHit;
            PositionOnHit = onHit;
            SweepRatioOnHit = ratio;
        }
        #endregion
    }
}
