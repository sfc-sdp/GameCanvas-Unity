/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2024 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
#nullable enable

namespace GameCanvas
{
    static class GcSoundTrackExtensions
    {
        public static string GetVolumeKey(this GcSoundTrack track)
        {
            return track switch
            {
                GcSoundTrack.BGM1 => "VolumeBGM1",
                GcSoundTrack.BGM2 => "VolumeBGM2",
                GcSoundTrack.BGM3 => "VolumeBGM3",
                GcSoundTrack.SE => "VolumeSE",
                GcSoundTrack.Master => "VolumeMaster",
                _ => throw new System.NotImplementedException(),
            };
        }
    }
}
