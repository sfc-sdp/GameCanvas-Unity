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
using UnityEngine;

namespace GameCanvas
{
    public interface IInputGeolocation
    {
        /// <summary>
        /// 前回のフレーム処理以降に 位置情報イベントの更新があったかどうか
        /// </summary>
        bool DidUpdateGeolocationThisFrame { get; }

        /// <summary>
        /// 位置情報サービスの状態
        /// </summary>
        LocationServiceStatus GeolocationStatus { get; }

        /// <summary>
        /// 位置情報へのアクセス権限を取得済みかどうか
        /// </summary>
        bool HasUserAuthorizedPermissionGeolocation { get; }

        /// <summary>
        /// 最後に取得した位置情報イベント
        /// </summary>
        GcGeolocationEvent LastGeolocationEvent { get; }

        /// <summary>
        /// 位置情報へのアクセス権限を要求します
        /// </summary>
        /// <param name="callback">結果を通知するコールバック</param>
        void RequestUserAuthorizedPermissionGeolocationAsync(in System.Action<bool> callback);

        /// <summary>
        /// 位置情報サービスを起動します
        /// </summary>
        /// <remarks>
        /// 位置情報へのアクセス権限がない場合、この処理は失敗します
        /// </remarks>
        /// <param name="desiredAccuracy">望ましいサービス精度（メートル単位）</param>
        /// <param name="updateDistance">位置情報の更新に必要な最小移動距離（メートル単位）</param>
        void StartGeolocationService(float desiredAccuracy = 10f, float updateDistance = 10f);

        /// <summary>
        /// 位置情報サービスを停止します
        /// </summary>
        void StopGeolocationService();

        /// <summary>
        /// 前回のフレーム処理以降にあった 位置情報イベントの取得を試みます
        /// </summary>
        /// <param name="e">位置情報イベント</param>
        /// <returns>前回のフレーム処理以降に 位置情報イベントがあったかどうか</returns>
        bool TryGetGeolocationEvent(out GcGeolocationEvent e);
    }

    public interface IInputGeolocationEx : IInputGeolocation
    {
        [System.Obsolete("Use to `LastGeolocationEvent` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        float GeolocationLastAltitude { get; }

        [System.Obsolete("Use to `LastGeolocationEvent` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        float GeolocationLastLatitude { get; }

        [System.Obsolete("Use to `LastGeolocationEvent` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        float GeolocationLastLongitude { get; }

        [System.Obsolete("Use to `LastGeolocationEvent` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        System.DateTimeOffset GeolocationLastTime { get; }

        [System.Obsolete("Use to `HasUserAuthorizedPermissionGeolocation` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool HasGeolocationPermission { get; }

        [System.Obsolete("Use to `DidUpdateGeolocationThisFrame` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool HasGeolocationUpdate { get; }

        /// <summary>
        /// 位置情報サービスが起動しているかどうか（起動中を含む）
        /// </summary>
        bool IsGeolocationRunning { get; }
    }
}
