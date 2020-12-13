/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using System.ComponentModel;
using Unity.Mathematics;
using UnityEngine;

namespace GameCanvas
{
    public interface IGraphics
    {
        /// <summary>
        /// 背景色
        /// </summary>
        Color BackgroundColor { get; set; }

        /// <summary>
        /// キャンバス外に表示される帯の色
        /// </summary>
        /// <remarks>
        /// <see cref="ChangeBorderColor"/> を呼び出すことで変更できます
        /// </remarks>
        Color BorderColor { get; }

        /// <summary>
        /// キャンバス解像度
        /// </summary>
        /// <remarks>
        /// <see cref="ChangeCanvasSize"/> を呼び出すことで変更できます
        /// </remarks>
        int2 CanvasSize { get; }

        /// <summary>
        /// 円の解像度
        /// </summary>
        int CircleResolution { get; set; }

        /// <summary>
        /// 描画色
        /// </summary>
        Color Color { get; set; }

        /// <summary>
        /// 現在の座標系（変換行列）
        /// </summary>
        float2x3 CurrentCoordinate { get; set; }

        /// <summary>
        /// 現在のスタイル
        /// </summary>
        GcStyle CurrentStyle { get; set; }

        /// <summary>
        /// 端末スクリーン解像度
        /// </summary>
        int2 DeviceScreenSize { get; }

        /// <summary>
        /// フォント種別
        /// </summary>
        GcFont Font { get; set; }

        /// <summary>
        /// フォントサイズ
        /// </summary>
        int FontSize { get; set; }

        /// <summary>
        /// 描線の端点の形状
        /// </summary>
        GcLineCap LineCap { get; set; }

        /// <summary>
        /// 描線の太さ
        /// </summary>
        float LineWidth { get; set; }

        /// <summary>
        /// <see cref="PushCoordinate"/> と <see cref="PopCoordinate"/> が自動的に呼び出されるスコープ
        /// </summary>
        CoordianteScope CoordinateScope { get; }

        /// <summary>
        /// 矩形のアンカー位置
        /// </summary>
        GcAnchor RectAnchor { get; set; }

        /// <summary>
        /// 文字列のアンカー位置
        /// </summary>
        GcAnchor StringAnchor { get; set; }

        /// <summary>
        /// <see cref="PushStyle"/> と <see cref="PopStyle"/> が自動的に呼び出されるスコープ
        /// </summary>
        StyleScope StyleScope { get; }

        /// <summary>
        /// 文字列の縦幅を計算します
        /// </summary>
        /// <param name="str">文字列</param>
        /// <returns>縦幅</returns>
        float CalcStringHeight(in string str);

        /// <summary>
        /// 文字列のサイズを計算します
        /// </summary>
        /// <param name="str">文字列</param>
        /// <returns>サイズ</returns>
        float2 CalcStringSize(in string str);

        /// <summary>
        /// 文字列の横幅を計算します
        /// </summary>
        /// <param name="str">文字列</param>
        /// <returns>横幅</returns>
        float CalcStringWidth(in string str);

        /// <summary>
        /// キャンバス座標を端末スクリーン座標に変換します
        /// </summary>
        /// <param name="canvas">変換元 キャンバス座標</param>
        /// <param name="screen">変換後 端末スクリーン座標</param>
        void CanvasToScreenPoint(in float2 canvas, out float2 screen);

        /// <summary>
        /// キャンバス座標を端末スクリーン座標に変換します
        /// </summary>
        /// <param name="canvas">変換元 キャンバス座標</param>
        /// <param name="screen">変換後 端末スクリーン座標</param>
        void CanvasToScreenPoint(in float2 canvas, out int2 screen);

        /// <summary>
        /// キャンバス外の帯の色を変更します
        /// </summary>
        /// <remarks>
        /// 既存キャンバスの描画内容は全て破棄されます
        /// </remarks>
        /// <param name="color">新しい帯の色</param>
        void ChangeBorderColor(in Color color);

        /// <summary>
        /// キャンバス解像度を変更します
        /// </summary>
        /// <remarks>
        /// - 初期値は 720x1280 です<br />
        /// - ディスプレイ解像度と縦横比が異なる場合は、上下もしくは左右に帯がつきます<br />
        /// - 既存キャンバスの描画内容は全て破棄されます
        /// </remarks>
        /// <param name="size">新しいキャンバス解像度</param>
        void ChangeCanvasSize(in int2 size);

        /// <summary>
        /// <see cref="CurrentCoordinate"/> をリセットします
        /// </summary>
        void ClearCoordinate();

        /// <summary>
        /// キャンバスを <see cref="BackgroundColor"/> で塗りつぶします
        /// </summary>
        void ClearScreen();

        /// <summary>
        /// <see cref="CurrentStyle"/> をリセットします
        /// </summary>
        void ClearStyle();

        /// <summary>
        /// 中抜きの円を描画します
        /// </summary>
        void DrawCircle();

        /// <summary>
        /// 中抜きの円を描画します
        /// </summary>
        /// <param name="circle">円</param>
        void DrawCircle(in GcCircle circle);

        /// <summary>
        /// 画像を描画します
        /// </summary>
        /// <param name="image">描画する画像</param>
        void DrawImage(in GcImage image);

        /// <summary>
        /// 画像を描画します
        /// </summary>
        /// <param name="image">描画する画像</param>
        /// <param name="position">位置</param>
        /// <param name="degree">回転（度数法）</param>
        void DrawImage(in GcImage image, in float2 position, float degree = 0f);

        /// <summary>
        /// 画像を拡縮して描画します
        /// </summary>
        /// <param name="image">描画する画像</param>
        /// <param name="rect">画像をフィッティングする矩形領域</param>
        void DrawImage(in GcImage image, in GcRect rect);

        /// <summary>
        /// 線を描画します
        /// </summary>
        void DrawLine();

        /// <summary>
        /// 線を描画します
        /// </summary>
        /// <param name="line">線</param>
        void DrawLine(in GcLine line);

        /// <summary>
        /// 矩形を線で描画します
        /// </summary>
        void DrawRect();

        /// <summary>
        /// 矩形を線で描画します
        /// </summary>
        /// <param name="rect">矩形</param>
        void DrawRect(in GcRect rect);

        /// <summary>
        /// 文字列を描画します
        /// </summary>
        /// <param name="str">描画する文字列</param>
        void DrawString(in string str);

        /// <summary>
        /// 文字列を描画します
        /// </summary>
        /// <param name="str">描画する文字列</param>
        /// <param name="position">位置</param>
        /// <param name="degree">回転（度数法）</param>
        void DrawString(in string str, in float2 position, float degree = 0f);

        /// <summary>
        /// 文字列を拡縮して描画します
        /// </summary>
        /// <param name="str">描画する文字列</param>
        /// <param name="rect">文字列をフィッティングする矩形領域</param>
        void DrawString(in string str, in GcRect rect);

        /// <summary>
        /// テクスチャーを描画します
        /// </summary>
        /// <param name="texture">描画するテクスチャー</param>
        void DrawTexture(in Texture texture);

        /// <summary>
        /// テクスチャーを描画します
        /// </summary>
        /// <param name="texture">描画するテクスチャー</param>
        /// <param name="position">位置</param>
        /// <param name="degree">回転（度数法）</param>
        void DrawTexture(in Texture texture, in float2 position, float degree = 0f);

        /// <summary>
        /// テクスチャーを拡縮して描画します
        /// </summary>
        /// <param name="texture">描画するテクスチャー</param>
        /// <param name="rect">テクスチャーをフィッティングする矩形領域</param>
        void DrawTexture(in Texture texture, in GcRect rect);

        /// <summary>
        /// テクスチャーを変形して描画します
        /// </summary>
        /// <param name="texture">描画するテクスチャー</param>
        /// <param name="matrix">アフィン変換行列</param>
        void DrawTexture(in Texture texture, in float2x3 matrix);

        /// <summary>
        /// 円を塗りで描画します
        /// </summary>
        void FillCircle();

        /// <summary>
        /// 円を塗りで描画します
        /// </summary>
        /// <param name="circle">円</param>
        void FillCircle(in GcCircle circle);

        /// <summary>
        /// 矩形を塗りで描画します
        /// </summary>
        void FillRect();

        /// <summary>
        /// 矩形を塗りで描画します
        /// </summary>
        /// <param name="rect">矩形</param>
        void FillRect(in GcRect rect);

        /// <summary>
        /// スタックから座標系（変換行列）を取り出し <see cref="CurrentCoordinate"/> に上書きします
        /// </summary>
        void PopCoordinate();

        /// <summary>
        /// スタックから描画スタイルを取り出し <see cref="CurrentStyle"/> に上書きします
        /// </summary>
        void PopStyle();

        /// <summary>
        /// <see cref="CurrentCoordinate"/> をスタックに保存します
        /// </summary>
        void PushCoordinate();

        /// <summary>
        /// <see cref="CurrentStyle"/> をスタックに保存します
        /// </summary>
        void PushStyle();

        /// <summary>
        /// 座標系（変換行列）を回転させます
        /// </summary>
        /// <param name="degree">回転量（度数法）</param>
        void RotateCoordinate(in float degree);

        /// <summary>
        /// 座標系（変換行列）を指定した座標を中心に回転させます
        /// </summary>
        /// <param name="degree">回転量（度数法）</param>
        /// <param name="origin">回転中心</param>
        void RotateCoordinate(in float degree, in float2 origin);

        /// <summary>
        /// 座標系（変換行列）を拡縮させます
        /// </summary>
        /// <param name="scaling">拡縮率</param>
        void ScaleCoordinate(in float2 scaling);

        /// <summary>
        /// 端末スクリーン座標をキャンバス座標に変換します
        /// </summary>
        /// <param name="screen">変換元 端末スクリーン座標</param>
        /// <param name="canvas">変換後 キャンバス座標</param>
        void ScreenToCanvasPoint(in float2 screen, out float2 canvas);

        /// <summary>
        /// 端末スクリーン座標をキャンバス座標に変換します
        /// </summary>
        /// <param name="screen">変換元 端末スクリーン座標</param>
        /// <param name="canvas">変換後 キャンバス座標</param>
        void ScreenToCanvasPoint(in float2 screen, out int2 canvas);

        /// <summary>
        /// 座標系（変換行列）を平行移動させます
        /// </summary>
        /// <param name="translation">移動量</param>
        void TranslateCoordinate(in float2 translation);
    }

    public interface IGraphicsEx : IGraphics
    {
        /// <summary>
        /// キャンバスのAABB
        /// </summary>
        GcAABB CanvasAABB { get; }

        /// <summary>
        /// キャンバスの中心座標
        /// </summary>
        float2 CanvasCenter { get; }

        /// <summary>
        /// キャンバスの縦幅
        /// </summary>
        int CanvasHeight { get; }

        /// <summary>
        /// キャンバスの解像度とリフレッシュレート
        /// </summary>
        GcResolution CanvasResolution { get; }

        /// <summary>
        /// キャンバスの横幅
        /// </summary>
        int CanvasWidth { get; }

        /// <summary>
        /// 水色
        /// </summary>
        Color ColorAqua { get; }

        /// <summary>
        /// 黒色
        /// </summary>
        Color ColorBlack { get; }

        /// <summary>
        /// 青色
        /// </summary>
        Color ColorBlue { get; }

        /// <summary>
        /// シアン
        /// </summary>
        Color ColorCyan { get; }

        /// <summary>
        /// 灰色
        /// </summary>
        Color ColorGray { get; }

        /// <summary>
        /// 緑色
        /// </summary>
        Color ColorGreen { get; }

        /// <summary>
        /// 紫色
        /// </summary>
        Color ColorPurple { get; }

        /// <summary>
        /// 赤色
        /// </summary>
        Color ColorRed { get; }

        /// <summary>
        /// 白色
        /// </summary>
        Color ColorWhite { get; }

        /// <summary>
        /// 黄色
        /// </summary>
        Color ColorYellow { get; }

        /// <summary>
        /// 端末スクリーンの縦幅
        /// </summary>
        int DeviceScreenHeight { get; }

        /// <summary>
        /// 端末スクリーンの横幅
        /// </summary>
        int DeviceScreenWidth { get; }

        /// <summary>
        /// 帯の色を変更します
        /// </summary>
        /// <param name="r">帯の色の赤成分</param>
        /// <param name="g">帯の色の緑成分</param>
        /// <param name="b">帯の色の青成分</param>
        void ChangeBorderColor(in float r, in float g, in float b);

        /// <summary>
        /// キャンバス解像度を指定します
        /// </summary>
        /// <remarks>
        /// - 初期値は 720x1280 です<br />
        /// - ディスプレイ解像度と縦横比が異なる場合は、上下もしくは左右に帯がつきます
        /// </remarks>
        /// <param name="width">横幅</param>
        /// <param name="height">縦幅</param>
        void ChangeCanvasSize(in int width, in int height);

        [System.Obsolete("Use to `DrawRect`  instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        void DrawCenterRect(in float2 center, in float2 size, float degree = 0f);

        [System.Obsolete("Use to `DrawString`  instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        void DrawCenterString(in string str, in float x, in float y, float degree = 0f);

        /// <summary>
        /// 円を線で描画します
        /// </summary>
        /// <param name="x">中心のX座標</param>
        /// <param name="y">中心のY座標</param>
        /// <param name="radius">半径</param>
        void DrawCircle(in float x, in float y, in float radius);

        /// <summary>
        /// 円を線で描画します
        /// </summary>
        /// <param name="position">中心の座標</param>
        /// <param name="radius">半径</param>
        void DrawCircle(in float2 position, in float radius);

        /// <summary>
        /// 画像を描画します
        /// </summary>
        /// <param name="image">描画する画像</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="degree">回転（度数法）</param>
        void DrawImage(in GcImage image, in float x, in float y, float degree = 0f);

        /// <summary>
        /// 画像を拡縮して描画します
        /// </summary>
        /// <param name="image">描画する画像</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="width">横幅。画像の横幅がこれになるように拡縮される</param>
        /// <param name="height">縦幅。画像の縦幅がこれになるように拡縮される</param>
        /// <param name="degree">回転（度数法）</param>
        void DrawImage(in GcImage image, in float x, in float y, in float width, in float height, float degree = 0f);

        /// <summary>
        /// 線を描画します
        /// </summary>
        /// <param name="begin">始点</param>
        /// <param name="end">終点</param>
        void DrawLine(in float2 begin, in float2 end);

        /// <summary>
        /// 線を描画します
        /// </summary>
        /// <param name="x0">始点のX座標</param>
        /// <param name="y0">始点のY座標</param>
        /// <param name="x1">終点のX座標</param>
        /// <param name="y1">終点のY座標</param>
        void DrawLine(in float x0, in float y0, in float x1, in float y1);

        /// <summary>
        /// 矩形を線で描画します
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="width">横幅</param>
        /// <param name="height">縦幅</param>
        /// <param name="degree">回転（度数法）</param>
        void DrawRect(in float x, in float y, in float width, in float height, float degree = 0f);

        /// <summary>
        /// 矩形を線で描画します
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="size">大きさ</param>
        /// <param name="degree">回転（度数法）</param>
        void DrawRect(in float2 position, in float2 size, float degree = 0f);

        [System.Obsolete("Use to `DrawString`  instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        void DrawRightString(in string str, in float x, in float y, float degree = 0f);

        /// <summary>
        /// 文字列を描画します
        /// </summary>
        /// <param name="str">描画する文字列</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="degree">回転（度数法）</param>
        void DrawString(in string str, in float x, in float y, float degree = 0f);

        /// <summary>
        /// 文字列を拡縮して描画します
        /// </summary>
        /// <param name="str">描画する文字列</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="width">横幅。文字列の横幅がこれになるように拡縮される</param>
        /// <param name="height">縦幅。文字列の縦幅がこれになるように拡縮される</param>
        /// <param name="degree">回転（度数法）</param>
        void DrawString(in string str, in float x, in float y, in float width, in float height, float degree = 0f);

        /// <summary>
        /// テクスチャーを拡縮して描画します
        /// </summary>
        /// <param name="texture">描画するテクスチャー</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="width">横幅。画像の横幅がこれになるように拡縮される</param>
        /// <param name="height">縦幅。画像の縦幅がこれになるように拡縮される</param>
        /// <param name="degree">回転（度数法）</param>
        void DrawTexture(in Texture texture, in float x, in float y, in float width, in float height, float degree = 0f);

        [System.Obsolete("Use to `FillRect`  instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        void FillCenterRect(in float2 center, in float2 size, float degree = 0f);

        /// <summary>
        /// 円を塗りで描画します
        /// </summary>
        /// <param name="x">中心のX座標</param>
        /// <param name="y">中心のY座標</param>
        /// <param name="radius">半径</param>
        void FillCircle(in float x, in float y, in float radius);

        /// <summary>
        /// 円を塗りで描画します
        /// </summary>
        /// <param name="position">中心の座標</param>
        /// <param name="radius">半径</param>
        void FillCircle(in float2 position, in float radius);

        /// <summary>
        /// 矩形を塗りで描画します
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="width">横幅</param>
        /// <param name="height">縦幅</param>
        /// <param name="degree">回転（度数法）</param>
        void FillRect(in float x, in float y, in float width, in float height, float degree = 0f);

        /// <summary>
        /// 矩形を塗りで描画します
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="size">大きさ</param>
        /// <param name="degree">回転（度数法）</param>
        void FillRect(in float2 position, in float2 size, float degree = 0f);

        /// <summary>
        /// 画像の縦幅を取得します
        /// </summary>
        /// <param name="image">画像</param>
        /// <returns>縦幅</returns>
        int GetImageHeight(in GcImage image);

        /// <summary>
        /// 画像のサイズを取得します
        /// </summary>
        /// <param name="image">画像</param>
        /// <returns>サイズ</returns>
        int2 GetImageSize(in GcImage image);

        /// <summary>
        /// 画像の横幅を取得します
        /// </summary>
        /// <param name="image">画像</param>
        /// <returns>横幅</returns>
        int GetImageWidth(in GcImage image);

        /// <summary>
        /// 座標系（変換行列）を回転させます
        /// </summary>
        /// <param name="degree">回転量（度数法）</param>
        /// <param name="originX">回転中心のX座標</param>
        /// <param name="originY">回転中心のY座標</param>
        void RotateCoordinate(in float degree, in float originX, in float originY);

        /// <summary>
        /// 座標系（変換行列）を拡縮させます
        /// </summary>
        /// <param name="sx">X軸方向の拡縮率</param>
        /// <param name="sy">Y軸方向の拡縮率</param>
        void ScaleCoordinate(in float sx, in float sy);

        /// <summary>
        /// 背景色を指定します
        /// </summary>
        /// <param name="color">背景色</param>
        void SetBackgroundColor(in Color color);

        /// <summary>
        /// 背景色を指定します
        /// </summary>
        /// <param name="r">背景色の赤成分</param>
        /// <param name="g">背景色の緑成分</param>
        /// <param name="b">背景色の青成分</param>
        void SetBackgroundColor(in float r, in float g, in float b);

        /// <summary>
        /// 描画色を指定します
        /// </summary>
        /// <param name="r">描画色の赤成分</param>
        /// <param name="g">描画色の緑成分</param>
        /// <param name="b">描画色の青成分</param>
        /// <param name="a">描画色の不透明度</param>
        void SetColor(in float r, in float g, in float b, float a = 1f);

        /// <summary>
        /// 描画色を指定します
        /// </summary>
        /// <param name="r">描画色の赤成分</param>
        /// <param name="g">描画色の緑成分</param>
        /// <param name="b">描画色の青成分</param>
        /// <param name="a">描画色の不透明度</param>
        void SetColor(in byte r, in byte g, in byte b, byte a = 255);

        /// <summary>
        /// 描画色を指定します
        /// </summary>
        /// <param name="color">描画色</param>
        void SetColor(in Color color);

        /// <summary>
        /// 描画色を指定します
        /// </summary>
        /// <param name="color">描画色</param>
        /// <param name="alpha">描画色の不透明度</param>
        void SetColor(in Color color, in float alpha);

        /// <summary>
        /// フォントを指定します
        /// </summary>
        /// <param name="font">フォント</param>
        void SetFont(in GcFont font);

        /// <summary>
        /// フォントサイズを指定します
        /// </summary>
        /// <param name="fontSize">フォントサイズ</param>
        void SetFontSize(in int fontSize);

        /// <summary>
        /// 描線の端点の形状を指定します
        /// </summary>
        /// <param name="lineCap">描線の端点の形状</param>
        void SetLineCap(in GcLineCap lineCap);

        /// <summary>
        /// 描線の太さを指定します
        /// </summary>
        /// <param name="lineWidth">描線の太さ</param>
        void SetLineWidth(in float lineWidth);

        /// <summary>
        /// 座標系（変換行列）を指定します
        /// </summary>
        /// <param name="matrix">座標系（変換行列）</param>
        void SetCoordinate(in float2x3 matrix);

        /// <summary>
        /// 矩形や画像のアンカー位置を指定します
        /// </summary>
        /// <param name="anchor">アンカー位置</param>
        void SetRectAnchor(in GcAnchor anchor);

        [System.Obsolete("Use to `ChangeCanvasSize`  instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        void SetResolution(in int width, in int height);

        /// <summary>
        /// 文字列のアンカー位置を指定します
        /// </summary>
        /// <param name="anchor">アンカー位置</param>
        void SetStringAnchor(in GcAnchor anchor);

        /// <summary>
        /// スタイルを指定します
        /// </summary>
        /// <param name="style"></param>
        void SetStyle(in GcStyle style);

        /// <summary>
        /// 座標系（変換行列）を平行移動させます
        /// </summary>
        /// <param name="tx">X軸方向の移動量</param>
        /// <param name="ty">Y軸方向の移動量</param>
        void TranslateCoordinate(in float tx, in float ty);
    }
}
