/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using Unity.Mathematics;

namespace GameCanvas
{
    public readonly partial struct GcImage : System.IEquatable<GcImage>
    {
        internal readonly string m_Path;
        internal readonly int2 m_Size;

        private GcImage(in string path, in int w, in int h) { m_Path = path; m_Size = new int2(w, h); }

        public bool Invalid => string.IsNullOrEmpty(m_Path);
        public static bool operator !=(GcImage lhs, GcImage rhs) => !lhs.Equals(rhs);
        public static bool operator ==(GcImage lhs, GcImage rhs) => lhs.Equals(rhs);
        public bool Equals(GcImage other) => m_Path == other.m_Path && m_Size.Equals(other.m_Size);
        public override bool Equals(object obj) => (obj is GcImage other) && Equals(other);
        public override int GetHashCode() => m_Path?.GetHashCode() ?? 0;
        public override string ToString() => $"{nameof(GcImage)} [{m_Path}, {m_Size.x}, {m_Size.y}]";
    }
}
