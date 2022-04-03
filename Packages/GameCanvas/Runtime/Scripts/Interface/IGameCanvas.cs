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
    public interface IGameCanvas
        : IGraphicsEx, ISoundEx, INetworkEx, ISceneManagementEx
        , ITimeEx, IPhysicsEx, IStorageEx, IMathEx
        , IInputPointerEx, IInputKeyEx, IInputAccelerationEx
        , IInputGeolocationEx, IInputCameraEx
    { }
}
