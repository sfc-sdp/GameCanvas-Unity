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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameCanvas.Engine
{
    sealed class GcSoundEngine : ISound, IEngine
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        const int k_TrackBgmNum = 3;
        const int k_TrackNum = 4;
        const float k_MinDecibel = -96f;
        const float k_MaxDecibel = 20f;

        readonly GcContext m_Context;
        readonly GcReferenceMixer m_Mixer;
        readonly GcSound[] m_PausingId;
        readonly GcSound[] m_PlayingId;
        readonly Dictionary<GcSound, GcReferenceSound> m_Sound;
        readonly AudioSource[] m_Sources;
        // AudioMixer.SetFloat は AudioMixer がオーディオシステムにロードされる前
        // (constructor / InitGame 等) に呼ばれると無視される既知の問題への対策。
        // 全ての mixer 書き込みをここに保存し、OnBeforeUpdate で読み戻し検証して
        // 値が反映されていなければ再送信する。
        readonly Dictionary<string, float> m_PendingMixerFloat = new();
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal GcSoundEngine(in GcContext context)
        {
            m_Context = context;
            m_Mixer = GcReferenceMixer.Load("GcAudioMixer");
            m_PlayingId = new GcSound[k_TrackBgmNum];
            m_PausingId = new GcSound[k_TrackBgmNum];
            m_Sound = new Dictionary<GcSound, GcReferenceSound>(GcSound.__Length__);
            m_Sources = new AudioSource[k_TrackNum];

            var sources = m_Context.Behaviour.GetComponents<AudioSource>();
            foreach (var s in sources)
            {
                if (s.outputAudioMixerGroup == null) continue;

                int track;
                switch (s.outputAudioMixerGroup.name)
                {
                    case "BGM1": track = (int)GcSoundTrack.BGM1; break;
                    case "BGM2": track = (int)GcSoundTrack.BGM2; break;
                    case "BGM3": track = (int)GcSoundTrack.BGM3; break;
                    case "SE": track = (int)GcSoundTrack.SE; break;
                    default: continue;
                }

                SetupAudioSource(s);
                m_Sources[track] = s;
            }

            // コンポーネントが足りなければ足す
            for (var i = 0; i < k_TrackNum; i++)
            {
                if (m_Sources[i] != null) continue;

                m_Sources[i] = m_Context.Behaviour.gameObject.AddComponent<AudioSource>();
                SetupAudioSource(m_Sources[i]);

                var trackName = ((GcSoundTrack)i).ToString();
                var candidate = m_Mixer.Get()!.FindMatchingGroups(trackName);
                Assert.IsTrue(candidate.Length == 1);
                m_Sources[i].outputAudioMixerGroup = candidate[0];
            }

            ClearSound();
        }

        public void ClearSound()
        {
            for (var i = 0; i < k_TrackBgmNum; i++)
            {
                m_PlayingId[i] = default;
                m_PausingId[i] = default;

                if (m_Sources[i].isPlaying)
                {
                    m_Sources[i].Stop();
                }
            }

            WriteMixerFloat("VolumeBGM1", 0f);
            WriteMixerFloat("VolumeBGM2", 0f);
            WriteMixerFloat("VolumeBGM3", 0f);
            WriteMixerFloat("VolumeSE", 0f);
            WriteMixerFloat("VolumeMaster", 0f);
        }

        public float GetSoundLevel(GcSoundTrack track = GcSoundTrack.Master)
        {
            var key = track.GetVolumeKey();
            if (string.IsNullOrEmpty(key)) throw new System.ArgumentException("invalid value", nameof(track));

            // 初期化前に SetSoundLevel/ClearSound で書き込まれた値は mixer に未反映の
            // 可能性があるため、pending dict を優先して返す (#160)
            if (m_PendingMixerFloat.TryGetValue(key, out var pending)) return pending;

            m_Mixer.Get()!.GetFloat(key, out var decibel);
            return decibel;
        }

        public bool IsPlayingSound(GcSoundTrack track = GcSoundTrack.BGM1)
        {
            if (track == GcSoundTrack.Master)
            {
                // いずれかのトラックで再生中であれば真を返す
                for (var i = 0; i < k_TrackNum; i++)
                {
                    if (m_Sources[i].isPlaying) return true;
                }
            }
            else
            {
                var i = (int)track;
                if (i < k_TrackNum) return m_Sources[i].isPlaying;
            }
            return false;
        }

        public void PauseSound(GcSoundTrack track = GcSoundTrack.BGM1)
        {
            var i = (int)track;
            if (i >= k_TrackBgmNum) return;

            if (m_Sources[i].isPlaying)
            {
                m_PausingId[i] = m_PlayingId[i];
                m_PlayingId[i] = default;
                m_Sources[i].Pause();
            }
        }

        public void PlaySound(in GcSound sound, GcSoundTrack track = GcSoundTrack.BGM1, bool loop = false)
        {
            switch (track)
            {
                case GcSoundTrack.BGM1:
                case GcSoundTrack.BGM2:
                case GcSoundTrack.BGM3:
                {
                    var i = (int)track;
                    if (m_PlayingId[i] == sound) return;
                    if (m_PausingId[i] == sound)
                    {
                        m_Sources[i].loop = loop;
                        UnpauseSound(track);
                        return;
                    }
                    if (TryGetAudioClip(sound, out var clip))
                    {
                        if (m_Sources[i].isPlaying)
                        {
                            m_Sources[i].Stop();
                        }

                        m_PlayingId[i] = sound;
                        m_PausingId[i] = GcSound.Null;
                        m_Sources[i].loop = loop;
                        m_Sources[i].clip = clip;
                        m_Sources[i].Play();
                    }
                }
                break;

                case GcSoundTrack.SE:
                {
                    if (TryGetAudioClip(sound, out var clip))
                    {
                        m_Sources[(int)GcSoundTrack.SE].PlayOneShot(clip);
                    }
                }
                break;
            }
        }

        public void PlaySound(in AudioClip clip, GcSoundTrack track = GcSoundTrack.BGM1, bool loop = false)
        {
            switch (track)
            {
                case GcSoundTrack.BGM1:
                case GcSoundTrack.BGM2:
                case GcSoundTrack.BGM3:
                {
                    var i = (int)track;
                    if (m_Sources[i].isPlaying)
                    {
                        m_Sources[i].Stop();
                    }

                    m_PlayingId[i] = GcSound.External;
                    m_PausingId[i] = GcSound.Null;
                    m_Sources[i].loop = loop;
                    m_Sources[i].clip = clip;
                    m_Sources[i].Play();
                }
                break;

                case GcSoundTrack.SE:
                {
                    m_Sources[(int)GcSoundTrack.SE].PlayOneShot(clip);
                }
                break;
            }
        }

        public void SetSoundLevel(in float decibel, GcSoundTrack track = GcSoundTrack.Master)
        {
            var key = track.GetVolumeKey();
            if (string.IsNullOrEmpty(key) || float.IsNaN(decibel)) return;

            WriteMixerFloat(key, math.clamp(decibel, k_MinDecibel, k_MaxDecibel));
        }

        public void StopSound(GcSoundTrack track = GcSoundTrack.BGM1)
        {
            var i = (int)track;
            if (i >= k_TrackBgmNum) return;

            m_PlayingId[i] = GcSound.Null;
            m_PausingId[i] = GcSound.Null;

            if (m_Sources[i].isPlaying)
            {
                m_Sources[i].Stop();
            }
        }

        public void UnpauseSound(GcSoundTrack track = GcSoundTrack.BGM1)
        {
            var i = (int)track;
            if (i >= k_TrackBgmNum) return;

            if (m_PausingId[i] != GcSound.Null)
            {
                m_PlayingId[i] = m_PausingId[i];
                m_PausingId[i] = GcSound.Null;
                m_Sources[i].UnPause();
            }
        }
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        void System.IDisposable.Dispose()
        {
            foreach (var sound in m_Sound.Values) sound.Unload();
            m_Sound.Clear();
            m_Mixer.Unload();
        }

        void IEngine.OnAfterDraw() { }

        void IEngine.OnBeforeUpdate(in System.DateTimeOffset now)
        {
            for (var i = 0; i < k_TrackBgmNum; i++)
            {
                if (m_PlayingId[i] != GcSound.Null && !m_Sources[i].isPlaying)
                {
                    m_PlayingId[i] = GcSound.Null;
                }
            }

            // 反映されていない pending mixer 値を再送信し、確認できたものを除去する。
            // Unity の AudioMixer.SetFloat は初期化前 (constructor / InitGame 等) では
            // 無視されるため、確認できるまで毎フレーム replay する (#160)。
            FlushPendingMixerFloats();
        }

        /// <summary>
        /// AudioMixer に値を書き込む。pending dict にも記録し、後で読み戻し検証する。
        /// </summary>
        private void WriteMixerFloat(in string key, in float value)
        {
            m_PendingMixerFloat[key] = value;
            m_Mixer.Get()?.SetFloat(key, value);
        }

        /// <summary>
        /// pending mixer 値を確認・再送信する。
        /// AudioMixer.GetFloat は SetFloat が反映済みの場合に正しい値を返すため、
        /// それを使って flush 完了判定する。
        /// </summary>
        private void FlushPendingMixerFloats()
        {
            if (m_PendingMixerFloat.Count == 0) return;

            var mixer = m_Mixer.Get();
            if (mixer == null) return;

            var confirmed = new List<string>(m_PendingMixerFloat.Count);
            foreach (var kv in m_PendingMixerFloat)
            {
                if (mixer.GetFloat(kv.Key, out var current) && math.abs(current - kv.Value) < 0.01f)
                {
                    confirmed.Add(kv.Key);
                }
                else
                {
                    mixer.SetFloat(kv.Key, kv.Value);
                }
            }
            for (var i = 0; i < confirmed.Count; i++)
            {
                m_PendingMixerFloat.Remove(confirmed[i]);
            }
        }

        private static void SetupAudioSource(in AudioSource src)
        {
            src.bypassEffects = true;
            src.bypassListenerEffects = true;
            src.bypassReverbZones = true;
            src.ignoreListenerPause = true;
            src.ignoreListenerVolume = true;
            src.mute = false;
            src.panStereo = 0f;
            src.pitch = 1f;
            src.clip = null;
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            src.volume = 1f;
        }

        private bool TryGetAudioClip(in GcSound sound, [NotNullWhen(true)] out AudioClip? clip)
        {
            if (sound == GcSound.Null || sound == GcSound.External)
            {
                clip = null;
                return false;
            }

            if (!m_Sound.TryGetValue(sound, out var res))
            {
                res = GcReferenceSound.Load(sound.m_Path);
                m_Sound.Add(sound, res);
            }

            clip = res.Get();
            return (clip != null);
        }
        #endregion
    }
}
