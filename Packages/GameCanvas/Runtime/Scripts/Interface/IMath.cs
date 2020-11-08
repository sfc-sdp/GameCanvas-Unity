/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using System.ComponentModel;
using Unity.Mathematics;

namespace GameCanvas
{
    public interface IMath
    {
        /// <summary>
        /// 絶対値を計算します
        /// </summary>
        /// <param name="value">入力値</param>
        /// <returns>絶対値</returns>
        float Abs(in float value);

        /// <summary>
        /// 絶対値を計算します
        /// </summary>
        /// <param name="value">入力値</param>
        /// <returns>絶対値</returns>
        int Abs(in int value);

        /// <summary>
        /// 計算誤差を考慮して同値かどうか判定します
        /// </summary>
        /// <returns>同値かどうか</returns>
        bool AlmostSame(in float a, in float b);

        /// <summary>
        /// 計算誤差を考慮してゼロかどうか判定します
        /// </summary>
        /// <param name="value">入力値</param>
        /// <returns>ゼロかどうか</returns>
        bool AlmostZero(in float value);

        /// <summary>
        /// ベクトルとX+軸平面のなす角度を計算します
        /// </summary>
        /// <param name="v">ベクトル</param>
        /// <returns>角度（度数法）</returns>
        float Atan2(in float2 v);

        /// <summary>
        /// 値を丸めます
        /// </summary>
        /// <param name="value">入力値</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        float Clamp(in float value, in float min, in float max);

        /// <summary>
        /// コサインを計算します
        /// </summary>
        /// <param name="degree">角度（度数法）</param>
        /// <returns>コサイン</returns>
        float Cos(in float degree);

        /// <summary>
        /// ベクトルの外積を計算します
        /// </summary>
        /// <returns>外積</returns>
        float Cross(in float2 a, in float2 b);

        /// <summary>
        /// ベクトルの内積を計算します
        /// </summary>
        /// <returns>内積</returns>
        float Dot(in float2 a, in float2 b);

        /// <summary>
        /// 2つの値を比較して、より大きい方を返します
        /// </summary>
        /// <returns>大きい方の値</returns>
        float Max(in float a, in float b);

        /// <summary>
        /// 2つの値を比較して、より小さい方を返します
        /// </summary>
        /// <returns>小さい方の値</returns>
        float Min(in float a, in float b);

        /// <summary>
        /// 0以上1未満のランダムな値を算出します
        /// </summary>
        /// <returns>ランダムな値</returns>
        float Random();

        /// <summary>
        /// <paramref name="min"/>以上<paramref name="max"/>未満のランダムな値を算出します
        /// </summary>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <returns>ランダムな値</returns>
        float Random(in float min, in float max);

        /// <summary>
        /// <paramref name="min"/>以上<paramref name="max"/>以下のランダムな値を算出します
        /// </summary>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <returns>ランダムな値</returns>
        int Random(in int min, in int max);

        /// <summary>
        /// 値を 0 から <paramref name="max"/> までの範囲を繰り返すよう値を丸めます
        /// </summary>
        /// <param name="value">入力値</param>
        /// <param name="max">最大値</param>
        float Repeat(in float value, in float max);

        /// <summary>
        /// ベクトルを回転します
        /// </summary>
        /// <param name="vector">ベクトル</param>
        /// <param name="degree">回転量（度数法）</param>
        /// <returns>回転後のベクトル</returns>
        float2 RotateVector(in float2 vector, in float degree);

        /// <summary>
        /// 値を四捨五入します
        /// </summary>
        /// <param name="value">入力値</param>
        /// <returns>四捨五入された値</returns>
        int Round(in float value);

        /// <summary>
        /// 値を四捨五入します
        /// </summary>
        /// <param name="value">入力値</param>
        /// <returns>四捨五入された値</returns>
        int Round(in double value);

        /// <summary>
        /// ランダム計算のシード値を設定します
        /// </summary>
        /// <param name="seed">シード値</param>
        void SetRandomSeed(in uint seed);

        /// <summary>
        /// サインを計算します
        /// </summary>
        /// <param name="degree">角度（度数法）</param>
        /// <returns>サイン</returns>
        float Sin(in float degree);

        /// <summary>
        /// 平方根を計算します
        /// </summary>
        /// <param name="value">入力値</param>
        /// <returns>平方根</returns>
        float Sqrt(in float value);
    }

    public interface IMathEx : IMath
    {
        /// <summary>
        /// ベクトルとX+軸平面のなす角度を計算します
        /// </summary>
        /// <param name="x">ベクトルのX軸の大きさ</param>
        /// <param name="y">ベクトルのY軸の大きさ</param>
        /// <returns>角度（度数法）</returns>
        float Atan2(in float x, in float y);

        [System.Obsolete("Use to `RotateVector` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        float2 Rotate(in float2 vector, in float degree);

        [System.Obsolete("Use to `SetRandomSeed` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        void SetSeed(in int seed);
    }
}
