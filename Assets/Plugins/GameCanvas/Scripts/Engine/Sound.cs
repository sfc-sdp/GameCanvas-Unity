/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2017 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/

namespace GameCanvas.Engine
{
    using UnityEngine;
    using UnityEngine.Assertions;

    public sealed class Sound
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        private readonly Resource cRes;
        private readonly AudioSource cSource;

        private int mPlayingId = -1;
        private int mPausingId = -1;

        #endregion

        //----------------------------------------------------------
        #region パブリック関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal Sound(Resource res, AudioSource source)
        {
            Assert.IsNotNull(res);
            Assert.IsNotNull(source);

            cRes = res;
            cSource = source;
            cSource.bypassEffects = true;
            cSource.bypassListenerEffects = true;
            cSource.bypassReverbZones = true;
            cSource.ignoreListenerPause = true;
            cSource.ignoreListenerVolume = true;
            cSource.mute = false;
            cSource.panStereo = 0f;
            cSource.pitch = 1f;
            cSource.clip = null;
            cSource.playOnAwake = false;
            cSource.spatialBlend = 0f;
            cSource.volume = 1f;
        }

        public void OnBeforeUpdate()
        {
            if (mPlayingId != -1 && !cSource.isPlaying)
            {
                mPlayingId = -1;
            }
        }

        public void Play(int soundId, bool loop)
        {
            if (mPlayingId == soundId) return;
            if (mPausingId == soundId)
            {
                cSource.loop = loop;
                Unpause();
                return;
            }

            var sound = cRes.GetSnd(soundId);
            if (sound.Data == null) return;

            if (cSource.isPlaying)
            {
                cSource.Stop();
            }

            mPlayingId = soundId;
            mPausingId = -1;
            cSource.loop = loop;
            cSource.clip = sound.Data;
            cSource.Play();
        }

        public void Stop()
        {
            mPlayingId = -1;
            mPausingId = -1;

            if (cSource.isPlaying)
            {
                cSource.Stop();
            }
        }

        public void Pause()
        {
            if (cSource.isPlaying)
            {
                mPausingId = mPlayingId;
                mPlayingId = -1;
                cSource.Pause();
            }
        }

        public void Unpause()
        {
            if (mPausingId != -1)
            {
                mPlayingId = mPausingId;
                mPausingId = -1;
                cSource.UnPause();
            }
        }

        public void ChangeVolume(int volume)
        {
            cSource.volume = volume * 0.01f;
        }

        #endregion

        //----------------------------------------------------------
        #region プライベート関数
        //----------------------------------------------------------

        // TODO

        #endregion
    }
}
