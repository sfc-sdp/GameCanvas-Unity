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
using Unity.Mathematics;

namespace GameCanvas
{
    public interface IInputCamera
    {
        GcCameraService Camera { get; }
        /// <summary>起動済みの映像を実寸で描く。許可や起動は行わない。基準点はSetRectAnchor。</summary>
        void DrawCamera();
        void DrawCamera(in GcPoint position, float rotation = 0);
        void DrawCamera(in float2 position, float rotation = 0);
        /// <summary>実寸で描く。rotationは時計回りの度数。</summary>
        void DrawCamera(float x, float y, float rotation = 0);
        /// <summary>指定した幅と高さに描く。映像の向きと上下反転は自動で補正する。</summary>
        void DrawCamera(float x, float y, float width, float height, float rotation = 0);
        void DrawCamera(in GcRect rect);
    }
}
