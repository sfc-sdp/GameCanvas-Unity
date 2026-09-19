#nullable enable
namespace GameCanvas
{
    /// <summary>1フレームのキー状態。DownとUpは同じフレームに成立することがあります。</summary>
    public readonly struct GcKeyState
    {
        public bool Down { get; }
        public bool Held { get; }
        public bool Up { get; }
        public bool Cancelled { get; }
        /// <summary>押してからの秒数。終了したフレームまで読めます。</summary>
        public double Duration { get; }
        internal GcKeyState(bool down, bool held, bool up, bool cancelled, double duration)
        { Down = down; Held = held; Up = up; Cancelled = cancelled; Duration = duration; }
    }
}
