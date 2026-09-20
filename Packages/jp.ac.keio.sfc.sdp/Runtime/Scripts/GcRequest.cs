#nullable enable
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace GameCanvas
{
    public enum GcRequestState { Pending, Succeeded, Failed, Cancelled, TimedOut, Disposed }
    public enum GcSoundFormat { Wav, Mp3, Ogg }

    /// <summary>1回の通信。再試行は新しい操作として開始する。Disposeで取得データを解放する。</summary>
    public abstract class GcRequest : IDisposable
    {
        public GcRequestState Status { get; private set; } = GcRequestState.Pending;
        public bool IsDone => Status != GcRequestState.Pending;
        public string Url { get; }
        public long ResponseCode { get; private set; }
        /// <summary>失敗の分類。応答本文やURLの秘密情報は含めない。</summary>
        public string ErrorCode { get; private set; } = "";
        internal UnityWebRequest? Native;
        internal readonly double Deadline;
        internal Action<GcRequest>? Released;
        internal GcRequest(string url, double deadline) { Url = url; Deadline = deadline; }
        internal void Start(UnityWebRequest native)
        {
            Native = native;
            try { native.SendWebRequest(); }
            catch { Finish(GcRequestState.Failed, "GC-NETWORK-START"); }
        }
        internal void Tick(double now)
        {
            if (Status != GcRequestState.Pending || Native == null) return;
            if (Native.isDone)
            {
                ResponseCode = Native.responseCode;
                if (Native.result != UnityWebRequest.Result.Success)
                {
                    Finish(GcRequestState.Failed, Native.result == UnityWebRequest.Result.ProtocolError
                        ? "GC-NETWORK-HTTP" : Native.result == UnityWebRequest.Result.DataProcessingError
                        ? "GC-NETWORK-DATA" : "GC-NETWORK-CONNECTION");
                    return;
                }
                try { Read(Native); Finish(GcRequestState.Succeeded, ""); }
                catch { ReleaseData(); Finish(GcRequestState.Failed, "GC-NETWORK-DATA"); }
            }
            else if (now >= Deadline) Finish(GcRequestState.TimedOut, "GC-NETWORK-TIMEOUT");
        }
        protected abstract void Read(UnityWebRequest native);
        protected abstract void ReleaseData();
        void Finish(GcRequestState status, string code)
        {
            Status = status; ErrorCode = code;
            var native = Native; Native = null;
            if (native != null)
            {
                if (!native.isDone) native.Abort();
                native.Dispose();
            }
        }
        /// <summary>待機中の通信を取り消す。送信済みのサーバ処理を取り消せるとは限らない。</summary>
        public void Cancel() { if (!IsDone) Finish(GcRequestState.Cancelled, "GC-NETWORK-CANCELLED"); }
        public void Dispose()
        {
            if (Status == GcRequestState.Disposed) return;
            Cancel(); ReleaseData(); Status = GcRequestState.Disposed;
            var released = Released; Released = null; released?.Invoke(this);
        }
        internal static void Destroy(UnityEngine.Object? value)
        {
            if (value == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(value);
            else UnityEngine.Object.DestroyImmediate(value);
        }
    }
    public sealed class GcTextRequest : GcRequest
    {
        /// <summary>成功した応答本文。取得前と破棄後は空文字。</summary>
        public string Text { get; private set; } = "";
        internal GcTextRequest(string url, double deadline) : base(url, deadline) { }
        protected override void Read(UnityWebRequest native) => Text = native.downloadHandler.text;
        protected override void ReleaseData() => Text = "";
    }
    public sealed class GcImageRequest : GcRequest
    {
        internal Texture2D? Texture;
        public int Width => Texture != null ? Texture.width : 0;
        public int Height => Texture != null ? Texture.height : 0;
        internal GcImageRequest(string url, double deadline) : base(url, deadline) { }
        protected override void Read(UnityWebRequest native)
        {
            Texture = DownloadHandlerTexture.GetContent(native);
            if (Texture == null) throw new InvalidOperationException();
        }
        protected override void ReleaseData() { Destroy(Texture); Texture = null; }
    }
    public sealed class GcSoundRequest : GcRequest
    {
        internal AudioClip? Clip;
        public float Duration => Clip != null ? Clip.length : 0;
        internal GcSoundRequest(string url, double deadline) : base(url, deadline) { }
        protected override void Read(UnityWebRequest native)
        {
            Clip = DownloadHandlerAudioClip.GetContent(native);
            if (Clip == null) throw new InvalidOperationException();
        }
        protected override void ReleaseData() { Destroy(Clip); Clip = null; }
    }
}
