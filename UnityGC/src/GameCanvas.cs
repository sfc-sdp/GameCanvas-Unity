using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameCanvas
{
    using ColorArrRoc = ReadOnlyCollection<Color[]>;
    
    /// <summary>
    /// GameCanvasの様々な機能を取りまとめたクラス
    /// </summary>
    /// <author>
    /// 2010 kuro    (shift→sega)     ※Java版
    /// 2010 fujieda (shift→ntt)      ※Java版
    /// 2016 seibe   (shift→nintendo) ※Unity版
    /// </author>
    public class GameCanvas : SingletonMonoBehaviour<GameCanvas>
    {
        /* ---------------------------------------------------------------------------------------------------- */

        #region UnityGC：定数

        private const float MAX_TAP_TIME_LENGTH     = 0.3f;         // タッチ：タップ判定時間長
        private const float MIN_FLICK_DISTANCE      = 3f;           // タッチ：フリック判定移動量
        private const int   MIN_HOLD_FRAME          = 8;            // タッチ：ホールド判定フレーム数
        private const float MAX_PINCH_IN_SCALE      = 0.95f;        // ピンチ：ピンチイン判定縮小率
        private const float MIN_PINCH_OUT_SCALE     = 1.05f;        // ピンチ：ピンチアウト判定拡大率

        #endregion

        #region UnityGC：変数

        private int         _deviceWidth            = 640;          // 画面解像度：横幅
        private int         _deviceHeight           = 480;          // 画面解像度：縦幅
        private int         _canvasWidth            = 640;          // 描画解像度：横幅
        private int         _canvasHeight           = 480;          // 描画解像度：縦幅
        private float       _canvasDisplayScale     = 1f;           // キャンバス解像度 / デバイス解像度
        private Vector2     _canvasBorderSize       = Vector2.zero; // キャンバス左・上の黒縁の大きさ

        private Color       _palletColor            = Color.black;  // 現在のパレットカラー
        private Color[]     _canvasColorArray       = null;         // 各ピクセルの色情報
        private Color[]     _canvasPrevColorArray   = null;         // 各ピクセルの色情報：前回フレーム

        private int         _numImage               = 0;            // 認識済みの画像：数量
        private Texture2D[] _imageArray             = null;         // 認識済みの画像：データ配列
        private ColorArrRoc _imagePixelArrayList    = null;         // 認識済みの画像：データ配列：画素
        private int         _numAudio               = 0;            // 認識済みの音源：数量
        private AudioClip[] _audioArray             = null;         // 認識済みの音源：データ配列

        private bool        _touchSupported         = false;        // タッチ：実行環境がタッチ操作対応かどうか
        private bool        _isTouch                = false;        // タッチ：タッチされているかどうか
        private bool        _isTouchBegan           = false;        // タッチ：タッチされ始めた瞬間かどうか
        private bool        _isTouchEnded           = false;        // タッチ：タッチされ終えた瞬間かどうか
        private bool        _isTapped               = false;        // タッチ：タップされた瞬間かどうか
        private bool        _isFlicked              = false;        // タッチ：フリックされた瞬間かどうか
        private float       _touchBeganTime         = -1f;          // タッチ：開始時刻(アプリ開始からの経過時間:秒)
        private Vector2     _touchPoint             = -Vector2.one; // タッチ：座標
        private Vector2     _scaledTouchPoint       = -Vector2.one; // タッチ：キャンバスピクセルに対応する座標
        private Vector2     _touchBeganPoint        = -Vector2.one; // タッチ：開始座標
        private float       _touchTimeLength        = 0f;           // タッチ：連続時間(秒)
        private float       _touchHoldTimeLength    = 0f;           // タッチ：連続静止時間(秒)
        private float       _pinchLength            = 0f;           // ピンチインアウト：2点間距離
        private float       _pinchLengthBegan       = 0f;           // ピンチインアウト：2点間距離：タッチ開始時
        private float       _pinchScale             = 0f;           // ピンチインアウト：拡縮率：前フレーム差分
        private float       _pinchScaleBegan        = 0f;           // ピンチインアウト：拡縮率：タッチ開始時から
        private Vector2     _mousePrevPoint         = -Vector2.one; // マウス互換：前回マウス位置

        private Camera      _camera                 = null;         // コンポーネント：Camera
        private Canvas      _canvas                 = null;         // コンポーネント：Canvas
        private CanvasScaler _canvasScaler          = null;         // コンポーネント：CanvasScaler
        private RawImage    _canvasRawImage         = null;         // コンポーネント：RawImage
        private Texture2D   _canvasTexture          = null;         // コンポーネント：Texture2D

        #endregion


        /* ---------------------------------------------------------------------------------------------------- */

        #region Unity：イベント関数

        /// <summary>
        /// 構築処理
        /// </summary>
        private new void Awake()
        {
            // 実行環境の記録
            _deviceWidth    = Screen.width;
            _deviceHeight   = Screen.height;
            _touchSupported = Input.touchSupported;

            // GameCanvasインスタンスの作成
            base.Awake();
            name = "GameCanvas";

            // アプリの設定
            Application.targetFrameRate     = 60;
            Screen.fullScreen               = false;
            Screen.sleepTimeout             = SleepTimeout.NeverSleep;
            Screen.orientation              = ScreenOrientation.Landscape;
            Input.multiTouchEnabled         = true;
            Input.simulateMouseWithTouches  = true;

            // Cameraコンポーネントの配置。既に配置されているCameraは問答無用で抹消する
            {
                var camArray = UnityEngine.Object.FindObjectsOfType<Camera>();
                foreach (Camera cam in camArray)
                {
                    UnityEngine.Object.DestroyImmediate(cam.gameObject);
                }

                var obj = new GameObject("Camera");
                obj.tag = "MainCamera";
                obj.transform.parent = baseTransform;

                _camera = obj.AddComponent<Camera>();
                _camera.transform.localPosition = new Vector3(0.0f, 0.0f, -10.0f);
                _camera.clearFlags = CameraClearFlags.SolidColor;
                _camera.backgroundColor = Color.black;
                _camera.orthographic = true;
                _camera.orthographicSize = 5;
                _camera.depth = -1;

                obj.AddComponent<AudioListener>();
            }

            // Canvasコンポーネントの配置
            {
                var obj                 = new GameObject("Canvas");
                obj.transform.parent    = baseTransform;

                _canvas                 = obj.AddComponent<Canvas>();
                _canvas.renderMode      = RenderMode.ScreenSpaceOverlay;

                _canvasScaler                     = obj.AddComponent<CanvasScaler>();
                _canvasScaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                _canvasScaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.Expand;
                _canvasScaler.referenceResolution = new Vector2(_canvasWidth, _canvasHeight);
            }

            // UI.RawImageコンポーネントの配置。2D描画に用いる
            {
                var obj                 = new GameObject("RawImage");
                obj.transform.parent    = _canvas.transform;

                _canvasTexture          = Texture2D.whiteTexture;
                _canvasColorArray       = _canvasTexture.GetPixels();

                _canvasRawImage         = obj.AddComponent<RawImage>();
                _canvasRawImage.color   = Color.white;
                _canvasRawImage.texture = _canvasTexture;

                var rect                = _canvasRawImage.rectTransform;
                rect.anchoredPosition   = Vector2.zero;
                rect.anchorMin          = Vector2.one * 0.5f;
                rect.anchorMax          = Vector2.one * 0.5f;
                rect.sizeDelta          = new Vector2(_canvasWidth, _canvasHeight);
                rect.localScale         = new Vector3(1, -1, 1);
            }

            // UI.EventSystemコンポーネントの配置。タッチ情報の取得に用いる
            {
                var obj                 = new GameObject("EventSystem");
                obj.transform.parent    = baseTransform;

                obj.AddComponent<EventSystem>();
            }

            // キャンバスの初期化
            SetResolution(_canvasWidth, _canvasHeight);

            // 外部画像・音源データの読み込み
            {
                _imageArray = Resources.LoadAll<Texture2D>("");
                _numImage   = _imageArray.Length;
                var list = new List<Color[]>(_numImage);
                for (int i = 0; i < _numImage; ++i)
                {
                    if (_imageArray[i].width * _imageArray[i].height > 120 * 120)
                    {
                        Debug.LogWarningFormat("画像{0}の画素数が多いため、実行性能が大幅に低下する可能性があります", i);
                    }
                    list.Add(_imageArray[i].GetPixels());
                }
                _imagePixelArrayList = list.AsReadOnly();
                Debug.Log("Load Images (" + _numImage + ")");

                _audioArray = Resources.LoadAll<AudioClip>("");
                _numAudio   = _audioArray.Length;
                Debug.Log("Load Sounds (" + _numAudio + ")");
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void Update()
        {
            // 初期化
            _isTouchBegan = false;
            _isTouchEnded = false;
            _isTapped     = false;
            _isFlicked    = false;

            // タッチ判定
            _isTouch = _touchSupported ? Input.touchCount > 0 : Input.GetMouseButton(0);
            if (_isTouch)
            {
                // 連続時間を記録
                _touchTimeLength += Time.deltaTime;

                // タッチ・マウス互換処理
                TouchPhase phase;
                if (_touchSupported)
                {
                    var t0 = Input.GetTouch(0);
                    _touchPoint   = t0.position;
                    _touchPoint.y = Screen.height - _touchPoint.y;
                    phase = t0.phase;
                }
                else
                {
                    _touchPoint   = Input.mousePosition;
                    _touchPoint.y = Screen.height - _touchPoint.y;
                    if      (Input.GetMouseButtonDown(0)   ) { phase = TouchPhase.Began; _mousePrevPoint = _touchPoint;  }
                    else if (Input.GetMouseButtonUp(0)     ) { phase = TouchPhase.Ended; _mousePrevPoint = -Vector2.one; }
                    else if (_mousePrevPoint != _touchPoint) { phase = TouchPhase.Moved; _mousePrevPoint = _touchPoint;  }
                    else                                     { phase = TouchPhase.Stationary;                            }
                }

                // タッチ座標の変換
                _scaledTouchPoint = (_touchPoint - _canvasBorderSize) * _canvasDisplayScale;

                // タッチ関連挙動の検出
                switch (phase)
                {
                    case TouchPhase.Began:
                        // 開始時間を記録
                        _touchBeganTime  = Time.realtimeSinceStartup;
                        _touchBeganPoint = _touchPoint;
                        _isTouchBegan    = true;
                        break;

                    case TouchPhase.Canceled:
                    case TouchPhase.Ended:
                        // タップ・フリック判定
                        if (_touchHoldTimeLength <= MAX_TAP_TIME_LENGTH)
                        {
                            var diff   = Vector2.Distance(_touchBeganPoint, _touchPoint);
                            _isFlicked = diff > MIN_FLICK_DISTANCE;
                            _isTapped  = !_isFlicked;
                        }

                        // 初期化
                        _touchBeganTime      = -1f;
                        _touchBeganPoint     = -Vector2.one;
                        _touchTimeLength     = 0f;
                        _touchHoldTimeLength = 0f;
                        _isTouchEnded        = true;
                        break;

                    case TouchPhase.Moved:
                        // 初期化
                        _touchHoldTimeLength = 0;
                        break;

                    case TouchPhase.Stationary:
                        // 連続静止時間を記録
                        _touchHoldTimeLength += Time.deltaTime;
                        break;
                }
                
                // ピンチイン・アウト判定
                if (Input.touchCount > 1)
                {
                    var t0 = Input.GetTouch(0);
                    var t1 = Input.GetTouch(1);

                    switch (t1.phase)
                    {
                        case TouchPhase.Began:
                            // 2点間の距離を記録
                            _pinchLength      = Vector2.Distance(t0.position, t1.position);
                            _pinchLengthBegan = _pinchLength;
                            _pinchScale       = 1f;
                            _pinchScaleBegan  = 1f;
                            break;

                        case TouchPhase.Canceled:
                        case TouchPhase.Ended:
                            // 初期化
                            _pinchLength      = 0f;
                            _pinchLengthBegan = 0f;
                            _pinchScale       = 0f;
                            _pinchScaleBegan  = 0f;
                            break;

                        case TouchPhase.Moved:
                        case TouchPhase.Stationary:
                            // ピンチインアウト処理
                            if (t0.phase == TouchPhase.Moved || t1.phase == TouchPhase.Moved)
                            {
                                var length       = Vector2.Distance(t0.position, t1.position);
                                _pinchScale      = length / _pinchLength;
                                _pinchScaleBegan = length / _pinchLengthBegan;
                                _pinchLength     = length;
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 描画確定処理
        /// </summary>
        private void LateUpdate()
        {
            // キャンバスの描画更新
            _canvasTexture.SetPixels(_canvasColorArray);
            _canvasTexture.Apply();
        }

        #endregion


        /* ---------------------------------------------------------------------------------------------------- */

        #region UnityGC：グラフィックAPI (基本図形)

        /// <summary>
        /// FPS（1秒あたりのフレーム更新回数）
        /// </summary>
        public int frameRate
        {
            set
            {
                Application.targetFrameRate = value;
            }
            get
            {
                return Application.targetFrameRate;
            }
        }

        /// <summary>
        /// 画面X軸方向のゲーム解像度（幅）
        /// </summary>
        public int screenWidth
        {
            get
            {
                return _canvasWidth;
            }
        }

        /// <summary>
        /// 画面Y軸方向のゲーム解像度（高さ）
        /// </summary>
        public int screenHeight
        {
            get
            {
                return _canvasHeight;
            }
        }

        /// <summary>
        /// フルスクリーンかどうか
        /// </summary>
        public bool isFullScreen
        {
            set
            {
                Screen.fullScreen = value;
                SetResolution(_canvasWidth, _canvasHeight);
            }
            get
            {
                return Screen.fullScreen;
            }
        }

        /// <summary>
        /// ゲーム画面が端末の向きに合わせて自動回転するかどうか
        /// </summary>
        public bool isScreenAutoRotation
        {
            set
            {
                if (value)
                {
                    Screen.orientation = ScreenOrientation.AutoRotation;
                }
                else
                {
                    Screen.orientation = isPortrait ? ScreenOrientation.Portrait : ScreenOrientation.Landscape;
                }
            }
            get
            {
                return Screen.orientation == ScreenOrientation.AutoRotation;
            }
        }

        /// <summary>
        /// ゲーム画面が縦向きかどうか。この値はゲーム解像度によって自動的に決定されます
        /// </summary>
        public bool isPortrait
        {
            get
            {
                return _canvasWidth <= _canvasHeight;
            }
        }

        /// <summary>
        /// 指定された画像の横幅を返します。画像が見つからない場合 0 を返します
        /// </summary>
        /// <param name="id">描画する画像のID。例えば、ファイル名が img0.png ならば、画像IDは 0</param>
        /// <returns>指定された画像の横幅</returns>
        public int GetImageWidth(int id)
        {
            if (id >= _numImage)
            {
                Debug.LogWarning("存在しないファイルが指定されました");
                return 0;
            }

            return _imageArray[id].width;
        }

        /// <summary>
        /// 指定された画像の高さを返します。画像が見つからない場合 0 を返します
        /// </summary>
        /// <param name="id">描画する画像のID。例えば、ファイル名が img0.png ならば、画像IDは 0</param>
        /// <returns>指定された画像の高さ</returns>
        public int GetImageHeight(int id)
        {
            if (id >= _numImage)
            {
                Debug.LogWarning("存在しないファイルが指定されました");
                return 0;
            }

            return _imageArray[id].height;
        }

        /// <summary>
        /// ゲームの解像度を設定します
        /// </summary>
        /// <param name="width">X軸方向の解像度（幅）</param>
        /// <param name="height">Y軸方向の解像度（高さ）</param>
        public void SetResolution(int width, int height)
        {
            // 実行解像度の変更
            // Screen.SetResolution(width, height, true);

            // キャンバス解像度の再設定
            _canvasWidth = width;
            _canvasHeight = height;
            var size = new Vector2(_canvasWidth, _canvasHeight);
            _canvasScaler.referenceResolution = size;
            _canvasRawImage.rectTransform.sizeDelta = size;

            var screenW = Screen.width;
            var screenH = Screen.height;
            var scaleW = (float)_canvasWidth  / screenW;
            var scaleH = (float)_canvasHeight / screenH;
            _canvasDisplayScale = Mathf.Max(scaleW, scaleH);
            _canvasBorderSize.x = (screenW - _canvasWidth  / _canvasDisplayScale) * 0.5f;
            _canvasBorderSize.y = (screenH - _canvasHeight / _canvasDisplayScale) * 0.5f;

            // 自動回転の再設定。回転固定設定は引き継がれる
            isScreenAutoRotation = isScreenAutoRotation;

            // 2Dテクスチャの再生成。色情報は引き継がれない
            _canvasTexture = new Texture2D(_canvasWidth, _canvasHeight, TextureFormat.RGB24, false);
            _canvasRawImage.texture = _canvasTexture;
            _canvasColorArray = _canvasTexture.GetPixels();
            _canvasPrevColorArray = new Color[_canvasColorArray.Length];
            ClearScreen();
        }

        /// <summary>
        /// DrawString や DrawRect などで塗りつぶしに用いる色を指定します
        /// </summary>
        /// <param name="color">塗りの色</param>
        public void SetColor(Color color)
        {
            _palletColor = color;
        }

        /// <summary>
        /// DrawString や DrawRect などで塗りつぶしに用いる色を指定します
        /// </summary>
        /// <param name="red">R成分</param>
        /// <param name="green">G成分</param>
        /// <param name="blue">B成分</param>
        public void SetColor(int red, int green, int blue)
        {
            SetColor(new Color(red, green, blue));
        }

        /// <summary>
        /// 画面を白で塗りつぶします
        /// </summary>
        public void ClearScreen()
        {
            var num = _canvasWidth * _canvasHeight;
            var white = Color.white;
            for (int i = 0; i < num; ++i)
            {
                _canvasColorArray[i] = white;
            }
        }

        /// <summary>
        /// 指定された座標1点を塗りつぶします
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawPoint(int x, int y)
        {
            // 範囲外は無視する
            if (x >= 0 && y >= 0 && x < _canvasWidth && y < _canvasHeight)
            {
                _canvasColorArray[x + y * _canvasWidth] = _palletColor;
            }
        }

        /// <summary>
        /// 中抜きの円を描画します
        /// </summary>
        /// <param name="x">中心点のX座標</param>
        /// <param name="y">中心点のY座標</param>
        /// <param name="radius">半径</param>
        public void DrawCircle(int x, int y, int radius)
        {
            if (radius < 1)
            {
                // 負の半径は許容しない
                Debug.LogWarning("引数の値が不正です");
                return;
            }
            
            var d    = 0.25f - radius;
            var dy   = radius;
            var dxe  = Mathf.CeilToInt(radius / Mathf.Sqrt(2));
            var size = _canvasWidth * _canvasHeight;

            for (int dx = 0; dx <= dxe; ++dx)
            {
                DrawPoint(x + dx, y + dy);
                DrawPoint(x + dx, y - dy);
                DrawPoint(x - dx, y + dy);
                DrawPoint(x - dx, y - dy);
                DrawPoint(x + dy, y + dx);
                DrawPoint(x - dy, y + dx);
                DrawPoint(x + dy, y - dx);
                DrawPoint(x - dy, y - dx);

                d += 2 * dx + 1;
                if (d > 0)
                {
                    d += 2 - 2 * dy--;
                }
            }
        }

        /// <summary>
        /// 線分を描画します
        /// </summary>
        /// <param name="startX">開始点のX座標</param>
        /// <param name="startY">開始点のY座標</param>
        /// <param name="endX">終了点のX座標</param>
        /// <param name="endY">終了点のY座標</param>
        public void DrawLine(int startX, int startY, int endX, int endY)
        {
            var fraction = 0f;
            var x  = startX;
            var y  = startY;
            var dx = endX - startX;
            var dy = endY - startY;
            int stepX, stepY;

            if (dx < 0)
            {
                // 右から左
                dx = -dx;
                stepX = -1;
            }
            else
            {
                // 左から右
                stepX = 1;
            }

            if (dy < 0)
            {
                // 下から上
                dy = -dy;
                stepY = -1;
            }
            else
            {
                // 上から下
                stepY = 1;
            }

            // 直線の場合
            if (dx == 0)
            {
                // 縦直線
                DrawPoint(x, y);
                while (y != endY)
                {
                    y += stepY;
                    DrawPoint(x, y);
                }
                return;
            }
            else if (dy == 0)
            {
                // 横直線
                DrawPoint(x, y);
                while (x != endX)
                {
                    x += stepX;
                    DrawPoint(x, y);
                }
                return;
            }

            // 斜め線の場合
            dx <<= 1;
            dy <<= 1;

            DrawPoint(x, y);
            if (dx > dy)
            {
                fraction = dy - (dx >> 1);
                while ((x > endX ? x - endX : endX - x) > 1)
                {
                    if (fraction >= 0)
                    {
                        y += stepY;
                        fraction -= dx;
                    }
                    x += stepX;
                    fraction += dy;
                    DrawPoint(x, y);
                }
            }
            else
            {
                fraction = dx - (dy >> 1);
                while ((y > endY ? y - endY : endY - y) > 1)
                {
                    if (fraction >= 0)
                    {
                        x += stepX;
                        fraction -= dy;
                    }
                    y += stepY;
                    fraction += dx;
                    DrawPoint(x, y);
                }
            }
        }

        /// <summary>
        /// 中抜きの長方形を描画します
        /// </summary>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="width">横幅</param>
        /// <param name="height">縦幅</param>
        public void DrawRect(int x, int y, int width, int height)
        {
            if (width < 1 || height < 1)
            {
                // 負の幅は許容しない
                Debug.LogWarning("引数の値が不正です");
                return;
            }

            DrawLine(x, y, x + width, y);
            DrawLine(x, y + height, x + width, y + height);
            DrawLine(x, y, x, y + height);
            DrawLine(x + width, y, x + width, y + height);
        }

        /// <summary>
        /// 画像を描画します
        /// </summary>
        /// <param name="id">描画する画像のID。例えば、ファイル名が img0.png ならば、画像IDは 0</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        public void DrawImage(int id, int x, int y)
        {
            if (id >= _numImage)
            {
                Debug.LogWarning("存在しないファイルが指定されました");
                return;
            }

            if (x >= _canvasWidth || y >= _canvasHeight)
            {
                // 描画範囲外である
                return;
            }

            int pw = _imageArray[id].width;
            int ph = _imageArray[id].height;
            int pwc = pw;
            int phc = ph;
            if (x + pw > _canvasWidth) pw = _canvasWidth - x;
            if (y + ph > _canvasWidth) ph = _canvasHeight - y;

            var img = _imagePixelArrayList[id];

            for (int i = 0; i < pw; ++i)
            {
                // 範囲外は無視する
                var rx = x + i;
                if (rx >= _canvasWidth) break;
                if (rx < 0) continue;

                for (int j = 0; j < ph; ++j)
                {
                    // 範囲外は無視する
                    var ry = y + j;
                    if (ry >= _canvasHeight) break;
                    if (ry < 0) continue;

                    var color = img[i + (phc - j - 1) * pwc];
                    if (color.a != 0f)
                    {
                        color.a = 1f;

                        // -MEMO-
                        // 処理最適化のため DrawPoint() は用いない
                        _canvasColorArray[rx + ry * _canvasWidth] = color;
                    }
                }
            }
        }

        /// <summary>
        /// 塗りつぶしの円を描画します
        /// </summary>
        /// <param name="x">中心点のX座標</param>
        /// <param name="y">中心点のY座標</param>
        /// <param name="radius">半径</param>
        public void FillCircle(int x, int y, int radius)
        {
            if (radius < 1)
            {
                // 負の半径は許容しない
                Debug.LogWarning("引数の値が不正です");
                return;
            }

            float d = 0.25f - radius;
            int dy = radius;
            int dxe = Mathf.CeilToInt(radius / Mathf.Sqrt(2));
            int size = _canvasWidth * _canvasHeight;

            for (int dx = 0; dx <= dxe; ++dx)
            {
                for (int i = 0; i < dy; ++i)
                {
                    DrawPoint(x + dx, y + i);
                    DrawPoint(x + dx, y - i);
                    DrawPoint(x - dx, y + i);
                    DrawPoint(x - dx, y - i);
                }
                for (int i = dxe; i < dy; ++i)
                {
                    DrawPoint(x + i, y + dx);
                    DrawPoint(x + i, y - dx);
                    DrawPoint(x - i, y + dx);
                    DrawPoint(x - i, y - dx);
                }

                d += 2 * dx + 1;
                if (d > 0)
                {
                    d += 2 - 2 * dy--;
                }
            }
        }

        /// <summary>
        /// 塗りつぶしの長方形を描画します
        /// </summary>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="width">横幅</param>
        /// <param name="height">縦幅</param>
        public void FillRect(int x, int y, int width, int height)
        {
            if (width < 1 || height < 1)
            {
                // 負の幅は許容しない
                Debug.LogWarning("引数の値が不正です");
                return;
            }

            // -MEMO-
            // 最適化のため DrawPoint() は用いない

            int sx = x;
            int sy = y;
            int ex = x + width;
            int ey = y + height;

            if (sx < 0) sx = 0;
            if (sy < 0) sy = 0;
            if (ex >= _canvasWidth ) ex = _canvasWidth  - 1;
            if (ey >= _canvasHeight) ey = _canvasHeight - 1;

            if (sx >= _canvasWidth || sy >= _canvasHeight || ex < 0 || ey < 0)
            {
                // 描画範囲外である
                return;
            }

            for (int i = sx; i < ex; ++i)
            {
                for (int j = sy; j < ey; ++j)
                {
                    _canvasColorArray[i + j * _canvasWidth] = _palletColor;
                }
            }
        }

        #endregion

        #region UnityGC：入力API (タッチ)

        /// <summary>
        /// タッチされた状態かどうか
        /// </summary>
        public bool isTouch
        {
            get
            {
                return _isTouch;
            }
        }

        /// <summary>
        /// タッチを始めた瞬間かどうか
        /// </summary>
        public bool isTouchBegan
        {
            get
            {
                return _isTouchBegan;
            }
        }

        /// <summary>
        /// タッチを終えた瞬間かどうか
        /// </summary>
        public bool isTouchEnded
        {
            get
            {
                return _isTouchEnded;
            }
        }

        /// <summary>
        /// ホールド（指で触れたまま静止）された状態かどうか
        /// </summary>
        public bool isHold
        {
            get
            {
                return _touchHoldTimeLength >= MIN_HOLD_FRAME;
            }
        }

        /// <summary>
        /// タップ（指で軽く触れる）された瞬間かどうか
        /// </summary>
        public bool isTap
        {
            get
            {
                return _isTapped;
            }
        }

        /// <summary>
        /// フリック（指で軽くはじく）された瞬間かどうか
        /// </summary>
        public bool isFlick
        {
            get
            {
                return _isFlicked;
            }
        }

        /// <summary>
        /// ピンチインまたはピンチアウトされた状態かどうか
        /// </summary>
        public bool isPinchInOut
        {
            get
            {
                return isPinchIn || isPinchOut;
            }
        }

        /// <summary>
        /// ピンチインされた状態かどうか
        /// </summary>
        public bool isPinchIn
        {
            get
            {
                return _pinchScale != 0f && _pinchScaleBegan < 0.95f;
            }
        }

        /// <summary>
        /// ピンチアウトされた状態かどうか
        /// </summary>
        public bool isPinchOut
        {
            get
            {
                return _pinchScale != 0f && _pinchScaleBegan > 1.05f;
            }
        }

        /// <summary>
        /// タッチされている座標X。タッチされていないときは、最後にタッチされた座標を返します
        /// </summary>
        public int touchX
        {
            get
            {
                return (int)touchPoint.x;
            }
        }

        /// <summary>
        /// タッチされている座標Y。タッチされていないときは、最後にタッチされた座標を返します
        /// </summary>
        public int touchY
        {
            get
            {
                return (int)touchPoint.y;
            }
        }

        /// <summary>
        /// タッチされている座標。タッチされていないときは、最後にタッチされた座標を返します
        /// </summary>
        public Vector2 touchPoint
        {
            get
            {
                return _scaledTouchPoint;
            }
        }

        /// <summary>
        /// 同時にタッチされている数
        /// </summary>
        public int touchCount
        {
            get
            {
                if (_touchSupported)
                    return Input.touchCount;
                else
                    return _isTouch ? 1 : 0;
            }
        }

        /// <summary>
        /// タッチされている時間
        /// </summary>
        public float touchTimeLength
        {
            get
            {
                return _touchTimeLength;
            }
        }

        /// <summary>
        /// ピンチインアウトの拡縮率。ピンチインアウトされていない場合、0を返します
        /// </summary>
        public float pinchRatio
        {
            get
            {
                return _pinchScaleBegan;
            }
        }

        /// <summary>
        /// 前回フレームを基準としたピンチインアウトの拡縮率。ピンチインアウトされていない場合、0を返します
        /// </summary>
        public float pinchRatioInstant
        {
            get
            {
                return _pinchScale;
            }
        }

        /// <summary>
        /// タッチの詳細情報。タッチされていないときは(-1, -1)を返します
        /// </summary>
        /// <param name="fingerId">fingerId</param>
        public Vector2 GetTouchPoint(int fingerId)
        {
            return Input.touchCount > fingerId ? Input.GetTouch(fingerId).position : -Vector2.one;
        }

        #endregion

        #region UnityGC：数学API

        /// <summary>
        /// cosを求めます
        /// </summary>
        /// <param name="angle">角度（度数法）</param>
        /// <returns>計算結果</returns>
        public float Cos(float angle)
        {
            return Mathf.Cos(angle * Mathf.Deg2Rad);
        }

        /// <summary>
        /// sinを求めます
        /// </summary>
        /// <param name="angle">角度（度数法）</param>
        /// <returns>計算結果</returns>
        public float Sin(float angle)
        {
            return Mathf.Sin(angle * Mathf.Deg2Rad);
        }

        /// <summary>
        /// atan2 あるいは ベクトルの角度を求めます
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <returns>角度（度数法）</returns>
        public float Atan2(float x, float y)
        {
            return Mathf.Atan2(x, y) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// 角度を弧度法から度数法に変換します
        /// </summary>
        /// <param name="deg">角度（弧度法）</param>
        /// <returns>角度（度数法）</returns>
        public float Deg2Rad(float deg)
        {
            return deg * Mathf.Deg2Rad;
        }

        /// <summary>
        /// 角度を度数法から弧度法に変換します
        /// </summary>
        /// <param name="rad">角度（度数法）</param>
        /// <returns>角度（弧度法）</returns>
        public float Rad2Deg(float rad)
        {
            return rad * Mathf.Rad2Deg;
        }

        #endregion

        #region UnityGC：デバッグAPI

        /// <summary>
        /// デバッグ環境あるいはデバッグビルドで実行されている場合に真を返します
        /// </summary>
        public bool isDebug
        {
            get
            {
                return Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Debug.isDebugBuild;
            }
        }

        /// <summary>
        /// コンソールにログメッセージを出力します。ただし、スマートデバイス実機かつリリースビルドの場合は出力されません
        /// </summary>
        /// <param name="message">ログメッセージ</param>
        public void Trace(string message)
        {
            if (isDebug)
            {
                Debug.Log(message);
            }
        }

        /// <summary>
        /// コンソールにベクトル値を出力します。ただし、スマートデバイス実機かつリリースビルドの場合は出力されません
        /// </summary>
        /// <param name="value">ベクトル値</param>
        public void Trace(Vector2 value)
        {
            Trace(string.Format("x: {0}, y: {1}", value.x, value.y));
        }

        /// <summary>
        /// コンソールに数値を出力します。ただし、スマートデバイス実機かつリリースビルドの場合は出力されません
        /// </summary>
        /// <param name="value">数値</param>
        public void Trace(IEnumerable value)
        {
            Trace(value.ToString());
        }

        /// <summary>
        /// コンソールに真偽値を出力します。ただし、スマートデバイス実機かつリリースビルドの場合は出力されません
        /// </summary>
        /// <param name="value">真偽値</param>
        public void Trace(bool value)
        {
            Trace(value ? "True" : "False");
        }

        #endregion


        /* ---------------------------------------------------------------------------------------------------- */

        #region UnityJava後方互換：定数
        
        /*******************************
            後方互換 - 定数
        *******************************/

        /// <summary>[非推奨] 画面の幅</summary>
        [Obsolete("gc.screenWidth を使用してください"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public int WIDTH
        {
            get { return (int)_canvasScaler.referenceResolution.x; }
        }
        /// <summary>[非推奨] 画面の高さ</summary>
        [Obsolete("gc.screenHeight を使用してください"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public int HEIGHT
        {
            get { return (int)_canvasScaler.referenceResolution.y; }
        }
        /// <summary>[非推奨] フレームレート(FPS)</summary>
        [Obsolete("gc.frameRate を使用してください"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public int CONFIG_FPS
        {
            get { return Application.targetFrameRate; }
        }

        /// <summary>[非推奨] 上ボタン</summary>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly KeyCode KEY_UP = KeyCode.UpArrow;
        /// <summary>[非推奨] 右ボタン</summary>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly KeyCode KEY_RIGHT = KeyCode.RightArrow;
        /// <summary>[非推奨] 下ボタン</summary>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly KeyCode KEY_DOWN = KeyCode.DownArrow;
        /// <summary>[非推奨] 左ボタン</summary>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly KeyCode KEY_LEFT = KeyCode.LeftArrow;

        /// <summary>[非推奨] Zボタン</summary>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly KeyCode KEY_Z = KeyCode.Z;
        /// <summary>[非推奨] Xボタン</summary>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly KeyCode KEY_X = KeyCode.X;
        /// <summary>[非推奨] Cボタン</summary>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly KeyCode KEY_C = KeyCode.C;
        /// <summary>[非推奨] Vボタン</summary>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly KeyCode KEY_V = KeyCode.V;

        /// <summary>[非推奨] ENTERキー</summary>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly KeyCode KEY_ENTER = KeyCode.Return;
        /// <summary>[非推奨] ENTERキー</summary>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly KeyCode KEY_SPACE = KeyCode.Space;

        /// <summary>[非推奨] 白色</summary>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly Color COLOR_WHITE = Color.white;
        /// <summary>[非推奨] 黒色</summary>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly Color COLOR_BLACK = Color.black;
        /// <summary>[非推奨] 灰色</summary>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly Color COLOR_GRAY = Color.gray;
        /// <summary>[非推奨] 赤色</summary>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly Color COLOR_RED = Color.red;
        /// <summary>[非推奨] 黄色</summary>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly Color COLOR_BLUE = Color.blue;
        /// <summary>[非推奨] 緑色</summary>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly Color COLOR_GREEN = Color.green;
        /// <summary>[非推奨] 黄色</summary>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly Color COLOR_YELLOW = Color.yellow;
        /// <summary>[非推奨] 紫色</summary>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly Color COLOR_PURPLE = new Color(1, 0, 1);
        /// <summary>[非推奨] シアン</summary>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly Color COLOR_CYAN = Color.cyan;
        /// <summary>[非推奨] みずいろ</summary>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly Color COLOR_AQUA = new Color(0.5f, 0.5f, 1);

        #endregion

        #region UnityJava後方互換：グラフィックAPI

        /// <summary>
        /// [非推奨] 現在のゲーム画面をキャプチャ―して保存します。
        /// </summary>
        /// <param name="filename">拡張子を除いたファイル名</param>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void writeScreenImage(string filename)
        {
            Application.CaptureScreenshot(filename + ".png");
        }

        /// <summary>[使用禁止]</summary>
        [Obsolete("Java版GameCanvas固有のメソッドです", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void init(object _f, object g) { }

        /// <summary>[使用禁止]</summary>
        [Obsolete("Java版GameCanvas固有のメソッドです", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void finalize() { }

        /// <summary>[使用禁止]</summary>
        [Obsolete("Java版GameCanvas固有のメソッドです", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void setGraphics(object gr, object img) { }

        /// <summary>[使用禁止]</summary>
        /// <param name="title">ウィンドウタイトルに指定する文字列</param>
        [Obsolete("Java版GameCanvas固有のメソッドです", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void setWindowTitle(string title) { }

        /// <summary>
        /// [非推奨] 文字列を描画します
        /// </summary>
        /// <param name="str">描画する文字列</param>
        /// <param name="x">画面左上を原点とするX座標</param>
        /// <param name="y">画面左上を原点とするY座標</param>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void drawString(string str, int x, int y)
        {
            Debug.LogWarning("ToDo");
        }

        public void drawCenterString(string str, int x, int y)
        {
            Debug.LogWarning("ToDo");
        }

        public void drawRightString(string str, int x, int y)
        {
            Debug.LogWarning("ToDo");
        }

        public void setFont(string fontName, int fontStyle, int fontSize)
        {
            Debug.LogWarning("ToDo");
        }

        public void setFontSize(int fontSize)
        {
            Debug.LogWarning("ToDo");
        }

        public int getStringWidth(string str)
        {
            Debug.LogWarning("ToDo");
            return -1;
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void setColor(int color)
        {
            SetColor(new Color(color, color, color));
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void setColor(int red, int green, int blue)
        {
            SetColor(new Color(red, green, blue));
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void drawLine(int sx, int sy, int ex, int ey)
        {
            DrawLine(sx, sy, ex, ey);
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void drawRect(int x, int y, int w, int h)
        {
            DrawRect(x, y, w, h);
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void fillRect(int x, int y, int w, int h)
        {
            FillRect(x, y, w, h);
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void drawCircle(int x, int y, int r)
        {
            DrawCircle(x, y, r);
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void fillCircle(int x, int y, int r)
        {
            FillCircle(x, y, r);
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void drawImage(int id, int x, int y)
        {
            DrawImage(id, x, y);
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void drawClipImage(int id, int x, int y, int u, int v, int w, int h)
        {
            Debug.LogWarning("ToDo");
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void drawScaledRotateImage(int id, int x, int y, int xsize, int ysize, double rotate)
        {
            Debug.LogWarning("ToDo");
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void drawScaledRotateImage(int id, int x, int y, int xsize, int ysize, double rotate, double px, double py)
        {
            Debug.LogWarning("ToDo");
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public int getImageWidth(int id)
        {
            return GetImageWidth(id);
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public int getImageHeight(int id)
        {
            return GetImageHeight(id);
        }

        [Obsolete("Java版GameCanvas固有のメソッドです", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void setSeed(int seed) { }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public int rand(int min, int max)
        {
            return Mathf.FloorToInt(min + UnityEngine.Random.Range(0, 1) * (max - min + 1));
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void resetGame()
        {
            Debug.LogWarning("ToDo");
        }

        /// <summary>[使用禁止]</summary>
        [Obsolete("Java版GameCanvas固有のメソッドです", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void resetGameInstancle(object g) { }

        /// <summary>[使用禁止]</summary>
        [Obsolete("Java版GameCanvas固有のメソッドです", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void updatMessage() { }

        /// <summary>[使用禁止]</summary>
        [Obsolete("Java版GameCanvas固有のメソッドです", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void drawMessage() { }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void clearScreen()
        {
            ClearScreen();
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void exitApp()
        {
            Debug.LogWarning("ToDo");
        }

        /// <summary>[使用禁止]</summary>
        [Obsolete("Java版GameCanvas固有のメソッドです", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool showYesNoDialog(string message) { return false; }

        /// <summary>[使用禁止]</summary>
        [Obsolete("Java版GameCanvas固有のメソッドです", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public string showInputDialog(string message, string defaultInput) { return ""; }

        #endregion

        #region UnityJava後方互換：サウンドAPI

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void playBGM(int id)
        {
            Debug.LogWarning("ToDo");
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void playBGM(int id, bool loop)
        {
            Debug.LogWarning("ToDo");
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void changeBGMVolume(int volume)
        {
            Debug.LogWarning("ToDo");
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void stopBGM()
        {
            Debug.LogWarning("ToDo");
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void pauseBGM()
        {
            Debug.LogWarning("ToDo");
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void playSE(int id)
        {
            Debug.LogWarning("ToDo");
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void playSE(int id, bool loop)
        {
            Debug.LogWarning("ToDo");
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void changeSEVolume(int volume)
        {
            Debug.LogWarning("ToDo");
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void stopSE()
        {
            Debug.LogWarning("ToDo");
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void pauseSE()
        {
            Debug.LogWarning("ToDo");
        }

        #endregion

        #region UnityJava後方互換：入力API

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public int getKeyPressLength(KeyCode key)
        {
            Debug.LogWarning("ToDo");
            return 0;
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool isKeyPress(KeyCode key)
        {
            Debug.LogWarning("ToDo");
            return false;
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool isKeyPushed(KeyCode key)
        {
            Debug.LogWarning("ToDo");
            return false;
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool isKeyReleased(KeyCode key)
        {
            Debug.LogWarning("ToDo");
            return false;
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public int getMouseX()
        {
            return touchX;
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public int getMouseY()
        {
            return touchY;
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public int getMouseClickLength()
        {
            return Mathf.CeilToInt(touchTimeLength / frameRate);
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool isMousePushed()
        {
            return isTouchBegan;
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool isMouseReleased()
        {
            return isTouchEnded;
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool isMousePress()
        {
            return isTouch;
        }

        #endregion

        #region UnityJava後方互換：セーブデータAPI

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public int load(int idx)
        {
            Debug.LogWarning("ToDo");
            return -1;
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void save(int idx, int param)
        {
            Debug.LogWarning("ToDo");
        }

        #endregion

        #region UnityJava後方互換：当たり判定API

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool checkHitRect(int x1, int y1, int w1, int h1, int x2, int y2, int w2, int h2)
        {
            Debug.LogWarning("ToDo");
            return false;
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool checkHitImage(int img1, int x1, int y1, int img2, int x2, int y2)
        {
            Debug.LogWarning("ToDo");
            return false;
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool checkHitCircle(int x1, int y1, int r1, int x2, int y2, int r2)
        {
            Debug.LogWarning("ToDo");
            return false;
        }

        #endregion

        #region UnityJava後方互換：数学API

        /// <summary>[非推奨] 平方根(√)を求めます</summary>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public float sqrt(float data)
        {
            return Mathf.Sqrt(data);
        }

        /// <summary>[非推奨] cosを求めます</summary>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public float cos(float angle)
        {
            return Cos(angle);
        }

        /// <summary>[非推奨] sinを求めます</summary>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public float sin(float angle)
        {
            return Sin(angle);
        }

        /// <summary>[非推奨] atan2(ベクトルの角度)を求めます</summary>
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public float atan2(float x, float y)
        {
            return Atan2(x, y);
        }

        #endregion


        /* ---------------------------------------------------------------------------------------------------- */

        #region 継承元メンバーへのアクセス無効化措置
        // 親クラスのメンバーへのアクセスを無効化する

        // Object
        private new HideFlags hideFlags
        {
            set { base.hideFlags = value; }
            get { return base.hideFlags;  }
        }

        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static void Destroy(UnityEngine.Object obj) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static void Destroy(UnityEngine.Object obj, float t) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static void DestroyImmediate(UnityEngine.Object obj) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static void DestroyImmediate(UnityEngine.Object obj, bool allowDestroyingAssets) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static void DestroyObject(UnityEngine.Object obj) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static void DestroyObject(UnityEngine.Object obj, float t) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static void DontDestroyOnLoad(UnityEngine.Object target) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static UnityEngine.Object FindObjectOfType(Type type) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static T FindObjectOfType<T>() where T : UnityEngine.Object { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static UnityEngine.Object[] FindObjectsOfType(Type type) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static T[] FindObjectsOfType<T>() where T : UnityEngine.Object { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static UnityEngine.Object Instantiate(UnityEngine.Object original) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static UnityEngine.Object Instantiate(UnityEngine.Object original, Vector3 position, Quaternion rotation) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static T Instantiate<T>(T original) where T : UnityEngine.Object { return null; }

        // Component
        private GameObject baseGameObject
        {
            get { return base.gameObject; }
        }
        private string baseTag
        {
            set { base.tag = value; }
            get { return base.tag; }
        }
        private Transform baseTransform
        {
            get { return base.transform; }
        }

        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new string tag;
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component animation;
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component audio;
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component camera;
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component collider;
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component collider2D;
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component constantForce;
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component guiElement;
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component guiText;
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component guiTexture;
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component hingeJoint;
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component light;
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component networkView;
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component particleEmitter;
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component particleSystem;
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component renderer;
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component rigidbody;
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component rigidbody2D;
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new GameObject gameObject;
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new Transform transform;

        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void BroadcastMessage(string methodName) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void BroadcastMessage(string methodName, object parameter) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void BroadcastMessage(string methodName, SendMessageOptions options) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void BroadcastMessage(string methodName, object parameter, SendMessageOptions options) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool CompareTag(string tag) { return false; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component GetComponent(Type type) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component GetComponent(string type) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T GetComponent<T>() { return default(T); }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component GetComponentInChildren(Type t) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component GetComponentInChildren(Type t, bool includeInactive) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T GetComponentInChildren<T>() { return default(T); }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T GetComponentInChildren<T>(bool includeInactive) { return default(T); }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component GetComponentInParent(Type t) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T GetComponentInParent<T>() { return default(T); }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component[] GetComponents(Type type) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void GetComponents(Type type, List<UnityEngine.Component> results) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T[] GetComponents<T>() { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void GetComponents<T>(List<T> results) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component[] GetComponentsInChildren(Type t) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component[] GetComponentsInChildren(Type t, bool includeInactive) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T[] GetComponentsInChildren<T>() { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T[] GetComponentsInChildren<T>(bool includeInactive) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void GetComponentsInChildren<T>(List<T> results) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void GetComponentsInChildren<T>(bool includeInactive, List<T> result) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component[] GetComponentsInParent(Type t) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component[] GetComponentsInParent(Type t, bool includeInactive) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T[] GetComponentsInParent<T>() { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T[] GetComponentsInParent<T>(bool includeInactive) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void GetComponentsInParent<T>(bool includeInactive, List<T> results) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void SendMessage(string methodName) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void SendMessage(string methodName, object value) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void SendMessage(string methodName, SendMessageOptions options) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void SendMessage(string methodName, object value, SendMessageOptions options) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void SendMessageUpwards(string methodName) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void SendMessageUpwards(string methodName, SendMessageOptions options) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void SendMessageUpwards(string methodName, object value) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void SendMessageUpwards(string methodName, object value, SendMessageOptions options) { }

        // Behaviour

        private bool baseEnabled
        {
            set { base.enabled = value; }
            get { return base.enabled;  }
        }
        
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool enabled;
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool isActiveAndEnabled;

        // MonoBehaviour
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool useGUILayout;
        /// <summary>使用禁止</summary>
        //[Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        //public new static void print(object message) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void CancelInvoke() { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void CancelInvoke(string methodName) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void Invoke(string methodName, float time) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void InvokeRepeating(string methodName, float time, float repeatRate) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool IsInvoking() { return false; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool IsInvoking(string methodName) { return false; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new Coroutine StartCoroutine(IEnumerator routine) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new Coroutine StartCoroutine(string methodName) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new Coroutine StartCoroutine(string methodName, object value) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new Coroutine StartCoroutine_Auto(IEnumerator routine) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void StopAllCoroutines() { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void StopCoroutine(string methodName) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void StopCoroutine(IEnumerator routine) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void StopCoroutine(Coroutine routine) { }

        #endregion
    }
}
