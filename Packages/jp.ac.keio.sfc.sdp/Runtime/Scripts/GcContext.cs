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
using GameCanvas.Engine;
using UnityEngine;

namespace GameCanvas
{
    sealed class GcContext : System.IDisposable
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        public readonly BehaviourBase Behaviour;
        public readonly GcGraphicsEngine Graphics;
        public readonly GcInputKeyEngine InputKey;
        public readonly GcInputPointerEngine InputPointer;
        public readonly Camera MainCamera;
        public readonly GcNetworkService Network;
        public readonly GcSoundEngine Sound;
        public readonly GcStorageEngine Storage;
        public readonly GcTimeEngine Time;
        public readonly IEngine[] EngineArray;
        #endregion

        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        public GcContext(in BehaviourBase behaviour)
        {
            Behaviour = behaviour;
            MainCamera = Behaviour.m_Camera;

            Time = new GcTimeEngine();
            Storage = new GcStorageEngine(this);
            Network = new GcNetworkService();
            Graphics = new GcGraphicsEngine(this);
            Sound = new GcSoundEngine(this);

            InputPointer = new GcInputPointerEngine(this);
            InputKey = new GcInputKeyEngine(this);

            EngineArray = new IEngine[]
            {
                Time,
                Storage,
                Network,
                Graphics,
                Sound,
                InputPointer,
                InputKey
            };
        }

        public void Dispose()
        {
            foreach (var engine in EngineArray)
            {
                engine.Dispose();
            }
        }

        #endregion
    }
}
