/**
 * GameCanvas for Unity [GameCanvas Env]
 * 
 * Copyright (c) 2015-2016 Seibe TAKAHASHI
 * 
 * This software is released under the MIT License.
 * http://opensource.org/licenses/mit-license.php
 */

namespace GameCanvas
{
    /// <summary>
    /// 環境変数
    /// </summary>
    public class Env
    {
        /// <summary>
        /// GameCanvas のバージョン情報
        /// </summary>
        public const string Version = "v1.0.5";

        /// <summary>
        /// GameCanvas API のバージョン情報
        /// </summary>
        public const string APIVersion = "v1.0";

        /// <summary>
        /// GameCanvas の著作権表記
        /// </summary>
        public const string Copyright = "Copyright (c) 2015-2016 Smart Device Programming.";

        /// <summary>
        /// GameCanvas の制作者情報
        /// </summary>
        public readonly static string[] Authors = 
        {
            "kuro",
            "fujieda",
            "seibe"
        };
    }
}
