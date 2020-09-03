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
    public enum GcSoundTrack : byte
    {
        BGM1 = 0,
        BGM2 = 1,
        BGM3 = 2,
        SE = 3,
        Master = 255
    }

    public static class GcSoundTrackExtension
    {
        public static string GetVolumeKey(this GcSoundTrack track)
        {
            switch (track)
            {
                case GcSoundTrack.BGM1: return "VolumeBGM1";
                case GcSoundTrack.BGM2: return "VolumeBGM2";
                case GcSoundTrack.BGM3: return "VolumeBGM3";
                case GcSoundTrack.SE: return "VolumeSE";
                case GcSoundTrack.Master: return "VolumeMaster";
                default: throw new System.NotImplementedException();
            }
        }
    }
}
