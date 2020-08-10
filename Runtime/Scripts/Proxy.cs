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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using HiddenAttribute = System.ComponentModel.EditorBrowsableAttribute;
using HiddenState = System.ComponentModel.EditorBrowsableState;
using Network = GameCanvas.Engine.Network;
using Time = GameCanvas.Engine.Time;

namespace GameCanvas
{
    /// <summary>
    /// ユーザー定義クラスからGameCanvasの機能を呼び出すためのクラス
    /// </summary>
    public class Proxy
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        internal readonly Time Time;
        internal readonly Graphic Graphic;
        internal readonly Sound Sound;
        internal readonly Physics Physics;
        internal readonly Network Network;
        internal readonly Pointer Pointer;
        internal readonly Keyboard Keyboard;
        internal readonly Accelerometer Accelerometer;
        internal readonly Geolocation Geolocation;
        internal readonly CameraDevice CameraDevice;
        private readonly Resource Resource;
        private readonly Dictionary<System.Type, Scene> SceneDict;

#pragma warning disable IDE0032
        private Scene m_CurrentScene;
        private bool m_SceneLeaveFlag;
        private bool m_SceneEnterFlag;
        private Scene m_NextScene;
        private object m_NextSceneState;
        private double m_TargetFrameInterval;
        private bool m_VSyncEnabled;
#pragma warning restore IDE0032

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawString(string str, in int x, in int y) => Graphic.DrawString(str, x, y);

        /// <summary>
        /// 文字列を中央寄せで描画します。
        /// </summary>
        /// <param name="str"></param>
        /// <param name="str">描画する文字列</param>
        /// <param name="x">中心のX座標</param>
        /// <param name="y">上のY座標</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawCenterString(string str, in int x, in int y) => Graphic.DrawCenterString(str, x, y);

        /// <summary>
        /// 文字列を右寄せで描画します。
        /// </summary>
        /// <param name="str">描画する文字列</param>
        /// <param name="x">右端のX座標</param>
        /// <param name="y">上のY座標</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRightString(string str, in int x, in int y) => Graphic.DrawRightString(str, x, y);

        /// <summary>
        /// オンラインテキストを取得します。（非同期）
        /// 
        /// すぐに取得できなかった場合は null を返します。戻り値から取得状況を確認できます。
        /// </summary>
        /// <param name="url">テキストURL</param>
        /// <param name="text">取得したテキスト</param>
        /// <returns>取得状況</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFont(in int fontId, in FontStyle fontStyle, in int fontSize) => Graphic.SetFont(fontId, fontStyle, fontSize);

        /// <summary>
        /// フォントサイズを指定します。
        /// </summary>
        /// <param name="fontSize">フォントサイズ</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFontSize(in int fontSize) => Graphic.SetFontSize(fontSize);

        /// <summary>
        /// 文字列の横幅を計算します。
        /// </summary>
        /// <param name="str">調べる文字列</param>
        /// <returns>横幅</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetStringWidth(string str) => Graphic.GetStringWidth(str);

        // 描画：図形

        /// <summary>
        /// 描画に用いる塗りの色を指定します。
        /// </summary>
        /// <param name="color"><see cref="Color"/> 構造体で色を指定</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetColor(in Color color) => Graphic.SetColor(color);

        /// <summary>
        /// 描画に用いる塗りの色を指定します。
        /// </summary>
        /// <param name="color">888 で色を指定</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetColor(in int color) => Graphic.SetColor(color);

        /// <summary>
        /// 描画に用いる塗りの色を指定します。
        /// </summary>
        /// <param name="r">色の赤成分 (0～255)</param>
        /// <param name="g">色の緑成分 (0～255)</param>
        /// <param name="b">色の青成分 (0～255)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetColor(in int r, in int g, in int b) => Graphic.SetColor(r, g, b);

        /// <summary>
        /// 描画に用いる塗りの色を指定します。
        /// </summary>
        /// <param name="r">色の赤成分 (0～255)</param>
        /// <param name="g">色の緑成分 (0～255)</param>
        /// <param name="b">色の青成分 (0～255)</param>
        /// <param name="a">色の不透明度 (0～255)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetColor(in int r, in int g, in int b, in int a) => Graphic.SetColor(r, g, b, a);

        /// <summary>
        /// 描画に用いる塗りの色をHSV色空間で指定します。
        /// </summary>
        /// <param name="h">色相（0f～1f）</param>
        /// <param name="s">彩度（0f～1f）</param>
        /// <param name="v">明度（0f～1f）</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetColorHsv(in float h, in float s, in float v) => Graphic.SetColorHSV(h, s, v);

        /// <summary>
        /// 直線を描画します。
        /// </summary>
        /// <param name="startX">開始点のX座標</param>
        /// <param name="startY">開始点のY座標</param>
        /// <param name="endX">終了点のX座標</param>
        /// <param name="endY">終了点のY座標</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawLine(in int startX, in int startY, in int endX, in int endY) => Graphic.DrawLine(startX, startY, endX, endY);

        /// <summary>
        /// 中抜きの矩形を描画します。
        /// </summary>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRect(in int x, in int y, in int width, in int height) => Graphic.DrawRect(new Rect(x, y, width, height));

        /// <summary>
        /// 中抜きの矩形を描画します。
        /// </summary>
        /// <param name="position">左上の座標</param>
        /// <param name="size">大きさ (横幅と縦幅)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRect(in float2 position, in float2 size) => Graphic.DrawRect(new Rect(position, size));

        /// <summary>
        /// 中抜きの矩形を描画します。
        /// </summary>
        /// <param name="rect">位置と大きさ</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRect(in Rect rect) => Graphic.DrawRect(rect);

        /// <summary>
        /// 中抜きの矩形を描画します。
        /// </summary>
        /// <param name="center">中心の座標</param>
        /// <param name="size">大きさ (横幅と縦幅)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawCenterRect(in float2 center, in float2 size) => Graphic.DrawRect(new Rect(center - size * 0.5f, size));

        /// <summary>
        /// 塗りつぶしの矩形を描画します。
        /// </summary>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillRect(in int x, in int y, in int width, in int height) => Graphic.FillRect(new Rect(x, y, width, height));

        /// <summary>
        /// 塗りつぶしの矩形を描画します。
        /// </summary>
        /// <param name="position">左上の座標</param>
        /// <param name="size">大きさ (横幅と縦幅)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillRect(in float2 position, in float2 size) => Graphic.FillRect(new Rect(position, size));

        /// <summary>
        /// 塗りつぶしの矩形を描画します。
        /// </summary>
        /// <param name="center">中心の座標</param>
        /// <param name="size">大きさ (横幅と縦幅)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillCenterRect(in float2 center, in float2 size) => Graphic.FillRect(new Rect(center - size * 0.5f, size));

        /// <summary>
        /// 中塗りつぶしの矩形を描画します。
        /// </summary>
        /// <param name="rect">位置と大きさ</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillRect(in Rect rect) => Graphic.FillRect(rect);

        /// <summary>
        /// 中抜きの円を描画します。
        /// </summary>
        /// <param name="x">中心のX座標</param>
        /// <param name="y">中心のY座標</param>
        /// <param name="radius">半径</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawCircle(in int x, in int y, in int radius) => Graphic.DrawCircle(new float2(x, y), radius);

        /// <summary>
        /// 中抜きの円を描画します。
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="radius">半径</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawCircle(in float2 center, in float radius) => Graphic.DrawCircle(center, radius);

        /// <summary>
        /// 塗りつぶしの円を描画します。
        /// </summary>
        /// <param name="x">中心のX座標</param>
        /// <param name="y">中心のY座標</param>
        /// <param name="radius">半径</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillCircle(in int x, in int y, in int radius) => Graphic.FillCircle(new float2(x, y), radius);

        /// <summary>
        /// 塗りつぶしの円を描画します。
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="radius">半径</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillCircle(in float2 center, in float radius) => Graphic.FillCircle(center, radius);

        // 描画：画像

        /// <summary>
        /// 画像の透明度を指定します。
        /// </summary>
        /// <param name="alpha">画像の不透明度 (0～255)。初期値は255=不透明</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetImageAlpha(in int alpha) => Graphic.SetImageAlpha(alpha);

        /// <summary>
        /// 画像に加算する色を指定します。
        /// </summary>
        /// <param name="r">色の赤成分 (0～255)</param>
        /// <param name="g">色の緑成分 (0～255)</param>
        /// <param name="b">色の青成分 (0～255)</param>
        /// <param name="a">色の不透明度 (0～255)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetImageMultiplyColor(in int r, in int g, in int b, in int a) => Graphic.SetImageMultiplyColor(r, g, b, a);

        /// <summary>
        /// 画像に加算する色を指定します。
        /// </summary>
        /// <param name="color">色</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetImageMultiplyColor(in Color color) => Graphic.SetImageMultiplyColor(color);

        /// <summary>
        /// 画像に加算する色情報を破棄(画像をそのまま描画)します。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearImageMultiplyColor() => Graphic.ClearImageMultiplyColor();

        /// <summary>
        /// 画像を描画します。
        /// </summary>
        /// <param name="imageId">画像ID（img0.png なら 0, img1.jpg なら 1）</param>
        /// <param name="position">座標(左上)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawImage(in int imageId, in float2 position) => Graphic.DrawImage(imageId, position);

        /// <summary>
        /// 画像を描画します。
        /// </summary>
        /// <param name="imageId">画像ID（img0.png なら 0, img1.jpg なら 1）</param>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawImage(in int imageId, in int x, in int y) => Graphic.DrawImage(imageId, new float2(x, y));

        /// <summary>
        /// 画像を拡大縮小して描画します。
        /// </summary>
        /// <param name="imageId">画像ID（img0.png なら 0, img1.jpg なら 1）</param>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="scaleX">横の拡縮率（1fで等倍, 2fなら倍の大きさ）</param>
        /// <param name="scaleY">縦の拡縮率（1fで等倍, 2fなら倍の大きさ）</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawImage(in int imageId, in int x, in int y, float scaleX, float scaleY) => Graphic.DrawImage(imageId, x, y, scaleX, scaleY);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawImage(in int imageId, in int x, in int y, float scaleX, float scaleY, float pivotX, float pivotY) => Graphic.DrawImage(imageId, x, y, scaleX, scaleY, pivotX, pivotY);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawClippedImage(in int imageId, in int x, in int y, in int u, in int v, in int width, in int height) => Graphic.DrawClippedImage(imageId, x, y, u, v, width, height);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawClipImage(in int imageId, in int x, in int y, in int u, in int v, in int width, in int height) => Graphic.DrawClippedImage(imageId, x, y, u, v, width, height);

        /// <summary>
        /// 画像を拡大縮小・回転をかけて描画します。
        /// </summary>
        /// <param name="imageId">画像ID（img0.png なら 0, img1.jpg なら 1）</param>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="xSize">横の拡縮率（100で等倍, 200なら倍の大きさ）</param>
        /// <param name="ySize">縦の拡縮率（100で等倍, 200なら倍の大きさ）</param>
        /// <param name="degree">回転角（度数法）</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawScaledRotateImage(in int imageId, in int x, in int y, in int xSize, in int ySize, float degree) => Graphic.DrawScaledRotateImage(imageId, x, y, xSize, ySize, degree);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawScaledRotateImage(in int imageId, in int x, in int y, in int xSize, in int ySize, float degree, float centerX, float centerY) => Graphic.DrawScaledRotateImage(imageId, x, y, xSize, ySize, degree, centerX, centerY);

        /// <summary>
        /// オンライン画像をダウンロードし描画します。
        /// </summary>
        /// <param name="url">画像URL</param>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <returns>ダウンロード状態</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EDownloadState DrawOnlineImage(string url, in int x, in int y) => Network.DrawOnlineImage(url, x, y);

#if !GC_DISABLE_CAMERAINPUT
        /// <summary>
        /// カメラ映像を描画します。事前に <see cref="StartCameraService"/> を呼んでおく必要があります。
        /// </summary>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawCameraImage(in int x, in int y) => CameraDevice.Draw(x, y);

        /// <summary>
        /// カメラ映像を部分的に描画します。事前に <see cref="StartCameraService"/> を呼んでおく必要があります。
        /// </summary>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="u">元画像左上を原点としたときの描画部分左上のX座標</param>
        /// <param name="v">元画像左上を原点としたときの描画部分左上のY座標</param>
        /// <param name="width">描画部分の幅</param>
        /// <param name="height">描画部分の高さ</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawClipCameraImage(in int x, in int y, in int u, in int v, in int width, in int height) => CameraDevice.Draw(x, y, u, v, width, height);

        /// <summary>
        /// カメラ映像を拡大縮小・回転をかけて描画します。事前に <see cref="StartCameraService"/> を呼んでおく必要があります。
        /// </summary>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="xSize">横の拡縮率（100で等倍, 200なら倍の大きさ）</param>
        /// <param name="ySize">縦の拡縮率（100で等倍, 200なら倍の大きさ）</param>
        /// <param name="degree">回転角（度数法）</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawScaledRotateCameraImage(in int x, in int y, in int xSize, in int ySize, float degree) => CameraDevice.Draw(x, y, xSize, ySize, degree);
#endif //!GC_DISABLE_CAMERAINPUT

        /// <summary>
        /// オンライン画像の幅を取得します。
        /// </summary>
        /// <param name="url">画像URL</param>
        /// <returns>幅（ダウンロードが完了していない場合は常に0）</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOnlineImageWidth(string url) => Network.GetOnlineImageWidth(url);

        /// <summary>
        /// オンライン画像の高さを取得します。
        /// </summary>
        /// <param name="url">画像URL</param>
        /// <returns>高さ（ウンロードが完了していない場合は常に0）</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOnlineImageHeight(string url) => Network.GetOnlineImageHeight(url);

        /// <summary>
        /// 画像の幅を取得します。
        /// </summary>
        /// <param name="imageId">画像ID（img0.png なら 0, img1.jpg なら 1）</param>
        /// <returns>幅（存在しない画像IDの場合は常に0）</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetImageWidth(in int imageId) => Graphic.GetImageWidth(imageId);

        /// <summary>
        /// 画像の高さを取得します。
        /// </summary>
        /// <param name="imageId">画像ID（img0.png なら 0, img1.jpg なら 1）</param>
        /// <returns>高さ（存在しない画像IDの場合は常に0）</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetImageHeight(in int imageId) => Graphic.GetImageHeight(imageId);

        // 描画：その他

        /// <summary>
        /// 画面を白で塗りつぶします。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearScreen() => Graphic.ClearScreen();

        /// <summary>
        /// 画面を任意の色で塗りつぶします。
        /// </summary>
        /// <param name="color">塗りつぶしの色</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillScreen(in Color color) => Graphic.FillScreen(color);

        /// <summary>
        /// 現在の画面を、画像として保存します。
        /// </summary>
        /// <param name="file">拡張子を除いたファイル名</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetResolution(in int width, in int height) => Graphic.SetResolution(width, height);

        /// <summary>
        /// UpdateGame や DrawGame が呼び出される時間間隔を設定します。
        /// </summary>
        /// <param name="targetDeltaTime">フレーム更新間隔の目標値（秒）</param>
        /// <param name="vSyncEnabled">垂直同期の有無</param>
        /// <remarks>
        /// 垂直同期を無効にした場合、間隔の揺らぎは減少しますが、ディスプレイのリフレッシュレートを常に無視して描画するため、画面のちらつきが発生する場合があります。
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFrameInterval(in double targetDeltaTime, in bool vSyncEnabled = true)
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFrameRate(in int targetFrameRate, bool vSyncEnabled = true)
        {
            SetFrameInterval(1d / targetFrameRate, vSyncEnabled);
        }

        /// <summary>
        /// 端末スクリーン解像度
        /// </summary>
        public float2 DeviceScreenSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Graphic.DeviceScreenSize;
        }

        /// <summary>
        /// 端末スクリーン解像度（X方向）
        /// </summary>
        public int DeviceScreenWidth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)Graphic.DeviceScreenSize.x;
        }

        /// <summary>
        /// 端末スクリーン解像度（Y方向）
        /// </summary>
        public int DeviceScreenHeight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)Graphic.DeviceScreenSize.y;
        }

        /// <summary>
        /// 画面サイズ
        /// </summary>
        public Rect CanvasRect
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new Rect(new float2(0, 0), Graphic.CanvasSize);
        }

        /// <summary>
        /// 画面サイズ
        /// </summary>
        public Box CanvasAABB
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new Box(Graphic.CanvasSize * 0.5f, Graphic.CanvasSize * 0.5f);
        }

        /// <summary>
        /// 画面サイズ
        /// </summary>
        public float2 CanvasSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Graphic.CanvasSize;
        }

        /// <summary>
        /// 画面の幅
        /// </summary>
        public int CanvasWidth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)Graphic.CanvasSize.x;
        }

        /// <summary>
        /// 画面の高さ
        /// </summary>
        public int CanvasHeight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)Graphic.CanvasSize.y;
        }

        /// <summary>
        /// 垂直同期の有無
        /// </summary>
        /// <remarks>
        /// この設定は、<see cref="SetFrameInterval"/> や <see cref="SetFrameRate"/> の第二引数から変更できます。
        /// </remarks>
        public bool VSyncEnabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_VSyncEnabled;
        }

        /// <summary>
        /// フレーム更新間隔の目標値（秒）
        /// </summary>
        public double TargetFrameInterval
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_TargetFrameInterval;
        }

        /// <summary>
        /// フレームレート（1秒あたりのフレーム数）の目標値
        /// </summary>
        public int TargetFrameRate
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)(1d / m_TargetFrameInterval);
        }

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PlaySound(in int soundId, in bool loop = false, in ESoundTrack track = ESoundTrack.BGM1) => Sound.Play(soundId, loop, track);

        /// <summary>
        /// 効果音を1回再生します。
        /// </summary>
        /// <param name="soundId">サウンドID（snd0.wav なら 0, snd1.mp3 なら 1）</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PlaySE(in int soundId) => Sound.PlaySE(soundId);

        /// <summary>
        /// 指定された音声トラックの音量を変更します。
        /// </summary>
        /// <param name="volume">音量（0～100）</param>
        /// <param name="track">対象の音声トラック</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSoundVolume(in int volume, in ESoundTrack track = ESoundTrack.BGM1) => Sound.SetVolume(volume, track);

        /// <summary>
        /// 指定された音声トラックの音量を変更します。
        /// </summary>
        /// <param name="decibel">音量 (-80〜20dB)</param>
        /// <param name="track">対象の音声トラック</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSoundDecibel(float decibel, in ESoundTrack track = ESoundTrack.BGM1) => Sound.SetVolume(decibel, track);

        /// <summary>
        /// 指定された音声トラックのサウンドを停止します。BGMトラック以外では常に無視されます。
        /// </summary>
        /// <param name="track">対象の音声トラック</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StopSound(in ESoundTrack track = ESoundTrack.BGM1) => Sound.Stop(track);

        /// <summary>
        /// 指定された音声トラックのサウンドを一時停止します。BGMトラック以外では常に無視されます。
        /// </summary>
        /// <param name="track">対象の音声トラック</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PauseSound(in ESoundTrack track = ESoundTrack.BGM1) => Sound.Pause(track);

        /// <summary>
        /// 指定された音声トラックのサウンドを一時停止していた場合、再生を再開します。BGMトラック以外では常に無視されます。
        /// </summary>
        /// <param name="track">対象の音声トラック</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnpauseSound(in ESoundTrack track = ESoundTrack.BGM1) => Sound.Unpause(track);

        // キー入力

        /// <summary>
        /// バックボタンが押されているかどうか（Androidのみ）
        /// </summary>
        public bool IsPressBackButton
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Keyboard.IsPressBackButton;
        }

        /// <summary>
        /// キーが押されているフレーム数（押された瞬間を1フレーム目とする経過フレーム数）を取得します。
        /// </summary>
        /// <param name="key">キー番号</param>
        /// <returns>フレーム数（押されていない場合は常に0を返します）</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetKeyPressFrameCount(in EKeyCode key) => Keyboard.GetPressFrameCount(key);

        /// <summary>
        /// キーが押されている時間（押された瞬間からの経過時間）を取得します。
        /// </summary>
        /// <param name="key">キー番号</param>
        /// <returns>時間（秒。押されていない場合と押された瞬間は0を返します）</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetKeyPressDuration(in EKeyCode key) => Keyboard.GetPressDuration(key);

        /// <summary>
        /// キーが押されているかどうかを取得します。
        /// </summary>
        /// <param name="key">キー番号</param>
        /// <returns>押されているかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetIsKeyPress(in EKeyCode key) => Keyboard.GetIsPress(key);

        /// <summary>
        /// キーが押された瞬間どうかを取得します。
        /// </summary>
        /// <param name="key">キー番号</param>
        /// <returns>押された瞬間かどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetIsKeyBegan(in EKeyCode key) => Keyboard.GetIsBegan(key);

        /// <summary>
        /// キーが離された瞬間どうかを取得します。
        /// </summary>
        /// <param name="key">キー番号</param>
        /// <returns>離された瞬間かどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetIsKeyEnded(in EKeyCode key) => Keyboard.GetIsEnded(key);

        /// <summary>
        /// スクリーンキーボードを表示します。（実機のみ）
        /// </summary>
        /// <returns>正常に表示できたかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ShowScreenKeyboard() => Keyboard.Open();

        // ポインタ入力

        /// <summary>
        /// 有効なポインタイベントの数を取得します。
        /// </summary>
        public int PointerCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Pointer.Count;
        }

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
        public bool HasPointerEvent
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Pointer.HasEvent;
        }

        /// <summary>
        /// タップされた瞬間かどうかを取得します。
        /// </summary>
        /// <returns>タップされた瞬間かどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTapped() => Pointer.IsTapped();

        /// <summary>
        /// タップされた瞬間かどうかを取得します。
        /// </summary>
        /// <param name="x">タップされた箇所のX座標</param>
        /// <param name="y">タップされた箇所のY座標</param>
        /// <returns>タップされた瞬間かどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTapped(out int x, out int y) => Pointer.IsTapped(out x, out y);

        /// <summary>
        /// タップされた瞬間かどうかを取得します。
        /// </summary>
        /// <param name="point">タップされた座標</param>
        /// <returns>タップされた瞬間かどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTapped(out float2 point) => Pointer.IsTapped(out point);

        /// <summary>
        /// 指定された領域がタップされた瞬間かどうかを取得します。
        /// </summary>
        /// <param name="x">タップ判定領域のX座標</param>
        /// <param name="y">タップ判定領域のY座標</param>
        /// <param name="w">タップ判定領域の横幅</param>
        /// <param name="h">タップ判定領域の縦幅</param>
        /// <returns>指定された領域がタップされた瞬間かどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTapped(in int x, in int y, in int w, in int h) => Pointer.IsTapped(new Rect(x, y, w, h));

        /// <summary>
        /// 指定された領域がタップされた瞬間かどうかを取得します。
        /// </summary>
        /// <param name="area">タップ判定領域</param>
        /// <returns>指定された領域がタップされた瞬間かどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTapped(in Rect area) => Pointer.IsTapped(area);

        /// <summary>
        /// ポインタイベントを取得します。
        /// </summary>
        /// <param name="i">ポインタ通し番号（0～<see cref="PointerCount"/>-1）</param>
        /// <returns>ポインタイベント</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PointerEvent GetPointerEvent(in int i) => Pointer.GetEvent(i);

        /// <summary>
        /// ポインタイベントの取得を試みます。
        /// </summary>
        /// <param name="i">ポインタ通し番号（0～<see cref="PointerCount"/>-1）</param>
        /// <param name="e">ポインタイベント</param>
        /// <returns>ポインタイベントが存在するかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetPointerEvent(in int i, out PointerEvent e) => Pointer.TryGetEvent(i, out e);

        /// <summary>
        /// ポインタイベントの取得を試みます。マルチタッチを扱う場合は <see cref="TryGetPointerEvent"/> を使用してください。
        /// </summary>
        /// <param name="e">ポインタイベント</param>
        /// <returns>ポインタイベントが存在するかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetPrimaryPointerEvent(out PointerEvent e) => Pointer.TryGetEvent(0, out e);

        /// <summary>
        /// ポインタのX座標を取得します。
        /// </summary>
        /// <param name="i">ポインタ通し番号（0～<see cref="PointerCount"/>-1）</param>
        /// <returns>X座標</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPointerX(in int i) => Pointer.GetX(i);

        /// <summary>
        /// ポインタのY座標を取得します。
        /// </summary>
        /// <param name="i">ポインタ通し番号（0～<see cref="PointerCount"/>-1）</param>
        /// <returns>Y座標</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPointerY(in int i) => Pointer.GetY(i);

        /// <summary>
        /// ポインタが押された瞬間かどうかを取得します。
        /// </summary>
        /// <param name="i">ポインタ通し番号（0～<see cref="PointerCount"/>-1）</param>
        /// <returns>押された瞬間かどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetIsPointerBegan(in int i) => Pointer.GetIsBegan(i);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetIsPointerEnded(in int i) => Pointer.GetIsEnded(i);

        /// <summary>
        /// ポインタが押されているフレーム数（押された瞬間を1フレーム目とする経過フレーム数）を取得します。
        /// </summary>
        /// <param name="i">ポインタ通し番号（0～<see cref="PointerCount"/>-1）</param>
        /// <returns>押された瞬間を1フレーム目とする経過フレーム数（押されていない場合は常に0を返します）</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPointerFrameCount(in int i) => Pointer.GetFrameCount(i);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetPointerDuration(in int i) => Pointer.GetDulation(i);

        /// <summary>
        /// タップ感度を調整します。
        /// </summary>
        /// <param name="maxDuration">画面に触れてから離すまでの最長時間</param>
        /// <param name="maxDistance">ポインターの最大移動距離</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetTapSensitivity(in float maxDuration, in float maxDistance) => Pointer.SetTapSensitivity(maxDuration, maxDistance);

        // 数学

        /// <summary>
        /// 乱数の種をセットします。
        /// </summary>
        /// <param name="seed">シード</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSeed(in int seed) => Math.SetSeed(seed);

        /// <summary>
        /// ランダムな整数値（<paramref name="min"/> ≦ n ≦ <paramref name="max"/>）を算出します。
        /// </summary>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <returns><paramref name="min"/>以上<paramref name="max"/>以下のランダムな値</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Random(in int min, in int max) => Math.Random(min, max);

        /// <summary>
        /// ランダムな値（0 ≦ n ＜ 1）を算出します。
        /// </summary>
        /// <returns>0以上f未満のランダムな値</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Random() => Math.Random();

        /// <summary>
        /// 計算誤差を考慮してゼロかどうか判定します。
        /// </summary>
        /// <param name="value"></param>
        /// <returns>ゼロかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AlmostZero(in float value) => Math.AlmostZero(value);

        /// <summary>
        /// 計算誤差を考慮して同値かどうか判定します。
        /// </summary>
        /// <returns>同値かどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AlmostSame(in float a, in float b) => Math.AlmostSame(a, b);

        /// <summary>
        /// 値を四捨五入します。
        /// </summary>
        /// <returns>四捨五入された値</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Round(in double value) => Math.Round(value);

        /// <summary>
        /// 絶対値を計算します。
        /// </summary>
        /// <returns>絶対値</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Abs(in float value) => math.abs(value);

        /// <summary>
        /// 2つの値を比較して、小さい方を返します。
        /// </summary>
        /// <returns>小さい方の値</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Min(in float a, in float b) => math.min(a, b);

        /// <summary>
        /// 2つの値を比較して、大きい方を返します。
        /// </summary>
        /// <returns>大きい方の値</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Max(in float a, in float b) => math.max(a, b);

        /// <summary>
        /// 値が<paramref name="min"/>以上<paramref name="max"/>以下の範囲に収まるように加工します。
        /// </summary>
        /// <param name="value">入力値</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <returns>クランプされた値</returns>
        public float Clamp(in float value, in float min, in float max)
            => math.clamp(value, min, max);

        /// <summary>
        /// 平方根を計算します。
        /// </summary>
        /// <param name="value">平方根を求める値</param>
        /// <returns>平方根</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Sqrt(in float value) => Math.Sqrt(value);

        /// <summary>
        /// コサインを計算します。
        /// </summary>
        /// <param name="degree">角度（度数法）</param>
        /// <returns>コサイン</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Cos(in float degree) => Math.Cos(degree);

        /// <summary>
        /// サインを計算します。
        /// </summary>
        /// <param name="degree">角度（度数法）</param>
        /// <returns>サイン</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Sin(in float degree) => Math.Sin(degree);

        /// <summary>
        /// ベクトルの角度を計算します。
        /// </summary>
        /// <param name="x">ベクトルのX成分</param>
        /// <param name="y">ベクトルのY成分</param>
        /// <returns>ベクトルの角度（度数法）</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Atan2(in float x, in float y) => Math.Atan2(new float2(x, y));

        /// <summary>
        /// ベクトルの角度を計算します。
        /// </summary>
        /// <param name="v">ベクトル</param>
        /// <returns>ベクトルの角度（度数法）</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Atan2(in float2 v) => Math.Atan2(v);

        /// <summary>
        /// ベクトルを回転します。
        /// </summary>
        /// <param name="vector">回転前のベクトル</param>
        /// <param name="degree">回転量（度数法）</param>
        /// <returns>回転後のベクトル</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float2 Rotate(in float2 vector, in float degree) => Math.Rotate(vector, degree);

        /// <summary>
        /// ベクトルの内積を計算します。
        /// </summary>
        /// <param name="a">ベクトルA</param>
        /// <param name="b">ベクトルB</param>
        /// <returns>内積</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Dot(in float2 a, in float2 b) => Math.Dot(a, b);

        /// <summary>
        /// ベクトルの外積を計算します。
        /// </summary>
        /// <param name="a">ベクトルA</param>
        /// <param name="b">ベクトルB</param>
        /// <returns>外積</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Cross(in float2 a, in float2 b) => Math.Cross(a, b);

        // 物理

        /// <summary>
        /// 2つの矩形が重なり合っているかどうかを判定します。
        /// </summary>
        /// <param name="rect1">矩形1</param>
        /// <param name="rect2">矩形2</param>
        /// <returns>2つの矩形が重なり合っているかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HitTest(in Rect rect1, in Rect rect2) => Physics.HitTest(rect1, rect2);

        /// <summary>
        /// 2つの矩形領域が重なり合っているかどうかを判定します。
        /// </summary>
        /// <param name="box1">矩形1</param>
        /// <param name="box2">矩形2</param>
        /// <returns>2つの矩形が重なり合っているかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HitTest(in Box box1, in Box box2) => Physics.HitTest(box1, box2);

        /// <summary>
        /// 2つの矩形が重なり合っているかどうかを判定します。
        /// </summary>
        /// <param name="pos1">矩形1の座標(左上)</param>
        /// <param name="size1">矩形1の大きさ</param>
        /// <param name="pos2">矩形2の座標(左上)</param>
        /// <param name="size2">矩形2の大きさ</param>
        /// <returns>2つの矩形が重なり合っているかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HitTest(in float2 pos1, in float2 size1, in float2 pos2, in float2 size2)
            => Physics.HitTest(new Rect(pos1, size1), new Rect(pos2, size2));

        /// <summary>
        /// 2つの矩形が重なり合っているかどうかを判定します。
        /// </summary>
        /// <param name="x1">矩形1の左上X座標</param>
        /// <param name="y1">矩形1の左上Y座標</param>
        /// <param name="w1">矩形1の幅</param>
        /// <param name="h1">矩形1の高さ</param>
        /// <param name="x2">矩形2の左上X座標</param>
        /// <param name="y2">矩形2の左上Y座標</param>
        /// <param name="w2">矩形2の幅</param>
        /// <param name="h2">矩形2の高さ</param>
        /// <returns>2つの矩形が重なり合っているかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HitTest(in float x1, in float y1, in float w1, in float h1, in float x2, in float y2, in float w2, in float h2)
            => Physics.HitTest(new Rect(x1, y1, w1, h1), new Rect(x2, y2, w2, h2));

        /// <summary>
        /// 2つの円が重なり合っているかどうかを判定します。
        /// </summary>
        /// <param name="circle1">円1</param>
        /// <param name="circle2">円2</param>
        /// <returns>2つの円が重なり合っているかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HitTest(in Circle circle1, in Circle circle2) => Physics.HitTest(circle1, circle2);

        /// <summary>
        /// 2つの円が重なり合っているかどうかを判定します。
        /// </summary>
        /// <param name="center1">円1の座標(中心)</param>
        /// <param name="radius1">円1の半径</param>
        /// <param name="center2">円2の座標(中心)</param>
        /// <param name="radius2">円2の半径</param>
        /// <returns>2つの円が重なり合っているかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HitTest(in float2 center1, float radius1, in float2 center2, float radius2)
            => Physics.HitTest(new Circle(center1, radius1), new Circle(center2, radius2));

        /// <summary>
        /// 2つの円が重なり合っているかどうかを判定します。
        /// </summary>
        /// <param name="x1">円1の中心X座標</param>
        /// <param name="y1">円1の中心Y座標</param>
        /// <param name="r1">円1の半径</param>
        /// <param name="x2">円2の中心X座標</param>
        /// <param name="y2">円2の中心Y座標</param>
        /// <param name="r2">円2の半径</param>
        /// <returns>2つの円が重なり合っているかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HitTest(in float x1, in float y1, in float r1, in float x2, in float y2, in float r2)
            => Physics.HitTest(new Circle(x1, y1, r1), new Circle(x2, y2, r2));

        /// <summary>
        /// 矩形と点が重なり合っているかどうかを判定します。
        /// </summary>
        /// <param name="rect">矩形</param>
        /// <param name="point">点</param>
        /// <returns>矩形と点が重なり合っているかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HitTest(in Rect rect, in float2 point) => Physics.HitTest(rect, point);

        /// <summary>
        /// 矩形領域と点が重なり合っているかどうかを判定します。
        /// </summary>
        /// <param name="box">矩形</param>
        /// <param name="point">点</param>
        /// <returns>矩形領域と点が重なり合っているかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HitTest(in Box box, in float2 point) => Physics.HitTest(box, point);

        /// <summary>
        /// 円と点が重なり合っているかどうかを判定します。
        /// </summary>
        /// <param name="circle">円</param>
        /// <param name="point">点</param>
        /// <returns>円と点が重なり合っているかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HitTest(in Circle circle, in float2 point) => Physics.HitTest(circle, point);

        /// <summary>
        /// 2つの線分が交差しているかどうかを判定します。
        /// </summary>
        /// <param name="seg1">線分1</param>
        /// <param name="seg2">線分2</param>
        /// <returns>交差しているかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CrossTest(in Segment seg1, in Segment seg2) => Physics.CrossTest(seg1, seg2);

        /// <summary>
        /// 2つの線分が交差しているかどうかを判定します。
        /// </summary>
        /// <param name="seg1">線分1</param>
        /// <param name="seg2">線分2</param>
        /// <param name="intersection">交点の座標</param>
        /// <returns>交差しているかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CrossTest(in Segment seg1, in Segment seg2, out float2 intersection)
            => Physics.CrossTest(seg1, seg2, out intersection);

        /// <summary>
        /// 線分と直線が交差しているかどうかを判定します。
        /// </summary>
        /// <param name="seg">線分</param>
        /// <param name="line">直線</param>
        /// <returns>交差しているかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CrossTest(in Segment seg, in Line line) => Physics.CrossTest(seg, line);

        /// <summary>
        /// 線分と直線が交差しているかどうかを判定します。
        /// </summary>
        /// <param name="seg">線分</param>
        /// <param name="line">直線</param>
        /// <param name="intersection">交点の座標</param>
        /// <returns>交差しているかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CrossTest(in Segment seg, in Line line, out float2 intersection)
            => Physics.CrossTest(seg, line, out intersection);

        /// <summary>
        /// 2つの直線が交差しているかどうかを判定します。
        /// </summary>
        /// <param name="line1">直線1</param>
        /// <param name="line2">直線2</param>
        /// <returns>交差しているかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CrossTest(in Line line1, in Line line2) => Physics.CrossTest(line1, line2);

        /// <summary>
        /// 2つの直線が交差しているかどうかを判定します。
        /// </summary>
        /// <param name="line1">直線1</param>
        /// <param name="line2">直線2</param>
        /// <param name="intersection">交点の座標</param>
        /// <returns>交差しているかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CrossTest(in Line line1, in Line line2, out float2 intersection)
            => Physics.CrossTest(line1, line2, out intersection);

        /// <summary>
        /// 静的な矩形と動く点とで連続衝突検出(CCD)を行います。
        /// </summary>
        /// <param name="box">静的な矩形</param>
        /// <param name="point">点の初期位置</param>
        /// <param name="delta">点の移動量</param>
        /// <param name="hit">衝突検出の結果</param>
        /// <returns>衝突したかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SweepTest(in Box box, in float2 point, in float2 delta, out HitResult hit)
            => Physics.SweepTest(box, point, delta, out hit);

        /// <summary>
        /// 静的な矩形Aと動く矩形Bとで連続衝突検出(CCD)を行います。
        /// </summary>
        /// <param name="a">矩形A</param>
        /// <param name="point">矩形Bの初期位置</param>
        /// <param name="delta">矩形Bの移動量</param>
        /// <param name="hit">衝突検出の結果</param>
        /// <returns>衝突したかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SweepTest(in Box a, in Box b, in float2 delta, out HitResult hit)
            => Physics.SweepTest(a, b, delta, out hit);

        // 時間（日時、フレーム）

        /// <summary>
        /// 現在フレームのアプリ起動からの経過時間（秒）
        /// </summary>
        public float TimeSinceStartup
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Time.SinceStartup;
        }

        /// <summary>
        /// ひとつ前のフレームからの経過時間（秒）
        /// </summary>
        public float TimeSincePrevFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Time.SincePrevFrame;
        }

        /// <summary>
        /// 現在フレームの日付の西暦部分
        /// </summary>
        public int CurrentYear
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Time.Year;
        }

        /// <summary>
        /// 現在フレームの日付の月部分（1～12）
        /// </summary>
        public int CurrentMonth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Time.Month;
        }

        /// <summary>
        /// 現在フレームの日付（1～31）
        /// </summary>
        public int CurrentDay
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Time.Day;
        }

        /// <summary>
        /// 現在フレームの曜日（0～6）
        /// </summary>
        public System.DayOfWeek CurrentDayOfWeek
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Time.DayOfWeek;
        }

        /// <summary>
        /// 現在フレームの時刻の時間部分（0～23）
        /// </summary>
        public int CurrentHour
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Time.Hour;
        }

        /// <summary>
        /// 現在フレームの時刻の分部分（0～59）
        /// </summary>
        public int CurrentMinute
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Time.Minute;
        }

        /// <summary>
        /// 現在フレームの時刻の秒部分（0～59）
        /// </summary>
        public int CurrentSecond
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Time.Second;
        }

        /// <summary>
        /// 現在フレームの時刻のミリ秒部分（0～999）
        /// </summary>
        public int CurrentMillisecond
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Time.Millisecond;
        }

        /// <summary>
        /// 現在フレームのUnixタイムスタンプ
        /// </summary>
        public long CurrentTimestamp
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Time.Timestamp;
        }

        /// <summary>
        /// アプリ起動からの累計フレーム数
        /// </summary>
        public int CurrentFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Time.FrameCount;
        }

        /// <summary>
        /// 現在フレームの日時
        /// </summary>
        public System.DateTimeOffset CurrentTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Time.Current;
        }

        /// <summary>
        /// 現在（関数呼び出し時点）の日時
        /// </summary>
        public System.DateTimeOffset NowTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Time.Now;
        }

#if !GC_DISABLE_ACCELEROMETER
        // 加速度計

        /// <summary>
        /// 最後に測定されたX軸加速度
        /// </summary>
        public float AccelerationLastX
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Accelerometer.LastX;
        }

        /// <summary>
        /// 最後に測定されたY軸加速度
        /// </summary>
        public float AccelerationLastY
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Accelerometer.LastY;
        }

        /// <summary>
        /// 最後に測定されたZ軸加速度
        /// </summary>
        public float AccelerationLastZ
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Accelerometer.LastZ;
        }

        /// <summary>
        /// 前のフレーム以降に測定された加速度の数
        /// </summary>
        public int AccelerationEventCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Accelerometer.EventCount;
        }

        /// <summary>
        /// X軸加速度を取得します。
        /// </summary>
        /// <param name="i">測定番号（0 以上 <see cref="AccelerationEventCount"/> 未満）</param>
        /// <param name="normalize">加速度を正規化するかどうか</param>
        /// <returns>X軸加速度</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetAccelerationX(in int i, in bool normalize = false)
        {
            return normalize ? Accelerometer.GetNormalizedX(i) : Accelerometer.GetX(i);
        }

        /// <summary>
        /// Y軸加速度を取得します。
        /// </summary>
        /// <param name="i">測定番号（0 以上 <see cref="AccelerationEventCount"/> 未満）</param>
        /// <param name="normalize">加速度を正規化するかどうか</param>
        /// <returns>Y軸加速度</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetAccelerationY(in int i, in bool normalize = false)
        {
            return normalize ? Accelerometer.GetNormalizedY(i) : Accelerometer.GetY(i);
        }

        /// <summary>
        /// Z軸加速度を取得します。
        /// </summary>
        /// <param name="i">測定番号（0 以上 <see cref="AccelerationEventCount"/> 未満）</param>
        /// <param name="normalize">加速度を正規化するかどうか</param>
        /// <returns>Z軸加速度</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetAccelerationZ(in int i, in bool normalize = false)
        {
            return normalize ? Accelerometer.GetNormalizedZ(i) : Accelerometer.GetZ(i);
        }
#endif //!GC_DISABLE_ACCELEROMETER

#if !GC_DISABLE_GEOLOCATION
        // 位置情報

        /// <summary>
        /// 位置情報サービスが現在動作しているかどうか
        /// </summary>
        public bool IsGeolocationRunning
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Geolocation.IsRunning;
        }

        /// <summary>
        /// 位置情報サービスの利用を許可されているかどうか
        /// </summary>
        public bool HasGeolocationPermission
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Geolocation.HasPermission;
        }

        /// <summary>
        /// 現在のフレームで位置情報が更新されたかどうか
        /// </summary>
        public bool HasGeolocationUpdate
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Geolocation.HasUpdate;
        }

        /// <summary>
        /// 最後の位置情報の高度
        /// </summary>
        public float GeolocationLastAltitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Geolocation.LastAltitude;
        }

        /// <summary>
        /// 最後の位置情報の緯度
        /// </summary>
        public float GeolocationLastLatitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Geolocation.LastLatitude;
        }

        /// <summary>
        /// 最後の位置情報の経度
        /// </summary>
        public float GeolocationLastLongitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Geolocation.LastLongitude;
        }

        /// <summary>
        /// 最後の位置情報の更新日時
        /// </summary>
        public System.DateTimeOffset GeolocationLastTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Geolocation.LastTime;
        }

        /// <summary>
        /// 位置情報サービスの状態
        /// </summary>
        public LocationServiceStatus GeolocationStatus
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Geolocation.Status;
        }

        /// <summary>
        /// 位置情報サービスを開始します。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartGeolocationService() => Geolocation.StartService();

        /// <summary>
        /// 位置情報サービスを終了します。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StopGeolocationService() => Geolocation.StopService();
#endif //!GC_DISABLE_GEOLOCATION

#if !GC_DISABLE_CAMERAINPUT
        // カメラ映像入力

        /// <summary>
        /// 検出されたカメラ映像入力デバイスの数
        /// </summary>
        public int CameraDeviceCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CameraDevice.Count;
        }

        /// <summary>
        /// 再生中のカメラ映像の幅
        /// </summary>
        public int CurrentCameraWidth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CameraDevice.CurrentWidth;
        }

        /// <summary>
        /// 再生中のカメラ映像の高さ
        /// </summary>
        public int CurrentCameraHeight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CameraDevice.CurrentHeight;
        }

        /// <summary>
        /// 再生中のカメラ映像入力のデバイス名
        /// </summary>
        public string CurrentCameraName
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CameraDevice.CurrentDeviceName;
        }

        /// <summary>
        /// 再生中のカメラ映像の必要回転角度 (度数法)
        /// </summary>
        public int CurrentCameraRotation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CameraDevice.CurrentRotation;
        }

        /// <summary>
        /// 再生中のカメラ映像の上下反転有無
        /// </summary>
        public bool IsCameraMirrored
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CameraDevice.IsMirrored;
        }

        /// <summary>
        /// 再生中のカメラ映像が現在フレームで更新されたかどうか
        /// </summary>
        public bool DidCameraUpdate
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CameraDevice.DidUpdate;
        }

        /// <summary>
        /// カメラ映像入力デバイスの一覧を更新します。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateCameraDeviceList() => CameraDevice.UpdateList();

        /// <summary>
        /// 指定されたカメラ映像入力のデバイス名を取得します。
        /// </summary>
        /// <param name="deviceIndex">カメラデバイス通し番号 (0以上<see cref="CameraDeviceCount"/>未満)</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetCameraDeviceName(in int deviceIndex) => CameraDevice.GetName(deviceIndex);

        /// <summary>
        /// 指定したカメラ映像入力が前面カメラかどうか
        /// </summary>
        /// <param name="deviceIndex">カメラデバイス通し番号 (0以上<see cref="CameraDeviceCount"/>未満)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetIsFrontCamera(in int deviceIndex) => CameraDevice.GetIsFront(deviceIndex);

        /// <summary>
        /// カメラ映像入力の選択と開始
        /// </summary>
        /// <param name="deviceIndex">カメラデバイス通し番号 (0以上<see cref="CameraDeviceCount"/>未満)</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartCameraService(in int deviceIndex = 0) { CameraDevice.Start(deviceIndex); }

        /// <summary>
        /// カメラ映像入力の停止
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StopCameraService() => CameraDevice.Stop();

        /// <summary>
        /// カメラ映像入力の一時停止
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PauseCameraService() => CameraDevice.Pause();

        /// <summary>
        /// カメラ映像入力の再開
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnpauseCameraService() => CameraDevice.Unpause();
#endif //!GC_DISABLE_CAMERAINPUT

        // シーンとアクター

        /// <summary>
        /// 新たなシーンを登録します。登録したシーンは <see cref="ChangeScene"/> を呼び出すことで有効になります。
        /// </summary>
        /// <typeparam name="T">登録するシーンの型</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterScene<T>() where T : Scene, new()
        {
            SceneDict.Add(typeof(T), new T());
        }

        /// <summary>
        /// 指定したシーンを登録します。登録したシーンは <see cref="ChangeScene"/> を呼び出すことで有効になります。
        /// </summary>
        /// <param name="scene">登録するシーン</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterScene(Scene scene)
        {
            SceneDict.Add(scene.GetType(), scene);
        }

        /// <summary>
        /// 指定したシーンをシーン一覧から削除します。
        /// 
        /// もし指定したシーンが現在有効なシーンだった場合、フレームの最後にシーンの離脱処理が走ります。
        /// </summary>
        /// <typeparam name="T">削除するシーンの型</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnregisterScene<T>() where T : Scene
        {
            var key = typeof(T);
            if (SceneDict.TryGetValue(key, out var scene))
            {
                SceneDict.Remove(key);

                if (m_CurrentScene == scene)
                {
                    m_SceneLeaveFlag = true;
                }
            }
        }

        /// <summary>
        /// 指定したシーンをシーン一覧から削除します。
        /// 
        /// もし指定したシーンが現在有効なシーンだった場合、フレームの最後にシーンの離脱処理が走ります。
        /// </summary>
        /// <param name="scene">削除するシーン</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnregisterScene(Scene scene)
        {
            if (SceneDict.ContainsValue(scene))
            {
                SceneDict.Remove(scene.GetType());

                if (m_CurrentScene == scene)
                {
                    m_SceneLeaveFlag = true;
                }
            }
        }

        /// <summary>
        /// シーンを切り替えます。
        /// 
        /// これまで有効だったシーンは、現在のフレームの最後に終了処理が実行されます。
        /// これから有効になるシーンは、次のフレームの最初に開始処理が実行されます。
        /// </summary>
        /// <typeparam name="T">開始するシーンの型</typeparam>
        /// <param name="state">シーンの開始処理 (<see cref="IScene.EnterScene"/>) に引数として渡す任意の値</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeScene<T>(object state = null) where T : Scene
        {
            if (m_CurrentScene != null)
            {
                m_SceneLeaveFlag = true;
            }

            if (SceneDict.TryGetValue(typeof(T), out var nextScene))
            {
                m_SceneEnterFlag = true;
                m_NextScene = nextScene;
                m_NextSceneState = state;
            }
        }

        /// <summary>
        /// 新たなアクターを生成し、現在のシーンに登録します。
        /// </summary>
        /// <returns>新たなアクター</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T CreateActor<T>() where T : Actor, new() => m_CurrentScene.CreateActor<T>();

        /// <summary>
        /// 指定したアクターを現在のシーンに登録します。
        /// </summary>
        /// <param name="actor">登録するアクター</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddActor(Actor actor) => m_CurrentScene.AddActor(actor);

        /// <summary>
        /// 指定したアクターを現在のシーンから登録解除します。
        /// </summary>
        /// <param name="actor">登録解除するアクター</param>
        /// <returns>登録解除できたかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveActor(Actor actor) => m_CurrentScene.TryRemoveActor(actor);

        /// <summary>
        /// 現在のシーンに登録されているすべてのアクターを登録解除します。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveActorAll() => m_CurrentScene.RemoveActorAll();

        /// <summary>
        /// シーンに登録されているアクターのうち、指定した型のアクターを1つだけ取得します。
        /// </summary>
        /// <typeparam name="T">取得するアクターの型</typeparam>
        /// <returns>取得できたアクター</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetActor<T>() where T : Actor => m_CurrentScene.GetActor<T>();

        /// <summary>
        /// シーンに登録されているアクターのうち、指定した型のアクターを1つだけ取得します。
        /// </summary>
        /// <param name="actor">取得できたアクター</param>
        /// <typeparam name="T">取得するアクターの型</typeparam>
        /// <returns>取得できたかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetActor<T>(out T actor) where T : Actor => m_CurrentScene.TryGetActor(out actor);

        /// <summary>
        /// シーンに登録されているアクターの総数を取得します。
        /// </summary>
        /// <returns>シーンに登録されているアクターの総数</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetActorCount() => m_CurrentScene.GetActorCount();

        /// <summary>
        /// シーンに登録されているアクターのうち、指定した型のものが幾つあるか。
        /// </summary>
        /// <typeparam name="T">取得するアクターの型</typeparam>
        /// <returns>登録数</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetActorCount<T>() where T : Actor => m_CurrentScene.GetActorCount<T>();

        /// <summary>
        /// シーンに登録されているアクターのうち、指定した型のものを取得します。
        /// </summary>
        /// <typeparam name="T">取得するアクターの型</typeparam>
        /// <returns>取得できたアクターのリスト</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Scene.ReadOnlyActorList<T> GetActorList<T>() where T : Actor => m_CurrentScene.GetActorList<T>();

        // その他

        /// <summary>
        /// ローカルストレージに保存された値を取り出します。
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">保存された値</param>
        /// <returns>取り出しに成功したかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearDownloadCache() => Network.Clear();

        /// <summary>
        /// 初期状態にします。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetGame()
        {
            Sound.Reset();
            Graphic.ClearScreen();
        }
        /// <summary>
        /// アプリを終了します。（Androidのみ）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        public int Width
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)Graphic.CanvasSize.x;
        }
        [Hidden(HiddenState.Never), System.Obsolete("gc.CanvasWHeight")]
        public int Height
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)Graphic.CanvasSize.y;
        }
        [Hidden(HiddenState.Never), System.Obsolete, System.Diagnostics.Conditional("ENABLE_GAMECANVAS_JAVA")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetWindowTitle(string title) { }
        [Hidden(HiddenState.Never), System.Obsolete, System.Diagnostics.Conditional("ENABLE_GAMECANVAS_JAVA")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFont(string fontName, in int fontStyle, in int fontSize) { }
        [Hidden(HiddenState.Never), System.Obsolete("gc.PlaySE()")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PlaySE(in int soundId, bool loop) => Sound.PlaySE(soundId);

        [Hidden(HiddenState.Never), System.Obsolete("gc.ChangeVolume()")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeSEVolume(in int volume) => Sound.SetVolume(volume, ESoundTrack.SE);
        [Hidden(HiddenState.Never), System.Obsolete("gc.Stop()")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StopSE() { }
        [Hidden(HiddenState.Never), System.Obsolete("gc.Pause()")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PauseSE() { }
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetPointerX(0)")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetMouseX() => Pointer.LastX;
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetPointerY(0)")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetMouseY() => Pointer.LastY;
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetPointerFrameCount(0)")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetMouseClickLength() => Pointer.FrameCount;
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetIsPointerBegan(0)")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMousePushed() => Pointer.IsBegan;
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetIsPointerEnded(0)")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMouseReleased() => Pointer.IsEnded;
        [Hidden(HiddenState.Never), System.Obsolete("gc.HasPointerEvent")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMousePress() => Pointer.HasEvent;
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetKeyPressFrameCount")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetKeyPressLength(EKeyCode key)
        {
            return Keyboard.GetIsEnded(key) ? -1 : Keyboard.GetPressFrameCount(key);
        }
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetIsKeyPress")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKeyPress(EKeyCode key) => Keyboard.GetIsPress(key);
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetIsKeyBegan")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKeyPushed(EKeyCode key) => Keyboard.GetIsBegan(key);
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetIsKeyEnded")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKeyReleased(EKeyCode key) => Keyboard.GetIsEnded(key);

        [Hidden(HiddenState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PlayBGM(in int midiId, bool loop = true) { } // TODO
        [Hidden(HiddenState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeBGMVolume(in int volume) { } // TODO
        [Hidden(HiddenState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StopBGM() { } // TODO
        [Hidden(HiddenState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PauseBGM() { } // TODO
        [Hidden(HiddenState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ShowYesNoDialog(string message) => false; // TODO
        [Hidden(HiddenState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ShowInputDialog(string message, string defaultInput) => null; // TODO

        [Hidden(HiddenState.Never), System.Obsolete("gc.TryLoad")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Load(in int key) => PlayerPrefs.GetInt(key.ToString(), 0);
        [Hidden(HiddenState.Never), System.Obsolete("gc.Save(string, int)")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Save(in int key, in int value) => PlayerPrefs.SetInt(key.ToString(), value);

        [System.Obsolete("gc.HitTest")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckHitRect(in int x1, in int y1, in int w1, in int h1, in int x2, in int y2, in int w2, in int h2)
            => HitTest(x1, y1, w1, h1, x2, y2, w2, h2);
        [System.Obsolete()]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckHitImage(in int imageId1, in int x1, in int y1, in int imageId2, in int x2, in int y2)
            => Physics.CheckHitImage(Resource, imageId1, x1, y1, imageId2, x2, y2);
        [System.Obsolete("gc.HitTest")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CheckHitCircle(in int x1, in int y1, in int r1, in int x2, in int y2, in int r2)
            => HitTest(x1, y1, r1, x2, y2, r2);

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
            Physics = new Physics();
            Network = new Network(bhvr, Graphic);
            Pointer = new Pointer(Time, Graphic);
            Keyboard = new Keyboard();
            Accelerometer = new Accelerometer();
            Geolocation = new Geolocation();
            CameraDevice = new CameraDevice(Graphic);

            Scene.Inject(this);
            Actor.Inject(this);

            Math.ResetSeed();
            Resource = bhvr.Resource;
            SceneDict = new Dictionary<System.Type, Scene>();
            m_TargetFrameInterval = 1f / 60;
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

        internal void UpdateCurrentScene()
        {
            if (m_SceneEnterFlag)
            {
                if (m_NextScene != null)
                {
                    m_CurrentScene = m_NextScene;
                    m_CurrentScene.EnterScene(m_NextSceneState);
                    m_NextScene = null;
                    m_NextSceneState = null;
                }
                m_SceneEnterFlag = false;
            }

            m_CurrentScene?.Update();
        }

        internal void DrawCurrentScene()
        {
            m_CurrentScene?.Draw();
        }

        internal void OnAterDraw()
        {
            if (m_SceneLeaveFlag)
            {
                m_CurrentScene?.LeaveScene();
                m_CurrentScene = null;
                m_SceneLeaveFlag = false;
            }
        }

        #endregion
    }
}
