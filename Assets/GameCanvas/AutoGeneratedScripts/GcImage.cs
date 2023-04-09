/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2023 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
#nullable enable

namespace GameCanvas
{
    public readonly partial struct GcImage : System.IEquatable<GcImage>
    {
        internal const int __Length__ = 4;
        public static readonly GcImage BallRed = new GcImage("BallRed", 24, 24);
        public static readonly GcImage BallYellow = new GcImage("BallYellow", 24, 24);
        public static readonly GcImage BlueSky = new GcImage("BlueSky", 640, 480);
        public static readonly GcImage MapPin = new GcImage("MapPin", 50, 64);
    }
}
