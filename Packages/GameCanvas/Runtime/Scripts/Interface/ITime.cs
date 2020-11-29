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

namespace GameCanvas
{
    public interface ITime
    {
        /// <summary>
        /// アプリ起動からの累計フレーム数
        /// </summary>
        int CurrentFrame { get; }

        /// <summary>
        /// 現在フレームの日時
        /// </summary>
        System.DateTimeOffset CurrentTime { get; }

        /// <summary>
        /// 現在フレームのUnixタイムスタンプ
        /// </summary>
        long CurrentTimestamp { get; }

        /// <summary>
        /// 現在（関数呼び出し時点）の日時
        /// </summary>
        System.DateTimeOffset NowTime { get; }

        /// <summary>
        /// フレーム更新間隔の目標値（秒）
        /// </summary>
        double TargetFrameInterval { get; }

        /// <summary>
        /// フレームレート（1秒あたりのフレーム数）の目標値
        /// </summary>
        int TargetFrameRate { get; }

        /// <summary>
        /// ひとつ前のフレームからの経過時間（秒）
        /// </summary>
        float TimeSincePrevFrame { get; }

        /// <summary>
        /// 現在フレームのアプリ起動からの経過時間（秒）
        /// </summary>
        float TimeSinceStartup { get; }

        /// <summary>
        /// 垂直同期の有無
        /// </summary>
        /// <remarks>
        /// この設定は、<see cref="SetFrameInterval"/> や <see cref="SetFrameRate"/> の第二引数から変更できます。
        /// </remarks>
        bool VSyncEnabled { get; }

        /// <summary>
        /// UpdateGame や DrawGame が呼び出される時間間隔を設定します。
        /// </summary>
        /// <param name="targetDeltaTime">フレーム更新間隔の目標値（秒）</param>
        /// <param name="vSyncEnabled">垂直同期の有無</param>
        /// <remarks>
        /// 垂直同期を無効にした場合、間隔の揺らぎは減少しますが、ディスプレイのリフレッシュレートを常に無視して描画するため、画面のちらつきが発生する場合があります。
        /// </remarks>
        void SetFrameInterval(in double targetDeltaTime, bool vSyncEnabled = true);

        /// <summary>
        /// フレームレートの目標値を設定します。
        /// 
        /// 小数点以下を指定したい場合は、この関数の代わりに <see cref="SetFrameInterval"/> を使用してください。
        /// </summary>
        /// <param name="targetFrameRate">フレームレート（1秒あたりのフレーム数）の目標値</param>
        /// <param name="vSyncEnabled">垂直同期の有無</param>
        /// <remarks>
        /// 垂直同期を無効にした場合、間隔の揺らぎは減少しますが、ディスプレイのリフレッシュレートを常に無視して描画するため、画面のちらつきが発生する場合があります。
        /// </remarks>
        void SetFrameRate(in int targetFrameRate, bool vSyncEnabled = true);
    }

    public interface ITimeEx : ITime
    {
        [System.Obsolete("Use to `CurrentTimeDay` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        int CurrentDay { get; }

        [System.Obsolete("Use to `CurrentTimeDayOfWeek` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        System.DayOfWeek CurrentDayOfWeek { get; }

        [System.Obsolete("Use to `CurrentTimeHour` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        int CurrentHour { get; }

        [System.Obsolete("Use to `CurrentTimeMillisecond` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        int CurrentMillisecond { get; }

        [System.Obsolete("Use to `CurrentTimeMinute` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        int CurrentMinute { get; }

        [System.Obsolete("Use to `CurrentTimeMonth` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        int CurrentMonth { get; }

        [System.Obsolete("Use to `CurrentTimeSecond` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        int CurrentSecond { get; }

        /// <summary>
        /// 現在フレームの日付（1～31）
        /// </summary>
        int CurrentTimeDay { get; }

        /// <summary>
        /// 現在フレームの曜日（0～6）
        /// </summary>
        System.DayOfWeek CurrentTimeDayOfWeek { get; }

        /// <summary>
        /// 現在フレームの時刻の時間部分（0～23）
        /// </summary>
        int CurrentTimeHour { get; }

        /// <summary>
        /// 現在フレームの時刻のミリ秒部分（0～999）
        /// </summary>
        int CurrentTimeMillisecond { get; }

        /// <summary>
        /// 現在フレームの時刻の分部分（0～59）
        /// </summary>
        int CurrentTimeMinute { get; }

        /// <summary>
        /// 現在フレームの日付の月部分（1～12）
        /// </summary>
        int CurrentTimeMonth { get; }

        /// <summary>
        /// 現在フレームの時刻の秒部分（0～59）
        /// </summary>
        int CurrentTimeSecond { get; }

        /// <summary>
        /// 現在フレームの日付の西暦部分
        /// </summary>
        int CurrentTimeYear { get; }

        [System.Obsolete("Use to `CurrentTimeYear` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        int CurrentYear { get; }
    }
}
