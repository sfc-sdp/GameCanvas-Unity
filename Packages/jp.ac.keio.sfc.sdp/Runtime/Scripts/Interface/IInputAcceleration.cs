/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2026 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
#nullable enable
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
        System.ReadOnlySpan<GcAccelerationEvent> AccelerationEvents { get; }

        /// <summary>
        /// 加速度計の動作周波数
        /// </summary>
        float AccelerometerSamplingRate { get; set; }

        /// <summary>
        /// 前回のフレーム処理以降に 加速度イベントの更新があったかどうか
        /// </summary>
        bool DidUpdateAccelerationThisFrame { get; }

        /// <summary>
        /// 加速度計が現在有効かどうか
        /// </summary>
        bool IsAccelerometerEnabled { get; set; }

        /// <summary>
        /// 端末が加速度計を搭載しているかどうか
        /// </summary>
        bool IsAccelerometerSupported { get; }

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
        /// <param name="events">イベント配列</param>
        /// <returns>1つ以上の加速度イベントがあったかどうか</returns>
        bool TryGetAccelerationEventAll(out System.ReadOnlySpan<GcAccelerationEvent> events);
    }

    public interface IInputAccelerationEx : IInputAcceleration
    {
    }
}
