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
using UnityEngine;

namespace GameCanvas
{
    public interface IInputKey
    {
        /// <summary>
        /// スクリーンキーボードがサポートされているかどうか
        /// </summary>
        bool IsScreenKeyboardSupported { get; }

        /// <summary>
        /// スクリーンキーボードが表示されているかどうか
        /// </summary>
        bool IsScreenKeyboardVisible { get; }

        /// <summary>
        /// 前回のフレーム処理以降に 押されたキーの数
        /// </summary>
        int KeyDownCount { get; }

        /// <summary>
        /// 前回のフレームに引き続き 押されているキーの数（押された瞬間を除く）
        /// </summary>
        int KeyHoldCount { get; }

        /// <summary>
        /// 前回のフレーム処理以降に 離されたキーの数
        /// </summary>
        int KeyUpCount { get; }

        /// <summary>
        /// スクリーンキーボードを閉じます
        /// </summary>
        void HideScreenKeyboard();

        /// <summary>
        /// 指定されたキーが押されたかどうか
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>押されたかどうか</returns>
        bool IsKeyDown(in KeyCode key);

        /// <summary>
        /// 指定されたキーが押されているかどうか（押された瞬間を除く）
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>押されているかどうか（押された瞬間を除く）</returns>
        bool IsKeyHold(in KeyCode key);

        /// <summary>
        /// 指定されたキーが押されているかどうか（押された瞬間を含む）
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>押されているかどうか（押された瞬間を含む）</returns>
        bool IsKeyPress(in KeyCode key);

        /// <summary>
        /// 指定されたキーが離されたかどうか
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>離されたかどうか</returns>
        bool IsKeyUp(in KeyCode key);

        /// <summary>
        /// スクリーンキーボードを表示します
        /// </summary>
        /// <returns>表示できたかどうか</returns>
        bool ShowScreenKeyboard();

        /// <summary>
        /// 指定されたキーのキーイベントがあれば取得します
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="e">キーイベント</param>
        /// <returns>取得できたかどうか</returns>
        bool TryGetKeyEvent(in KeyCode key, out GcKeyEvent e);

        /// <summary>
        /// 前回のフレーム処理以降に更新された キーイベント全てを取得します
        /// </summary>
        /// <param name="array">キーイベント配列</param>
        /// <param name="count">キーイベント配列の要素数</param>
        /// <returns>要素数が1以上かどうか</returns>
        bool TryGetKeyEventArray(out NativeArray<GcKeyEvent>.ReadOnly array, out int count);

        /// <summary>
        /// 前回のフレーム処理以降に更新された 指定された状態のキーイベント全てを取得します
        /// </summary>
        /// <param name="phase">キーイベント状態</param>
        /// <param name="array">キーイベント配列</param>
        /// <param name="count">キーイベント配列の要素数</param>
        /// <returns>要素数が1以上かどうか</returns>
        bool TryGetKeyEventArray(in GcKeyEventPhase phase, out NativeArray<GcKeyEvent>.ReadOnly array, out int count);

        /// <summary>
        /// 指定されたキーの軌跡があれば取得します
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="trace">キーの軌跡</param>
        /// <returns>取得できたかどうか</returns>
        bool TryGetKeyTrace(in KeyCode key, out GcKeyTrace trace);

        /// <summary>
        /// 前回のフレーム処理以降に更新された キーの軌跡全てを取得します
        /// </summary>
        /// <param name="array">キーの軌跡配列</param>
        /// <param name="count">キーの軌跡配列の要素数</param>
        /// <returns>要素数が1以上かどうか</returns>
        bool TryGetKeyTraceArray(out NativeArray<GcKeyTrace>.ReadOnly array, out int count);

        /// <summary>
        /// 前回のフレーム処理以降に更新された 指定された状態のキーの軌跡全てを取得します
        /// </summary>
        /// <remarks>
        /// <paramref name="phase"/> に指定できる値は <see cref="GcKeyEventPhase.Hold"/> または <see cref="GcKeyEventPhase.Up"/> のみです
        /// </remarks>
        /// <param name="phase">キーイベント状態</param>
        /// <param name="array">キーの軌跡配列</param>
        /// <param name="count">キーの軌跡配列の要素数</param>
        /// <returns>要素数が1以上かどうか</returns>
        bool TryGetKeyTraceArray(in GcKeyEventPhase phase, out NativeArray<GcKeyTrace>.ReadOnly array, out int count);

        /// <summary>
        /// スクリーンキーボードの表示位置を取得します
        /// </summary>
        /// <remarks>
        /// - 表示中のみ取得できます<br />
        /// - 表示直後のアニメーション中は取得できません<br />
        /// - Android OSでは取得できません
        /// </remarks>
        /// <param name="area">表示位置（キャンバス座標系）</param>
        /// <returns>取得できたかどうか</returns>
        bool TryGetScreenKeyboardArea(out GcAABB area);
    }

    public interface IInputKeyEx : IInputKey
    {
        /// <summary>
        /// なんらかのキーイベントがあるかどうか
        /// </summary>
        bool IsAnyKey { get; }

        /// <summary>
        /// いずれかのキーが押されたかどうか
        /// </summary>
        bool IsAnyKeyDown { get; }

        /// <summary>
        /// いずれかのキーが押されているかどうか（押された瞬間を除く）
        /// </summary>
        bool IsAnyKeyHold { get; }

        /// <summary>
        /// いずれかのキーが押されているかどうか（押された瞬間を含む）
        /// </summary>
        bool IsAnyKeyPress { get; }

        /// <summary>
        /// いずれかのキーが離されたかどうか
        /// </summary>
        bool IsAnyKeyUp { get; }

        [System.Obsolete("Use to `IsKeyDown` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool IsPressBackButton { get; }

        /// <summary>
        /// 戻るボタン（Androidのみ）
        /// </summary>
        KeyCode KeyEscape { get; }

        /// <summary>
        /// 前回のフレームに引き続き 押されているキーの数（押された瞬間を含む）
        /// </summary>
        int KeyPressCount { get; }

        [System.Obsolete("Use to `IsKeyDown` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool GetIsKeyBegan(in KeyCode key);

        [System.Obsolete("Use to `IsKeyUp` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool GetIsKeyEnded(in KeyCode key);

        /// <summary>
        /// 指定されたキーが押されている時間（秒）
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>時間（秒）</returns>
        float GetKeyPressDuration(in KeyCode key);

        /// <summary>
        /// 指定されたキーが押されている時間（秒）
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>時間（秒）</returns>
        float GetKeyPressDuration(in char key);

        /// <summary>
        /// 指定されたキーが押されているフレーム数
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>フレーム数</returns>
        int GetKeyPressFrameCount(in KeyCode key);

        /// <summary>
        /// 指定されたキーが押されているフレーム数
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>フレーム数</returns>
        int GetKeyPressFrameCount(in char key);

        /// <summary>
        /// 指定されたキーが押されたかどうか
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>押されたかどうか</returns>
        bool IsKeyDown(in char key);

        /// <summary>
        /// 指定されたキーが押されているかどうか（押された瞬間を除く）
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>押されているかどうか（押された瞬間を除く）</returns>
        bool IsKeyHold(in char key);

        /// <summary>
        /// 指定されたキーが押されているかどうか（押された瞬間を除く）
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="trace">キーの軌跡</param>
        /// <returns>押されているかどうか（押された瞬間を除く）</returns>
        bool IsKeyHold(in KeyCode key, out GcKeyTrace trace);

        /// <summary>
        /// 指定されたキーが押されているかどうか（押された瞬間を含む）
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>押されているかどうか（押された瞬間を含む）</returns>
        bool IsKeyPress(in char key);

        /// <summary>
        /// 指定されたキーが離されたかどうか
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>離されたかどうか</returns>
        bool IsKeyUp(in char key);

        /// <summary>
        /// 指定されたキーが離されたかどうか
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="trace">キーの軌跡</param>
        /// <returns>離されたかどうか</returns>
        bool IsKeyUp(in KeyCode key, out GcKeyTrace trace);

        /// <summary>
        /// 指定されたキーのキーイベントがあれば取得します
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="e">キーイベント</param>
        /// <returns>取得できたかどうか</returns>
        bool TryGetKeyEvent(in char key, out GcKeyEvent e);

        /// <summary>
        /// 指定されたキーの軌跡があれば取得します
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="trace">キーの軌跡</param>
        /// <returns>取得できたかどうか</returns>
        bool TryGetKeyTrace(in char key, out GcKeyTrace trace);
    }
}
