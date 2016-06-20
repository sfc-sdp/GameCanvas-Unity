using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace GameCanvas
{
    public class GameCanvas : SingletonMonoBehaviour<GameCanvas>
    {
        /// <summary>画面の幅</summary>
        public const int WIDTH = 640;
        /// <summary>画面の高さ</summary>
        public const int HEIGHT = 480;
        /// <summary>フレームレート(FPS)</summary>
        public const int CONFIG_FPS = 30;

        /// <summary>[非推奨] 上ボタン</summary>
        public const KeyCode KEY_UP = KeyCode.UpArrow;
        /// <summary>[非推奨] 右ボタン</summary>
        public const KeyCode KEY_RIGHT = KeyCode.RightArrow;
        /// <summary>[非推奨] 下ボタン</summary>
        public const KeyCode KEY_DOWN = KeyCode.DownArrow;
        /// <summary>[非推奨] 左ボタン</summary>
        public const KeyCode KEY_LEFT = KeyCode.LeftArrow;

        /// <summary>[非推奨] Zボタン</summary>
        public const KeyCode KEY_Z = KeyCode.Z;
        /// <summary>[非推奨] Xボタン</summary>
        public const KeyCode KEY_X = KeyCode.X;
        /// <summary>[非推奨] Cボタン</summary>
        public const KeyCode KEY_C = KeyCode.C;
        /// <summary>[非推奨] Vボタン</summary>
        public const KeyCode KEY_V = KeyCode.V;

        /// <summary>[非推奨] ENTERキー</summary>
        public const KeyCode KEY_ENTER = KeyCode.Return;
        /// <summary>[非推奨] ENTERキー</summary>
        public const KeyCode KEY_SPACE = KeyCode.Space;

        /// <summary>[非推奨] 白色</summary>
        public static readonly Color COLOR_WHITE = Color.white;
        /// <summary>[非推奨] 黒色</summary>
        public static readonly Color COLOR_BLACK = Color.black;
        /// <summary>[非推奨] 灰色</summary>
        public static readonly Color COLOR_GRAY = Color.gray;
        /// <summary>[非推奨] 赤色</summary>
        public static readonly Color COLOR_RED = Color.red;
        /// <summary>[非推奨] 黄色</summary>
        public static readonly Color COLOR_BLUE = Color.blue;
        /// <summary>[非推奨] 緑色</summary>
        public static readonly Color COLOR_GREEN = Color.green;
        /// <summary>[非推奨] 黄色</summary>
        public static readonly Color COLOR_YELLOW = Color.yellow;
        /// <summary>[非推奨] 紫色</summary>
        public static readonly Color COLOR_PURPLE = new Color(1, 0, 1);
        /// <summary>[非推奨] シアン</summary>
        public static readonly Color COLOR_CYAN = Color.cyan;
        /// <summary>[非推奨] みずいろ</summary>
        public static readonly Color COLOR_AQUA = new Color(0.5f, 0.5f, 1);


        /*******************************
            後方互換 - 基本
        *******************************/

        public void init()
        {
            //
        }

        public void finalize()
        {
            //
        }

        public void setGraphics(object gr, object img)
        {
            //
        }

        public bool writeScreenImage(string filename)
        {
            return false;
        }

        public void setWindowTitle(string title)
        {
            //
        }

        public void drawString(string str, int x, int y)
        {
            //
        }

        public void drawCenterString(string str, int x, int y)
        {
            //
        }

        public void drawRightString(string str, int x, int y)
        {
            //
        }

        public void setFont(string fontName, int fontStyle, int fontSize)
        {
            //
        }

        public void setFontSize(int fontSize)
        {
            //
        }

        public int getStringWidth(string str)
        {
            return -1;
        }

        public void setColor(Color color)
        {
            //
        }

        public void setColor(int color)
        {
            //
        }

        public void setColor(int red, int green, int blue)
        {
            //
        }

        public void drawLine(int sx, int sy, int ex, int ey)
        {
            //
        }

        public void drawRect(int x, int y, int w, int h)
        {
            //
        }

        public void fillRect(int x, int y, int w, int h)
        {
            //
        }

        public void drawCircle(int x, int y, int r)
        {
            //
        }

        public void fillCircle(int x, int y, int r)
        {
            //
        }

        public void drawImage(int id, int x, int y)
        {
            //
        }

        public void drawClipImage(int id, int x, int y, int u, int v, int w, int h)
        {
            //
        }

        public void drawScaledRotateImage(int id, int x, int y, int xsize, int ysize, double rotate)
        {
            //
        }

        public void drawScaledRotateImage(int id, int x, int y, int xsize, int ysize, double rotate, double px, double py)
        {
            //
        }

        public int getImageWidth(int id)
        {
            return -1;
        }

        public int getImageHeight(int id)
        {
            return -1;
        }

        public void setSeed(int seed)
        {
            //
        }

        public int rand(int min, int max)
        {
            return Mathf.FloorToInt(min + UnityEngine.Random.Range(0, 1) * (max - min + 1));
        }

        public void resetGame()
        {
            //
        }

        public void resetGameInstancle(object g)
        {
            //
        }

        public void updatMessage()
        {
            //
        }

        public void drawMessage()
        {
            //
        }

        public void clearScreen()
        {
            //
        }

        public void exitApp()
        {
            //
        }

        public bool showYesNoDialog(string message)
        {
            return true;
        }

        public string showInputDialog(string message, string defaultInput)
        {
            return "";
        }


        /*******************************
            後方互換 - サウンド
        *******************************/

        public void playBGM(int id)
        {
            //
        }

        public void playBGM(int id, bool loop)
        {
            //
        }

        public void changeBGMVolume(int volume)
        {
            //
        }

        public void stopBGM()
        {
            //
        }

        public void pauseBGM()
        {
            //
        }

        public void playSE(int id)
        {
            //
        }

        public void playSE(int id, bool loop)
        {
            //
        }

        public void changeSEVolume(int volume)
        {
            //
        }

        public void stopSE()
        {
            //
        }

        public void pauseSE()
        {
            //
        }


        /*******************************
            後方互換 - 入力
        *******************************/

        public int getKeyPressLength(KeyCode key)
        {
            return 0;
        }

        public bool isKeyPress(KeyCode key)
        {
            return false;
        }

        public bool isKeyPushed(KeyCode key)
        {
            return false;
        }

        public bool isKeyReleased(KeyCode key)
        {
            return false;
        }

        public int getMouseX()
        {
            return 0;
        }

        public int getMouseY()
        {
            return 0;
        }

        public int getMouseClickLength()
        {
            return 0;
        }

        public bool isMousePushed()
        {
            return false;
        }

        public bool isMouseReleased()
        {
            return false;
        }

        public bool isMousePress()
        {
            return false;
        }


        /*******************************
            後方互換 - セーブデータ
        *******************************/

        public int load(int idx)
        {
            return -1;
        }

        public void save(int idx, int param)
        {
            //
        }


        /*******************************
            後方互換 - 当たり判定
        *******************************/

        public bool checkHitRect(int x1, int y1, int w1, int h1, int x2, int y2, int w2, int h2)
        {
            return false;
        }

        public bool checkHitImage(int img1, int x1, int y1, int img2, int x2, int y2)
        {
            return false;
        }

        public bool checkHitCircle(int x1, int y1, int r1, int x2, int y2, int r2)
        {
            return false;
        }


        /*******************************
            後方互換 - 数学
        *******************************/

        public float sqrt(float data)
        {
            return Mathf.Sqrt(data);
        }

        public float cos(float angle)
        {
            return Mathf.Cos(angle * Mathf.PI / 180.0f);
        }

        public float sin(float angle)
        {
            return Mathf.Sin(angle * Mathf.PI / 180.0f);
        }

        public float atan2(float x, float y)
        {
            return Mathf.Atan2(x, y) * 180.0f / Mathf.PI;
        }


        /*******************************
            親クラスのメンバーへの
            アクセスを無効化する
        *******************************/

        // Component
        /// <summary>使用禁止</summary>
        [NonSerialized, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component animation;
        /// <summary>使用禁止</summary>
        [NonSerialized, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component audio;
        /// <summary>使用禁止</summary>
        [NonSerialized, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component camera;
        /// <summary>使用禁止</summary>
        [NonSerialized, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component collider;
        /// <summary>使用禁止</summary>
        [NonSerialized, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component collider2D;
        /// <summary>使用禁止</summary>
        [NonSerialized, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component constantForce;
        /// <summary>使用禁止</summary>
        [NonSerialized, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new GameObject gameObject;
        /// <summary>使用禁止</summary>
        [NonSerialized, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component guiElement;
        /// <summary>使用禁止</summary>
        [NonSerialized, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component guiText;
        /// <summary>使用禁止</summary>
        [NonSerialized, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component guiTexture;
        /// <summary>使用禁止</summary>
        [NonSerialized, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component hingeJoint;
        /// <summary>使用禁止</summary>
        [NonSerialized, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component light;
        /// <summary>使用禁止</summary>
        [NonSerialized, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component networkView;
        /// <summary>使用禁止</summary>
        [NonSerialized, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component particleEmitter;
        /// <summary>使用禁止</summary>
        [NonSerialized, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component particleSystem;
        /// <summary>使用禁止</summary>
        [NonSerialized, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component renderer;
        /// <summary>使用禁止</summary>
        [NonSerialized, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component rigidbody;
        /// <summary>使用禁止</summary>
        [NonSerialized, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component rigidbody2D;
        /// <summary>使用禁止</summary>
        [NonSerialized, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new string tag;
        /// <summary>使用禁止</summary>
        [NonSerialized, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new Transform transform;
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void BroadcastMessage(string methodName) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void BroadcastMessage(string methodName, object parameter) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void BroadcastMessage(string methodName, SendMessageOptions options) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void BroadcastMessage(string methodName, object parameter, SendMessageOptions options) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool CompareTag(string tag) { return false; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component GetComponent(Type type) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component GetComponent(string type) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T GetComponent<T>() { return default(T); }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component GetComponentInChildren(Type t) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component GetComponentInChildren(Type t, bool includeInactive) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T GetComponentInChildren<T>() { return default(T); }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T GetComponentInChildren<T>(bool includeInactive) { return default(T); }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component GetComponentInParent(Type t) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T GetComponentInParent<T>() { return default(T); }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component[] GetComponents(Type type) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void GetComponents(Type type, List<UnityEngine.Component> results) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T[] GetComponents<T>() { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void GetComponents<T>(List<T> results) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component[] GetComponentsInChildren(Type t) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component[] GetComponentsInChildren(Type t, bool includeInactive) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T[] GetComponentsInChildren<T>() { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T[] GetComponentsInChildren<T>(bool includeInactive) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void GetComponentsInChildren<T>(List<T> results) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void GetComponentsInChildren<T>(bool includeInactive, List<T> result) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component[] GetComponentsInParent(Type t) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new UnityEngine.Component[] GetComponentsInParent(Type t, bool includeInactive) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T[] GetComponentsInParent<T>() { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new T[] GetComponentsInParent<T>(bool includeInactive) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void GetComponentsInParent<T>(bool includeInactive, List<T> results) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void SendMessage(string methodName) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void SendMessage(string methodName, object value) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void SendMessage(string methodName, SendMessageOptions options) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void SendMessage(string methodName, object value, SendMessageOptions options) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void SendMessageUpwards(string methodName) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void SendMessageUpwards(string methodName, SendMessageOptions options) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void SendMessageUpwards(string methodName, object value) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void SendMessageUpwards(string methodName, object value, SendMessageOptions options) { }

        // Behaviour
        /// <summary>使用禁止</summary>
        [NonSerialized, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool enabled;
        /// <summary>使用禁止</summary>
        [NonSerialized, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool isActiveAndEnabled;

        // MonoBehaviour
        /// <summary>使用禁止</summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool useGUILayout;
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new static void print(object message) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void CancelInvoke() { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void CancelInvoke(string methodName) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void Invoke(string methodName, float time) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void InvokeRepeating(string methodName, float time, float repeatRate) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool IsInvoking() { return false; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new bool IsInvoking(string methodName) { return false; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new Coroutine StartCoroutine(IEnumerator routine) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new Coroutine StartCoroutine(string methodName) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new Coroutine StartCoroutine(string methodName, object value) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new Coroutine StartCoroutine_Auto(IEnumerator routine) { return null; }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void StopAllCoroutines() { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void StopCoroutine(string methodName) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void StopCoroutine(IEnumerator routine) { }
        /// <summary>使用禁止</summary>
        [Obsolete("このメソッドをGCから呼び出してはいけません", true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new void StopCoroutine(Coroutine routine) { }
    }
}
