using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace GameCanvas
{
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

        private int         _deviceWidth            = 0;            // 画面解像度：横幅
        private int         _deviceHeight           = 0;            // 画面解像度：縦幅
        private int         _canvasWidth            = 0;            // 描画解像度：横幅
        private int         _canvasHeight           = 0;            // 描画解像度：縦幅
        private float       _canvasDisplayScale     = 1f;           // キャンバス解像度 / デバイス解像度
        private Vector2     _canvasBorderSize       = Vector2.zero; // キャンバス左・上の黒縁の大きさ

        private Color       _palletColor            = Color.black;  // 現在のパレットカラー
        private int         _lineWidth              = 1;            // 現在の線の太さ

        private int         _numImage               = 0;            // 認識済みの画像：数量
        private Texture2D[] _imageArray             = null;         // 認識済みの画像：データ配列
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
        
        private Material    _materialInit           = null;         // マテリアル：初期化
        private Material    _materialDrawCircle     = null;         // マテリアル：図形描画：円
        private Material    _materialDrawRect       = null;         // マテリアル：図形描画：矩形
        private Material    _materialDrawImage      = null;         // マテリアル：画像描画
        private RenderTexture _canvasRender         = null;         // レンダーテクスチャー
        private MeshRenderer _quad                  = null;         // プリミティブ：Quad
        private Camera      _camera                 = null;         // コンポーネント：Camera

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
                _camera.orthographicSize = Screen.height * 0.5f;
                _camera.depth = -1;

                obj.AddComponent<AudioListener>();
            }

            // Quadプリミティブの配置。2D描画の表示先として用いる
            {
                var obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                obj.name = "Canvas";
                obj.transform.parent = baseTransform;
                
                _materialInit       = new Material(Shader.Find("Custom/GameCanvas/Init"));
                _materialDrawCircle = new Material(Shader.Find("Custom/GameCanvas/DrawCircle"));
                _materialDrawRect   = new Material(Shader.Find("Custom/GameCanvas/DrawRect"));
                _materialDrawImage  = new Material(Shader.Find("Custom/GameCanvas/DrawImage"));

                _quad = obj.GetComponent<MeshRenderer>();
                _quad.material.shader = Shader.Find("Unlit/Texture");
                // [memo] SetResolution()で設定する
                // _quad.material.mainTexture = _canvasRender;
            }

            // キャンバスの初期化
            SetResolution(640, 480);

            // 外部画像・音源データの読み込み
            {
                _imageArray = Resources.LoadAll<Texture2D>("");
                _numImage   = _imageArray.Length;
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
            // 画面サイズ判定
            if (_deviceWidth != Screen.width || _deviceHeight != Screen.height)
            {
                UpdateDisplayScale();
            }

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

        private void UpdateDisplayScale()
        {
            // 表示倍率と黒縁サイズの計算
            _deviceWidth  = Screen.width;
            _deviceHeight = Screen.height;
            var scaleW = (float)_canvasWidth  / _deviceWidth;
            var scaleH = (float)_canvasHeight / _deviceHeight;
            _canvasDisplayScale = Mathf.Max(scaleW, scaleH);
            _canvasBorderSize.x = (_deviceWidth  - _canvasWidth  / _canvasDisplayScale) * 0.5f;
            _canvasBorderSize.y = (_deviceHeight - _canvasHeight / _canvasDisplayScale) * 0.5f;

            // カメラ倍率
            _camera.orthographicSize = _deviceHeight * _canvasDisplayScale * 0.5f;
        }

        #endregion


        /* ---------------------------------------------------------------------------------------------------- */

        #region UnityGC：グラフィックAPI (基本図形)

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
        /// DrawString や DrawRect などで用いる色を指定します
        /// </summary>
        /// <param name="color">塗りの色</param>
        public void SetColor(Color color)
        {
            _palletColor = color;
        }

        /// <summary>
        /// DrawString や DrawRect などで用いる色を指定します
        /// </summary>
        /// <param name="red">赤成分 [0～1]</param>
        /// <param name="green">緑成分 [0～1]</param>
        /// <param name="blue">青成分 [0～1]</param>
        /// <param name="alpha">不透明度 [0～1]</param>
        public void SetColor(float red, float green, float blue, float alpha = 1f)
        {
            SetColor(new Color(red, green, blue, alpha));
        }

        /// <summary>
        /// DrawString や DrawRect などで用いる色を、HSV色空間で指定します
        /// </summary>
        /// <param name="h">hue [0～1]</param>
        /// <param name="s">saturation [0～1]</param>
        /// <param name="v">calue [0～1]</param>
        /// <param name="alpha">不透明度 [0～1]</param>
        public void SetColorHSV(float h, float s, float v, float alpha = 1f)
        {
            var c = Color.HSVToRGB(h, s, v);
            c.a = alpha;
            SetColor(c);
        }

        /// <summary>
        /// DrawRect や DrawCircle などに用いる線の太さを指定します
        /// </summary>
        /// <param name="lineWidth"></param>
        public void SetLineWidth(int lineWidth)
        {
            if (lineWidth <= 0)
            {
                // 0以下は許容しない
                Debug.LogWarning("引数の値が不正です");
                return;
            }

            _lineWidth = lineWidth;
        }

        /// <summary>
        /// 線分を描画します
        /// </summary>
        /// <param name="startX">開始点のX座標</param>
        /// <param name="startY">開始点のY座標</param>
        /// <param name="endX">終了点のX座標</param>
        /// <param name="endY">終了点のY座標</param>
        public void DrawLine(float startX, float startY, float endX, float endY)
        {
            var diffX = endX - startX;
            var diffY = endY - startY;
            FillRotatedRect(startX, startY, Mathf.RoundToInt(Mathf.Sqrt(diffX * diffX + diffY * diffY)), _lineWidth, Atan2(diffX, diffY), 0f, _lineWidth * 0.5f);
        }

        /// <summary>
        /// 中抜きの円を描画します
        /// </summary>
        /// <param name="x">中心点のX座標</param>
        /// <param name="y">中心点のY座標</param>
        /// <param name="radius">半径</param>
        /// <param name="lineWidth">線の太さ</param>
        public void DrawCircle(float x, float y, int radius)
        {
            if (radius <= 0)
            {
                // 0以下は許容しない
                Debug.LogWarning("引数の値が不正です");
                return;
            }

            var mat = Matrix4x4.TRS(new Vector3(x, y, 0f), Quaternion.identity, new Vector3(radius, radius, 1f));

            _materialDrawCircle.SetColor("_Color", _palletColor);
            _materialDrawCircle.SetMatrix("_Matrix", mat.inverse);
            _materialDrawCircle.SetFloat("_IsFill", 0);
            _materialDrawCircle.SetFloat("_LineWidth", (float)_lineWidth / radius * 0.5f);

            var temp = RenderTexture.GetTemporary(_canvasWidth, _canvasHeight, 0);
            Graphics.Blit(_canvasRender, temp);
            Graphics.Blit(temp, _canvasRender, _materialDrawCircle);
            RenderTexture.ReleaseTemporary(temp);
        }

        /// <summary>
        /// 中抜きの長方形を描画します
        /// </summary>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="width">横幅</param>
        /// <param name="height">縦幅</param>
        public void DrawRect(float x, float y, float width, float height)
        {
            DrawRotatedRect(x, y, width, height, 0);
        }

        /// <summary>
        /// 中抜きの回転させた長方形を描画します
        /// </summary>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="width">横幅</param>
        /// <param name="height">縦幅</param>
        /// <param name="angle">回転角度 (度数法)</param>
        /// <param name="rotationX">長方形の横幅を1としたときの回転の中心位置X</param>
        /// <param name="rotationY">長方形の高さを1としたときの回転の中心位置Y</param>
        public void DrawRotatedRect(float x, float y, float width, float height, float angle, float rotationX = 0f, float rotationY = 0f)
        {
            if (width < 1 || height < 1)
            {
                // 負の幅は許容しない
                Debug.LogWarning("引数の値が不正です");
                return;
            }

            Matrix4x4 mat;
            if (rotationX == 0 && rotationY == 0)
            {
                mat = Matrix4x4.TRS(new Vector3(x, y, 0f), Quaternion.AngleAxis(angle, Vector3.forward), new Vector3(width, height, 1f));
            }
            else
            {
                mat = Matrix4x4.TRS(new Vector3(x + rotationX, y + rotationY, 0f), Quaternion.AngleAxis(angle, Vector3.forward), Vector3.one);
                mat *= Matrix4x4.TRS(new Vector3(-rotationX, -rotationY, 0f), Quaternion.identity, new Vector3(width, height, 1f));
            }

            _materialDrawRect.SetColor("_Color", _palletColor);
            _materialDrawRect.SetMatrix("_Matrix", mat.inverse);
            _materialDrawRect.SetFloat("_IsFill", 0);
            _materialDrawRect.SetFloat("_LineWidth", _lineWidth);

            var temp = RenderTexture.GetTemporary(_canvasWidth, _canvasHeight, 0);
            Graphics.Blit(_canvasRender, temp);
            Graphics.Blit(temp, _canvasRender, _materialDrawRect);
            RenderTexture.ReleaseTemporary(temp);
        }

        /// <summary>
        /// 画像を描画します
        /// </summary>
        /// <param name="id">描画する画像のID。例えば、ファイル名が img0.png ならば、画像IDは 0</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        public void DrawImage(int id, float x, float y)
        {
            DrawImageSRT(id, x, y, 1f, 1f, 0f);
        }

        /// <summary>
        /// 一部分を切り取った画像を描画します
        /// </summary>
        /// <param name="id">描画する画像のID。例えば、ファイル名が img0.png ならば、画像IDは 0</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="clipTop">画像上側の切り取る縦幅</param>
        /// <param name="clipRight">画像右側の切り取る横幅</param>
        /// <param name="clipBottom">画像下側の切り取る縦幅</param>
        /// <param name="clipLeft">画像左側の切り取る横幅</param>
        public void DrawClippedImage(int id, float x, float y, float clipTop, float clipRight, float clipBottom, float clipLeft)
        {
            DrawClippedImageSRT(id, x, y, clipTop, clipRight, clipBottom, clipLeft, 1f, 1f, 0f);
        }

        /// <summary>
        /// 大きさを変えた画像を描画します
        /// </summary>
        /// <param name="id">描画する画像のID。例えば、ファイル名が img0.png ならば、画像IDは 0</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="scaleH">横の拡縮率</param>
        /// <param name="scaleV">縦の拡縮率</param>
        public void DrawScaledImage(int id, float x, float y, float scaleH, float scaleV)
        {
            DrawImageSRT(id, x, y, scaleH, scaleV, 0f);
        }

        /// <summary>
        /// 回転させた画像を描画します
        /// </summary>
        /// <param name="id">描画する画像のID。例えば、ファイル名が img0.png ならば、画像IDは 0</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="angle">回転角度 (度数法)</param>
        /// <param name="rotationX">画像の横幅を1としたときの回転の中心位置X</param>
        /// <param name="rotationY">画像の高さを1としたときの回転の中心位置Y</param>
        public void DrawRotatedImage(int id, float x, float y, float angle, float rotationX = 0f, float rotationY = 0f)
        {
            DrawImageSRT(id, x, y, 1f, 1f, angle, rotationX, rotationY);
        }

        /// <summary>
        /// 画像を位置・拡縮率・回転角度を指定して描画します
        /// </summary>
        /// <param name="id">描画する画像のID。例えば、ファイル名が img0.png ならば、画像IDは 0</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="scaleH">縦の拡縮率</param>
        /// <param name="scaleV">横の拡縮率</param>
        /// <param name="angle">回転角度 (度数法)</param>
        /// <param name="rotationX">画像の横幅を1としたときの回転の中心位置X</param>
        /// <param name="rotationY">画像の高さを1としたときの回転の中心位置Y</param>
        public void DrawImageSRT(int id, float x, float y, float scaleH, float scaleV, float angle, float rotationX = 0f, float rotationY = 0f)
        {
            DrawClippedImageSRT(id, x, y, 0, 0, 0, 0, scaleH, scaleV, angle, rotationX, rotationY);
        }

        /// <summary>
        /// 一部分を切り取った画像を、位置・拡縮率・回転角度を指定して描画します
        /// </summary>
        /// <param name="id">描画する画像のID。例えば、ファイル名が img0.png ならば、画像IDは 0</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="clipTop">画像上側の切り取る縦幅</param>
        /// <param name="clipRight">画像右側の切り取る横幅</param>
        /// <param name="clipBottom">画像下側の切り取る縦幅</param>
        /// <param name="clipLeft">画像左側の切り取る横幅</param>
        /// <param name="scaleH">縦の拡縮率</param>
        /// <param name="scaleV">横の拡縮率</param>
        /// <param name="angle">回転角度 (度数法)</param>
        /// <param name="rotationX">画像の横幅を1としたときの回転の中心位置X</param>
        /// <param name="rotationY">画像の高さを1としたときの回転の中心位置Y</param>
        public void DrawClippedImageSRT(int id, float x, float y, float clipTop, float clipRight, float clipBottom, float clipLeft, float scaleH, float scaleV, float angle, float rotationX = 0f, float rotationY = 0f)
        {
            if (id >= _numImage)
            {
                Debug.LogWarning("存在しないファイルが指定されました");
                return;
            }

            if (clipLeft < 0 || clipTop < 0 || clipRight < 0 || clipBottom < 0)
            {
                // 負の切り取り幅は許容しない
                Debug.LogWarning("引数の値が不正です");
                return;
            }

            if (scaleH == 0 || scaleV == 0)
            {
                // ゼロの拡縮率は許容しない
                Debug.LogWarning("引数の値が不正です");
                return;
            }

            if (x >= _canvasWidth || y >= _canvasHeight)
            {
                // 描画範囲外である
                return;
            }

            Matrix4x4 mat;
            if (rotationX == 0 && rotationY == 0)
            {
                mat = Matrix4x4.TRS(new Vector3(x, y, 0f), Quaternion.AngleAxis(angle, Vector3.forward), new Vector3(scaleH, scaleV, 1f));
            }
            else
            {
                mat = Matrix4x4.TRS(new Vector3(x + rotationX, y + rotationY, 0f), Quaternion.AngleAxis(angle, Vector3.forward), Vector3.one);
                mat *= Matrix4x4.TRS(new Vector3(-rotationX, -rotationY, 0f), Quaternion.identity, new Vector3(scaleH, scaleV, 1f));
            }

            _materialDrawImage.SetTexture("_ImageTex", _imageArray[id]);
            _materialDrawImage.SetColor("_Color", _palletColor);
            _materialDrawImage.SetMatrix("_Matrix", mat.inverse);
            _materialDrawImage.SetVector("_Clip", new Vector4(clipLeft, clipTop, clipRight, clipBottom));

            var temp = RenderTexture.GetTemporary(_canvasWidth, _canvasHeight, 0);
            Graphics.Blit(_canvasRender, temp);
            Graphics.Blit(temp, _canvasRender, _materialDrawImage);
            RenderTexture.ReleaseTemporary(temp);
        }

        /// <summary>
        /// 塗りつぶしの円を描画します
        /// </summary>
        /// <param name="x">中心点のX座標</param>
        /// <param name="y">中心点のY座標</param>
        /// <param name="radius">半径</param>
        public void FillCircle(float x, float y, int radius)
        {
            if (radius < 1)
            {
                // 負の半径は許容しない
                Debug.LogWarning("引数の値が不正です");
                return;
            }

            var mat = Matrix4x4.TRS(new Vector3(x, y, 0f), Quaternion.identity, new Vector3(radius, radius, 1f));

            _materialDrawCircle.SetColor("_Color", _palletColor);
            _materialDrawCircle.SetMatrix("_Matrix", mat.inverse);
            _materialDrawCircle.SetFloat("_IsFill", 1);

            var temp = RenderTexture.GetTemporary(_canvasWidth, _canvasHeight, 0);
            Graphics.Blit(_canvasRender, temp);
            Graphics.Blit(temp, _canvasRender, _materialDrawCircle);
            RenderTexture.ReleaseTemporary(temp);
        }

        /// <summary>
        /// 塗りつぶしの長方形を描画します
        /// </summary>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="width">横幅</param>
        /// <param name="height">縦幅</param>
        public void FillRect(float x, float y, float width, float height)
        {
            FillRotatedRect(x, y, width, height, 0);
        }

        /// <summary>
        /// 塗りつぶしの回転させた長方形を描画します
        /// </summary>
        /// <param name="x">左上のX座標</param>
        /// <param name="y">左上のY座標</param>
        /// <param name="width">横幅</param>
        /// <param name="height">縦幅</param>
        /// <param name="angle">回転角度 (度数法)</param>
        /// <param name="rotationX">長方形の横幅を1としたときの回転の中心位置X</param>
        /// <param name="rotationY">長方形の高さを1としたときの回転の中心位置Y</param>
        public void FillRotatedRect(float x, float y, float width, float height, float angle, float rotationX = 0f, float rotationY = 0f)
        {
            if (width < 1 || height < 1)
            {
                // 負の幅は許容しない
                Debug.LogWarning("引数の値が不正です");
                return;
            }

            Matrix4x4 mat;
            if (rotationX == 0 && rotationY == 0)
            {
                mat = Matrix4x4.TRS(new Vector3(x, y, 0f), Quaternion.AngleAxis(angle, Vector3.forward), new Vector3(width, height, 1f));
            }
            else
            {
                mat = Matrix4x4.TRS(new Vector3(x + rotationX, y + rotationY, 0f), Quaternion.AngleAxis(angle, Vector3.forward), Vector3.one);
                mat *= Matrix4x4.TRS(new Vector3(-rotationX, -rotationY, 0f), Quaternion.identity, new Vector3(width, height, 1f));
            }

            _materialDrawRect.SetColor("_Color", _palletColor);
            _materialDrawRect.SetMatrix("_Matrix", mat.inverse);
            _materialDrawRect.SetFloat("_IsFill", 1);

            var temp = RenderTexture.GetTemporary(_canvasWidth, _canvasHeight, 0);
            Graphics.Blit(_canvasRender, temp);
            Graphics.Blit(temp, _canvasRender, _materialDrawRect);
            RenderTexture.ReleaseTemporary(temp);
        }

        #endregion

        #region UnityGC：グラフィックAPI (その他)

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
        /// ゲームの解像度を設定します
        /// </summary>
        /// <param name="width">X軸方向の解像度（幅）</param>
        /// <param name="height">Y軸方向の解像度（高さ）</param>
        public void SetResolution(int width, int height)
        {
            if (_canvasWidth == width && _canvasHeight == height) return;

            _canvasWidth = width;
            _canvasHeight = height;

            // 自動回転の再設定。回転固定設定は引き継がれる
            isScreenAutoRotation = isScreenAutoRotation;

            UpdateDisplayScale();

            // キャンバスの再生成
            if (_canvasRender != null) _canvasRender.Release();
            _canvasRender = new RenderTexture(_canvasWidth, _canvasHeight, 0);
            _canvasRender.Create();
            _quad.transform.localScale = new Vector3(_canvasWidth, -_canvasHeight, 1f);
            _quad.material.mainTexture = _canvasRender;

            ClearScreen();
        }

        /// <summary>
        /// 画面を白で塗りつぶします
        /// </summary>
        public void ClearScreen()
        {
            Graphics.Blit(null, _canvasRender, _materialInit);
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

        #region UnityGC：入力API (加速度)

        /// <summary>
        /// 加速度センサーで測定されたX軸の加速度
        /// </summary>
        public float acceX
        {
            get { return Input.acceleration.x; }
        }

        /// <summary>
        /// 加速度センサーで測定されたY軸の加速度
        /// </summary>
        public float acceY
        {
            get { return -Input.acceleration.y; }
        }

        /// <summary>
        /// 加速度センサーで測定されたZ軸の加速度
        /// </summary>
        public float acceZ
        {
            get { return -Input.acceleration.z; }
        }

        #endregion

        #region UnityGC：入力API (ジャイロスコープ)

        /// <summary>
        /// ジャイロスコープが有効かどうか
        /// </summary>
        public bool isGyroEnabled
        {
            set { Input.gyro.enabled = value; }
            get { return Input.gyro.enabled;  }
        }

        /// <summary>
        /// ジャイロスコープで測定されたX軸の回転率
        /// </summary>
        public float gyroX
        {
            get { return Input.gyro.rotationRateUnbiased.x; }
        }

        /// <summary>
        /// ジャイロスコープで測定されたY軸の回転率
        /// </summary>
        public float gyroY
        {
            get { return Input.gyro.rotationRateUnbiased.y; }
        }

        /// <summary>
        /// ジャイロスコープで測定されたZ軸の回転率
        /// </summary>
        public float gyroZ
        {
            get { return Input.gyro.rotationRateUnbiased.z; }
        }

        #endregion

        #region UnityGC：入力API (コンパス)

        /// <summary>
        /// 地磁気センサーが有効かどうか
        /// </summary>
        public bool isCompassEnabled
        {
            set { Input.compass.enabled = value; }
            get { return Input.compass.enabled;  }
        }

        /// <summary>
        /// 地磁気センサーで測定された磁北極方向への回転角度 (度数法)
        /// </summary>
        public float compass
        {
            get { return Input.compass.magneticHeading; }
        }

        #endregion

        #region UnityGC：入力API (位置情報)

        /// <summary>
        /// 位置情報の取得が有効かどうか。有効でない場合、ユーザーに許可を求めるダイアログが表示される場合があります
        /// </summary>
        public bool isLocationEnabled
        {
            get { return Input.location.isEnabledByUser; }
        }

        /// <summary>
        /// 位置情報の取得が正常に行われているかどうか
        /// </summary>
        public bool isRunningLocaltionService
        {
            get { return Input.location.status == LocationServiceStatus.Running; }
        }

        /// <summary>
        /// 最後に測定した場所の緯度情報
        /// </summary>
        public float lastLocationLatitude
        {
            get { return Input.location.lastData.latitude; }
        }

        /// <summary>
        /// 最後に測定した場所の経度情報
        /// </summary>
        public float lastLocationLongitude
        {
            get { return Input.location.lastData.longitude; }
        }

        /// <summary>
        /// 最後に位置情報を取得した時間から現在までの経過秒数
        /// </summary>
        public float lastLocationTime
        {
            get { return (float)(DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds - Input.location.lastData.timestamp); }
        }

        /// <summary>
        /// 位置情報の測定を開始します。この操作は一般に多くの電力を消費します
        /// </summary>
        public void StartLocationService()
        {
            Input.location.Start(5f, 5f);
        }

        /// <summary>
        /// 位置情報の測定を終了します
        /// </summary>
        public void StopLocationService()
        {
            Input.location.Stop();
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
            return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// 角度を度数法から弧度法に変換します
        /// </summary>
        /// <param name="degree">角度（度数法）</param>
        /// <returns>角度（弧度法）</returns>
        public float Deg2Rad(float degree)
        {
            return degree * Mathf.Deg2Rad;
        }

        /// <summary>
        /// 角度を弧度法から度数法に変換します
        /// </summary>
        /// <param name="radian">角度（弧度法）</param>
        /// <returns>角度（度数法）</returns>
        public float Rad2Deg(float radian)
        {
            return radian * Mathf.Rad2Deg;
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
        public void Trace(IComparable value)
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
        
        [Obsolete("gc.screenWidth を使用してください"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public int WIDTH
        {
            get { return _canvasWidth; }
        }
        [Obsolete("gc.screenHeight を使用してください"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public int HEIGHT
        {
            get { return _canvasHeight; }
        }
        [Obsolete("gc.frameRate を使用してください"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public int CONFIG_FPS
        {
            get { return Application.targetFrameRate; }
        }
        
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly KeyCode KEY_UP = KeyCode.UpArrow;
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly KeyCode KEY_RIGHT = KeyCode.RightArrow;
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly KeyCode KEY_DOWN = KeyCode.DownArrow;
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly KeyCode KEY_LEFT = KeyCode.LeftArrow;
        
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly KeyCode KEY_Z = KeyCode.Z;
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly KeyCode KEY_X = KeyCode.X;
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly KeyCode KEY_C = KeyCode.C;
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly KeyCode KEY_V = KeyCode.V;
        
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly KeyCode KEY_ENTER = KeyCode.Return;
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly KeyCode KEY_SPACE = KeyCode.Space;
        
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly Color COLOR_WHITE = Color.white;
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly Color COLOR_BLACK = Color.black;
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly Color COLOR_GRAY = Color.gray;
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly Color COLOR_RED = Color.red;
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly Color COLOR_BLUE = Color.blue;
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly Color COLOR_GREEN = Color.green;
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly Color COLOR_YELLOW = Color.yellow;
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly Color COLOR_PURPLE = new Color(1, 0, 1);
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly Color COLOR_CYAN = Color.cyan;
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public readonly Color COLOR_AQUA = new Color(0.5f, 0.5f, 1);

        #endregion

        #region UnityJava後方互換：グラフィックAPI
        
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void writeScreenImage(string filename)
        {
            Application.CaptureScreenshot(filename + ".png");
        }
        
        [Obsolete("Java版GameCanvas固有のメソッドです", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void init(object _f, object g) { }

        [Obsolete("Java版GameCanvas固有のメソッドです", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void finalize() { }
        
        [Obsolete("Java版GameCanvas固有のメソッドです", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void setGraphics(object gr, object img) { }
        
        [Obsolete("Java版GameCanvas固有のメソッドです", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void setWindowTitle(string title) { }
        
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void drawString(string str, float x, float y)
        {
            Debug.LogWarning("ToDo");
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void drawCenterString(string str, float x, float y)
        {
            Debug.LogWarning("ToDo");
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void drawRightString(string str, float x, float y)
        {
            Debug.LogWarning("ToDo");
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void setFont(string fontName, int fontStyle, int fontSize)
        {
            Debug.LogWarning("ToDo");
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void setFontSize(int fontSize)
        {
            Debug.LogWarning("ToDo");
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public int getStringWidth(string str)
        {
            Debug.LogWarning("ToDo");
            return -1;
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void setColor(int color)
        {
            var c = (float)color / 255;
            SetColor(new Color(c, c, c));
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void setColor(int red, int green, int blue)
        {
            var k = 1f / 255;
            SetColor(new Color(red * k, green * k, blue * k));
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void drawLine(int sx, int sy, int ex, int ey)
        {
            DrawLine(sx, sy, ex, ey);
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void drawRect(float x, float y, int w, int h)
        {
            DrawRect(x, y, w, h);
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void fillRect(float x, float y, int w, int h)
        {
            FillRect(x, y, w, h);
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void drawCircle(float x, float y, int r)
        {
            DrawCircle(x, y, r);
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void fillCircle(float x, float y, int r)
        {
            FillCircle(x, y, r);
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void drawImage(int id, float x, float y)
        {
            DrawImage(id, x, y);
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void drawClipImage(int id, float x, float y, int u, int v, int w, int h)
        {
            DrawClippedImage(id, x, y, u, v, GetImageWidth(id) - w - u, GetImageHeight(id) - h - v);
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void drawScaledRotateImage(int id, float x, float y, int xsize, int ysize, double rotate)
        {
            DrawImageSRT(id, x, y, xsize * 0.01f, ysize * 0.01f, (float)rotate);
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void drawScaledRotateImage(int id, float x, float y, int xsize, int ysize, double rotate, double px, double py)
        {
            DrawImageSRT(id, x, y, xsize * 0.01f, ysize * 0.01f, (float)rotate, (int)px, (int)py);
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

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void clearScreen()
        {
            ClearScreen();
        }

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
        
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public float sqrt(float data)
        {
            return Mathf.Sqrt(data);
        }
        
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public float cos(float angle)
        {
            return Cos(angle);
        }
        
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public float sin(float angle)
        {
            return Sin(angle);
        }
        
        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public float atan2(float x, float y)
        {
            return Atan2(x, y);
        }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public int rand(int min, int max)
        {
            return Mathf.FloorToInt(min + UnityEngine.Random.value * (max - min + 1));
        }

        #endregion

        #region UnityJava後方互換：その他
        
        [Obsolete("Java版GameCanvas固有のメソッドです", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void setSeed(int seed) { }
        
        [Obsolete("Java版GameCanvas固有のメソッドです", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void resetGame() { }
        
        [Obsolete("Java版GameCanvas固有のメソッドです", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void resetGameInstancle(object g) { }
        
        [Obsolete("Java版GameCanvas固有のメソッドです", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void updatMessage() { }
        
        [Obsolete("Java版GameCanvas固有のメソッドです", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void drawMessage() { }

        [Obsolete, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void exitApp()
        {
            Application.Quit();
        }
        
        [Obsolete("Java版GameCanvas固有のメソッドです", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool showYesNoDialog(string message) { return false; }
        
        [Obsolete("Java版GameCanvas固有のメソッドです", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public string showInputDialog(string message, string defaultInput) { return ""; }

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
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static void Destroy(UnityEngine.Object obj) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static void Destroy(UnityEngine.Object obj, float t) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static void DestroyImmediate(UnityEngine.Object obj) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static void DestroyImmediate(UnityEngine.Object obj, bool allowDestroyingAssets) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static void DestroyObject(UnityEngine.Object obj) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static void DestroyObject(UnityEngine.Object obj, float t) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static void DontDestroyOnLoad(UnityEngine.Object target) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static UnityEngine.Object FindObjectOfType(Type type) { return null; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static T FindObjectOfType<T>() where T : UnityEngine.Object { return null; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static UnityEngine.Object[] FindObjectsOfType(Type type) { return null; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static T[] FindObjectsOfType<T>() where T : UnityEngine.Object { return null; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static UnityEngine.Object Instantiate(UnityEngine.Object original) { return null; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static UnityEngine.Object Instantiate(UnityEngine.Object original, Vector3 position, Quaternion rotation) { return null; }
        
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
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new string tag;
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component animation;
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component audio;
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component camera;
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component collider;
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component collider2D;
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component constantForce;
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component guiElement;
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component guiText;
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component guiTexture;
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component hingeJoint;
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component light;
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component networkView;
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component particleEmitter;
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component particleSystem;
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component renderer;
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component rigidbody;
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component rigidbody2D;
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new GameObject gameObject;
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new Transform transform;
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void BroadcastMessage(string methodName) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void BroadcastMessage(string methodName, object parameter) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void BroadcastMessage(string methodName, SendMessageOptions options) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void BroadcastMessage(string methodName, object parameter, SendMessageOptions options) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool CompareTag(string tag) { return false; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component GetComponent(Type type) { return null; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component GetComponent(string type) { return null; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T GetComponent<T>() { return default(T); }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component GetComponentInChildren(Type t) { return null; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component GetComponentInChildren(Type t, bool includeInactive) { return null; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T GetComponentInChildren<T>() { return default(T); }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T GetComponentInChildren<T>(bool includeInactive) { return default(T); }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component GetComponentInParent(Type t) { return null; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T GetComponentInParent<T>() { return default(T); }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component[] GetComponents(Type type) { return null; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void GetComponents(Type type, List<UnityEngine.Component> results) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T[] GetComponents<T>() { return null; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void GetComponents<T>(List<T> results) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component[] GetComponentsInChildren(Type t) { return null; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component[] GetComponentsInChildren(Type t, bool includeInactive) { return null; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T[] GetComponentsInChildren<T>() { return null; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T[] GetComponentsInChildren<T>(bool includeInactive) { return null; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void GetComponentsInChildren<T>(List<T> results) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void GetComponentsInChildren<T>(bool includeInactive, List<T> result) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component[] GetComponentsInParent(Type t) { return null; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component[] GetComponentsInParent(Type t, bool includeInactive) { return null; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T[] GetComponentsInParent<T>() { return null; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T[] GetComponentsInParent<T>(bool includeInactive) { return null; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void GetComponentsInParent<T>(bool includeInactive, List<T> results) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void SendMessage(string methodName) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void SendMessage(string methodName, object value) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void SendMessage(string methodName, SendMessageOptions options) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void SendMessage(string methodName, object value, SendMessageOptions options) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void SendMessageUpwards(string methodName) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void SendMessageUpwards(string methodName, SendMessageOptions options) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void SendMessageUpwards(string methodName, object value) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void SendMessageUpwards(string methodName, object value, SendMessageOptions options) { }

        // Behaviour
        private bool baseEnabled
        {
            set { base.enabled = value; }
            get { return base.enabled;  }
        }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool enabled;
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool isActiveAndEnabled;

        // MonoBehaviour
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool useGUILayout;
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static void print(object message) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void CancelInvoke() { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void CancelInvoke(string methodName) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void Invoke(string methodName, float time) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void InvokeRepeating(string methodName, float time, float repeatRate) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool IsInvoking() { return false; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool IsInvoking(string methodName) { return false; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new Coroutine StartCoroutine(IEnumerator routine) { return null; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new Coroutine StartCoroutine(string methodName) { return null; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new Coroutine StartCoroutine(string methodName, object value) { return null; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new Coroutine StartCoroutine_Auto(IEnumerator routine) { return null; }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void StopAllCoroutines() { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void StopCoroutine(string methodName) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void StopCoroutine(IEnumerator routine) { }
        
        [Obsolete("このメソッドをGameCanvasから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void StopCoroutine(Coroutine routine) { }

        #endregion
    }
}
