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
using Unity.Collections;
using Unity.Mathematics;

namespace GameCanvas
{
    public interface IInputPointer
    {
        /// <summary>
        /// 実行端末でタッチ圧力がサポートされているかどうか
        /// </summary>
        bool IsTouchPressureSupported { get; }

        /// <summary>
        /// 実行端末でタッチ操作がサポートされているかどうか
        /// </summary>
        /// <remarks>
        /// Windowsエディタ環境では、タッチ対応デバイスであっても常に偽を返します
        /// </remarks>
        bool IsTouchSupported { get; }

        /// <summary>
        /// 最後に検出したポインターイベント。過去のフレームも含める
        /// </summary>
        GcPointerEvent LastPointerEvent { get; }

        /// <summary>
        /// 前回のフレーム処理以降に検出した ポインター開始イベントの数
        /// </summary>
        int PointerBeginCount { get; }

        /// <summary>
        /// 前回のフレーム処理以降に検出した ポインターイベントの数
        /// </summary>
        int PointerCount { get; }

        /// <summary>
        /// 前回のフレーム処理以降に検出した ポインター終了イベントの数
        /// </summary>
        int PointerEndCount { get; }

        /// <summary>
        /// 前回のフレーム処理以降に検出した タップポイントの数
        /// </summary>
        int PointerTapCount { get; }

        /// <summary>
        /// タップ感度の設定
        /// </summary>
        GcTapSettings TapSettings { get; set; }

        /// <summary>
        /// 前回のフレーム処理以降に検出した ポインターイベントのうち、1つを取得します
        /// </summary>
        /// <param name="i">イベントインデックス（0 から <see cref="PointerCount"/>-1 までの連番。イベントIDではない）</param>
        /// <param name="e">イベント</param>
        /// <returns>取得できたかどうか</returns>
        bool TryGetPointerEvent(in int i, out GcPointerEvent e);

        /// <summary>
        /// 前回のフレーム処理以降に検出した ポインターイベントのうち、1つを取得します
        /// </summary>
        /// <param name="phase">イベント状態</param>
        /// <param name="i">イベントインデックス（イベントIDではない）</param>
        /// <param name="e">イベント</param>
        /// <returns>取得できたかどうか</returns>
        bool TryGetPointerEvent(in GcPointerEventPhase phase, in int i, out GcPointerEvent e);

        /// <summary>
        /// 前回のフレーム処理以降に検出した ポインターイベント全てを取得します
        /// </summary>
        /// <param name="array">イベント配列</param>
        /// <param name="count">イベント配列の要素数</param>
        /// <returns>1つ以上 取得できたかどうか</returns>
        bool TryGetPointerEventArray(out NativeArray<GcPointerEvent>.ReadOnly array, out int count);

        /// <summary>
        /// 前回のフレーム処理以降に検出した 指定された状態のポインターイベント全てを取得します
        /// </summary>
        /// <param name="phase">イベント状態</param>
        /// <param name="array">イベント配列</param>
        /// <param name="count">イベント配列の要素数</param>
        /// <returns>1つ以上 取得できたかどうか</returns>
        bool TryGetPointerEventArray(in GcPointerEventPhase phase, out NativeArray<GcPointerEvent>.ReadOnly array, out int count);

        /// <summary>
        /// 前回のフレーム処理以降に検出した タップポイントのうち、1つを取得します
        /// </summary>
        /// <param name="i">タップインデックス</param>
        /// <param name="point">タップポイント</param>
        /// <returns>取得できたかどうか</returns>
        bool TryGetPointerTapPoint(in int i, out float2 point);

        /// <summary>
        /// 前回のフレーム処理以降に検出した タップポイント全てを取得します
        /// </summary>
        /// <param name="array">タップポイント配列</param>
        /// <param name="count">タップポイント配列の要素数</param>
        /// <returns>1つ以上 取得できたかどうか</returns>
        bool TryGetPointerTapPointArray(out NativeArray<float2>.ReadOnly array, out int count);

        /// <summary>
        /// 現在有効なポインターのうち、1つの軌跡を取得します
        /// </summary>
        /// <param name="i">イベントインデックス（0 から <see cref="PointerCount"/>-1 までの連番。イベントIDではない）</param>
        /// <param name="trace">軌跡</param>
        /// <returns>取得できたかどうか</returns>
        bool TryGetPointerTrace(in int i, out GcPointerTrace trace);

        /// <summary>
        /// 現在有効なポインターのうち、1つの軌跡を取得します
        /// </summary>
        /// <remarks>
        /// <paramref name="phase"/> に指定できる値は <see cref="GcPointerEventPhase.Hold"/> または <see cref="GcPointerEventPhase.End"/> のみです
        /// </remarks>
        /// <param name="phase">イベント状態</param>
        /// <param name="i">イベントインデックス（イベントIDではない）</param>
        /// <param name="trace">軌跡</param>
        /// <returns>取得できたかどうか</returns>
        bool TryGetPointerTrace(in GcPointerEventPhase phase, in int i, out GcPointerTrace trace);

        /// <summary>
        /// 前回のフレーム処理以降に検出した ポインターの軌跡全てを取得します
        /// </summary>
        /// <param name="array">軌跡配列</param>
        /// <param name="count">軌跡配列の要素数</param>
        /// <returns>1つ以上 取得できたかどうか</returns>
        bool TryGetPointerTraceArray(out NativeArray<GcPointerTrace>.ReadOnly array, out int count);

        /// <summary>
        /// 前回のフレーム処理以降に検出した 指定された状態のポインターの軌跡全てを取得します
        /// </summary>
        /// <remarks>
        /// <paramref name="phase"/> に指定できる値は <see cref="GcPointerEventPhase.Hold"/> または <see cref="GcPointerEventPhase.End"/> のみです
        /// </remarks>
        /// <param name="array">軌跡配列</param>
        /// <param name="count">軌跡配列の要素数</param>
        /// <returns>1つ以上 取得できたかどうか</returns>
        bool TryGetPointerTraceArray(in GcPointerEventPhase phase, out NativeArray<GcPointerTrace>.ReadOnly array, out int count);
    }

    public interface IInputPointerEx : IInputPointer
    {
        /// <summary>
        /// 最後に検出したポインターイベントの 検出フレーム番号
        /// </summary>
        int LastPointerFrame { get; }

        /// <summary>
        /// 最後に検出したポインターイベントの位置
        /// </summary>
        float2 LastPointerPoint { get; }

        /// <summary>
        /// 最後に検出したポインターイベントの 検出時間（起動からの経過秒数）
        /// </summary>
        float LastPointerTime { get; }

        /// <summary>
        /// 最後に検出したポインターイベントの X座標
        /// </summary>
        float LastPointerX { get; }

        /// <summary>
        /// 最後に検出したポインターイベントの Y座標
        /// </summary>
        float LastPointerY { get; }

        /// <summary>
        /// 現在有効なポインターイベントのうち、指定した1つの累計移動距離を取得します
        /// </summary>
        /// <remarks>
        /// 有効なイベントがなかった場合は 0 を返します
        /// </remarks>
        /// <param name="i">イベントインデックス（0 から <see cref="IInputPointer.PointerCount"/>-1 までの連番。イベントIDではない）</param>
        /// <returns>イベントの累計移動距離</returns>
        [System.Obsolete("Use to `TryGetPointerTrace` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        float GetPointerDistance(in int i);

        /// <summary>
        /// 現在有効なポインターイベントのうち、指定した1つの継続時間を取得します
        /// </summary>
        /// <remarks>
        /// 有効なイベントがなかった場合は 0 を返します
        /// </remarks>
        /// <param name="i">イベントインデックス（0 から <see cref="IInputPointer.PointerCount"/>-1 までの連番。イベントIDではない）</param>
        /// <returns>イベントの継続時間</returns>
        [System.Obsolete("Use to `TryGetPointerTrace` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        float GetPointerDuration(in int i);

        /// <summary>
        /// 現在有効なポインターイベントのうち、指定した1つの継続フレーム数を取得します
        /// </summary>
        /// <remarks>
        /// 有効なイベントがなかった場合は 0 を返します
        /// </remarks>
        /// <param name="i">イベントインデックス（0 から <see cref="IInputPointer.PointerCount"/>-1 までの連番。イベントIDではない）</param>
        /// <returns>イベントの継続フレーム数</returns>
        [System.Obsolete("Use to `TryGetPointerTrace` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        int GetPointerFrameCount(in int i);

        /// <summary>
        /// 現在有効なポインターイベントのうち、指定した1つのX座標を取得します
        /// </summary>
        /// <remarks>
        /// 有効なイベントがなかった場合は 0 を返します
        /// </remarks>
        /// <param name="i">イベントインデックス（0 から <see cref="IInputPointer.PointerCount"/>-1 までの連番。イベントIDではない）</param>
        /// <returns>イベントのX座標</returns>
        [System.Obsolete("Use to `TryGetPointerEvent` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        float GetPointerX(in int i);

        /// <summary>
        /// 現在有効なポインターイベントのうち、指定した1つのY座標を取得します
        /// </summary>
        /// <remarks>
        /// 有効なイベントがなかった場合は 0 を返します
        /// </remarks>
        /// <param name="i">イベントインデックス（0 から <see cref="IInputPointer.PointerCount"/>-1 までの連番。イベントIDではない）</param>
        /// <returns>イベントのY座標</returns>
        [System.Obsolete("Use to `TryGetPointerEvent` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        float GetPointerY(in int i);

        /// <summary>
        /// 前回のフレーム処理以降に タップされたかどうか
        /// </summary>
        /// <returns>タップされたかどうか</returns>
        bool IsTapped();

        /// <summary>
        /// 前回のフレーム処理以降に タップされたかどうか
        /// </summary>
        /// <param name="point">タップ位置</param>
        /// <returns>タップされたかどうか</returns>
        bool IsTapped(out float2 point);

        /// <summary>
        /// 前回のフレーム処理以降に タップされたかどうか
        /// </summary>
        /// <param name="x">タップ位置 X座標</param>
        /// <param name="y">タップ位置 Y座標</param>
        /// <returns>タップされたかどうか</returns>
        bool IsTapped(out float x, out float y);

        /// <summary>
        /// 前回のフレーム処理以降に 指定した領域がタップされたかどうか
        /// </summary>
        /// <param name="aabb">領域</param>
        /// <param name="point">タップ位置</param>
        /// <returns>指定した領域がタップされたかどうか</returns>
        bool IsTapped(in GcAABB aabb, out float2 point);

        /// <summary>
        /// 前回のフレーム処理以降に 指定した領域がタップされたかどうか
        /// </summary>
        /// <param name="x">領域 左上X座標</param>
        /// <param name="y">領域 左上Y座標</param>
        /// <param name="width">領域 横幅</param>
        /// <param name="height">領域 縦幅</param>
        /// <param name="px">タップ位置 X座標</param>
        /// <param name="py">タップ位置 Y座標</param>
        /// <returns>指定した領域がタップされたかどうか</returns>
        bool IsTapped(in float x, in float y, in float width, in float height, out float px, out float py);

        /// <summary>
        /// 前回のフレーム処理以降に タッチされ始めたかどうか
        /// </summary>
        /// <returns>タッチされ始めたかどうか</returns>
        bool IsTouchBegan();

        /// <summary>
        /// 前回のフレーム処理以降に タッチされ始めたかどうか
        /// </summary>
        /// <param name="point">タッチ位置</param>
        /// <returns>タッチされ始めたかどうか</returns>
        bool IsTouchBegan(out float2 point);

        /// <summary>
        /// 前回のフレーム処理以降に タッチされ始めたかどうか
        /// </summary>
        /// <param name="pointer">ポインターイベント</param>
        /// <returns>タッチされ始めたかどうか</returns>
        bool IsTouchBegan(out GcPointerEvent pointer);

        /// <summary>
        /// 前回のフレーム処理以降に タッチされ始めたかどうか
        /// </summary>
        /// <param name="x">タッチ位置 X座標</param>
        /// <param name="y">タッチ位置 Y座標</param>
        /// <returns>タッチされ始めたかどうか</returns>
        bool IsTouchBegan(out float x, out float y);

        /// <summary>
        /// 前回のフレーム処理以降に 指定した領域がタッチされ始めたかどうか
        /// </summary>
        /// <param name="aabb">領域</param>
        /// <param name="point">タッチ位置</param>
        /// <returns>指定した領域がタッチされ始めたかどうか</returns>
        bool IsTouchBegan(in GcAABB aabb, out float2 point);

        /// <summary>
        /// 前回のフレーム処理以降に 指定した領域がタッチされ始めたかどうか
        /// </summary>
        /// <param name="x">領域 左上X座標</param>
        /// <param name="y">領域 左上Y座標</param>
        /// <param name="width">領域 横幅</param>
        /// <param name="height">領域 縦幅</param>
        /// <param name="px">タッチ位置 X座標</param>
        /// <param name="py">タッチ位置 Y座標</param>
        /// <returns>指定した領域がタッチされ始めたかどうか</returns>
        bool IsTouchBegan(in float x, in float y, in float width, in float height, out float px, out float py);

        /// <summary>
        /// 前回のフレーム処理以降 タッチされていたか（開始と終了を含む）
        /// </summary>
        /// <returns>タッチされていたか（開始と終了を含む）</returns>
        bool IsTouched();

        /// <summary>
        /// 前回のフレーム処理以降 タッチされていたか（開始と終了を含む）
        /// </summary>
        /// <param name="point">タッチ位置</param>
        /// <returns>タッチされていたか（開始と終了を含む）</returns>
        bool IsTouched(out float2 point);

        /// <summary>
        /// 前回のフレーム処理以降 タッチされていたか（開始と終了を含む）
        /// </summary>
        /// <param name="e">ポインターイベント</param>
        /// <returns>タッチされていたか（開始と終了を含む）</returns>
        bool IsTouched(out GcPointerEvent e);

        /// <summary>
        /// 前回のフレーム処理以降 タッチされていたか（開始と終了を含む）
        /// </summary>
        /// <param name="trace">タッチの軌跡</param>
        /// <returns>タッチされていたか（開始と終了を含む）</returns>
        bool IsTouched(out GcPointerTrace trace);

        /// <summary>
        /// 前回のフレーム処理以降 タッチされていたか（開始と終了を含む）
        /// </summary>
        /// <param name="x">タッチ位置 X座標</param>
        /// <param name="y">タッチ位置 Y座標</param>
        /// <returns>タッチされていたか（開始と終了を含む）</returns>
        bool IsTouched(out float x, out float y);

        /// <summary>
        /// 前回のフレーム処理以降 指定した領域がタッチされていたか（開始と終了を含む）
        /// </summary>
        /// <param name="aabb">領域</param>
        /// <param name="point">タッチ位置</param>
        /// <returns>指定した領域がタッチされていたか（開始と終了を含む）</returns>
        bool IsTouched(in GcAABB aabb, out float2 point);

        /// <summary>
        /// 前回のフレーム処理以降 指定した領域がタッチされていたか（開始と終了を含む）
        /// </summary>
        /// <param name="x">領域 左上X座標</param>
        /// <param name="y">領域 左上Y座標</param>
        /// <param name="width">領域 横幅</param>
        /// <param name="height">領域 縦幅</param>
        /// <param name="px">タッチ位置 X座標</param>
        /// <param name="py">タッチ位置 Y座標</param>
        /// <returns>指定した領域がタッチされていたか（開始と終了を含む）</returns>
        bool IsTouched(in float x, in float y, in float width, in float height, out float px, out float py);

        /// <summary>
        /// 前回のフレーム処理以降 タッチされ終えたかどうか
        /// </summary>
        /// <returns>タッチされ終えたかどうか</returns>
        bool IsTouchEnded();

        /// <summary>
        /// 前回のフレーム処理以降 タッチされ終えたかどうか
        /// </summary>
        /// <param name="point">タッチ位置</param>
        /// <returns>タッチされ終えたかどうか</returns>
        bool IsTouchEnded(out float2 point);

        /// <summary>
        /// 前回のフレーム処理以降 タッチされ終えたかどうか
        /// </summary>
        /// <param name="pointer">ポインターイベント</param>
        /// <returns>タッチされ終えたかどうか</returns>
        bool IsTouchEnded(out GcPointerEvent e);

        /// <summary>
        /// 前回のフレーム処理以降 タッチされ終えたかどうか
        /// </summary>
        /// <param name="pointer">タッチの軌跡</param>
        /// <returns>タッチされ終えたかどうか</returns>
        bool IsTouchEnded(out GcPointerTrace trace);

        /// <summary>
        /// 前回のフレーム処理以降 タッチされ終えたかどうか
        /// </summary>
        /// <param name="x">タッチ位置 X座標</param>
        /// <param name="y">タッチ位置 Y座標</param>
        /// <returns>タッチされ終えたかどうか</returns>
        bool IsTouchEnded(out float x, out float y);

        /// <summary>
        /// 前回のフレーム処理以降 指定した領域がタッチされ終えたかどうか
        /// </summary>
        /// <param name="aabb">領域</param>
        /// <param name="point">タッチ位置</param>
        /// <returns>指定した領域がタッチされ終えたかどうか</returns>
        bool IsTouchEnded(in GcAABB aabb, out float2 point);

        /// <summary>
        /// 前回のフレーム処理以降 指定した領域がタッチされ終えたかどうか
        /// </summary>
        /// <param name="x">領域 左上X座標</param>
        /// <param name="y">領域 左上Y座標</param>
        /// <param name="width">領域 横幅</param>
        /// <param name="height">領域 縦幅</param>
        /// <param name="px">タッチ位置 X座標</param>
        /// <param name="py">タッチ位置 Y座標</param>
        /// <returns>指定した領域がタッチされ終えたかどうか</returns>
        bool IsTouchEnded(in float x, in float y, in float width, in float height, out float px, out float py);

        [System.Obsolete("Use `gc.TapSettings` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        void SetTapSensitivity(in float maxDuration, in float maxDistance);
    }
}
