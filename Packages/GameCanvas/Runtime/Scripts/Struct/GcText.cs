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
    public readonly partial struct GcText : System.IEquatable<GcText>
    {
        internal readonly string m_Path;

        private GcText(in string path) { m_Path = path; }

        public bool Invalid => string.IsNullOrEmpty(m_Path);
        public static bool operator !=(GcText lhs, GcText rhs) => !lhs.Equals(rhs);
        public static bool operator ==(GcText lhs, GcText rhs) => lhs.Equals(rhs);
        public bool Equals(GcText other)
            => (m_Path == null) ? (other.m_Path == null) : m_Path.Equals(other.m_Path);
        public override bool Equals(object obj) => (obj is GcText other) && Equals(other);
        public override int GetHashCode() => m_Path?.GetHashCode() ?? 0;
        public override string ToString() => $"{nameof(GcText)} [{m_Path}]";
    }
}
