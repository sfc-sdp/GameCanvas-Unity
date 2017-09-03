﻿/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2017 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/

namespace GameCanvas
{
    using UnityEngine;
    using UnityEngine.Assertions;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera), typeof(AudioListener))]
    public abstract class BehaviourBase : MonoBehaviour
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        [SerializeField]
        private int CanvasWidth = 720;
        [SerializeField]
        private int CanvasHeight = 1280;
        [SerializeField]
        private Resource Resource;

        private Camera mCamera;
        private AudioListener mListener;

        private Graphic mGraphic;
        private Proxy mProxy;

        #endregion

        //----------------------------------------------------------
        #region  Unity イベント関数
        //----------------------------------------------------------

        private void Awake()
        {
            mCamera = GetComponent<Camera>();
            mListener = GetComponent<AudioListener>();
            Assert.IsNotNull(mCamera);
            Assert.IsNotNull(mListener);
            Assert.IsNotNull(Resource);

            Resource.Initialize();
            mGraphic = new Graphic(Resource, mCamera);
            mGraphic.SetResolution(CanvasWidth, CanvasHeight);
            mProxy = new Proxy(mGraphic);

            // TODO
        }

        private void Start()
        {
            initGame();
        }

        private void Update()
        {
            mGraphic.OnBeforeUpdate();

            updateGame();
            drawGame();
        }

        private void OnApplicationFocus(bool focus)
        {
            // TODO
        }

        private void OnApplicationPause(bool pause)
        {
            // TODO
        }

        private void OnEnable()
        {
            mGraphic.OnEnable();
        }

        private void OnDisable()
        {
            mGraphic.OnDisable();
        }

        private void OnDestroy()
        {
            mGraphic.Dispose();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            mGraphic?.SetResolution(CanvasWidth, CanvasHeight);
        }
#endif //UNITY_EDITOR

        #endregion

        //----------------------------------------------------------
        #region パブリック関数 (Game.cs に公開している関数)
        //----------------------------------------------------------

        protected Proxy gc => mProxy;

        public abstract void initGame();

        public abstract void updateGame();

        public abstract void drawGame();

        #endregion

        //----------------------------------------------------------
        #region プライベート関数
        //----------------------------------------------------------

        // TODO

        #endregion
    }
}