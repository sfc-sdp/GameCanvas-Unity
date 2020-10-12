/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using System.Collections;

namespace GameCanvas
{
    public abstract class GameBase : BehaviourBase
    {
        public override void InitGame() { }

        public override void UpdateGame() { }

        public override void DrawGame() { }

        public override void PauseGame() { }

        public override void ResumeGame() { }

        public override IEnumerator Entry()
        {
            yield break;
        }
    }
}
