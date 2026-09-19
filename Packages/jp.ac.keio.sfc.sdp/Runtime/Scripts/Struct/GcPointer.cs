#nullable enable
namespace GameCanvas
{
    /// <summary>1フレーム分のポインター状態。保存すると値のコピーになる。</summary>
    public readonly struct GcPointer
    {
        /// <summary>操作中に変わらないID。一覧の添字やOSの指番号とは異なる。</summary>
        public int Id { get; }
        public GcPointerType Kind { get; }
        /// <summary>座標が有効。終了した接触もそのフレーム中は有効。</summary>
        public bool Present { get; }
        public bool Down { get; }
        /// <summary>フレーム末に押している。押し始めたフレームも含む。</summary>
        public bool Held { get; }
        public bool Up { get; }
        /// <summary>OSによる中断。正常な解放を表すUpとは異なる。</summary>
        public bool Cancelled { get; }
        public bool Inside { get; }
        public GcPoint Position { get; }
        public float X => Position.X;
        public float Y => Position.Y;
        public GcPoint StartPosition { get; }
        public float StartX => StartPosition.X;
        public float StartY => StartPosition.Y;
        /// <summary>前フレーム末からの変位。新しい接触では開始位置からの変位。</summary>
        public GcPoint Delta { get; }
        /// <summary>押下からの秒数。ホバーだけの場合は0。</summary>
        public double Duration { get; }
        internal GcPointer(int id, GcPointerType kind, bool present, bool down, bool held,
            bool up, bool cancelled, bool inside, GcPoint position, GcPoint start,
            GcPoint delta, double duration)
        {
            Id = id; Kind = kind; Present = present; Down = down; Held = held;
            Up = up; Cancelled = cancelled; Inside = inside; Position = position;
            StartPosition = start; Delta = delta; Duration = duration;
        }
    }
}
