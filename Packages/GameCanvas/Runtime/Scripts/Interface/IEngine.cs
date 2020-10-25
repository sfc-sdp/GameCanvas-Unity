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
    public interface IEngine : System.IDisposable
    {
        void OnBeforeUpdate(in System.DateTimeOffset now);
        void OnAfterDraw();
    }
}
