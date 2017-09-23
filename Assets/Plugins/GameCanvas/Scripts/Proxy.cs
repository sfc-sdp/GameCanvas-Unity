/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2017 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/

namespace GameCanvas
{
    using UnityEngine;
    using GameCanvas.Engine;
    using GameCanvas.Input;
    using Time = Engine.Time;
    using Collision = Engine.Collision;
    using HiddenAttribute = System.ComponentModel.EditorBrowsableAttribute;
    using HiddenState = System.ComponentModel.EditorBrowsableState;

    public sealed class Proxy
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        private readonly Time cTime;
        private readonly Graphic cGraphic;
        private readonly Sound cSound;
        private readonly Collision cCollision;
        private readonly Pointer cPointer;
        private readonly Keyboard cKeyboard;

        private System.Random mRandom;

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

        #endregion

        //----------------------------------------------------------
        #region パブリック関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal Proxy(Time time, Graphic graphic, Sound sound, Collision collision, Pointer pointer, Keyboard keyboard)
        {
            cTime = time;
            cGraphic = graphic;
            cSound = sound;
            cCollision = collision;
            cPointer = pointer;
            cKeyboard = keyboard;
            mRandom = new System.Random();
        }

        // 描画：文字列

        /// <summary>
        /// 文字列を左寄せで描画します。
        /// </summary>
        /// <param name="str">描画する文字列</param>
        /// <param name="x">左端のX座標</param>
        /// <param name="y">上のY座標</param>
        public void DrawString(string str, int x, int y) => cGraphic.DrawString(ref str, ref x, ref y);
        /// <summary>
        /// 文字列を中央寄せで描画します。
        /// </summary>
        /// <param name="str"></param>
        /// <param name="str">描画する文字列</param>
        /// <param name="x">中心のX座標</param>
        /// <param name="y">上のY座標</param>
        public void DrawCenterString(string str, int x, int y) => cGraphic.DrawCenterString(ref str, ref x, ref y);
        /// <summary>
        /// 文字列を右寄せで描画します。
        /// </summary>
        /// <param name="str">描画する文字列</param>
        /// <param name="x">右端のX座標</param>
        /// <param name="y">上のY座標</param>
        public void DrawRightString(string str, int x, int y) => cGraphic.DrawRightString(ref str, ref x, ref y);
        /// <summary>
        /// 文字列描画で使用するフォントを指定します。
        /// </summary>
        /// <param name="fontId">フォントID（fnt0.ttf なら 0, fnt1.otf なら 1）</param>
        /// <param name="fontStyle">フォントスタイル</param>
        /// <param name="fontSize">フォントサイズ</param>
        public void SetFont(int fontId, FontStyle fontStyle, int fontSize) => cGraphic.SetFont(ref fontId, ref fontStyle, ref fontSize);
        /// <summary>
        /// フォントサイズを指定します。
        /// </summary>
        /// <param name="fontSize">フォントサイズ</param>
        public void SetFontSize(int fontSize) => cGraphic.SetFontSize(ref fontSize);
        /// <summary>
        /// 文字列の横幅を計算します。
        /// </summary>
        /// <param name="str">調べる文字列</param>
        /// <returns>横幅</returns>
        public int GetStringWidth(string str) => cGraphic.GetStringWidth(ref str);

        // 描画：図形

        /// <summary>
        /// 描画に用いる塗りの色を指定します。
        /// </summary>
        /// <param name="color"><see cref="Color"/> 構造体で色を指定</param>
        public void SetColor(Color color) => cGraphic.SetColor(ref color);
        /// <summary>
        /// 描画に用いる塗りの色を指定します。
        /// </summary>
        /// <param name="color">888 で色を指定</param>
        public void SetColor(int color) => cGraphic.SetColor(ref color);
        /// <summary>
        /// 描画に用いる塗りの色を指定します。
        /// </summary>
        /// <param name="r">色の赤成分 (0～255)</param>
        /// <param name="g">色の緑成分 (0～255)</param>
        /// <param name="b">色の青成分 (0～255)</param>
        public void SetColor(int r, int g, int b) => cGraphic.SetColor(ref r, ref g, ref b);
        /// <summary>
        /// 描画に用いる塗りの色をHSV色空間で指定します。
        /// </summary>
        /// <param name="h">色相（0f～1f）</param>
        /// <param name="s">彩度（0f～1f）</param>
        /// <param name="v">明度（0f～1f）</param>
        public void SetColorHsv(float h, float s, float v) => cGraphic.SetColorHSV(ref h, ref s, ref v);
        /// <summary>
        /// 直線を描画します。
        /// </summary>
        /// <param name="startX">開始点のX座標</param>
        /// <param name="startY">開始点のY座標</param>
        /// <param name="endX">終了点のX座標</param>
        /// <param name="endY">終了点のY座標</param>
        public void DrawLine(int startX, int startY, int endX, int endY) => cGraphic.DrawLine(ref startX, ref startY, ref endX, ref endY);
        /// <summary>
        /// 中抜きの長方形を描画します。
        /// </summary>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        public void DrawRect(int x, int y, int width, int height) => cGraphic.DrawRect(ref x, ref y, ref width, ref height);
        /// <summary>
        /// 塗りつぶしの長方形を描画します。
        /// </summary>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        public void FillRect(int x, int y, int width, int height) => cGraphic.FillRect(ref x, ref y, ref width, ref height);
        /// <summary>
        /// 中抜きの円を描画します。
        /// </summary>
        /// <param name="x">中心のX座標</param>
        /// <param name="y">中心のY座標</param>
        /// <param name="radius">半径</param>
        public void DrawCircle(int x, int y, int radius) => cGraphic.DrawCircle(ref x, ref y, ref radius);
        /// <summary>
        /// 塗りつぶしの円を描画します。
        /// </summary>
        /// <param name="x">中心のX座標</param>
        /// <param name="y">中心のY座標</param>
        /// <param name="radius">半径</param>
        public void FillCircle(int x, int y, int radius) => cGraphic.FillCircle(ref x, ref y, ref radius);

        // 描画：画像

        /// <summary>
        /// 画像を描画します。
        /// </summary>
        /// <param name="imageId">画像ID（img0.png なら 0, img1.jpg なら 1）</param>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        public void DrawImage(int imageId, int x, int y) => cGraphic.DrawImage(ref imageId, ref x, ref y);
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
        public void DrawClipImage(int imageId, int x, int y, int u, int v, int width, int height) => cGraphic.DrawClipImage(ref imageId, ref x, ref y, ref u, ref v, ref width, ref height);
        /// <summary>
        /// 画像を拡大縮小・回転をかけて描画します。
        /// </summary>
        /// <param name="imageId">画像ID（img0.png なら 0, img1.jpg なら 1）</param>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="xSize">横の拡縮率（100で等倍, 200なら倍の大きさ）</param>
        /// <param name="ySize">縦の拡縮率（100で等倍, 200なら倍の大きさ）</param>
        /// <param name="degree">回転角（度数法）</param>
        public void DrawScaledRotateImage(int imageId, int x, int y, int xSize, int ySize, float degree) => cGraphic.DrawScaledRotateImage(ref imageId, ref x, ref y, ref xSize, ref ySize, ref degree);
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
        public void DrawScaledRotateImage(int imageId, int x, int y, int xSize, int ySize, float degree, float centerX, float centerY) => cGraphic.DrawScaledRotateImage(ref imageId, ref x, ref y, ref xSize, ref ySize, ref degree, ref centerX, ref centerY);
        /// <summary>
        /// 画像の幅を取得します。
        /// </summary>
        /// <param name="imageId">画像ID（img0.png なら 0, img1.jpg なら 1）</param>
        /// <returns>幅（存在しない画像IDの場合は常に0）</returns>
        public int GetImageWidth(int imageId) => cGraphic.GetImageWidth(ref imageId);
        /// <summary>
        /// 画像の高さを取得します。
        /// </summary>
        /// <param name="imageId">画像ID（img0.png なら 0, img1.jpg なら 1）</param>
        /// <returns>高さ（存在しない画像IDの場合は常に0）</returns>
        public int GetImageHeight(int imageId) => cGraphic.GetImageHeight(ref imageId);

        // 描画：その他

        /// <summary>
        /// 画面を白で塗りつぶします。
        /// </summary>
        public void ClearScreen() => cGraphic.ClearScreen();
        /// <summary>
        /// 現在の画面を、画像として保存します。
        /// </summary>
        /// <param name="file">拡張子を除いたファイル名</param>
        public void WriteScreenImage(string file)
        {
            var filename = file.EndsWith(".png") ? file : file + ".png";
            ScreenCapture.CaptureScreenshot(filename);
        }
        /// <summary>
        /// 画面の幅と高さを設定します。
        /// </summary>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        public void SetResolution(int width, int height) => cGraphic.SetResolution(width, height);
        /// <summary>
        /// 画面の幅
        /// </summary>
        public int CanvasWidth => cGraphic.CanvasWidth;
        /// <summary>
        /// 画面の高さ
        /// </summary>
        public int CanvasHeight => cGraphic.CanvasHeight;
        /// <summary>
        /// フレームレート
        /// </summary>
        public int ConfigFps
        {
            get { return Application.targetFrameRate; }
            set { Application.targetFrameRate = value; }
        }

        // 音声

        /// <summary>
        /// サウンドを再生します。
        /// </summary>
        /// <param name="soundId">サウンドID（snd0.wav なら 0, snd1.mp3 なら 1）</param>
        /// <param name="loop">ループ再生するかどうか</param>
        public void PlaySound(int soundId, bool loop = false) => cSound.Play(soundId, loop);
        /// <summary>
        /// 音量を変更します。
        /// </summary>
        /// <param name="volume">音量（0～100）</param>
        public void SetSoundVolume(int volume) => cSound.SetVolume(ref volume);
        /// <summary>
        /// サウンドを停止します。
        /// </summary>
        public void StopSound() => cSound.Stop();
        /// <summary>
        /// サウンドを一時停止します。
        /// </summary>
        public void PauseSound() => cSound.Pause();
        /// <summary>
        /// サウンドを一時停止していた場合、再生を再開します。
        /// </summary>
        public void UnpauseSound() => cSound.Unpause();

        // 入力

        /// <summary>
        /// キーが押されているフレーム数を取得します。
        /// </summary>
        /// <param name="key">キー番号</param>
        /// <returns>フレーム数</returns>
        public int GetKeyPressFrameCount(EKeyCode key) => cKeyboard.GetPressFrameCount(ref key);
        /// <summary>
        /// キーが押されている時間を取得します。
        /// </summary>
        /// <param name="key">キー番号</param>
        /// <returns>時間（秒）</returns>
        public float GetKeyPressDuration(EKeyCode key) => cKeyboard.GetPressDuration(ref key);
        /// <summary>
        /// キーが押されているかどうかを取得します。
        /// </summary>
        /// <param name="key">キー番号</param>
        /// <returns>押されているかどうか</returns>
        public bool GetIsKeyPress(EKeyCode key) => cKeyboard.GetIsPress(ref key);
        /// <summary>
        /// キーが押された瞬間どうかを取得します。
        /// </summary>
        /// <param name="key">キー番号</param>
        /// <returns>押された瞬間かどうか</returns>
        public bool GetIsKeyBegan(EKeyCode key) => cKeyboard.GetIsBegan(ref key);
        /// <summary>
        /// キーが離された瞬間どうかを取得します。
        /// </summary>
        /// <param name="key">キー番号</param>
        /// <returns>離された瞬間かどうか</returns>
        public bool GetIsKeyEnded(EKeyCode key) => cKeyboard.GetIsEnded(ref key);
        /// <summary>
        /// 有効なポインタイベントの数を取得します。
        /// </summary>
        public int PointerCount => cPointer.Count;
        /// <summary>
        /// ポインタイベントがあるかどうかを取得します。
        /// </summary>
        public bool HasPointerEvent => cPointer.HasEvent;
        /// <summary>
        /// ポインタイベントを取得します。
        /// </summary>
        /// <param name="i">ポインタ通し番号（0～<see cref="PointerCount"/>-1）</param>
        /// <returns>イベント</returns>
        public PointerEvent GetPointerEvent(int i) => cPointer.GetRaw(ref i);
        /// <summary>
        /// ポインタのX座標を取得します。
        /// </summary>
        /// <param name="i">ポインタ通し番号（0～<see cref="PointerCount"/>-1）</param>
        /// <returns>X座標</returns>
        public int GetPointerX(int i) => cPointer.GetX(ref i);
        /// <summary>
        /// ポインタのY座標を取得します。
        /// </summary>
        /// <param name="i">ポインタ通し番号（0～<see cref="PointerCount"/>-1）</param>
        /// <returns>Y座標</returns>
        public int GetPointerY(int i) => cPointer.GetY(ref i);
        /// <summary>
        /// ポインタが押された瞬間かどうかを取得します。
        /// </summary>
        /// <param name="i">ポインタ通し番号（0～<see cref="PointerCount"/>-1）</param>
        /// <returns>押された瞬間かどうか</returns>
        public bool GetIsPointerBegan(int i) => cPointer.GetIsBegan(ref i);
        /// <summary>
        /// ポインタが離された瞬間かどうかを取得します。
        /// </summary>
        /// <param name="i">ポインタ通し番号（0～<see cref="PointerCount"/>-1）</param>
        /// <returns>離された瞬間かどうか</returns>
        public bool GetIsPointerEnded(int i) => cPointer.GetIsEnded(ref i);
        /// <summary>
        /// ポインタが押された瞬間からの経過フレーム数を取得します。
        /// </summary>
        /// <param name="i">ポインタ通し番号（0～<see cref="PointerCount"/>-1）</param>
        /// <returns>押された瞬間からの経過フレーム数</returns>
        public int GetPointerFrameCount(int i) => cPointer.GetFrameCount(ref i);
        /// <summary>
        /// ポインタが押された瞬間からの経過時間を取得します。
        /// </summary>
        /// <param name="i">ポインタ通し番号（0～<see cref="PointerCount"/>-1）</param>
        /// <returns>押された瞬間からの経過時間（秒）</returns>
        public float GetPointerDuration(int i) => cPointer.GetDulation(ref i);
        /// <summary>
        /// スクリーンキーボードを表示します。（実機のみ）
        /// </summary>
        /// <returns>正常に表示できたかどうか</returns>
        public bool ShowScreenKeyboard() => cKeyboard.Open();

        // 数学

        /// <summary>
        /// 乱数の種をセットします。
        /// </summary>
        /// <param name="seed">シード</param>
        public void SetSeed(int seed) => mRandom = new System.Random(seed);
        /// <summary>
        /// <paramref name="min"/> から <paramref name="max"/> までのランダムな値を計算します。
        /// </summary>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <returns><paramref name="min"/> から <paramref name="max"/> までのランダムな値</returns>
        public int Random(int min, int max) => mRandom.Next(min, max + 1);
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
        public bool CheckHitRect(int x1, int y1, int w1, int h1, int x2, int y2, int w2, int h2) => cCollision.CheckHitRect(ref x1, ref y1, ref w1, ref h1, ref x2, ref y2, ref w2, ref h2);
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
        public bool CheckHitImage(int imageId1, int x1, int y1, int imageId2, int x2, int y2) => cCollision.CheckHitImage(ref imageId1, ref x1, ref y1, ref imageId2, ref x2, ref y2);
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
        public bool CheckHitCircle(int x1, int y1, int r1, int x2, int y2, int r2) => cCollision.CheckHitCircle(ref x1, ref y1, ref r1, ref x2, ref y2, ref r2);
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
        public float TimeSinceStartup => cTime.SinceStartup;
        /// <summary>
        /// ひとつ前のフレームからの経過時間（秒）
        /// </summary>
        public float TimeSincePrevFrame => cTime.SincePrevFrame;
        /// <summary>
        /// 現在フレームの日付の西暦部分
        /// </summary>
        public int CurrentYear => cTime.Year;
        /// <summary>
        /// 現在フレームの日付の月部分（1～12）
        /// </summary>
        public int CurrentMonth => cTime.Month;
        /// <summary>
        /// 現在フレームの日付（1～31）
        /// </summary>
        public int CurrentDay => cTime.Day;
        /// <summary>
        /// 現在フレームの曜日（0～6）
        /// </summary>
        public System.DayOfWeek CurrentDayOfWeek => cTime.DayOfWeek;
        /// <summary>
        /// 現在フレームの時刻の時間部分（0～23）
        /// </summary>
        public int CurrentHour => cTime.Hour;
        /// <summary>
        /// 現在フレームの時刻の分部分（0～59）
        /// </summary>
        public int CurrentMinute => cTime.Minute;
        /// <summary>
        /// 現在フレームの時刻の秒部分（0～59）
        /// </summary>
        public int CurrentSecond => cTime.Second;
        /// <summary>
        /// 現在フレームの時刻のミリ秒部分（0～59）
        /// </summary>
        public int CurrentMillisecond => cTime.Millisecond;
        /// <summary>
        /// 現在フレームのUnixタイムスタンプ
        /// </summary>
        public long CurrentTimestamp => cTime.Timestamp;
        /// <summary>
        /// アプリ起動からの累計フレーム数
        /// </summary>
        public int CurrentFrame => cTime.FrameCount;
        /// <summary>
        /// 現在フレームの日時
        /// </summary>
        public System.DateTimeOffset CurrentTime => cTime.Current;
        /// <summary>
        /// 現在（関数呼び出し時点）の日時
        /// </summary>
        public System.DateTimeOffset NowTime => cTime.Now;

        // その他

        /// <summary>
        /// 保存された値を取り出します。
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>保存された値（存在しない場合は0）</returns>
        public int Load(int key) => PlayerPrefs.GetInt(key.ToString(), 0);
        /// <summary>
        /// 値を保存します。
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">保存する値</param>
        public void Save(int key, int value) => PlayerPrefs.SetInt(key.ToString(), value);
        /// <summary>
        /// 初期状態にします。
        /// </summary>
        public void ResetGame()
        {
            cSound.Stop();
            cGraphic.ClearScreen();
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
        public int Width => cGraphic.CanvasWidth;
        [Hidden(HiddenState.Never), System.Obsolete("gc.CanvasWHeight")]
        public int Height => cGraphic.CanvasHeight;
        [Hidden(HiddenState.Never), System.Obsolete, System.Diagnostics.Conditional("ENABLE_GAMECANVAS_JAVA")]
        public void SetWindowTitle(string title) { }
        [Hidden(HiddenState.Never), System.Obsolete, System.Diagnostics.Conditional("ENABLE_GAMECANVAS_JAVA")]
        public void SetFont(string fontName, int fontStyle, int fontSize) { }
        [Hidden(HiddenState.Never), System.Obsolete("gc.Play()")]
        public void PlaySE(int soundId, bool loop = false) => cSound.Play(soundId, loop);
        [Hidden(HiddenState.Never), System.Obsolete("gc.ChangeVolume()")]
        public void ChangeSEVolume(int volume) => cSound.SetVolume(ref volume);
        [Hidden(HiddenState.Never), System.Obsolete("gc.Stop()")]
        public void StopSE() => cSound.Stop();
        [Hidden(HiddenState.Never), System.Obsolete("gc.Pause()")]
        public void PauseSE() => cSound.Pause();
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetPointerX(0)")]
        public int GetMouseX() => cPointer.LastX;
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetPointerY(0)")]
        public int GetMouseY() => cPointer.LastY;
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetPointerFrameCount(0)")]
        public int GetMouseClickLength() => cPointer.FrameCount;
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetIsPointerBegan(0)")]
        public bool IsMousePushed() => cPointer.IsBegan;
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetIsPointerEnded(0)")]
        public bool IsMouseReleased() => cPointer.IsEnded;
        [Hidden(HiddenState.Never), System.Obsolete("gc.HasPointerEvent")]
        public bool IsMousePress() => cPointer.HasEvent;
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetKeyPressFrameCount")]
        public int GetKeyPressLength(EKeyCode key) => cKeyboard.GetIsEnded(ref key) ? -1 : cKeyboard.GetPressFrameCount(ref key);
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetIsKeyPress")]
        public bool IsKeyPress(EKeyCode key) => cKeyboard.GetIsPress(ref key);
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetIsKeyBegan")]
        public bool IsKeyPushed(EKeyCode key) => cKeyboard.GetIsBegan(ref key);
        [Hidden(HiddenState.Never), System.Obsolete("gc.GetIsKeyEnded")]
        public bool IsKeyReleased(EKeyCode key) => cKeyboard.GetIsEnded(ref key);

        [Hidden(HiddenState.Never)]
        public void PlayBGM(int midiId, bool loop = true) { } // TODO
        [Hidden(HiddenState.Never)]
        public void ChangeBGMVolume(int volume) { } // TODO
        [Hidden(HiddenState.Never)]
        public void StopBGM() { } // TODO
        [Hidden(HiddenState.Never)]
        public void PauseBGM() { } // TODO
        [Hidden(HiddenState.Never)]
        public bool ShowYesNoDialog(string message) { return false; } // TODO
        [Hidden(HiddenState.Never)]
        public string ShowInputDialog(string message, string defaultInput) { return null; } // TODO

        #endregion
    }
}
