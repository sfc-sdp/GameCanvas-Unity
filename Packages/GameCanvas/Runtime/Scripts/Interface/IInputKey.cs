/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2022 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
#nullable enable
using System.ComponentModel;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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
        bool IsKeyDown(in Key key);

        /// <summary>
        /// 指定されたキーが押されているかどうか（押された瞬間を除く）
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>押されているかどうか（押された瞬間を除く）</returns>
        bool IsKeyHold(in Key key);

        /// <summary>
        /// 指定されたキーが押されているかどうか（押された瞬間を含む）
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>押されているかどうか（押された瞬間を含む）</returns>
        bool IsKeyPress(in Key key);

        /// <summary>
        /// 指定されたキーが離されたかどうか
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>離されたかどうか</returns>
        bool IsKeyUp(in Key key);

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
        bool TryGetKeyEvent(in Key key, out GcKeyEvent e);

        /// <summary>
        /// 前回のフレーム処理以降に更新された キーイベント全てを取得します
        /// </summary>
        /// <param name="events">キーイベントの一覧</param>
        /// <returns>要素数が1以上かどうか</returns>
        bool TryGetKeyEventAll(out System.ReadOnlySpan<GcKeyEvent> events);

        /// <summary>
        /// 前回のフレーム処理以降に更新された 指定された状態のキーイベント全てを取得します
        /// </summary>
        /// <param name="phase">キーイベント状態</param>
        /// <param name="events">キーイベントの一覧</param>
        /// <returns>要素数が1以上かどうか</returns>
        bool TryGetKeyEventAll(in GcKeyEventPhase phase, out System.ReadOnlySpan<GcKeyEvent> events);

        /// <summary>
        /// 指定されたキーの軌跡があれば取得します
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="trace">キーの軌跡</param>
        /// <returns>取得できたかどうか</returns>
        bool TryGetKeyTrace(in Key key, out GcKeyTrace trace);

        /// <summary>
        /// 前回のフレーム処理以降に更新された キーの軌跡全てを取得します
        /// </summary>
        /// <param name="traces">キーの軌跡の一覧</param>
        /// <returns>要素数が1以上かどうか</returns>
        bool TryGetKeyTraceAll(out System.ReadOnlySpan<GcKeyTrace> traces);

        /// <summary>
        /// 前回のフレーム処理以降に更新された キーの軌跡全てを取得します
        /// </summary>
        /// <param name="phase">キーイベント状態</param>
        /// <param name="traces">キーの軌跡の一覧</param>
        /// <returns>要素数が1以上かどうか</returns>
        bool TryGetKeyTraceAll(in GcKeyEventPhase phase, out System.ReadOnlySpan<GcKeyTrace> traces);

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

        #region Obsolete
        [System.Obsolete("Use to `TryGetKeyEventAll` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool TryGetKeyEventArray(out NativeArray<GcKeyEvent>.ReadOnly array, out int count);

        [System.Obsolete("Use to `TryGetKeyEventAll` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool TryGetKeyEventArray(in GcKeyEventPhase phase, out NativeArray<GcKeyEvent>.ReadOnly array, out int count);

        [System.Obsolete("Use to `TryGetKeyTraceAll` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool TryGetKeyTraceArray(out NativeArray<GcKeyTrace>.ReadOnly array, out int count);

        [System.Obsolete("Use to `TryGetKeyTraceAll` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool TryGetKeyTraceArray(in GcKeyEventPhase phase, out NativeArray<GcKeyTrace>.ReadOnly array, out int count);
        #endregion
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

        /// <summary>
        /// 戻るボタン（Androidのみ）
        /// </summary>
        Key KeyEscape { get; }

        /// <summary>
        /// 前回のフレームに引き続き 押されているキーの数（押された瞬間を含む）
        /// </summary>
        int KeyPressCount { get; }

        /// <summary>
        /// 指定されたキーが押されている時間（秒）
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>時間（秒）</returns>
        float GetKeyPressDuration(in Key key);

        /// <summary>
        /// 指定されたキーが押されているフレーム数
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>フレーム数</returns>
        int GetKeyPressFrameCount(in Key key);

        /// <summary>
        /// 指定されたキーが押されているかどうか（押された瞬間を除く）
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="trace">キーの軌跡</param>
        /// <returns>押されているかどうか（押された瞬間を除く）</returns>
        bool IsKeyHold(in Key key, out GcKeyTrace trace);

        /// <summary>
        /// 指定されたキーが離されたかどうか
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="trace">キーの軌跡</param>
        /// <returns>離されたかどうか</returns>
        bool IsKeyUp(in Key key, out GcKeyTrace trace);

        #region Obsolete
        [System.Obsolete("Use to `IsKeyDown` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool IsPressBackButton { get; }

        [System.Obsolete("Use to `IsKeyDown` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool GetIsKeyBegan(in KeyCode key);

        [System.Obsolete("Use to `IsKeyUp` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool GetIsKeyEnded(in KeyCode key);

        [System.Obsolete("parameter `char` is depricated. Please Use `UnityEngine.InputSystem.Key` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        float GetKeyPressDuration(in char key);

        [System.Obsolete("parameter `char` is depricated. Please Use `UnityEngine.InputSystem.Key` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        int GetKeyPressFrameCount(in char key);

        [System.Obsolete("parameter `char` is depricated. Please Use `UnityEngine.InputSystem.Key` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool IsKeyDown(in char key);

        [System.Obsolete("`UnityEngine.KeyCode` is depricated. Please Use `UnityEngine.InputSystem.Key` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool IsKeyDown(in KeyCode key);

        [System.Obsolete("parameter `char` is depricated. Please Use `UnityEngine.InputSystem.Key` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool IsKeyHold(in char key);

        [System.Obsolete("`UnityEngine.KeyCode` is depricated. Please Use `UnityEngine.InputSystem.Key` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool IsKeyHold(in KeyCode key);

        [System.Obsolete("parameter `char` is depricated. Please Use `UnityEngine.InputSystem.Key` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool IsKeyPress(in char key);

        [System.Obsolete("`UnityEngine.KeyCode` is depricated. Please Use `UnityEngine.InputSystem.Key` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool IsKeyPress(in KeyCode key);

        [System.Obsolete("parameter `char` is depricated. Please Use `UnityEngine.InputSystem.Key` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool IsKeyUp(in char key);

        [System.Obsolete("`UnityEngine.KeyCode` is depricated. Please Use `UnityEngine.InputSystem.Key` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool IsKeyUp(in KeyCode key);

        [System.Obsolete("parameter `char` is depricated. Please Use `UnityEngine.InputSystem.Key` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool TryGetKeyEvent(in char key, out GcKeyEvent e);

        [System.Obsolete("`UnityEngine.KeyCode` is depricated. Please Use `UnityEngine.InputSystem.Key` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool TryGetKeyEvent(in KeyCode key, out GcKeyEvent e);

        [System.Obsolete("parameter `char` is depricated. Please Use `UnityEngine.InputSystem.Key` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool TryGetKeyTrace(in char key, out GcKeyTrace trace);

        [System.Obsolete("`UnityEngine.KeyCode` is depricated. Please Use `UnityEngine.InputSystem.Key` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool TryGetKeyTrace(in KeyCode key, out GcKeyTrace trace);
        #endregion
    }
}
