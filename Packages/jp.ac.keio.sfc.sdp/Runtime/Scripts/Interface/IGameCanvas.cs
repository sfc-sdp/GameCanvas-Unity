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
        : IGraphicsEx, ISoundEx, INetworkEx, ISceneManagementEx
        , ITimeEx, IPhysicsEx, IStorageEx, IMathEx
        , IInputPointer, IInputKey, IInputAccelerationEx
        , IInputCameraEx
    {
        /// <summary>位置情報。対応状況はLocation.IsSupportedを確認する。</summary>
        GcLocationService Location { get; }
    }
}
