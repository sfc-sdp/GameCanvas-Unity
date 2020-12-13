/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using System.Collections.ObjectModel;
using Unity.Mathematics;
using UnityEngine;

namespace GameCanvas
{
    public interface IInputCamera
    {
        /// <summary>
        /// 認識可能なカメラ（外部入力映像）の数
        /// </summary>
        int CameraDeviceCount { get; }

        /// <summary>
        /// カメラデバイスへのアクセス権限を取得済みかどうか
        /// </summary>
        bool HasUserAuthorizedPermissionCamera { get; }

        /// <summary>
        /// 前回のフレーム処理以降に 指定されたカメラ（外部入力映像）テクスチャーに更新があったかどうか
        /// </summary>
        /// <param name="camera">カメラ（外部入力映像）</param>
        /// <returns>テクスチャーに更新があったかどうか</returns>
        bool DidUpdateCameraImageThisFrame(in GcCameraDevice camera);

        /// <summary>
        /// カメラ（外部入力映像）の任意の点に焦点をあわせるように要求します
        /// </summary>
        /// <remarks>
        /// - このAPIは、対応するカメラデバイスに対して、実機上での実行したときのみ動作します<br />
        /// - <paramref name="uv"/>引数は、キャンバス座標系ではなく、左下を原点とする 0.0～1.0 のUV座標系を指定します<br />
        /// - 焦点あわせを解除するには、<paramref name="uv"/>引数に null を渡します
        /// </remarks>
        /// <param name="camera">カメラ（外部入力映像）</param>
        /// <param name="uv">焦点をあわせる位置（左下を原点とする 0.0～1.0 のUV座標系）</param>
        void FocusCameraImage(in GcCameraDevice camera, in float2? uv);

        /// <summary>
        /// 指定されたカメラ（外部入力映像）のテクスチャーを生成もしくは取得します
        /// </summary>
        /// <remarks>
        /// - 既にテクスチャーが生成済みの場合は、<paramref name="request"/>引数の値は無視されます<br />
        /// - 生成後に解像度を変更する場合は <see cref="TryChangeCameraImageResolution"/> 関数を呼び出してください
        /// </remarks>
        /// <param name="camera">カメラ（外部入力映像）</param>
        /// <param name="request">テクスチャーが未生成だった場合に、希望する解像度とリフレッシュレート</param>
        /// <returns>テクスチャー</returns>
        WebCamTexture GetOrCreateCameraTexture(in GcCameraDevice camera, in GcResolution request);

        /// <summary>
        /// 指定されたカメラ（外部入力映像）は上下が反転しているかどうか
        /// </summary>
        /// <param name="camera">カメラ（外部入力映像）</param>
        /// <returns>上下が反転しているかどうか</returns>
        bool IsFlippedCameraImage(in GcCameraDevice camera);

        /// <summary>
        /// 指定されたカメラ（外部入力映像）の更新が行われているかどうか
        /// </summary>
        /// <param name="camera">カメラ（外部入力映像）</param>
        /// <returns>更新が行われているかどうか</returns>
        bool IsPlayingCameraImage(in GcCameraDevice camera);

        /// <summary>
        /// 指定されたカメラ（外部入力映像）の更新処理を一時停止します
        /// </summary>
        /// <param name="camera">カメラ（外部入力映像）</param>
        /// <returns>停止したかどうか（元から停止していた場合を含まない）</returns>
        bool PauseCameraImage(in GcCameraDevice camera);

        /// <summary>
        /// 指定されたカメラ（外部入力映像）の更新処理を開始します
        /// </summary>
        /// <remarks>
        /// - カメラごとにこのAPIを呼び出すことで、それらを同時に更新することが可能です<br />
        /// - ただし、同じ名称のカメラは、同時に更新することができません<br />
        /// - 既にテクスチャーが生成済みの場合は、<paramref name="request"/>引数の値は無視されます<br />
        /// - 生成後に解像度を変更する場合は <see cref="TryChangeCameraImageResolution"/> 関数を呼び出してください
        /// </remarks>
        /// <param name="camera">カメラ（外部入力映像）</param>
        /// <param name="request">テクスチャーが未生成だった場合に、希望する解像度とリフレッシュレート</param>
        /// <param name="resolution">カメラ（外部入力映像）の解像度</param>
        /// <returns>開始したかどうか（元から更新していた場合を含まない）</returns>
        bool PlayCameraImage(in GcCameraDevice camera, in GcResolution request, out int2 resolution);

        /// <summary>
        /// カメラデバイスへのアクセス権限を要求します
        /// </summary>
        /// <param name="callback">結果を通知するコールバック</param>
        void RequestUserAuthorizedPermissionCameraAsync(in System.Action<bool> callback);

        /// <summary>
        /// 指定されたカメラ（外部入力映像）の更新処理を完全に停止します
        /// </summary>
        /// <param name="camera">カメラ（外部入力映像）</param>
        void StopCameraImage(in GcCameraDevice camera);

        /// <summary>
        /// 指定されたカメラ（外部入力映像）の解像度変更を試みます
        /// </summary>
        /// <param name="camera">カメラ（外部入力映像）</param>
        /// <param name="request">希望する解像度とリフレッシュレート</param>
        /// <returns>実際に設定された解像度</returns>
        int2 TryChangeCameraImageResolution(in GcCameraDevice camera, in GcResolution request);

        /// <summary>
        /// カメラ（外部入力映像）の取得を試みます
        /// </summary>
        /// <param name="camera">カメラ（外部入力映像）</param>
        /// <returns>取得できたかどうか</returns>
        bool TryGetCameraImage(out GcCameraDevice camera);

        /// <summary>
        /// デバイス名を指定して、カメラ（外部入力映像）の取得を試みます
        /// </summary>
        /// <param name="deviceName">デバイス名</param>
        /// <param name="camera">カメラ（外部入力映像）</param>
        /// <returns>取得できたかどうか</returns>
        bool TryGetCameraImage(in string deviceName, out GcCameraDevice camera);

        /// <summary>
        /// 全ての認識可能なカメラ（外部入力映像）の取得を試みます
        /// </summary>
        /// <param name="array">カメラ（外部入力映像）配列</param>
        /// <param name="count">カメラ（外部入力映像）配列の要素数</param>
        /// <returns>1つ以上 取得できたかどうか</returns>
        bool TryGetCameraImageAll(out ReadOnlyCollection<GcCameraDevice> array);

        /// <summary>
        /// 指定されたカメラ（外部入力映像）の回転角度取得を試みます
        /// </summary>
        /// <param name="camera">カメラ（外部入力映像）</param>
        /// <param name="degree">回転角度</param>
        /// <returns>取得できたかどうか</returns>
        bool TryGetCameraImageRotation(in GcCameraDevice camera, out float degree);

        /// <summary>
        /// 指定されたカメラ（外部入力映像）の解像度取得を試みます
        /// </summary>
        /// <remarks>
        /// - テクスチャーが未生成の場合は、取得に失敗します<br />
        /// - テクスチャーを生成するには、<see cref="PlayCameraImage"/> もしくは <see cref="GetOrCreateCameraTexture"/> 関数を呼び出してください
        /// </remarks>
        /// <param name="camera">カメラ（外部入力映像）</param>
        /// <param name="resolution">カメラ（外部入力映像）の解像度</param>
        /// <returns>取得できたかどうか</returns>
        bool TryGetCameraImageSize(in GcCameraDevice camera, out int2 resolution);

        /// <summary>
        /// カメラ（外部入力映像）の一覧を更新します
        /// </summary>
        /// <returns>更新後の 認識可能なカメラ（外部入力映像）の数</returns>
        int UpdateCameraDevice();
    }

    public interface IInputCameraEx : IInputCamera
    {
        /// <summary>
        /// カメラ（外部入力映像）を描画します
        /// </summary>
        /// <param name="camera">カメラ（外部入力映像）</param>
        /// <param name="autoPlay">カメラ（外部入力映像）が停止していた場合、内部で<see cref="PlayCameraImage"/>を呼び出すかどうか</param>
        void DrawCameraImage(in GcCameraDevice camera, bool autoPlay = true);

        /// <summary>
        /// カメラ（外部入力映像）を描画します
        /// </summary>
        /// <param name="camera">カメラ（外部入力映像）</param>
        /// <param name="position">位置</param>
        /// <param name="degree">回転（度数法）</param>
        /// <param name="autoPlay">カメラ（外部入力映像）が停止していた場合、内部で<see cref="PlayCameraImage"/>を呼び出すかどうか</param>
        void DrawCameraImage(in GcCameraDevice camera, in float2 position, float degree = 0f, bool autoPlay = true);

        /// <summary>
        /// カメラ（外部入力映像）を描画します
        /// </summary>
        /// <param name="camera">カメラ（外部入力映像）</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="degree">回転（度数法）</param>
        /// <param name="autoPlay">カメラ（外部入力映像）が停止していた場合、内部で<see cref="PlayCameraImage"/>を呼び出すかどうか</param>
        void DrawCameraImage(in GcCameraDevice camera, in float x, in float y, float degree = 0f, bool autoPlay = true);

        /// <summary>
        /// カメラ（外部入力映像）を拡縮して描画します
        /// </summary>
        /// <param name="camera">カメラ（外部入力映像）</param>
        /// <param name="rect">映像をフィッティングする矩形領域</param>
        /// <param name="autoPlay">カメラ（外部入力映像）が停止していた場合、内部で<see cref="PlayCameraImage"/>を呼び出すかどうか</param>
        void DrawCameraImage(in GcCameraDevice camera, in GcRect rect, bool autoPlay = true);

        /// <summary>
        /// カメラ（外部入力映像）を拡縮して描画します
        /// </summary>
        /// <param name="camera">カメラ（外部入力映像）</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="width">横幅。映像の横幅がこれになるように拡縮される</param>
        /// <param name="height">縦幅。映像の縦幅がこれになるように拡縮される</param>
        /// <param name="degree">回転（度数法）</param>
        /// <param name="autoPlay">カメラ（外部入力映像）が停止していた場合、内部で<see cref="PlayCameraImage"/>を呼び出すかどうか</param>
        void DrawCameraImage(in GcCameraDevice camera, in float x, in float y, in float width, in float height, float degree = 0f, bool autoPlay = true);

        /// <summary>
        /// 指定されたカメラ（外部入力映像）の推奨解像度を取得します
        /// </summary>
        /// <remarks>
        /// - 推奨解像度を取得できない場合、現在のキャンバス解像度とフレームレートに基づいた適当な値を返します
        /// - エディタでは常に推奨解像度を取得できません
        /// </remarks>
        /// <param name="camera">カメラ（外部入力映像）</param>
        /// <returns>解像度とリフレッシュレート</returns>
        GcResolution GetPrimaryCameraResolution(in GcCameraDevice camera);

        /// <summary>
        /// 指定されたカメラ（外部入力映像）の更新処理を開始します
        /// </summary>
        /// <remarks>
        /// - カメラごとにこのAPIを呼び出すことで、それらを同時に更新することが可能です<br />
        /// - ただし、同じ名称のカメラは、同時に更新することができません
        /// </remarks>
        /// <param name="camera">カメラ（外部入力映像）</param>
        /// <returns>開始したかどうか（元から更新していた場合を含まない）</returns>
        bool PlayCameraImage(in GcCameraDevice camera);

        /// <summary>
        /// 指定されたカメラ（外部入力映像）の更新処理を開始します
        /// </summary>
        /// <remarks>
        /// - カメラごとにこのAPIを呼び出すことで、それらを同時に更新することが可能です<br />
        /// - ただし、同じ名称のカメラは、同時に更新することができません<br />
        /// - テクスチャーが未生成だった場合は、内部で勝手に適当な解像度で生成します
        /// </remarks>
        /// <param name="camera">カメラ（外部入力映像）</param>
        /// <param name="resolution">カメラ（外部入力映像）の解像度</param>
        /// <returns>開始したかどうか（元から更新していた場合を含まない）</returns>
        bool PlayCameraImage(in GcCameraDevice camera, out int2 resolution);
    }
}
