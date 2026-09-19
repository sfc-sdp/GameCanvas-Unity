#nullable enable
namespace GameCanvas
{
    /// <summary>キャンバス座標。右がXの正方向、下がYの正方向。</summary>
    public readonly struct GcPoint : System.IEquatable<GcPoint>
    {
        public float X { get; }
        public float Y { get; }
        public GcPoint(float x, float y) { X = x; Y = y; }
        public bool Equals(GcPoint other) => X.Equals(other.X) && Y.Equals(other.Y);
        public override bool Equals(object? obj) => obj is GcPoint other && Equals(other);
        public override int GetHashCode() => System.HashCode.Combine(X, Y);
        public static GcPoint operator -(GcPoint a, GcPoint b) => new(a.X - b.X, a.Y - b.Y);
    }
}
