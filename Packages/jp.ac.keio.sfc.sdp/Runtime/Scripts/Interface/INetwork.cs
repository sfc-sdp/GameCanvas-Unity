#nullable enable
namespace GameCanvas
{
    public interface INetwork
    {
        GcNetworkService Network { get; }
        /// <summary>取得済みの画像を描く。通信は開始しない。RectAnchorと描画色を使う。</summary>
        void DrawImage(GcImageRequest image);
        void DrawImage(GcImageRequest image, float x, float y, float rotation = 0);
        void DrawImage(GcImageRequest image, in GcPoint position, float rotation = 0);
        void DrawImage(GcImageRequest image, float x, float y, float width, float height, float rotation = 0);
        void DrawImage(GcImageRequest image, in GcRect rect);
        /// <summary>取得済みの音を再生する。成功前と破棄後はfalseを返す。再生開始はユーザー操作に合わせる。</summary>
        bool PlaySound(GcSoundRequest sound, GcSoundTrack track = GcSoundTrack.BGM1, bool loop = false);
    }
}
