#nullable enable
namespace GameCanvas
{
    public interface IInputKey
    {
        /// <summary>指定したキーの、このフレームの状態を取得します。</summary>
        GcKeyState Key(GcKey key);

        /// <summary>
        /// スクリーンキーボードがサポートされているかどうか
        /// </summary>
        bool IsScreenKeyboardSupported { get; }

        /// <summary>
        /// スクリーンキーボードが表示されているかどうか
        /// </summary>
        bool IsScreenKeyboardVisible { get; }

        /// <summary>
        /// スクリーンキーボードを閉じます
        /// </summary>
        void HideScreenKeyboard();

        /// <summary>
        /// スクリーンキーボードを表示します
        /// </summary>
        /// <returns>表示できたかどうか</returns>
        bool ShowScreenKeyboard();

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

        /// <summary>このフレームに届いたキーの変化を、順番に読みます。</summary>
        GcReadOnlyList<GcKeyEvent> KeyEvents { get; }
    }
}
