/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using Unity.Collections;
using UnityEngine;

namespace GameCanvas
{
    /// <summary>
    /// カメラ（外部入力映像）デバイス
    /// </summary>
    public sealed class GcCameraDevice
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        /// <summary>
        /// 任意座標指定オートフォーカスに対応しているかどうか
        /// </summary>
        public readonly bool CanFocusPoint;

        /// <summary>
        /// デバイス名
        /// </summary>
        public readonly string DeviceName;

        /// <summary>
        /// 深度カメラかどうか
        /// </summary>
        public readonly bool IsDepth;

        /// <summary>
        /// 前面（自撮り）カメラかどうか
        /// </summary>
        public readonly bool IsFront;

        /// <summary>
        /// 解像度とリフレッシュレートの候補
        /// </summary>
        /// <remarks>
        /// iOS, Android の実機以外では常に無効（配列長が0）です
        /// </remarks>
        public readonly GcResolution[] Resolutions;
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public override string ToString()
        {
            var builder = new System.Text.StringBuilder();
            builder.AppendLine($"{nameof(GcCameraDevice)}: {{ Name: {DeviceName}, IsDepth: {IsDepth}, IsFront: {IsFront}, CanFocus: {CanFocusPoint} }}");
            for (var i = 0; i < Resolutions.Length; i++)
            {
                builder.AppendLine($"- {i:00}: {Resolutions[i].Size.x}x{Resolutions[i].Size.y} - {Resolutions[i].RefreshRate}Hz");
            }
            return builder.ToString();
        }
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal GcCameraDevice(in string name, in bool isDepth, in GcResolution[] resArray, in bool isFront, in bool canFocusPoint)
        {
            DeviceName = name;
            IsDepth = isDepth;
            IsFront = isFront;
            CanFocusPoint = !isDepth && canFocusPoint;
            Resolutions = resArray;
        }

        #endregion
    }
}
