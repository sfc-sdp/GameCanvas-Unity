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
    public readonly partial struct GcSound : System.IEquatable<GcSound>
    {
        internal static readonly GcSound External = new GcSound("@External");
        internal static readonly GcSound Null = default;
        internal readonly string m_Path;

        private GcSound(in string path) { m_Path = path; }

        public bool Invalid => string.IsNullOrEmpty(m_Path);
        public static bool operator !=(GcSound lhs, GcSound rhs) => !lhs.Equals(rhs);
        public static bool operator ==(GcSound lhs, GcSound rhs) => lhs.Equals(rhs);
        public bool Equals(GcSound other)
            => (m_Path == null) ? (other.m_Path == null) : m_Path.Equals(other.m_Path);
        public override bool Equals(object obj) => (obj is GcSound other) && Equals(other);
        public override int GetHashCode() => m_Path?.GetHashCode() ?? 0;
        public override string ToString() => $"{nameof(GcSound)} [{m_Path}]";
    }
}
