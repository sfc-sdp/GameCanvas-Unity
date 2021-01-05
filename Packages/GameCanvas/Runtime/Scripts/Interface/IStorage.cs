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
using System.Numerics;

namespace GameCanvas
{
    public interface IStorage
    {
        /// <summary>
        /// ローカルストレージに保存された全ての値を削除します
        /// </summary>
        void EraseSavedDataAll();

        /// <summary>
        /// ローカルストレージに値を保存します
        /// </summary>
        /// <remarks>
        /// <paramref name="value"/> に null を渡した場合、キーに紐づくデータを削除します
        /// </remarks>
        /// <param name="key">キー</param>
        /// <param name="value">保存する値</param>
        void Save(in string key, float? value);

        /// <summary>
        /// ローカルストレージに値を保存します
        /// </summary>
        /// <remarks>
        /// <paramref name="value"/> に null を渡した場合、キーに紐づくデータを削除します
        /// </remarks>
        /// <param name="key">キー</param>
        /// <param name="value">保存する値</param>
        void Save(in string key, int? value);

        /// <summary>
        /// ローカルストレージに値を保存します
        /// </summary>
        /// <remarks>
        /// <paramref name="value"/> に null を渡した場合、キーに紐づくデータを削除します
        /// </remarks>
        /// <param name="key">キー</param>
        /// <param name="value">保存する値</param>
        void Save(in string key, string value);

        /// <summary>
        /// 現在の画面を 画像として保存します
        /// </summary>
        /// <remarks>
        /// - 保存に成功した場合、<paramref name="onComplete"/> の引数には、画像保存先のパスが渡されます<br />
        /// - 保存に失敗した場合、<paramref name="onComplete"/> の引数には null が渡されます
        /// </remarks>
        /// <param name="onComplete">保存完了後に呼び出されるコールバック</param>
        void SaveScreenshotAsync(in System.Action<string> onComplete = null);

        /// <summary>
        /// ローカルストレージに保存された値を取り出します
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">取り出した値</param>
        /// <returns>取り出せたかどうか</returns>
        bool TryLoad(in string key, out float value);

        /// <summary>
        /// ローカルストレージに保存された値を取り出します
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">取り出した値</param>
        /// <returns>取り出せたかどうか</returns>
        bool TryLoad(in string key, out int value);

        /// <summary>
        /// ローカルストレージに保存された値を取り出します
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">取り出した値</param>
        /// <returns>取り出せたかどうか</returns>
        bool TryLoad(in string key, out string value);
    }

    public interface IStorageEx : IStorage
    {
        [System.Obsolete("Use to `SaveScreenshotAsync` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        void WriteScreenImage(in string file);
    }
}
