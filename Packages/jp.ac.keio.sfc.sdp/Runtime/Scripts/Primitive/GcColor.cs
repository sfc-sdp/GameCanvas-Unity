#nullable enable
namespace GameCanvas
{
    /// <summary>色。整数の指定は0〜255、各プロパティの値は0〜1です。</summary>
    public readonly struct GcColor : System.IEquatable<GcColor>
    {
        public float R { get; }
        public float G { get; }
        public float B { get; }
        public float A { get; }
        public GcColor(int r, int g, int b, int a = 255)
            : this(ClampByte(r), ClampByte(g), ClampByte(b), ClampByte(a)) { }
        GcColor(float r, float g, float b, float a) { R = r; G = g; B = b; A = a; }
        /// <summary>0〜1の値から色を作ります。範囲外は端の値へ丸め、NaNは拒否します。</summary>
        public static GcColor FromNormalized(float r, float g, float b, float a = 1)
            => new(Clamp(r), Clamp(g), Clamp(b), Clamp(a));
        static float ClampByte(int value) => System.Math.Max(0, System.Math.Min(255, value)) / 255f;
        static float Clamp(float value)
        {
            if (float.IsNaN(value)) throw new System.ArgumentOutOfRangeException(nameof(value));
            return System.Math.Max(0, System.Math.Min(1, value));
        }
        public bool Equals(GcColor other) => R == other.R && G == other.G && B == other.B && A == other.A;
        public override bool Equals(object? obj) => obj is GcColor other && Equals(other);
        public override int GetHashCode() => System.HashCode.Combine(R, G, B, A);
        internal UnityEngine.Color ToUnity() => new(R, G, B, A);
    }
}
