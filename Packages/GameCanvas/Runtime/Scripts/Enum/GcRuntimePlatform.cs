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

namespace GameCanvas
{
    /// <summary>
    /// 実行時プラットフォーム
    /// </summary>
    [System.Serializable]
    public enum GcRuntimePlatform
    {
        Unknown,
        Android,
        iOS,
        WebGL
    }
}
