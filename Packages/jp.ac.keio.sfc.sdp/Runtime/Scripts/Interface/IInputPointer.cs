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

namespace GameCanvas
{
    public interface IInputPointer
    {
        /// <summary>代表のポインター。接触終了後に残った指へ乗り移らない。</summary>
        GcPointer Pointer { get; }
        /// <summary>このフレームの状態一覧。終了した接触も含む。添字はIDではない。</summary>
        GcReadOnlyList<GcPointer> Pointers { get; }
        /// <summary>入力変化の順序付き一覧。通常の解放と中断を区別する。</summary>
        GcReadOnlyList<GcPointerEvent> PointerEvents { get; }

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

        /// <summary>このフレームに成立したタップの開始位置。中断は含みません。</summary>
        GcReadOnlyList<GcPoint> Taps { get; }
        /// <summary>タップとみなす移動距離と時間の上限。</summary>
        GcTapSettings TapSettings { get; set; }
    }
}
