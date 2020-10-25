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
    /// <summary>
    /// スタイル（スタック可能な描画設定群）
    /// </summary>
    public struct GcStyle : System.IEquatable<GcStyle>
    {
        public static readonly GcStyle Default = new GcStyle
        {
            CircleResolution = 24,
            Color = Color.black,
            FontSize = 25,
            LineCap = GcLineCap.Butt,
            LineWidth = 1f,
            RectAnchor = GcAnchor.UpperLeft,
            StringAnchor = GcAnchor.UpperLeft
        };

        public int CircleResolution;
        public Color Color;
        public int FontSize;
        public GcLineCap LineCap;
        public float LineWidth;
        public GcAnchor RectAnchor;
        public GcAnchor StringAnchor;

        public bool Equals(GcStyle other)
            => CircleResolution.Equals(other.CircleResolution)
            && Color.Equals(other.Color)
            && FontSize.Equals(other.FontSize)
            && (LineCap == other.LineCap)
            && LineWidth.Equals(other.LineWidth)
            && (RectAnchor == other.RectAnchor)
            && (StringAnchor == other.StringAnchor);

        public override bool Equals(object obj)
            => (obj is GcStyle other) && Equals(other);

        public override int GetHashCode()
            => CircleResolution.GetHashCode()
            ^ Color.GetHashCode()
            ^ FontSize.GetHashCode()
            ^ ((int)LineCap)
            ^ LineWidth.GetHashCode()
            ^ (int)RectAnchor
            ^ (int)StringAnchor;
    }
}
