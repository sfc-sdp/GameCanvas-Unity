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

namespace GameCanvas
{
    public interface ISound
    {
        /// <summary>
        /// すべてのサウンド再生を停止し、トラック音量を初期値に戻します
        /// </summary>
        void ClearSound();

        /// <summary>
        /// トラック音量を取得します
        /// </summary>
        /// <param name="track">対象の音声トラック</param>
        /// <returns>音量（-96db～20db）</returns>
        float GetSoundLevel(GcSoundTrack track = GcSoundTrack.Master);

        /// <summary>
        /// 指定された音声トラックが再生中かどうか
        /// </summary>
        /// <param name="track">対象の音声トラック</param>
        /// <returns>再生中かどうか</returns>
        bool IsPlayingSound(GcSoundTrack track = GcSoundTrack.BGM1);

        /// <summary>
        /// 指定された音声トラックのサウンドを一時停止します
        /// </summary>
        /// <remarks>
        /// BGMトラック以外では常に無視されます
        /// </remarks>
        /// <param name="track">対象の音声トラック</param>
        void PauseSound(GcSoundTrack track = GcSoundTrack.BGM1);

        /// <summary>
        /// サウンドを再生します
        /// </summary>
        /// <param name="soundId">サウンド</param>
        /// <param name="track">対象の音声トラック</param>
        /// <param name="loop">ループ再生するかどうか（SEトラック以外）</param>
        void PlaySound(in GcSound sound, GcSoundTrack track = GcSoundTrack.BGM1, bool loop = false);

        /// <summary>
        /// サウンドを再生します
        /// </summary>
        /// <param name="clip">サウンド</param>
        /// <param name="track">対象の音声トラック</param>
        /// <param name="loop">ループ再生するかどうか（SEトラック以外）</param>
        void PlaySound(in AudioClip clip, GcSoundTrack track = GcSoundTrack.BGM1, bool loop = false);

        /// <summary>
        /// トラック音量を変更します
        /// </summary>
        /// <param name="decibel">音量（-96db～20db）</param>
        /// <param name="track">対象の音声トラック</param>
        void SetSoundLevel(in float decibel, GcSoundTrack track = GcSoundTrack.Master);

        /// <summary>
        /// 指定された音声トラックのサウンドを停止します
        /// </summary>
        /// <remarks>
        /// BGMトラック以外では常に無視されます
        /// </remarks>
        /// <param name="track">対象の音声トラック</param>
        void StopSound(GcSoundTrack track = GcSoundTrack.BGM1);

        /// <summary>
        /// 指定された音声トラックのサウンドを一時停止していた場合、再生を再開します
        /// </summary>
        /// <remarks>
        /// BGMトラック以外では常に無視されます
        /// </remarks>
        /// <param name="track">対象の音声トラック</param>
        void UnpauseSound(GcSoundTrack track = GcSoundTrack.BGM1);
    }

    public interface ISoundEx : ISound
    {
        /// <summary>
        /// トラック音量を取得します
        /// </summary>
        /// <param name="track">対象の音声トラック</param>
        /// <returns>音量（0f～1f）</returns>
        float GetSoundVolume(GcSoundTrack track = GcSoundTrack.Master);

        /// <summary>
        /// サウンドを1回再生します
        /// </summary>
        /// <param name="sound">サウンド</param>
        void PlaySE(in GcSound sound);

        /// <summary>
        /// サウンドを1回再生します
        /// </summary>
        /// <param name="clip">サウンド</param>
        void PlaySE(in AudioClip clip);

        /// <summary>
        /// トラック音量を変更します
        /// </summary>
        /// <param name="volume">音量（0f～1f）</param>
        /// <param name="track">対象の音声トラック</param>
        void SetSoundVolume(in float volume, GcSoundTrack track = GcSoundTrack.Master);
    }
}
