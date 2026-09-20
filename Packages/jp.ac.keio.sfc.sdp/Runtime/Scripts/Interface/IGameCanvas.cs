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
    public interface IGameCanvas
        : IGraphicsEx, ISoundEx, INetwork, ISceneManagementEx
        , ITimeEx, IPhysicsEx, IStorageEx, IMathEx
        , IInputPointer, IInputKey
        , IInputCamera
    {
        /// <summary>RectAnchorとCurrentCoordinateを使って、描画する矩形の内部を判定する。右端と下端は含まない。</summary>
        bool Contains(in GcRect rect, in GcPoint point);
        /// <summary>このフレームのポインターで矩形を動かす。描画と同じAnchor・座標系で1回呼ぶ。</summary>
        void Drag(GcDrag drag, ref GcRect rect);
        /// <summary>位置情報。対応状況はLocation.IsSupportedを確認する。</summary>
        GcLocationService Location { get; }
        /// <summary>加速度。対応状況はAcceleration.IsSupportedを確認する。</summary>
        GcAccelerationService Acceleration { get; }
    }
}
