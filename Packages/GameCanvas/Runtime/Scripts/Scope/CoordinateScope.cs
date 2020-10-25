/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
namespace GameCanvas
{
    public readonly struct CoordianteScope : System.IDisposable
    {
        readonly IGraphics m_Graphics;

        internal CoordianteScope(in IGraphics graphics)
        {
            m_Graphics = graphics;
            m_Graphics.PushCoordinate();
        }

        public void Dispose()
        {
            m_Graphics?.PopCoordinate();
        }
    }
}
