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

namespace GameCanvas
{
    public interface IInputAcceleration
    {
        /// <summary>
        /// 前回のフレーム処理以降に検出した 加速度イベントの数
        /// </summary>
        int AccelerationEventCount { get; }

        /// <summary>
        /// 前回のフレーム処理以降に検出した 加速度イベントの列挙子
        /// </summary>
        GcAccelerationEvent.Enumerable AccelerationEvents { get; }

        /// <summary>
        /// 前回のフレーム処理以降に 加速度イベントの更新があったかどうか
        /// </summary>
        bool DidUpdateAccelerationThisFrame { get; }

        /// <summary>
        /// 加速度計が有効かどうか
        /// </summary>
        bool IsAccelerometerEnabled { get; set; }

        /// <summary>
        /// 最後に検出した加速度イベント
        /// </summary>
        GcAccelerationEvent LastAccelerationEvent { get; }

        /// <summary>
        /// 前回のフレーム処理以降に検出した 加速度イベントの取得を試みます
        /// </summary>
        /// <param name="i">イベントインデックス（0 から <see cref="AccelerationEventCount"/>-1 までの連番）</param>
        /// <param name="e">イベント</param>
        /// <returns>ポインターイベントを取得できたかどうか</returns>
        bool TryGetAccelerationEvent(int i, out GcAccelerationEvent e);

        /// <summary>
        /// 前回のフレーム処理以降に検出した 加速度イベントの取得を試みます
        /// </summary>
        /// <param name="array">イベント配列</param>
        /// <param name="count">イベント配列の要素数</param>
        /// <returns>1つ以上の加速度イベントがあったかどうか</returns>
        bool TryGetAccelerationEvents(out NativeArray<GcAccelerationEvent>.ReadOnly array, out int count);
    }

    public interface IInputAccelerationEx : IInputAcceleration
    {
        /// <summary>
        /// 最後に検出した加速度イベントの X方向の加速度
        /// </summary>
        [System.Obsolete("Use to `LastAcceleration`  instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        float AccelerationLastX { get; }

        /// <summary>
        /// 最後に検出した加速度イベントの Y方向の加速度
        /// </summary>
        [System.Obsolete("Use to `LastAcceleration`  instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        float AccelerationLastY { get; }

        /// <summary>
        /// 最後に検出した加速度イベントの Z方向の加速度
        /// </summary>
        [System.Obsolete("Use to `LastAcceleration`  instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        float AccelerationLastZ { get; }

        [System.Obsolete("Use to `TryGetAccelerationEvent`  instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        float GetAccelerationX(in int i, bool normalize = false);

        [System.Obsolete("Use to `TryGetAccelerationEvent`  instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        float GetAccelerationY(in int i, bool normalize = false);

        [System.Obsolete("Use to `TryGetAccelerationEvent`  instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        float GetAccelerationZ(in int i, bool normalize = false);
    }
}
