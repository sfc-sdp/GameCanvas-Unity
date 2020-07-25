/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using GameCanvas.Engine;
using GameCanvas.Input;
using UnityEngine;
using Collision = GameCanvas.Engine.Collision;
using HiddenAttribute = System.ComponentModel.EditorBrowsableAttribute;
using HiddenState = System.ComponentModel.EditorBrowsableState;
using Network = GameCanvas.Engine.Network;
using Time = GameCanvas.Engine.Time;

namespace GameCanvas
{
    public sealed class Proxy
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        internal readonly Time Time;
        internal readonly Graphic Graphic;
        internal readonly Sound Sound;
        internal readonly Collision Collision;
        internal readonly Network Network;
        internal readonly Pointer Pointer;
        internal readonly Keyboard Keyboard;
        internal readonly Accelerometer Accelerometer;
        internal readonly Geolocation Geolocation;
        internal readonly CameraDevice CameraDevice;

        private System.Random m_Random;
        private double m_TargetFrameInterval;
        private bool m_VSyncEnabled;

        // 定数

        /// <summary>
        /// 戻るボタン（Androidのみ）
        /// </summary>
        public readonly EKeyCode KeyEscape = EKeyCode.Escape;

        /// <summary>
        /// 白色
        /// </summary>
        public readonly Color ColorWhite = new Color(1f, 1f, 1f);
        /// <summary>
        /// 黒色
        /// </summary>
        public readonly Color ColorBlack = new Color(0f, 0f, 0f);
        /// <summary>
        /// 灰色
        /// </summary>
        public readonly Color ColorGray = new Color(.5f, .5f, .5f);
        /// <summary>
        /// 赤色
        /// </summary>
        public readonly Color ColorRed = new Color(1f, 0f, 0f);
        /// <summary>
        /// 青色
        /// </summary>
        public readonly Color ColorBlue = new Color(0f, 0f, 1f);
        /// <summary>
        /// 緑色
        /// </summary>
        public readonly Color ColorGreen = new Color(0f, 1f, 0f);
        /// <summary>
        /// 黄色
        /// </summary>
        public readonly Color ColorYellow = new Color(1f, 1f, 0f);
        /// <summary>
        /// 紫色
        /// </summary>
        public readonly Color ColorPurple = new Color(1f, 0f, 1f);
        /// <summary>
        /// シアン色
        /// </summary>
        public readonly Color ColorCyan = new Color(0f, 1f, 1f);
        /// <summary>
        /// みずいろ
        /// </summary>
        public readonly Color ColorAqua = new Color(.5f, .5f, 1f);

        /// <summary>
        /// 次のフレームを待つ (yield return 限定)
        /// </summary>
        public readonly object WaitForNextFrame = null;

        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        // 描画：文字列

        /// <summary>
        /// 文字列を左寄せで描画します。
        /// </summary>
        /// <param name="str">描画する文字列</param>
        /// <param name="x">左端のX座標</param>
        /// <param name="y">上のY座標</param>
        public void DrawString(string str, int x, int y) => Graphic.DrawString(str, x, y);

        /// <summary>
        /// 文字列を中央寄せで描画します。
        /// </summary>
        /// <param name="str"></param>
        /// <param name="str">描画する文字列</param>
        /// <param name="x">中心のX座標</param>
        /// <param name="y">上のY座標</param>
        public void DrawCenterString(string str, int x, int y) => Graphic.DrawCenterString(str, x, y);

        /// <summary>
        /// 文字列を右寄せで描画します。
        /// </summary>
        /// <param name="str">描画する文字列</param>
        /// <param name="x">右端のX座標</param>
        /// <param name="y">上のY座標</param>
        public void DrawRightString(string str, int x, int y) => Graphic.DrawRightString(str, x, y);

        /// <summary>
        /// オンラインテキストを取得します。（非同期）
        /// 
        /// すぐに取得できなかった場合は null を返します。戻り値から取得状況を確認できます。
        /// </summary>
        /// <param name="url">テキストURL</param>
        /// <param name="text">取得したテキスト</param>
        /// <returns>取得状況</returns>
        public EDownloadState GetOnlineTextAsync(string url, out string text)
        {
            return Network.GetOnlineText(url, out text);
        }

        /// <summary>
        /// 文字列描画で使用するフォントを指定します。
        /// </summary>
        /// <param name="fontId">フォントID（fnt0.ttf なら 0, fnt1.otf なら 1）</param>
        /// <param name="fontStyle">フォントスタイル</param>
        /// <param name="fontSize">フォントサイズ</param>
        public void SetFont(int fontId, FontStyle fontStyle, int fontSize) => Graphic.SetFont(fontId, fontStyle, fontSize);

        /// <summary>
        /// フォントサイズを指定します。
        /// </summary>
        /// <param name="fontSize">フォントサイズ</param>
        public void SetFontSize(int fontSize) => Graphic.SetFontSize(fontSize);

        /// <summary>
        /// 文字列の横幅を計算します。
        /// </summary>
        /// <param name="str">調べる文字列</param>
        /// <returns>横幅</returns>
        public int GetStringWidth(string str) => Graphic.GetStringWidth(str);

        // 描画：図形

        /// <summary>
        /// 描画に用いる塗りの色を指定します。
        /// </summary>
        /// <param name="color"><see cref="Color"/> 構造体で色を指定</param>
        public void SetColor(Color color) => Graphic.SetColor(color);

        /// <summary>
        /// 描画に用いる塗りの色を指定します。
        /// </summary>
        /// <param name="color">888 で色を指定</param>
        public void SetColor(int color) => Graphic.SetColor(color);

        /// <summary>
        /// 描画に用いる塗りの色を指定します。
        /// </summary>
        /// <param name="r">色の赤成分 (0～255)</param>
        /// <param name="g">色の緑成分 (0～255)</param>
        /// <param name="b">色の青成分 (0～255)</param>
        public void SetColor(int r, int g, int b) => Graphic.SetColor(r, g, b);

        /// <summary>
        /// 描画に用いる塗りの色を指定します。
        /// </summary>
        /// <param name="r">色の赤成分 (0～255)</param>
        /// <param name="g">色の緑成分 (0～255)</param>
        /// <param name="b">色の青成分 (0～255)</param>
        /// <param name="a">色の不透明度 (0～255)</param>
        public void SetColor(int r, int g, int b, int a) => Graphic.SetColor(r, g, b, a);

        /// <summary>
        /// 描画に用いる塗りの色をHSV色空間で指定します。
        /// </summary>
        /// <param name="h">色相（0f～1f）</param>
        /// <param name="s">彩度（0f～1f）</param>
        /// <param name="v">明度（0f～1f）</param>
        public void SetColorHsv(float h, float s, float v) => Graphic.SetColorHSV(h, s, v);

        /// <summary>
        /// 直線を描画します。
        /// </summary>
        /// <param name="startX">開始点のX座標</param>
        /// <param name="startY">開始点のY座標</param>
        /// <param name="endX">終了点のX座標</param>
        /// <param name="endY">終了点のY座標</param>
        public void DrawLine(int startX, int startY, int endX, int endY) => Graphic.DrawLine(startX, startY, endX, endY);

        /// <summary>
        /// 中抜きの長方形を描画します。
        /// </summary>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        public void DrawRect(int x, int y, int width, int height) => Graphic.DrawRect(x, y, width, height);

        /// <summary>
        /// 塗りつぶしの長方形を描画します。
        /// </summary>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        public void FillRect(int x, int y, int width, int height) => Graphic.FillRect(x, y, width, height);

        /// <summary>
        /// 中抜きの円を描画します。
        /// </summary>
        /// <param name="x">中心のX座標</param>
        /// <param name="y">中心のY座標</param>
        /// <param name="radius">半径</param>
        public void DrawCircle(int x, int y, int radius) => Graphic.DrawCircle(x, y, radius);

        /// <summary>
        /// 塗りつぶしの円を描画します。
        /// </summary>
        /// <param name="x">中心のX座標</param>
        /// <param name="y">中心のY座標</param>
        /// <param name="radius">半径</param>
        public void FillCircle(int x, int y, int radius) => Graphic.FillCircle(x, y, radius);

        // 描画：画像

        /// <summary>
        /// 画像の透明度を指定します。
        /// </summary>
        /// <param name="alpha">画像の不透明度 (0～255)。初期値は255=不透明</param>
        public void SetImageAlpha(int alpha) => Graphic.SetImageAlpha(alpha);

        /// <summary>
        /// 画像に加算する色を指定します。
        /// </summary>
        /// <param name="r">色の赤成分 (0～255)</param>
        /// <param name="g">色の緑成分 (0～255)</param>
        /// <param name="b">色の青成分 (0～255)</param>
        /// <param name="a">色の不透明度 (0～255)</param>
        public void SetImageMultiplyColor(int r, int g, int b, int a) => Graphic.SetImageMultiplyColor(r, g, b, a);

        /// <summary>
        /// 画像に加算する色を指定します。
        /// </summary>
        /// <param name="color">色</param>
        public void SetImageMultiplyColor(Color color) => Graphic.SetImageMultiplyColor(color);

        /// <summary>
        /// 画像に加算する色情報を破棄(画像をそのまま描画)します。
        /// </summary>
        public void ClearImageMultiplyColor() => Graphic.ClearImageMultiplyColor();

        /// <summary>
        /// 画像を描画します。
        /// </summary>
        /// <param name="imageId">画像ID（img0.png なら 0, img1.jpg なら 1）</param>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        public void DrawImage(int imageId, int x, int y) => Graphic.DrawImage(imageId, x, y);

        /// <summary>
        /// 画像を拡大縮小して描画します。
        /// </summary>
        /// <param name="imageId">画像ID（img0.png なら 0, img1.jpg なら 1）</param>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="scaleX">横の拡縮率（1fで等倍, 2fなら倍の大きさ）</param>
        /// <param name="scaleY">縦の拡縮率（1fで等倍, 2fなら倍の大きさ）</param>
        public void DrawImage(int imageId, int x, int y, float scaleX, float scaleY) => Graphic.DrawImage(imageId, x, y, scaleX, scaleY);

        /// <summary>
        /// 画像を拡大縮小して描画します。
        /// </summary>
        /// <param name="imageId">画像ID（img0.png なら 0, img1.jpg なら 1）</param>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="scaleX">横の拡縮率（1fで等倍, 2fなら倍の大きさ）</param>
        /// <param name="scaleY">縦の拡縮率（1fで等倍, 2fなら倍の大きさ）</param>
        /// <param name="pivotX">横の拡縮基準位置（0fで左端, 1fなら右端）</param>
        /// <param name="pivotY">縦の拡縮基準位置（0fで上端, 1fなら下端）</param>
        public void DrawImage(int imageId, int x, int y, float scaleX, float scaleY, float pivotX, float pivotY) => Graphic.DrawImage(imageId, x, y, scaleX, scaleY, pivotX, pivotY);

        /// <summary>
        /// 画像を部分的に描画します。
        /// </summary>
        /// <param name="imageId">画像ID（img0.png なら 0, img1.jpg なら 1）</param>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="u">元画像左上を原点としたときの描画部分左上のX座標</param>
        /// <param name="v">元画像左上を原点としたときの描画部分左上のY座標</param>
        /// <param name="width">描画部分の幅</param>
        /// <param name="height">描画部分の高さ</param>
        public void DrawClippedImage(int imageId, int x, int y, int u, int v, int width, int height) => Graphic.DrawClippedImage(imageId, x, y, u, v, width, height);

        /// <summary>
        /// 画像を部分的に描画します。
        /// </summary>
        /// <param name="imageId">画像ID（img0.png なら 0, img1.jpg なら 1）</param>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="u">元画像左上を原点としたときの描画部分左上のX座標</param>
        /// <param name="v">元画像左上を原点としたときの描画部分左上のY座標</param>
        /// <param name="width">描画部分の幅</param>
        /// <param name="height">描画部分の高さ</param>
        [System.Obsolete("gc.DrawClippedImage")]
        public void DrawClipImage(int imageId, int x, int y, int u, int v, int width, int height) => Graphic.DrawClippedImage(imageId, x, y, u, v, width, height);

        /// <summary>
        /// 画像を拡大縮小・回転をかけて描画します。
        /// </summary>
        /// <param name="imageId">画像ID（img0.png なら 0, img1.jpg なら 1）</param>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="xSize">横の拡縮率（100で等倍, 200なら倍の大きさ）</param>
        /// <param name="ySize">縦の拡縮率（100で等倍, 200なら倍の大きさ）</param>
        /// <param name="degree">回転角（度数法）</param>
        public void DrawScaledRotateImage(int imageId, int x, int y, int xSize, int ySize, float degree) => Graphic.DrawScaledRotateImage(imageId, x, y, xSize, ySize, degree);

        /// <summary>
        /// 画像を拡大縮小・回転をかけて描画します。
        /// </summary>
        /// <param name="imageId">画像ID（img0.png なら 0, img1.jpg なら 1）</param>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="xSize">横の拡縮率（100で等倍, 200なら倍の大きさ）</param>
        /// <param name="ySize">縦の拡縮率（100で等倍, 200なら倍の大きさ）</param>
        /// <param name="degree">回転角（度数法）</param>
        /// <param name="centerX">回転中心のX座標</param>
        /// <param name="centerY">回転中心のY座標</param>
        public void DrawScaledRotateImage(int imageId, int x, int y, int xSize, int ySize, float degree, float centerX, float centerY) => Graphic.DrawScaledRotateImage(imageId, x, y, xSize, ySize, degree, centerX, centerY);

        /// <summary>
        /// オンライン画像をダウンロードし描画します。
        /// </summary>
        /// <param name="url">画像URL</param>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <returns>ダウンロード状態</returns>
        public EDownloadState DrawOnlineImage(string url, int x, int y) => Network.DrawOnlineImage(url, x, y);

#if !GC_DISABLE_CAMERAINPUT
        /// <summary>
        /// カメラ映像を描画します。事前に <see cref="StartCameraService"/> を呼んでおく必要があります。
        /// </summary>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        public void DrawCameraImage(int x, int y) => CameraDevice.Draw(x, y);

        /// <summary>
        /// カメラ映像を部分的に描画します。事前に <see cref="StartCameraService"/> を呼んでおく必要があります。
        /// </summary>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="u">元画像左上を原点としたときの描画部分左上のX座標</param>
        /// <param name="v">元画像左上を原点としたときの描画部分左上のY座標</param>
        /// <param name="width">描画部分の幅</param>
        /// <param name="height">描画部分の高さ</param>
        public void DrawClipCameraImage(int x, int y, int u, int v, int width, int height) => CameraDevice.Draw(x, y, u, v, width, height);

        /// <summary>
        /// カメラ映像を拡大縮小・回転をかけて描画します。事前に <see cref="StartCameraService"/> を呼んでおく必要があります。
        /// </summary>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="xSize">横の拡縮率（100で等倍, 200なら倍の大きさ）</param>
        /// <param name="ySize">縦の拡縮率（100で等倍, 200なら倍の大きさ）</param>
        /// <param name="degree">回転角（度数法）</param>
        public void DrawScaledRotateCameraImage(int x, int y, int xSize, int ySize, float degree) => CameraDevice.Draw(x, y, xSize, ySize, degree);
#endif //!GC_DISABLE_CAMERAINPUT

        /// <summary>
        /// オンライン画像の幅を取得します。
        /// </summary>
        /// <param name="url">画像URL</param>
        /// <returns>幅（ダウンロードが完了していない場合は常に0）</returns>
        public int GetOnlineImageWidth(string url) => Network.GetOnlineImageWidth(url);

        /// <summary>
        /// オンライン画像の高さを取得します。
        /// </summary>
        /// <param name="url">画像URL</param>
        /// <returns>高さ（ウンロードが完了していない場合は常に0）</returns>
        public int GetOnlineImageHeight(string url) => Network.GetOnlineImageHeight(url);

        /// <summary>
        /// 画像の幅を取得します。
        /// </summary>
        /// <param name="imageId">画像ID（img0.png なら 0, img1.jpg なら 1）</param>
        /// <returns>幅（存在しない画像IDの場合は常に0）</returns>
        public int GetImageWidth(int imageId) => Graphic.GetImageWidth(imageId);

        /// <summary>
        /// 画像の高さを取得します。
        /// </summary>
        /// <param name="imageId">画像ID（img0.png なら 0, img1.jpg なら 1）</param>
        /// <returns>高さ（存在しない画像IDの場合は常に0）</returns>
        public int GetImageHeight(int imageId) => Graphic.GetImageHeight(imageId);

        // 描画：その他

        /// <summary>
        /// 画面を白で塗りつぶします。
        /// </summary>
        public void ClearScreen() => Graphic.ClearScreen();

        /// <summary>
        /// 現在の画面を、画像として保存します。
        /// </summary>
        /// <param name="file">拡張子を除いたファイル名</param>
        public void WriteScreenImage(string file)
        {
            if (!file.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)) file += ".png";
            ScreenCapture.CaptureScreenshot(file);
        }

        /// <summary>
        /// 画面の幅と高さを設定します。
        /// </summary>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        public void SetResolution(int width, int height) => Graphic.SetResolution(width, height);

        /// <summary>
        /// UpdateGame や DrawGame が呼び出される時間間隔を設定します。
        /// </summary>
        /// <param name="targetDeltaTime">フレーム更新間隔の目標値（秒）</param>
        /// <param name="vSyncEnabled">垂直同期の有無</param>
        /// <remarks>
        /// 垂直同期を無効にした場合、間隔の揺らぎは減少しますが、ディスプレイのリフレッシュレートを常に無視して描画するため、画面のちらつきが発生する場合があります。
        /// </remarks>
        public void SetFrameInterval(double targetDeltaTime, bool vSyncEnabled = true)
        {
            if (targetDeltaTime <= 0)
            {
                Debug.LogError($"{nameof(targetDeltaTime)} に 0 以下の値は指定できません");
                return;
            }
            m_TargetFrameInterval = targetDeltaTime;
            m_VSyncEnabled = vSyncEnabled;

            QualitySettings.vSyncCount = m_VSyncEnabled ? 1 : 0;
            Application.targetFrameRate = m_VSyncEnabled ? TargetFrameRate : int.MaxValue;
        }

        /// <summary>
        /// フレームレートの目標値を設定します。
        /// 
        /// 小数点以下を指定したい場合は、この関数の代わりに <see cref="SetFrameInterval"/> を使用してください。
        /// </summary>
        /// <param name="targetFrameRate">フレームレート（1秒あたりのフレーム数）の目標値</param>
        /// <param name="vSyncEnabled">垂直同期の有無</param>
        /// <remarks>
        /// 垂直同期を無効にした場合、間隔の揺らぎは減少しますが、ディスプレイのリフレッシュレートを常に無視して描画するため、画面のちらつきが発生する場合があります。
        /// </remarks>
        public void SetFrameRate(int targetFrameRate, bool vSyncEnabled = true)
        {
            SetFrameInterval(1d / targetFrameRate, vSyncEnabled);
        }

        /// <summary>
        /// 端末スクリーン解像度（X方向）
        /// </summary>
        public int DeviceScreenWidth => Graphic.DeviceScreenWidth;

        /// <summary>
        /// 端末スクリーン解像度（Y方向）
        /// </summary>
        public int DeviceScreenHeight => Graphic.DeviceScreenHeight;

        /// <summary>
        /// 画面の幅
        /// </summary>
        public int CanvasWidth => Graphic.CanvasWidth;

        /// <summary>
        /// 画面の高さ
        /// </summary>
        public int CanvasHeight => Graphic.CanvasHeight;

        /// <summary>
        /// 垂直同期の有無
        /// </summary>
        /// <remarks>
        /// この設定は、<see cref="SetFrameInterval"/> や <see cref="SetFrameRate"/> の第二引数から変更できます。
        /// </remarks>
        public bool VSyncEnabled => m_VSyncEnabled;

        /// <summary>
        /// フレーム更新間隔の目標値（秒）
        /// </summary>
        public double TargetFrameInterval => m_TargetFrameInterval;

        /// <summary>
        /// フレームレート（1秒あたりのフレーム数）の目標値
        /// </summary>
        public int TargetFrameRate => (int)(1d / m_TargetFrameInterval);

        /// <summary>
        /// フレームレート（1秒あたりのフレーム数）の目標値
        /// </summary>
        [System.Obsolete("gc.FrameInterval, gc.SetFrameRate()")]
        public int ConfigFps
        {
            get { return (int)(1d / m_TargetFrameInterval); }
            set { SetFrameRate(value, m_VSyncEnabled); }
        }

        // 音声

        /// <summary>
        /// サウンドを再生します。
        /// </summary>
        /// <param name="soundId">サウンドID（snd0.wav なら 0, snd1.mp3 なら 1）</param>
        /// <param name="loop">ループ再生するかどうか (SEトラックでは常に無視されます)</param>
        /// <param name="track">対象の音声トラック</param>
        public void PlaySound(int soundId, bool loop = false, ESoundTrack track = ESoundTrack.BGM1) => Sound.Play(soundId, loop, track);

        /// <summary>
        /// 効果音を1回再生します。
        /// </summary>
        /// <param name="soundId">サウンドID（snd0.wav なら 0, snd1.mp3 なら 1）</param>
        public void PlaySE(int soundId) => Sound.PlaySE(soundId);

        /// <summary>
        /// 指定された音声トラックの音量を変更します。
        /// </summary>
        /// <param name="volume">音量（0～100）</param>
        /// <param name="track">対象の音声トラック</param>
        public void SetSoundVolume(int volume, ESoundTrack track = ESoundTrack.BGM1) => Sound.SetVolume(ref volume, track);

        /// <summary>
        /// 指定された音声トラックの音量を変更します。
        /// </summary>
        /// <param name="decibel">音量 (-80〜20dB)</param>
        /// <param name="track">対象の音声トラック</param>
        public void SetSoundDecibel(float decibel, ESoundTrack track = ESoundTrack.BGM1) => Sound.SetVolume(decibel, track);

        /// <summary>
        /// 指定された音声トラックのサウンドを停止します。BGMトラック以外では常に無視されます。
        /// </summary>
        /// <param name="track">対象の音声トラック</param>
        public void StopSound(ESoundTrack track = ESoundTrack.BGM1) => Sound.Stop(track);

        /// <summary>
        /// 指定された音声トラックのサウンドを一時停止します。BGMトラック以外では常に無視されます。
        /// </summary>
        /// <param name="track">対象の音声トラック</param>
        public void PauseSound(ESoundTrack track = ESoundTrack.BGM1) => Sound.Pause(track);

        /// <summary>
        /// 指定された音声トラックのサウンドを一時停止していた場合、再生を再開します。BGMトラック以外では常に無視されます。
        /// </summary>
        /// <param name="track">対象の音声トラック</param>
        public void UnpauseSound(ESoundTrack track = ESoundTrack.BGM1) => Sound.Unpause(track);

        // キー入力

        /// <summary>
        /// バックボタンが押されているかどうか（Androidのみ）
        /// </summary>
        public bool IsPressBackButton => Keyboard.IsPressBackButton;

        /// <summary>
        /// キーが押されているフレーム数（押された瞬間を1フレーム目とする経過フレーム数）を取得します。
        /// </summary>
        /// <param name="key">キー番号</param>
        /// <returns>フレーム数（押されていない場合は常に0を返します）</returns>
        public int GetKeyPressFrameCount(EKeyCode key) => Keyboard.GetPressFrameCount(ref key);

        /// <summary>
        /// キーが押されている時間（押された瞬間からの経過時間）を取得します。
        /// </summary>
        /// <param name="key">キー番号</param>
        /// <returns>時間（秒。押されていない場合と押された瞬間は0を返します）</returns>
        public float GetKeyPressDuration(EKeyCode key) => Keyboard.GetPressDuration(ref key);

        /// <summary>
        /// キーが押されているかどうかを取得します。
        /// </summary>
        /// <param name="key">キー番号</param>
        /// <returns>押されているかどうか</returns>
        public bool GetIsKeyPress(EKeyCode key) => Keyboard.GetIsPress(ref key);

        /// <summary>
        /// キーが押された瞬間どうかを取得します。
        /// </summary>
        /// <param name="key">キー番号</param>
        /// <returns>押された瞬間かどうか</returns>
        public bool GetIsKeyBegan(EKeyCode key) => Keyboard.GetIsBegan(ref key);

        /// <summary>
        /// キーが離された瞬間どうかを取得します。
        /// </summary>
        /// <param name="key">キー番号</param>
        /// <returns>離された瞬間かどうか</returns>
        public bool GetIsKeyEnded(EKeyCode key) => Keyboard.GetIsEnded(ref key);

        /// <summary>
        /// スクリーンキーボードを表示します。（実機のみ）
        /// </summary>
        /// <returns>正常に表示できたかどうか</returns>
        public bool ShowScreenKeyboard() => Keyboard.Open();

        // ポインタ入力

        /// <summary>
        /// 有効なポインタイベントの数を取得します。
        /// </summary>
        public int PointerCount => Pointer.Count;

        /// <summary>
        /// ポインタイベントがあるかどうかを取得します。
        /// </summary>
        /// <example>
        /// 以下はポインタ入力の有無を判定するコードの例です。
        /// <code>
        /// string str;
        /// 
        /// public override void UpdateGame()
        /// {
        ///     if (gc.HasPointerEvent)
        ///     {
        ///         str = "ポインタ入力あり！";
        ///     }
        ///     else
        ///     {
        ///         str = "ポインタ入力なし……";
        ///     }
        /// }
        /// </code>
        /// </example>
        public bool HasPointerEvent => Pointer.HasEvent;

        /// <summary>
        /// タップされた瞬間かどうかを取得します。
        /// </summary>
        /// <param name="x">タップされた箇所のX座標</param>
        /// <param name="y">タップされた箇所のY座標</param>
        /// <returns>タップされた瞬間かどうか</returns>
        public bool IsTapped(out int x, out int y) => Pointer.IsTapped(out x, out y);

        /// <summary>
        /// 指定された領域がタップされた瞬間かどうかを取得します。
        /// </summary>
        /// <param name="x">タップ判定領域のX座標</param>
        /// <param name="y">タップ判定領域のY座標</param>
        /// <param name="w">タップ判定領域の横幅</param>
        /// <param name="h">タップ判定領域の縦幅</param>
        /// <returns>指定された領域がタップされた瞬間かどうか</returns>
        public bool IsTapped(in int x, in int y, in int w, in int h) => Pointer.IsTapped(new RectInt(x, y, w, h));

        /// <summary>
        /// ポインタイベントを取得します。
        /// </summary>
        /// <param name="i">ポインタ通し番号（0～<see cref="PointerCount"/>-1）</param>
        /// <returns>イベント</returns>
        public PointerEvent GetPointerEvent(int i) => Pointer.GetRaw(i);

        /// <summary>
        /// ポインタのX座標を取得します。
        /// </summary>
        /// <param name="i">ポインタ通し番号（0～<see cref="PointerCount"/>-1）</param>
        /// <returns>X座標</returns>
        public int GetPointerX(int i) => Pointer.GetX(i);

        /// <summary>
        /// ポインタのY座標を取得します。
        /// </summary>
        /// <param name="i">ポインタ通し番号（0～<see cref="PointerCount"/>-1）</param>
        /// <returns>Y座標</returns>
        public int GetPointerY(int i) => Pointer.GetY(i);

        /// <summary>
        /// ポインタが押された瞬間かどうかを取得します。
        /// </summary>
        /// <param name="i">ポインタ通し番号（0～<see cref="PointerCount"/>-1）</param>
        /// <returns>押された瞬間かどうか</returns>
        public bool GetIsPointerBegan(int i) => Pointer.GetIsBegan(i);

        /// <summary>
        /// ポインタが離された瞬間かどうかを取得します。
        /// </summary>
        /// <param name="i">ポインタ通し番号（0～<see cref="PointerCount"/>-1）</param>
        /// <returns>離された瞬間かどうか</returns>
        /// <example>
        /// 以下は <see cref="GetPointerDuration"/> と組み合わせてタップを判定するコードの例です。
        /// <code>
        /// bool isTap;
        /// 
        /// public override void UpdateGame()
        /// {
        ///     // 0.3 秒以内に指を離したらタップと判定する
        ///     <![CDATA[isTap = (gc.GetIsPointerEnded(0) && gc.GetPointerDuration(0) < 0.3f));]]>
        /// }
        /// </code>
        /// </example>
        public bool GetIsPointerEnded(int i) => Pointer.GetIsEnded(i);

        /// <summary>
        /// ポインタが押されているフレーム数（押された瞬間を1フレーム目とする経過フレーム数）を取得します。
        /// </summary>
        /// <param name="i">ポインタ通し番号（0～<see cref="PointerCount"/>-1）</param>
        /// <returns>押された瞬間を1フレーム目とする経過フレーム数（押されていない場合は常に0を返します）</returns>
        public int GetPointerFrameCount(int i) => Pointer.GetFrameCount(i);

        /// <summary>
        /// ポインタが押されている時間（押された瞬間からの経過時間）を取得します。
        /// </summary>
        /// <param name="i">ポインタ通し番号（0～<see cref="PointerCount"/>-1）</param>
        /// <returns>押された瞬間からの経過時間（秒。押されていない場合と押された瞬間は0を返します）</returns>
        /// <example>
        /// 以下は <see cref="GetIsPointerEnded"/> と組み合わせてタップを判定するコードの例です。
        /// <code>
        /// bool isTap;
        /// 
        /// public override void UpdateGame()
        /// {
        ///     // 0.3 秒以内に指を離したらタップと判定する
        ///     <![CDATA[isTap = (gc.GetIsPointerEnded(0) && gc.GetPointerDuration(0) < 0.3f));]]>
        /// }
        /// </code>
        /// </example>
        public float GetPointerDuration(int i) => Pointer.GetDulation(i);

        /// <summary>
        /// タップ感度を調整します。
        /// </summary>
        /// <param name="maxDuration">画面に触れてから離すまでの最長時間</param>
        /// <param name="maxDistance">ポインターの最大移動距離</param>
        public void SetTapSensitivity(float maxDuration, float maxDistance) => Pointer.SetTapSensitivity(maxDuration, maxDistance);

        // 数学

        /// <summary>
        /// 乱数の種をセットします。
        /// </summary>
        /// <param name="seed">シード</param>
        public void SetSeed(int seed)
        {
            m_Random = new System.Random(seed);
        }

        /// <summary>
        /// <paramref name="min"/> から <paramref name="max"/> までのランダムな値を計算します。
        /// </summary>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <returns><paramref name="min"/> から <paramref name="max"/> までのランダムな値</returns>
        public int Random(int min, int max) => m_Random.Next(min, max + 1);

        /// <summary>
        /// 0f から 1f までのランダムな値を計算します。
        /// </summary>
        /// <returns>0f から 1f までのランダムな値</returns>
        public float Random() => UnityEngine.Random.value;

        /// <summary>
        /// 長方形A と 長方形B が重なっているかどうか判定します。
        /// </summary>
        /// <param name="x1">長方形Aの左上X座標</param>
        /// <param name="y1">長方形Aの左上Y座標</param>
        /// <param name="w1">長方形Aの幅</param>
        /// <param name="h1">長方形Aの高さ</param>
        /// <param name="x2">長方形Bの左上X座標</param>
        /// <param name="y2">長方形Bの左上Y座標</param>
        /// <param name="w2">長方形Bの幅</param>
        /// <param name="h2">長方形Bの高さ</param>
        /// <returns>重なっているかどうか</returns>
        public bool CheckHitRect(int x1, int y1, int w1, int h1, int x2, int y2, int w2, int h2)
            => Collision.CheckHitRect(ref x1, ref y1, ref w1, ref h1, ref x2, ref y2, ref w2, ref h2);

        /// <summary>
        /// 画像A と 画像B が重なっているかどうか判定します。
        /// </summary>
        /// <param name="imageId1">画像Aの画像ID</param>
        /// <param name="x1">画像Aの左上X座標</param>
        /// <param name="y1">画像Aの左上Y座標</param>
        /// <param name="imageId2">画像Bの画像ID</param>
        /// <param name="x2">画像Bの左上X座標</param>
        /// <param name="y2">画像Bの左上Y座標</param>
        /// <returns>重なっているかどうか</returns>
        public bool CheckHitImage(int imageId1, int x1, int y1, int imageId2, int x2, int y2)
            => Collision.CheckHitImage(ref imageId1, ref x1, ref y1, ref imageId2, ref x2, ref y2);

        /// <summary>
        /// 円A と　円B が重なっているかどうか判定します。
        /// </summary>
        /// <param name="x1">円Aの中心X座標</param>
        /// <param name="y1">円Aの中心Y座標</param>
        /// <param name="r1">円Aの半径</param>
        /// <param name="x2">円Bの中心X座標</param>
        /// <param name="y2">円Bの中心Y座標</param>
        /// <param name="r2">円Bの半径</param>
        /// <returns>重なっているかどうか</returns>
        public bool CheckHitCircle(int x1, int y1, int r1, int x2, int y2, int r2)
            => Collision.CheckHitCircle(ref x1, ref y1, ref r1, ref x2, ref y2, ref r2);

        /// <summary>
        /// 平方根を計算します。
        /// </summary>
        /// <param name="value">平方根を求める値</param>
        /// <returns>平方根</returns>
        public float Sqrt(float value) => Mathf.Sqrt(value);

        /// <summary>
        /// コサインを計算します。
        /// </summary>
        /// <param name="degree">角度（度数法）</param>
        /// <returns>コサイン</returns>
        public float Cos(float degree) => Mathf.Cos(degree * Mathf.Deg2Rad);

        /// <summary>
        /// サインを計算します。
        /// </summary>
        /// <param name="degree">角度（度数法）</param>
        /// <returns>サイン</returns>
        public float Sin(float degree) => Mathf.Sin(degree * Mathf.Deg2Rad);

        /// <summary>
        /// ベクトルの角度を計算します。
        /// </summary>
        /// <param name="x">ベクトルのX成分</param>
        /// <param name="y">ベクトルのY成分</param>
        /// <returns>ベクトルの角度（度数法）</returns>
        public float Atan2(float x, float y) => Mathf.Atan2(x, y);

        // 時間（日時、フレーム）

        /// <summary>
        /// 現在フレームのアプリ起動からの経過時間（秒）
        /// </summary>
        public float TimeSinceStartup => Time.SinceStartup;

        /// <summary>
        /// ひとつ前のフレームからの経過時間（秒）
        /// </summary>
        public float TimeSincePrevFrame => Time.SincePrevFrame;

        /// <summary>
        /// 現在フレームの日付の西暦部分
        /// </summary>
        public int CurrentYear => Time.Year;

        /// <summary>
        /// 現在フレームの日付の月部分（1～12）
        /// </summary>
        public int CurrentMonth => Time.Month;

        /// <summary>
        /// 現在フレームの日付（1～31）
        /// </summary>
        public int CurrentDay => Time.Day;

        /// <summary>
        /// 現在フレームの曜日（0～6）
        /// </summary>
        public System.DayOfWeek CurrentDayOfWeek => Time.DayOfWeek;

        /// <summary>
        /// 現在フレームの時刻の時間部分（0～23）
        /// </summary>
        public int CurrentHour => Time.Hour;

        /// <summary>
        /// 現在フレームの時刻の分部分（0～59）
        /// </summary>
        public int CurrentMinute => Time.Minute;

        /// <summary>
        /// 現在フレームの時刻の秒部分（0～59）
        /// </summary>
        public int CurrentSecond => Time.Second;

        /// <summary>
        /// 現在フレームの時刻のミリ秒部分（0～999）
        /// </summary>
        public int CurrentMillisecond => Time.Millisecond;

        /// <summary>
        /// 現在フレームのUnixタイムスタンプ
        /// </summary>
        public long CurrentTimestamp => Time.Timestamp;

        /// <summary>
        /// アプリ起動からの累計フレーム数
        /// </summary>
        public int CurrentFrame => Time.FrameCount;

        /// <summary>
        /// 現在フレームの日時
        /// </summary>
        public System.DateTimeOffset CurrentTime => Time.Current;

        /// <summary>
        /// 現在（関数呼び出し時点）の日時
        /// </summary>
        public System.DateTimeOffset NowTime => Time.Now;

#if !GC_DISABLE_ACCELEROMETER
        // 加速度計

        /// <summary>
        /// 最後に測定されたX軸加速度
        /// </summary>
        public float AccelerationLastX => Accelerometer.LastX;

        /// <summary>
        /// 最後に測定されたY軸加速度
        /// </summary>
        public float AccelerationLastY => Accelerometer.LastY;

        /// <summary>
        /// 最後に測定されたZ軸加速度
        /// </summary>
        public float AccelerationLastZ => Accelerometer.LastZ;

        /// <summary>
        /// 前のフレーム以降に測定された加速度の数
        /// </summary>
        public int AccelerationEventCount => Accelerometer.EventCount;

        /// <summary>
        /// X軸加速度を取得します。
        /// </summary>
        /// <param name="i">測定番号（0 以上 <see cref="AccelerationEventCount"/> 未満）</param>
        /// <param name="normalize">加速度を正規化するかどうか</param>
        /// <returns>X軸加速度</returns>
        public float GetAccelerationX(int i, bool normalize = false)
        {
            return normalize ? Accelerometer.GetNormalizedX(ref i) : Accelerometer.GetX(ref i);
        }

        /// <summary>
        /// Y軸加速度を取得します。
        /// </summary>
        /// <param name="i">測定番号（0 以上 <see cref="AccelerationEventCount"/> 未満）</param>
        /// <param name="normalize">加速度を正規化するかどうか</param>
        /// <returns>Y軸加速度</returns>
        public float GetAccelerationY(int i, bool normalize = false)
        {
            return normalize ? Accelerometer.GetNormalizedY(ref i) : Accelerometer.GetY(ref i);
        }

        /// <summary>
        /// Z軸加速度を取得します。
        /// </summary>
        /// <param name="i">測定番号（0 以上 <see cref="AccelerationEventCount"/> 未満）</param>
        /// <param name="normalize">加速度を正規化するかどうか</param>
        /// <returns>Z軸加速度</returns>
        public float GetAccelerationZ(int i, bool normalize = false)
        {
            return normalize ? Accelerometer.GetNormalizedZ(ref i) : Accelerometer.GetZ(ref i);
        }
#endif //!GC_DISABLE_ACCELEROMETER

#if !GC_DISABLE_GEOLOCATION
        // 位置情報

        /// <summary>
        /// 位置情報サービスが現在動作しているかどうか
        /// </summary>
        public bool IsGeolocationRunning => Geolocation.IsRunning;

        /// <summary>
        /// 位置情報サービスの利用を許可されているかどうか
        /// </summary>
        public bool HasGeolocationPermission => Geolocation.HasPermission;

        /// <summary>
        /// 現在のフレームで位置情報が更新されたかどうか
        /// </summary>
        public bool HasGeolocationUpdate => Geolocation.HasUpdate;

        /// <summary>
        /// 最後の位置情報の高度
        /// </summary>
        public float GeolocationLastAltitude => Geolocation.LastAltitude;

        /// <summary>
        /// 最後の位置情報の緯度
        /// </summary>
        public float GeolocationLastLatitude => Geolocation.LastLatitude;

        /// <summary>
        /// 最後の位置情報の経度
        /// </summary>
        public float GeolocationLastLongitude => Geolocation.LastLongitude;

        /// <summary>
        /// 最後の位置情報の更新日時
        /// </summary>
        public System.DateTimeOffset GeolocationLastTime => Geolocation.LastTime;

        /// <summary>
        /// 位置情報サービスの状態
        /// </summary>
        public LocationServiceStatus GeolocationStatus => Geolocation.Status;

        /// <summary>
        /// 位置情報サービスを開始します。
        /// </summary>
        public void StartGeolocationService() => Geolocation.StartService();

        /// <summary>
        /// 位置情報サービスを終了します。
        /// </summary>
        public void StopGeolocationService() => Geolocation.StopService();        
#endif //!GC_DISABLE_GEOLOCATION

#if !GC_DISABLE_CAMERAINPUT
        // カメラ映像入力

        /// <summary>
        /// 検出されたカメラ映像入力デバイスの数
        /// </summary>
        public int CameraDeviceCount => CameraDevice.Count;

        /// <summary>
        /// 再生中のカメラ映像の幅
        /// </summary>
        public int CurrentCameraWidth => CameraDevice.CurrentWidth;

        /// <summary>
        /// 再生中のカメラ映像の高さ
        /// </summary>
        public int CurrentCameraHeight => CameraDevice.CurrentHeight;

        /// <summary>
        /// 再生中のカメラ映像入力のデバイス名
        /// </summary>
        public string CurrentCameraName => CameraDevice.CurrentDeviceName;

        /// <summary>
        /// 再生中のカメラ映像の必要回転角度 (度数法)
        /// </summary>
        public int CurrentCameraRotation => CameraDevice.CurrentRotation;

        /// <summary>
        /// 再生中のカメラ映像の上下反転有無
        /// </summary>
        public bool IsCameraMirrored => CameraDevice.IsMirrored;

        /// <summary>
        /// 再生中のカメラ映像が現在フレームで更新されたかどうか
        /// </summary>
        public bool DidCameraUpdate => CameraDevice.DidUpdate;

        /// <summary>
        /// カメラ映像入力デバイスの一覧を更新します。
        /// </summary>
        public void UpdateCameraDeviceList() => CameraDevice.UpdateList();

        /// <summary>
        /// 指定されたカメラ映像入力のデバイス名を取得します。
        /// </summary>
        /// <param name="deviceIndex">カメラデバイス通し番号 (0以上<see cref="CameraDeviceCount"/>未満)</param>
        /// <returns></returns>
        public string GetCameraDeviceName(int deviceIndex) => CameraDevice.GetName(deviceIndex);

        /// <summary>
        /// 指定したカメラ映像入力が前面カメラかどうか
        /// </summary>
        /// <param name="deviceIndex">カメラデバイス通し番号 (0以上<see cref="CameraDeviceCount"/>未満)</param>
        public bool GetIsFrontCamera(int deviceIndex) => CameraDevice.GetIsFront(deviceIndex);

        /// <summary>
        /// カメラ映像入力の選択と開始
        /// </summary>
        /// <param name="deviceIndex">カメラデバイス通し番号 (0以上<see cref="CameraDeviceCount"/>未満)</param>
        public void StartCameraService(int deviceIndex = 0) { CameraDevice.Start(deviceIndex); }

        /// <summary>
        /// カメラ映像入力の停止
        /// </summary>
        public void StopCameraService() => CameraDevice.Stop();

        /// <summary>
        /// カメラ映像入力の一時停止
        /// </summary>
        public void PauseCameraService() => CameraDevice.Pause();

        /// <summary>
        /// カメラ映像入力の再開
        /// </summary>
        public void UnpauseCameraService() => CameraDevice.Unpause();
#endif //!GC_DISABLE_CAMERAINPUT

        // その他

        /// <summary>
        /// ローカルストレージに保存された値を取り出します。
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">保存された値</param>
        /// <returns>取り出しに成功したかどうか</returns>
        public bool TryLoad(string key, out string value)
        {
            value = PlayerPrefs.GetString(key, null);
            return PlayerPrefs.HasKey(key);
        }

        /// <summary>
        /// ローカルストレージに保存された値を取り出します。
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">保存された値</param>
        /// <returns>取り出しに成功したかどうか</returns>
        public bool TryLoad(string key, out int value)
        {
            value = PlayerPrefs.GetInt(key, 0);
            return PlayerPrefs.HasKey(key);
        }

        /// <summary>
        /// ローカルストレージに保存された値を取り出します。
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">保存された値</param>
        /// <returns>取り出しに成功したかどうか</returns>
        public bool TryLoad(string key, out float value)
        {
            value = PlayerPrefs.GetFloat(key, 0f);
            return PlayerPrefs.HasKey(key);
        }

        /// <summary>
        /// ローカルストレージに保存された値を取り出します。
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">保存された値</param>
        /// <returns>取り出しに成功したかどうか</returns>
        public bool TryLoad(string key, out System.Numerics.BigInteger value)
        {
            if (PlayerPrefs.HasKey(key))
            {
                var str = PlayerPrefs.GetString(key, null);
                return System.Numerics.BigInteger.TryParse(str, out value);
            }
            value = System.Numerics.BigInteger.Zero;
            return false;
        }

        /// <summary>
        /// ローカルストレージに値を保存します。<paramref name="value"/> に null を渡した場合、キーに紐づくデータを削除します。
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">保存する値</param>
        public void Save(string key, string value)
        {
            if (value != null)
            {
                PlayerPrefs.SetString(key, value);
            }
            else if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
            }
        }

        /// <summary>
        /// ローカルストレージに値を保存します。<paramref name="value"/> に null を渡した場合、キーに紐づくデータを削除します。
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">保存する値</param>
        public void Save(string key, int? value)
        {
            if (value.HasValue)
            {
                PlayerPrefs.SetInt(key, value.Value);
            }
            else if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
            }
        }

        /// <summary>
        /// ローカルストレージに値を保存します。<paramref name="value"/> に null を渡した場合、キーに紐づくデータを削除します。
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">保存する値</param>
        public void Save(string key, float? value)
        {
            if (value.HasValue)
            {
                PlayerPrefs.SetFloat(key, value.Value);
            }
            else if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
            }
        }

        /// <summary>
        /// ローカルストレージに値を保存します。<paramref name="value"/> に null を渡した場合、キーに紐づくデータを削除します。
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">保存する値</param>
        public void Save(string key, System.Numerics.BigInteger? value)
        {
            if (value.HasValue)
            {
                PlayerPrefs.SetString(key, value.Value.ToString());
            }
            else if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
            }
        }

        /// <summary>
        /// すべてのダウンロードキャッシュを削除します。
        /// </summary>
        public void ClearDownloadCache() => Network.Clear();

        /// <summary>
        /// 初期状態にします。
        /// </summary>
        public void ResetGame()
        {
            Sound.Reset();
            Graphic.ClearScreen();
        }
        /// <summary>
        /// アプリを終了します。（Androidのみ）
        /// </summary>
        public void ExitApp() => Application.Quit();

        // 廃止、改名

        [Hidden(HiddenState.Never), System.Obsolete]
        public readonly EKeyCode KeyUp = EKeyCode.Up;
        [Hidden(HiddenState.Never), System.Obsolete]
        public readonly EKeyCode KeyDown = EKeyCode.Down;
        [Hidden(HiddenState.Never), System.Obsolete]
        public readonly EKeyCode KeyLeft = EKeyCode.Left;
        [Hidden(HiddenState.Never), System.Obsolete]
        public readonly EKeyCode KeyRight = EKeyCode.Right;
        [Hidden(HiddenState.Never), System.Obsolete]
        public readonly EKeyCode KeyZ = EKeyCode.Z;
        [Hidden(HiddenState.Never), System.Obsolete]
        public readonly EKeyCode KeyX = EKeyCode.X;
        [Hidden(HiddenState.Never), System.Obsolete]
        public readonly EKeyCode KeyC = EKeyCode.C;
        [Hidden(HiddenState.Never), System.Obsolete]
        public readonly EKeyCode KeyV = EKeyCode.V;
        [Hidden(HiddenState.Never), System.Obsolete]
        public readonly EKeyCode KeyEnter = EKeyCode.Enter;
        [Hidden(HiddenState.Never), System.Obsolete]
        public readonly EKeyCode KeySpace = EKeyCode.Space;

        [Hidden(HiddenState.Never), System.Obsolete("gc.CanvasWidth")]
        public int Width => Graphic.CanvasWidth;
        [Hidden(HiddenState.Never), System.Obsolete("gc.CanvasWHeight")]
        public int Height => Graphic.CanvasHeight;
        [Hidden(HiddenState.Never), System.Obsolete, System.Diagnostics.Conditional("ENABLE_GAMECANVAS_JAVA")]
        public void SetWindowTitle(string title) { }
        [Hidden(HiddenState.Never), System.Obsolete, System.Diagnostics.Conditional("ENABLE_GAMECANVAS_JAVA")]
        public void SetFont(string fontName, int fontStyle, int fontSize) { }
        [Hidden(HiddenState.Never), System.Obsolete("gc.PlaySE()")]
        public void PlaySE(int soundId, bool loop) => Sound.PlaySE(soundId);

        [Hidden(HiddenState.Never), System.Obsolete("gc.ChangeVolume()")]
        public void ChangeSEVolume(int volume) => Sound.SetVolume(ref volume, ESoundTrack.SE);
        [Hidden(HiddenState.Never), System.Obsolete("gc.Stop()")]
        public void StopSE() {}
        [Hidden(HiddenState.Never), System.Obsolete("gc.Pause()")]
        public void PauseSE() {}
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetPointerX(0)")]
        public int GetMouseX() => Pointer.LastX;
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetPointerY(0)")]
        public int GetMouseY() => Pointer.LastY;
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetPointerFrameCount(0)")]
        public int GetMouseClickLength() => Pointer.FrameCount;
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetIsPointerBegan(0)")]
        public bool IsMousePushed() => Pointer.IsBegan;
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetIsPointerEnded(0)")]
        public bool IsMouseReleased() => Pointer.IsEnded;
        [Hidden(HiddenState.Never), System.Obsolete("gc.HasPointerEvent")]
        public bool IsMousePress() => Pointer.HasEvent;
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetKeyPressFrameCount")]
        public int GetKeyPressLength(EKeyCode key)
        {
            return Keyboard.GetIsEnded(ref key) ? -1 : Keyboard.GetPressFrameCount(ref key);
        }
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetIsKeyPress")]
        public bool IsKeyPress(EKeyCode key) => Keyboard.GetIsPress(ref key);
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetIsKeyBegan")]
        public bool IsKeyPushed(EKeyCode key) => Keyboard.GetIsBegan(ref key);
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetIsKeyEnded")]
        public bool IsKeyReleased(EKeyCode key) => Keyboard.GetIsEnded(ref key);

        [Hidden(HiddenState.Never)]
        public void PlayBGM(int midiId, bool loop = true) { } // TODO
        [Hidden(HiddenState.Never)]
        public void ChangeBGMVolume(int volume) { } // TODO
        [Hidden(HiddenState.Never)]
        public void StopBGM() { } // TODO
        [Hidden(HiddenState.Never)]
        public void PauseBGM() { } // TODO
        [Hidden(HiddenState.Never)]
        public bool ShowYesNoDialog(string message) => false; // TODO
        [Hidden(HiddenState.Never)]
        public string ShowInputDialog(string message, string defaultInput) => null; // TODO

        /// <summary>
        /// ローカルストレージに保存された値を取り出します。
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>保存された値（存在しない場合は0）</returns>
        [Hidden(HiddenState.Never), System.Obsolete("gc.TryLoad")]
        public int Load(int key) => PlayerPrefs.GetInt(key.ToString(), 0);
        /// <summary>
        /// ローカルストレージに値を保存します。
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">保存する値</param>
        [Hidden(HiddenState.Never), System.Obsolete("gc.Save(string, int)")]
        public void Save(int key, int value) => PlayerPrefs.SetInt(key.ToString(), value);

        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal Proxy(BehaviourBase bhvr)
        {
            Time = new Time();
            Graphic = new Graphic(bhvr.Resource, bhvr.m_Camera);
            Graphic.SetResolution(bhvr.CanvasWidth, bhvr.CanvasHeight);
            Sound = new Sound(bhvr, bhvr.Resource, bhvr.GetComponents<AudioSource>());
            Collision = new Collision(bhvr.Resource);
            Network = new Network(bhvr, Graphic);
            Pointer = new Pointer(Time, Graphic);
            Keyboard = new Keyboard();
            Accelerometer = new Accelerometer();
            Geolocation = new Geolocation();
            CameraDevice = new CameraDevice(Graphic);

            m_TargetFrameInterval = 1f / 60;
            m_Random = new System.Random();
            SetFrameRate(60, true);
        }

        internal void OnBeforeUpdate(in System.DateTimeOffset now)
        {
            Time.OnBeforeUpdate(now);
            Graphic.OnBeforeUpdate();
            Sound.OnBeforeUpdate();
            Pointer.OnBeforeUpdate();
            Keyboard.OnBeforeUpdate();
            Accelerometer.OnBeforeUpdate();
            Geolocation.OnBeforeUpdate();
        }

        #endregion
    }
}
