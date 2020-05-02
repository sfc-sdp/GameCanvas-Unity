/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using UnityEngine;
using UnityEngine.Assertions;

namespace GameCanvas.Engine
{
    /// <summary>
    /// サウンドエンジン
    /// </summary>
    public sealed class Sound
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        private const int cTrackNum = 4;
        private const int cTrackBgmNum = 3;
        private readonly Resource cRes;
        private readonly AudioSource[] cSources;
        private readonly int[] cPlayingId;
        private readonly int[] cPausingId;

        #endregion

        //----------------------------------------------------------
        #region パブリック関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal Sound(BehaviourBase behaviour, Resource res, AudioSource[] sources)
        {
            Assert.IsNotNull(res);
            Assert.IsNotNull(sources);

            cRes = res;
            cSources = new AudioSource[cTrackNum];
            cPlayingId = new int[cTrackBgmNum];
            cPausingId = new int[cTrackBgmNum];

            foreach (var s in sources)
            {
                if (s.outputAudioMixerGroup == null) continue;

                int track;
                switch (s.outputAudioMixerGroup.name)
                {
                    case "BGM1": track = (int)ESoundTrack.BGM1; break;
                    case "BGM2": track = (int)ESoundTrack.BGM2; break;
                    case "BGM3": track = (int)ESoundTrack.BGM3; break;
                    case "SE": track = (int)ESoundTrack.SE; break;
                    default: continue;
                }

                InitSource(s);
                cSources[track] = s;
            }

            // コンポーネントが足りなければ足す
            for (var i = 0; i < cTrackNum; i++)
            {
                if (cSources[i] != null) continue;

                cSources[i] = behaviour.gameObject.AddComponent<AudioSource>();
                InitSource(cSources[i]);

                var trackName = ((ESoundTrack)i).ToString();
                var candidate = cRes.AudioMixer.FindMatchingGroups(trackName);
                Assert.IsTrue(candidate.Length == 1);
                cSources[i].outputAudioMixerGroup = candidate[0];
            }

            Reset();
        }

        public void OnBeforeUpdate()
        {
            for (var i = 0; i < cTrackBgmNum; i++)
            {
                if (cPlayingId[i] != -1 && !cSources[i].isPlaying)
                {
                    cPlayingId[i] = -1;
                }
            }
        }

        public void Play(int soundId, bool loop, ESoundTrack track)
        {
            if (track == ESoundTrack.SE)
            {
                PlaySE(soundId);
                return;
            }
            var i = (int)track;
            if (i >= cTrackBgmNum) return;

            if (cPlayingId[i] == soundId) return;
            if (cPausingId[i] == soundId)
            {
                cSources[i].loop = loop;
                Unpause(track);
                return;
            }

            var sound = cRes.GetSnd(soundId);
            if (sound.Data == null) return;

            if (cSources[i].isPlaying)
            {
                cSources[i].Stop();
            }

            cPlayingId[i] = soundId;
            cPausingId[i] = -1;
            cSources[i].loop = loop;
            cSources[i].clip = sound.Data;
            cSources[i].Play();
        }

        public void Stop(ESoundTrack track)
        {
            var i = (int)track;
            if (i >= cTrackBgmNum) return;

            cPlayingId[i] = -1;
            cPausingId[i] = -1;

            if (cSources[i].isPlaying)
            {
                cSources[i].Stop();
            }
        }

        public void Pause(ESoundTrack track)
        {
            var i = (int)track;
            if (i >= cTrackBgmNum) return;

            if (cSources[i].isPlaying)
            {
                cPausingId[i] = cPlayingId[i];
                cPlayingId[i] = -1;
                cSources[i].Pause();
            }
        }

        public void Unpause(ESoundTrack track)
        {
            var i = (int)track;
            if (i >= cTrackBgmNum) return;

            if (cPausingId[i] != -1)
            {
                cPlayingId[i] = cPausingId[i];
                cPausingId[i] = -1;
                cSources[i].UnPause();
            }
        }

        public void PlaySE(int soundId)
        {
            var sound = cRes.GetSnd(soundId);
            if (sound.Data == null) return;
            cSources[(int)ESoundTrack.SE].PlayOneShot(sound.Data);
        }

        public void SetVolume(ref int volume, ESoundTrack track)
        {
            var decibel = 20f * Mathf.Log10(volume * 0.01f);
            SetVolume(decibel, track);
        }

        public void SetVolume(float decibel, ESoundTrack track)
        {
            string key;
            switch (track)
            {
                case ESoundTrack.BGM1: key = "VolumeBGM1"; break;
                case ESoundTrack.BGM2: key = "VolumeBGM2"; break;
                case ESoundTrack.BGM3: key = "VolumeBGM3"; break;
                case ESoundTrack.SE: key = "VolumeSE"; break;
                case ESoundTrack.Master: key = "VolumeMaster"; break;
                default: return;
            }

            if (float.IsNaN(decibel) || decibel < -96f)
            {
                decibel = -96f;
            }
            else if (decibel > 20f)
            {
                decibel = 20f;
            }
            cRes.AudioMixer.SetFloat(key, decibel);
        }

        public void Reset()
        {
            for (var i = 0; i < cTrackBgmNum; i++)
            {
                cPlayingId[i] = -1;
                cPausingId[i] = -1;

                if (cSources[i].isPlaying)
                {
                    cSources[i].Stop();
                }
            }

            var mixer = cRes.AudioMixer;
            mixer.SetFloat("VolumeBGM1", 0f);
            mixer.SetFloat("VolumeBGM2", 0f);
            mixer.SetFloat("VolumeBGM3", 0f);
            mixer.SetFloat("VolumeSE", 0f);
            mixer.SetFloat("VolumeMaster", 0f);
        }

        #endregion

        //----------------------------------------------------------
        #region プライベート関数
        //----------------------------------------------------------

        private void InitSource(AudioSource s)
        {
            s.bypassEffects = true;
            s.bypassListenerEffects = true;
            s.bypassReverbZones = true;
            s.ignoreListenerPause = true;
            s.ignoreListenerVolume = true;
            s.mute = false;
            s.panStereo = 0f;
            s.pitch = 1f;
            s.clip = null;
            s.playOnAwake = false;
            s.spatialBlend = 0f;
            s.volume = 1f;
        }

        #endregion
    }
}
