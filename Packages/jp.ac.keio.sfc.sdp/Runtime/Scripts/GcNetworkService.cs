#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace GameCanvas
{
    /// <summary>HTTP(S)通信。呼び出しごとに通信を開始する。Webでは接続先のCORS許可が必要。</summary>
    public sealed class GcNetworkService : IDisposable, IEngine
    {
        readonly List<GcRequest> owned = new();
        readonly Func<double> clock;
        bool disposed;
        internal GcNetworkService(Func<double>? clock = null) { this.clock = clock ?? (() => Time.realtimeSinceStartupAsDouble); }
        double Deadline(string url, double timeoutSeconds)
        {
            if (disposed) throw new ObjectDisposedException(nameof(GcNetworkService));
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https"))
                throw new ArgumentException("An absolute HTTP(S) URL is required.", nameof(url));
            if (timeoutSeconds <= 0 || double.IsNaN(timeoutSeconds) || double.IsInfinity(timeoutSeconds))
                throw new ArgumentOutOfRangeException(nameof(timeoutSeconds));
            return clock() + timeoutSeconds;
        }
        T Begin<T>(T request, UnityWebRequest native) where T : GcRequest
        {
            owned.Add(request); request.Released = Release;
            request.Start(native); return request;
        }
        void Release(GcRequest request) => owned.Remove(request);
        /// <summary>テキストを取得する。タイムアウトは秒。</summary>
        public GcTextRequest GetText(string url, double timeoutSeconds = 30)
            => Begin(new GcTextRequest(url, Deadline(url, timeoutSeconds)), UnityWebRequest.Get(url));
        /// <summary>画像を取得する。取得した画像はDrawImageへ渡せる。</summary>
        public GcImageRequest GetImage(string url, double timeoutSeconds = 30)
            => Begin(new GcImageRequest(url, Deadline(url, timeoutSeconds)), UnityWebRequestTexture.GetTexture(url, true));
        /// <summary>音声を取得する。形式を指定し、取得後にPlaySoundへ渡す。</summary>
        public GcSoundRequest GetSound(string url, GcSoundFormat format, double timeoutSeconds = 30)
        {
            var deadline = Deadline(url, timeoutSeconds);
            var type = format switch { GcSoundFormat.Wav => AudioType.WAV, GcSoundFormat.Mp3 => AudioType.MPEG,
                GcSoundFormat.Ogg => AudioType.OGGVORBIS, _ => throw new ArgumentOutOfRangeException(nameof(format)) };
            return Begin(new GcSoundRequest(url, deadline), UnityWebRequestMultimedia.GetAudioClip(url, type));
        }
        /// <summary>UTF-8の本文を送信する。JSONを送る場合はcontentTypeをapplication/jsonにする。</summary>
        public GcTextRequest PostText(string url, string text, string contentType = "text/plain; charset=utf-8", double timeoutSeconds = 30)
        {
            var deadline = Deadline(url, timeoutSeconds);
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (string.IsNullOrWhiteSpace(contentType) || contentType.IndexOfAny(new[] {'\r', '\n'}) >= 0)
                throw new ArgumentException("Invalid content type.", nameof(contentType));
            var native = new UnityWebRequest(url, "POST", new DownloadHandlerBuffer(), new UploadHandlerRaw(Encoding.UTF8.GetBytes(text)));
            native.SetRequestHeader("Content-Type", contentType);
            return Begin(new GcTextRequest(url, deadline), native);
        }
        /// <summary>文字列のフォームを送信する。キーと値はUTF-8でURLエンコードする。</summary>
        public GcTextRequest PostForm(string url, IReadOnlyDictionary<string, string> fields, double timeoutSeconds = 30)
        {
            Deadline(url, timeoutSeconds);
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            var body = new StringBuilder();
            foreach (var field in fields)
            {
                if (field.Key == null || field.Value == null) throw new ArgumentException("Null form field.", nameof(fields));
                if (body.Length != 0) body.Append('&');
                body.Append(Uri.EscapeDataString(field.Key)).Append('=').Append(Uri.EscapeDataString(field.Value));
            }
            return PostText(url, body.ToString(), "application/x-www-form-urlencoded; charset=utf-8", timeoutSeconds);
        }
        /// <summary>待機中の通信をすべて取り消す。成功済みのデータは維持する。</summary>
        public void CancelAll() { for (int i = 0; i < owned.Count; i++) owned[i].Cancel(); }
        internal void Tick() { var now = clock(); for (int i = 0; i < owned.Count; i++) owned[i].Tick(now); }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            while (owned.Count != 0) owned[owned.Count - 1].Dispose();
        }
        void IEngine.OnBeforeUpdate(in DateTimeOffset now) => Tick();
        void IEngine.OnAfterDraw() { }
    }
}
