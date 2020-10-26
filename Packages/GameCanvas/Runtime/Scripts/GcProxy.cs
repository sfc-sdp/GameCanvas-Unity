/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace GameCanvas
{
    /// <summary>
    /// ユーザー定義クラスからGameCanvasの機能を呼び出すためのクラス
    /// </summary>
    public sealed class GcProxy : IGameCanvas
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        readonly GcContext m_Context;
        readonly Dictionary<System.Type, GcScene> m_SceneDict;

        GcScene m_CurrentScene;
        GcScene m_NextScene;
        object m_NextSceneState;
        bool m_SceneEnterFlag;
        bool m_SceneLeaveFlag;
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public int AccelerationEventCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputAcceleration.AccelerationEventCount;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public float AccelerationLastX
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputAcceleration.LastAccelerationEvent.Acceleration.x;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public float AccelerationLastY
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputAcceleration.LastAccelerationEvent.Acceleration.y;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public float AccelerationLastZ
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputAcceleration.LastAccelerationEvent.Acceleration.z;
        }

        public Color BackgroundColor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.BackgroundColor;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.Graphics.BackgroundColor = value; }
        }

        public Color BorderColor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.BorderColor;
        }

        public int CameraDeviceCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputCamera.CameraDeviceCount;
        }

        public GcAABB CanvasAABB
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GcAABB.WH(m_Context.Graphics.CanvasSize);
        }

        public float2 CanvasCenter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (float2)m_Context.Graphics.CanvasSize * 0.5f;
        }

        public int CanvasHeight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.CanvasSize.y;
        }

        public GcResolution CanvasResolution
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new GcResolution(m_Context.Graphics.CanvasSize, m_Context.Time.TargetFrameRate);
        }

        public int2 CanvasSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.CanvasSize;
        }

        public int CanvasWidth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.CanvasSize.x;
        }

        public int CircleResolution
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.CircleResolution;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.Graphics.CircleResolution = value; }
        }

        public Color Color
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.Color;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.Graphics.Color = value; }
        }

        public Color ColorAqua
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new Color(0.5f, 0.5f, 1f);
        }

        public Color ColorBlack
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new Color(0f, 0f, 0f);
        }

        public Color ColorBlue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new Color(0f, 0f, 1f);
        }

        public Color ColorCyan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new Color(0f, 1f, 1f);
        }

        public Color ColorGray
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new Color(0.5f, 0.5f, 0.5f);
        }

        public Color ColorGreen
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new Color(0f, 1f, 0f);
        }

        public Color ColorPurple
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new Color(1f, 0f, 1f);
        }

        public Color ColorRed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new Color(1f, 0f, 0f);
        }

        public Color ColorWhite
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new Color(1f, 1f, 1f);
        }

        public Color ColorYellow
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new Color(1f, 1f, 0f);
        }

        public CoordianteScope CoordinateScope
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.CoordinateScope;
        }

        public float2x3 CurrentCoordinate
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.CurrentCoordinate;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.Graphics.CurrentCoordinate = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public int CurrentDay
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime.Day;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public System.DayOfWeek CurrentDayOfWeek
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime.DayOfWeek;
        }

        public int CurrentFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentFrame;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public int CurrentHour
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime.Hour;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public int CurrentMillisecond
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime.Millisecond;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public int CurrentMinute
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime.Minute;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public int CurrentMonth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime.Month;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public int CurrentSecond
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime.Second;
        }

        public GcStyle CurrentStyle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.CurrentStyle;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.Graphics.CurrentStyle = value; }
        }

        public System.DateTimeOffset CurrentTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime;
        }

        public int CurrentTimeDay
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime.Day;
        }

        public System.DayOfWeek CurrentTimeDayOfWeek
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime.DayOfWeek;
        }

        public int CurrentTimeHour
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime.Hour;
        }

        public int CurrentTimeMillisecond
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime.Millisecond;
        }

        public int CurrentTimeMinute
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime.Minute;
        }

        public int CurrentTimeMonth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime.Month;
        }

        public int CurrentTimeSecond
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime.Second;
        }

        public long CurrentTimestamp
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTimestamp;
        }

        public int CurrentTimeYear
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime.Year;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public int CurrentYear
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime.Year;
        }

        public int DeviceScreenHeight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.DeviceScreenSize.y;
        }

        public int2 DeviceScreenSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.DeviceScreenSize;
        }

        public int DeviceScreenWidth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.DeviceScreenSize.x;
        }

        public bool DidUpdateGeolocationThisFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputGeolocation.DidUpdateGeolocationThisFrame;
        }

        public GcFont Font
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.Font;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.Graphics.Font = value; }
        }

        public int FontSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.FontSize;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.Graphics.FontSize = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public float GeolocationLastAltitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputGeolocation.LastGeolocationEvent.Altitude;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public float GeolocationLastLatitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputGeolocation.LastGeolocationEvent.Latitude;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public float GeolocationLastLongitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputGeolocation.LastGeolocationEvent.Longitude;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public System.DateTimeOffset GeolocationLastTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputGeolocation.LastGeolocationEvent.Time;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public LocationServiceStatus GeolocationStatus
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputGeolocation.GeolocationStatus;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool HasGeolocationPermission
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputGeolocation.HasUserAuthorizedPermissionGeolocation;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool HasGeolocationUpdate
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputGeolocation.DidUpdateGeolocationThisFrame;
        }

        public bool HasUserAuthorizedPermissionCamera
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputCamera.HasUserAuthorizedPermissionCamera;
        }

        public bool HasUserAuthorizedPermissionGeolocation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputGeolocation.HasUserAuthorizedPermissionGeolocation;
        }

        public bool IsAccelerometerEnabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputAcceleration.IsAccelerometerEnabled;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => m_Context.InputAcceleration.IsAccelerometerEnabled = value;
        }

        public bool IsAnyKey
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (m_Context.InputKey.KeyDownCount != 0)
                || (m_Context.InputKey.KeyHoldCount != 0)
                || (m_Context.InputKey.KeyUpCount != 0);
        }

        public bool IsAnyKeyDown
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (m_Context.InputKey.KeyDownCount != 0);
        }

        public bool IsAnyKeyHold
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (m_Context.InputKey.KeyHoldCount != 0);
        }

        public bool IsAnyKeyPress
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (m_Context.InputKey.KeyDownCount != 0)
                || (m_Context.InputKey.KeyHoldCount != 0);
        }

        public bool IsAnyKeyUp
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (m_Context.InputKey.KeyUpCount != 0);
        }

        public bool IsGeolocationRunning
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var status = m_Context.InputGeolocation.GeolocationStatus;
                return (status == LocationServiceStatus.Initializing)
                    || (status == LocationServiceStatus.Running);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsPressBackButton
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputKey.IsKeyPress(KeyEscape);
        }

        public bool IsScreenKeyboardSupported
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputKey.IsScreenKeyboardSupported;
        }

        public bool IsScreenKeyboardVisible
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputKey.IsScreenKeyboardVisible;
        }

        public bool IsTouchPressureSupported
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputPointer.IsTouchPressureSupported;
        }

        public bool IsTouchSupported
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputPointer.IsTouchSupported;
        }

        public int KeyDownCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputKey.KeyDownCount;
        }

        public KeyCode KeyEscape
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => KeyCode.Escape;
        }

        public int KeyHoldCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputKey.KeyHoldCount;
        }

        public int KeyPressCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputKey.KeyDownCount + m_Context.InputKey.KeyHoldCount;
        }

        public int KeyUpCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputKey.KeyUpCount;
        }

        public GcAccelerationEvent LastAccelerationEvent
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputAcceleration.LastAccelerationEvent;
        }

        public GcGeolocationEvent LastGeolocationEvent
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputGeolocation.LastGeolocationEvent;
        }

        public GcPointerEvent LastPointerEvent
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputPointer.LastPointerEvent;
        }

        public int LastPointerFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputPointer.LastPointerEvent.Frame;
        }

        public float2 LastPointerPoint
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputPointer.LastPointerEvent.Point;
        }

        public float LastPointerTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputPointer.LastPointerEvent.Time;
        }

        public float LastPointerX
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputPointer.LastPointerEvent.Point.x;
        }

        public float LastPointerY
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputPointer.LastPointerEvent.Point.y;
        }

        public GcLineCap LineCap
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.LineCap;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.Graphics.LineCap = value; }
        }

        public float LineWidth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.LineWidth;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.Graphics.LineWidth = value; }
        }

        public System.DateTimeOffset NowTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.NowTime;
        }

        public int PointerBeginCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputPointer.PointerBeginCount;
        }

        public int PointerCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputPointer.PointerCount;
        }

        public int PointerEndCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputPointer.PointerEndCount;
        }

        public int PointerTapCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputPointer.PointerTapCount;
        }

        public GcAnchor RectAnchor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.RectAnchor;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.Graphics.RectAnchor = value; }
        }

        public GcAnchor StringAnchor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.StringAnchor;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.Graphics.StringAnchor = value; }
        }

        public StyleScope StyleScope
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.StyleScope;
        }

        public GcTapSettings TapSettings
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputPointer.TapSettings;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.InputPointer.TapSettings = value; }
        }

        public double TargetFrameInterval
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.TargetFrameInterval;
        }

        public int TargetFrameRate
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.TargetFrameRate;
        }

        public float TimeSincePrevFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.TimeSincePrevFrame;
        }

        public float TimeSinceStartup
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.TimeSinceStartup;
        }

        public bool VSyncEnabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.VSyncEnabled;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Abs(in float value) => GcMath.Abs(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Abs(in int value) => GcMath.Abs(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddActor(in GcActor actor)
            => m_CurrentScene?.AddActor(actor);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AlmostSame(in float a, in float b) => GcMath.AlmostSame(a, b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AlmostZero(in float value) => GcMath.AlmostZero(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Atan2(in float x, in float y) => GcMath.Atan2(new float2(x, y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Atan2(in float2 v) => GcMath.Atan2(v);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float CalcStringHeight(in string str)
            => m_Context.Graphics.CalcStringHeight(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float2 CalcStringSize(in string str)
            => m_Context.Graphics.CalcStringSize(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float CalcStringWidth(in string str)
            => m_Context.Graphics.CalcStringWidth(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CanvasToScreenPoint(in float2 canvas, out float2 screen)
            => m_Context.Graphics.CanvasToScreenPoint(canvas, out screen);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CanvasToScreenPoint(in float2 canvas, out int2 screen)
            => m_Context.Graphics.CanvasToScreenPoint(canvas, out screen);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeBorderColor(in float r, in float g, in float b)
            => m_Context.Graphics.ChangeBorderColor(new Color(r, g, b));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeBorderColor(in Color color)
            => m_Context.Graphics.ChangeBorderColor(color);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeCanvasSize(in int width, in int height)
            => m_Context.Graphics.ChangeCanvasSize(new int2(width, height));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeCanvasSize(in int2 size)
            => m_Context.Graphics.ChangeCanvasSize(size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeScene<T>(object state = null) where T : GcScene
        {
            if (m_CurrentScene != null)
            {
                m_SceneLeaveFlag = true;
            }

            if (m_SceneDict.TryGetValue(typeof(T), out var nextScene))
            {
                m_SceneEnterFlag = true;
                m_NextScene = nextScene;
                m_NextSceneState = state;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool CheckHitCircle(in float x1, in float y1, in float r1, in float x2, in float y2, in float r2)
            => new GcCircle(x1, y1, r1).Overlaps(new GcCircle(x2, y2, r2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool CheckHitRect(in float x1, in float y1, in float w1, in float h1, in float x2, in float y2, in float w2, in float h2)
            => GcAABB.XYWH(x1, y1, w1, h1).Overlaps(GcAABB.XYWH(x2, y2, w2, h2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Clamp(in float value, in float min, in float max)
            => GcMath.Clamp(value, min, max);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearCoordinate()
            => m_Context.Graphics.ClearCoordinate();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ClearDownloadCache()
            => m_Context.Network.ClearDownloadCacheAll();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearDownloadCache(in string url)
            => m_Context.Network.ClearDownloadCache(url);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearDownloadCacheAll()
            => m_Context.Network.ClearDownloadCacheAll();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearScreen()
            => m_Context.Graphics.ClearScreen();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearSound()
            => m_Context.Sound.ClearSound();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearStyle()
            => m_Context.Graphics.ClearStyle();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Cos(in float degree) => GcMath.Cos(degree);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T CreateActor<T>() where T : GcActor, new()
            => m_CurrentScene?.CreateActor<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Cross(in float2 a, in float2 b) => GcMath.Cross(a, b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CrossTest(in GcLine a, in GcLine b)
            => a.Intersects(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CrossTest(in GcLine a, in GcLine b, out float2 intersection)
            => a.Intersects(b, out intersection);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool DidUpdateCameraImageThisFrame(in GcCameraDevice camera)
            => m_Context.InputCamera.DidUpdateCameraImageThisFrame(camera);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Dot(in float2 a, in float2 b) => GcMath.Dot(a, b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawCameraImage(in GcCameraDevice camera, bool autoPlay = true)
        {
            var texture = m_Context.InputCamera.GetOrCreateCameraTexture(camera, GetPrimaryCameraResolution(camera));
            if (texture != null)
            {
                if (autoPlay && !texture.isPlaying)
                {
                    texture.Play();
                }

                m_Context.Graphics.DrawTexture(texture);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawCameraImage(in GcCameraDevice camera, in float2 position, float degree = 0, bool autoPlay = true)
        {
            var texture = m_Context.InputCamera.GetOrCreateCameraTexture(camera, GetPrimaryCameraResolution(camera));
            if (texture != null)
            {
                if (autoPlay && !texture.isPlaying)
                {
                    texture.Play();
                }

                m_Context.Graphics.DrawTexture(texture, position, degree);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawCameraImage(in GcCameraDevice camera, in GcRect rect, bool autoPlay = true)
        {
            var texture = m_Context.InputCamera.GetOrCreateCameraTexture(camera, GetPrimaryCameraResolution(camera));
            if (texture != null)
            {
                if (autoPlay && !texture.isPlaying)
                {
                    texture.Play();
                }

                m_Context.Graphics.DrawTexture(texture, rect);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawCameraImage(in GcCameraDevice camera, in float x, in float y, in float width, in float height, float degree = 0f, bool autoPlay = true)
        {
            var texture = m_Context.InputCamera.GetOrCreateCameraTexture(camera, GetPrimaryCameraResolution(camera));
            if (texture != null)
            {
                if (autoPlay && !texture.isPlaying)
                {
                    texture.Play();
                }

                m_Context.Graphics.DrawTexture(texture, new GcRect(x, y, width, height, math.radians(degree)));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawCameraImage(in GcCameraDevice camera, in float x, in float y, float degree = 0, bool autoPlay = true)
        {
            var texture = m_Context.InputCamera.GetOrCreateCameraTexture(camera, GetPrimaryCameraResolution(camera));
            if (texture != null)
            {
                if (autoPlay && !texture.isPlaying)
                {
                    texture.Play();
                }

                m_Context.Graphics.DrawTexture(texture, new float2(x, y), degree);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DrawCenterRect(in float2 center, in float2 size, float degree = 0f)
        {
            using (StyleScope)
            {
                SetRectAnchor(GcAnchor.MiddleCenter);
                DrawRect(center, size, degree);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DrawCenterString(in string str, in float x, in float y, float degree = 0f)
        {
            using (StyleScope)
            {
                SetStringAnchor(GcAnchor.UpperCenter);
                DrawString(str, x, y, degree);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawCircle()
            => m_Context.Graphics.DrawCircle();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawCircle(in float x, in float y, in float radius)
            => m_Context.Graphics.DrawCircle(new GcCircle(x, y, radius));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawCircle(in float2 position, in float radius)
            => m_Context.Graphics.DrawCircle(new GcCircle(position, radius));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawCircle(in GcCircle circle)
            => m_Context.Graphics.DrawCircle(circle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawImage(in GcImage image)
            => m_Context.Graphics.DrawImage(image);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawImage(in GcImage image, in float x, in float y, float degree = 0f)
            => m_Context.Graphics.DrawImage(image, new float2(x, y), degree);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawImage(in GcImage image, in float x, in float y, in float width, in float height, float degree = 0f)
            => m_Context.Graphics.DrawImage(image, new GcRect(x, y, width, height, math.radians(degree)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawImage(in GcImage image, in float2 position, float degree = 0f)
            => m_Context.Graphics.DrawImage(image, position, degree);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawImage(in GcImage image, in GcRect rect)
            => m_Context.Graphics.DrawImage(image, rect);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawLine()
            => m_Context.Graphics.DrawLine();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawLine(in float2 begin, in float2 end)
            => m_Context.Graphics.DrawLine(GcLine.Segment(begin, end));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawLine(in float x0, in float y0, in float x1, in float y1)
            => m_Context.Graphics.DrawLine(GcLine.Segment(new float2(x0, y0), new float2(x1, y1)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawLine(in GcLine line)
            => m_Context.Graphics.DrawLine(line);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GcAvailability DrawOnlineImage(in string url)
        {
            var ret = m_Context.Network.TryGetOnlineImage(url, out var tex);
            if (ret == GcAvailability.Ready)
            {
                m_Context.Graphics.DrawTexture(tex);
            }
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GcAvailability DrawOnlineImage(in string url, in float2 position, float degree = 0f)
        {
            var ret = m_Context.Network.TryGetOnlineImage(url, out var tex);
            if (ret == GcAvailability.Ready)
            {
                m_Context.Graphics.DrawTexture(tex, position, degree);
            }
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GcAvailability DrawOnlineImage(in string url, in float x, in float y, float degree = 0f)
        {
            var ret = m_Context.Network.TryGetOnlineImage(url, out var tex);
            if (ret == GcAvailability.Ready)
            {
                m_Context.Graphics.DrawTexture(tex, new float2(x, y), degree);
            }
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GcAvailability DrawOnlineImage(in string url, in GcRect rect)
        {
            var ret = m_Context.Network.TryGetOnlineImage(url, out var tex);
            if (ret == GcAvailability.Ready)
            {
                m_Context.Graphics.DrawTexture(tex, rect);
            }
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GcAvailability DrawOnlineImage(in string url, in float x, in float y, in float width, in float height, float degree = 0f)
        {
            var ret = m_Context.Network.TryGetOnlineImage(url, out var tex);
            if (ret == GcAvailability.Ready)
            {
                m_Context.Graphics.DrawTexture(tex, new GcRect(x, y, width, height, math.radians(degree)));
            }
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRect()
            => m_Context.Graphics.DrawRect();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRect(in float x, in float y, in float width, in float height, float degree = 0f)
            => m_Context.Graphics.DrawRect(new GcRect(x, y, width, height, math.radians(degree)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRect(in float2 position, in float2 size, float degree = 0f)
            => m_Context.Graphics.DrawRect(new GcRect(position, size, math.radians(degree)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRect(in GcRect rect)
            => m_Context.Graphics.DrawRect(rect);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void DrawRightString(in string str, in float x, in float y, float degree = 0f)
        {
            using (StyleScope)
            {
                SetStringAnchor(GcAnchor.UpperRight);
                DrawString(str, x, y, degree);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawString(in string str)
            => m_Context.Graphics.DrawString(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawString(in string str, in float x, in float y, float degree = 0f)
            => m_Context.Graphics.DrawString(str, new float2(x, y), degree);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawString(in string str, in float x, in float y, in float width, in float height, float degree = 0f)
            => m_Context.Graphics.DrawString(str, new GcRect(x, y, width, height, math.radians(degree)));

        public void DrawString(in string str, in float2 position, float degree = 0f)
            => m_Context.Graphics.DrawString(str, position, degree);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawString(in string str, in GcRect rect)
            => m_Context.Graphics.DrawString(str, rect);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawTexture(in Texture texture)
            => m_Context.Graphics.DrawTexture(texture);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawTexture(in Texture texture, in float2 position, float degree = 0f)
            => m_Context.Graphics.DrawTexture(texture, position, degree);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawTexture(in Texture texture, in float x, in float y, in float width, in float height, float degree = 0f)
            => m_Context.Graphics.DrawTexture(texture, new GcRect(x, y, width, height, math.radians(degree)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawTexture(in Texture texture, in GcRect rect)
            => m_Context.Graphics.DrawTexture(texture, rect);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EraseSavedDataAll()
            => m_Context.Storage.EraseSavedDataAll();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void FillCenterRect(in float2 center, in float2 size, float degree = 0f)
        {
            using (StyleScope)
            {
                SetRectAnchor(GcAnchor.MiddleCenter);
                FillRect(center, size, degree);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillCircle()
            => m_Context.Graphics.FillCircle();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillCircle(in float x, in float y, in float radius)
            => m_Context.Graphics.FillCircle(new GcCircle(x, y, radius));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillCircle(in float2 position, in float radius)
            => m_Context.Graphics.FillCircle(new GcCircle(position, radius));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillCircle(in GcCircle circle)
            => m_Context.Graphics.FillCircle(circle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillRect()
            => m_Context.Graphics.FillRect();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillRect(in float x, in float y, in float width, in float height, float degree = 0f)
            => m_Context.Graphics.FillRect(new GcRect(x, y, width, height, math.radians(degree)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillRect(in float2 position, in float2 size, float degree = 0f)
            => m_Context.Graphics.FillRect(new GcRect(position, size, math.radians(degree)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillRect(in GcRect rect)
            => m_Context.Graphics.FillRect(rect);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FocusCameraImage(in GcCameraDevice camera, in float2? uv)
            => m_Context.InputCamera.FocusCameraImage(camera, uv);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public float GetAccelerationX(in int i, in bool normalize = false)
        {
            if (m_Context.InputAcceleration.TryGetAccelerationEvent(i, out var e))
            {
                return normalize ? math.normalizesafe(e.Acceleration).x : e.Acceleration.x;
            }
            return 0f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public float GetAccelerationY(in int i, in bool normalize = false)
        {
            if (m_Context.InputAcceleration.TryGetAccelerationEvent(i, out var e))
            {
                return normalize ? math.normalizesafe(e.Acceleration).y : e.Acceleration.y;
            }
            return 0f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public float GetAccelerationZ(in int i, in bool normalize = false)
        {
            if (m_Context.InputAcceleration.TryGetAccelerationEvent(i, out var e))
            {
                return normalize ? math.normalizesafe(e.Acceleration).z : e.Acceleration.z;
            }
            return 0f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GcActor GetActor()
            => (m_CurrentScene != null) && m_CurrentScene.TryGetActor(0, out var actor) ? actor : null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetActor<T>() where T : GcActor
            => (m_CurrentScene != null) && m_CurrentScene.TryGetActor<T>(0, out var actor) ? actor : null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetActorCount()
            => m_CurrentScene?.GetActorCount() ?? 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetActorCount<T>() where T : GcActor
            => m_CurrentScene?.GetActorCount<T>() ?? 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyActorList<T> GetActorList<T>() where T : GcActor
            => (m_CurrentScene != null) && m_CurrentScene.TryGetActorList<T>(out var list) ? list : default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetImageHeight(in GcImage image) => image.m_Size.y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int2 GetImageSize(in GcImage image) => image.m_Size;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetImageWidth(in GcImage image) => image.m_Size.x;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool GetIsKeyBegan(in KeyCode key)
            => m_Context.InputKey.IsKeyDown(key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool GetIsKeyEnded(in KeyCode key)
            => m_Context.InputKey.IsKeyUp(key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetKeyPressDuration(in KeyCode key)
        {
            if (m_Context.InputKey.TryGetKeyTrace(key, out var trace))
            {
                return trace.Duration;
            }
            return 0f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetKeyPressDuration(in char key)
        {
            if (key.TryGetKeyCode(out var code) && m_Context.InputKey.TryGetKeyTrace(code, out var trace))
            {
                return trace.Duration;
            }
            return 0f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetKeyPressFrameCount(in KeyCode key)
        {
            if (m_Context.InputKey.TryGetKeyTrace(key, out var trace))
            {
                return trace.FrameCount;
            }
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetKeyPressFrameCount(in char key)
        {
            if (key.TryGetKeyCode(out var code) && m_Context.InputKey.TryGetKeyTrace(code, out var trace))
            {
                return trace.FrameCount;
            }
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOnlineImageHeight(in string url)
            => m_Context.Network.TryGetOnlineImageSize(url, out var size) ? size.y : 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOnlineImageWidth(in string url)
            => m_Context.Network.TryGetOnlineImageSize(url, out var size) ? size.x : 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public GcAvailability GetOnlineTextAsync(in string url, out string text)
            => m_Context.Network.TryGetOnlineText(url, out text);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WebCamTexture GetOrCreateCameraTexture(in GcCameraDevice camera, in GcResolution request)
            => m_Context.InputCamera.GetOrCreateCameraTexture(camera, request);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public float GetPointerDistance(in int i)
            => m_Context.InputPointer.TryGetPointerTrace(i, out var t) ? t.Distance : 0f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public float GetPointerDuration(in int i)
            => m_Context.InputPointer.TryGetPointerTrace(i, out var t) ? t.Duration : 0f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int GetPointerFrameCount(in int i)
            => m_Context.InputPointer.TryGetPointerTrace(i, out var t) ? t.FrameCount : 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public float GetPointerX(in int i)
            => m_Context.InputPointer.TryGetPointerEvent(0, out var e) ? e.Point.x : 0f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public float GetPointerY(in int i)
            => m_Context.InputPointer.TryGetPointerEvent(0, out var e) ? e.Point.y : 0f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GcResolution GetPrimaryCameraResolution(in GcCameraDevice camera)
        {
            if (camera.Resolutions == null || camera.Resolutions.Length == 0)
            {
                return CanvasResolution;
            }
            return camera.Resolutions[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetSoundLevel(in GcSoundTrack track = GcSoundTrack.Master)
            => m_Context.Sound.GetSoundLevel(track);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetSoundVolume(in GcSoundTrack track = GcSoundTrack.Master)
            => math.pow(10, m_Context.Sound.GetSoundLevel(track) * 0.05f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HideScreenKeyboard()
            => m_Context.InputKey.HideScreenKeyboard();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HitTest(in GcAABB a, in GcAABB b)
            => a.Overlaps(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HitTest(in GcAABB aabb, in float2 point)
            => aabb.Contains(point);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HitTest(in GcCircle circle1, in GcCircle circle2)
            => circle1.Overlaps(circle2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HitTest(in GcCircle circle, in float2 point)
            => circle.Contains(point);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsFlippedCameraImage(in GcCameraDevice camera)
            => m_Context.InputCamera.IsPlayingCameraImage(camera);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKeyDown(in char key)
            => key.TryGetKeyCode(out var code)
            && m_Context.InputKey.IsKeyDown(code);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKeyDown(in KeyCode key)
            => m_Context.InputKey.IsKeyDown(key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKeyHold(in char key)
            => key.TryGetKeyCode(out var code)
            && m_Context.InputKey.IsKeyHold(code);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKeyHold(in KeyCode key, out GcKeyTrace trace)
            => m_Context.InputKey.TryGetKeyTrace(key, out trace)
            && (trace.Current.Phase == GcKeyEventPhase.Hold);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKeyHold(in KeyCode key)
            => m_Context.InputKey.IsKeyHold(key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKeyPress(in char key)
            => key.TryGetKeyCode(out var code)
            && m_Context.InputKey.IsKeyPress(code);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKeyPress(in KeyCode key)
            => m_Context.InputKey.IsKeyPress(key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKeyUp(in char key)
            => key.TryGetKeyCode(out var code)
            && m_Context.InputKey.IsKeyUp(code);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKeyUp(in KeyCode key, out GcKeyTrace trace)
            => m_Context.InputKey.TryGetKeyTrace(key, out trace)
            && (trace.Current.Phase == GcKeyEventPhase.Up);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKeyUp(in KeyCode key)
            => m_Context.InputKey.IsKeyUp(key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPlayingCameraImage(in GcCameraDevice camera)
            => m_Context.InputCamera.IsPlayingCameraImage(camera);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPlayingSound(in GcSoundTrack track = GcSoundTrack.BGM1)
            => m_Context.Sound.IsPlayingSound(track);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTapped()
            => (m_Context.InputPointer.PointerTapCount != 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTapped(out float2 point)
            => m_Context.InputPointer.TryGetPointerTapPoint(0, out point);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTapped(out float x, out float y)
        {
            if (m_Context.InputPointer.TryGetPointerTapPoint(0, out var point))
            {
                x = point.x;
                y = point.y;
                return true;
            }
            x = 0;
            y = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTapped(in GcAABB aabb, out float2 point)
        {
            if (m_Context.InputPointer.TryGetPointerTapPointArray(out var array, out var count))
            {
                for (var i = 0; i != count; i++)
                {
                    point = array[i];
                    if (aabb.Contains(point)) return true;
                }
            }
            point = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTapped(in float x, in float y, in float width, in float height, out float px, out float py)
        {
            if (IsTapped(GcAABB.XYWH(x, y, width, height), out var point))
            {
                px = point.x;
                py = point.y;
                return true;
            }
            px = 0f;
            py = 0f;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTouchBegan()
            => (m_Context.InputPointer.PointerBeginCount != 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTouchBegan(out float2 point)
        {
            if (m_Context.InputPointer.TryGetPointerEvent(GcPointerEventPhase.Begin, 0, out var e))
            {
                point = e.Point;
                return true;
            }
            point = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTouchBegan(out GcPointerEvent pointer)
            => m_Context.InputPointer.TryGetPointerEvent(GcPointerEventPhase.Begin, 0, out pointer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTouchBegan(out float x, out float y)
        {
            if (m_Context.InputPointer.TryGetPointerEvent(GcPointerEventPhase.Begin, 0, out var e))
            {
                x = e.Point.x;
                y = e.Point.y;
                return true;
            }
            x = 0f;
            y = 0f;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTouchBegan(in GcAABB aabb, out float2 point)
        {
            if (m_Context.InputPointer.TryGetPointerEventArray(GcPointerEventPhase.Begin, out var array, out var count))
            {
                for (var i = 0; i != count; i++)
                {
                    point = array[i].Point;
                    if (aabb.Contains(point)) return true;
                }
            }
            point = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTouchBegan(in float x, in float y, in float width, in float height, out float px, out float py)
        {
            if (IsTouchBegan(GcAABB.XYWH(x, y, width, height), out var point))
            {
                px = point.x;
                py = point.y;
                return true;
            }
            px = 0f;
            py = 0f;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTouched()
            => (m_Context.InputPointer.PointerCount != 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTouched(out float2 point)
        {
            if (m_Context.InputPointer.TryGetPointerEvent(0, out GcPointerEvent pointer))
            {
                point = pointer.Point;
                return true;
            }
            point = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTouched(out GcPointerEvent pointer)
            => m_Context.InputPointer.TryGetPointerEvent(0, out pointer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTouched(out GcPointerTrace pointer)
            => m_Context.InputPointer.TryGetPointerTrace(0, out pointer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTouched(out float x, out float y)
        {
            if (m_Context.InputPointer.TryGetPointerEvent(0, out var e))
            {
                x = e.Point.x;
                y = e.Point.y;
                return true;
            }
            x = 0f;
            y = 0f;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTouched(in GcAABB aabb, out float2 point)
        {
            if (m_Context.InputPointer.TryGetPointerEventArray(out var array, out var count))
            {
                for (var i = 0; i != count; i++)
                {
                    point = array[i].Point;
                    if (aabb.Contains(point)) return true;
                }
            }
            point = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTouched(in float x, in float y, in float width, in float height, out float px, out float py)
        {
            if (IsTouched(GcAABB.XYWH(x, y, width, height), out var point))
            {
                px = point.x;
                py = point.y;
                return true;
            }
            px = 0f;
            py = 0f;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTouchEnded()
            => (m_Context.InputPointer.PointerEndCount != 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTouchEnded(out float2 point)
        {
            if (m_Context.InputPointer.TryGetPointerEvent(GcPointerEventPhase.End, 0, out var e))
            {
                point = e.Point;
                return true;
            }
            point = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTouchEnded(out GcPointerEvent pointer)
            => m_Context.InputPointer.TryGetPointerEvent(GcPointerEventPhase.End, 0, out pointer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTouchEnded(out GcPointerTrace pointer)
            => m_Context.InputPointer.TryGetPointerTrace(GcPointerEventPhase.End, 0, out pointer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTouchEnded(out float x, out float y)
        {
            if (m_Context.InputPointer.TryGetPointerEvent(GcPointerEventPhase.End, 0, out var e))
            {
                x = e.Point.x;
                y = e.Point.y;
                return true;
            }
            x = 0f;
            y = 0f;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTouchEnded(in GcAABB aabb, out float2 point)
        {
            if (m_Context.InputPointer.TryGetPointerEventArray(GcPointerEventPhase.End, out var array, out var count))
            {
                for (var i = 0; i != count; i++)
                {
                    point = array[i].Point;
                    if (aabb.Contains(point)) return true;
                }
            }
            point = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTouchEnded(in float x, in float y, in float width, in float height, out float px, out float py)
        {
            if (IsTouchEnded(GcAABB.XYWH(x, y, width, height), out var point))
            {
                px = point.x;
                py = point.y;
                return true;
            }
            px = 0f;
            py = 0f;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Max(in float a, in float b) => GcMath.Max(a, b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Min(in float a, in float b) => GcMath.Min(a, b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PauseCameraImage(in GcCameraDevice camera)
            => m_Context.InputCamera.PauseCameraImage(camera);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PauseSound(in GcSoundTrack track = GcSoundTrack.BGM1)
            => m_Context.Sound.PauseSound(track);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PlayCameraImage(in GcCameraDevice camera)
            => m_Context.InputCamera.PlayCameraImage(camera, GetPrimaryCameraResolution(camera), out _);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PlayCameraImage(in GcCameraDevice camera, out int2 resolution)
            => m_Context.InputCamera.PlayCameraImage(camera, GetPrimaryCameraResolution(camera), out resolution);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PlayCameraImage(in GcCameraDevice camera, in GcResolution request, out int2 resolution)
            => m_Context.InputCamera.PlayCameraImage(camera, request, out resolution);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PlaySE(in GcSound sound)
            => m_Context.Sound.PlaySound(sound, GcSoundTrack.SE, false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PlaySE(in AudioClip clip)
            => m_Context.Sound.PlaySound(clip, GcSoundTrack.SE);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PlaySound(in GcSound sound, in GcSoundTrack track = GcSoundTrack.BGM1, in bool loop = false)
            => m_Context.Sound.PlaySound(sound, track, loop);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PlaySound(in AudioClip clip, in GcSoundTrack track = GcSoundTrack.BGM1, in bool loop = false)
            => m_Context.Sound.PlaySound(clip, track, loop);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PopCoordinate()
            => m_Context.Graphics.PopCoordinate();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PopStyle()
            => m_Context.Graphics.PopStyle();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushCoordinate()
            => m_Context.Graphics.PushCoordinate();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushStyle()
            => m_Context.Graphics.PushStyle();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Random() => GcMath.Random();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Random(in int min, in int max) => GcMath.Random(min, max);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Random(in float min, in float max) => GcMath.Random(min, max);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterScene<T>() where T : GcScene, new()
            => m_SceneDict.Add(typeof(T), new T());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterScene(in GcScene scene)
            => m_SceneDict.Add(scene.GetType(), scene);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveActorAll()
            => m_CurrentScene?.RemoveActorAll();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RequestUserAuthorizedPermissionCameraAsync(in System.Action<bool> callback)
            => m_Context.InputCamera.RequestUserAuthorizedPermissionCameraAsync(callback);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RequestUserAuthorizedPermissionGeolocationAsync(in System.Action<bool> callback)
            => m_Context.InputGeolocation.RequestUserAuthorizedPermissionGeolocationAsync(callback);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public float2 Rotate(in float2 vector, in float degree)
            => GcMath.RotateVector(vector, degree);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RotateCoordinate(in float degree)
            => m_Context.Graphics.RotateCoordinate(degree);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RotateCoordinate(in float degree, in float originX, in float originY)
            => m_Context.Graphics.RotateCoordinate(degree, new float2(originX, originY));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RotateCoordinate(in float degree, in float2 origin)
            => m_Context.Graphics.RotateCoordinate(degree, origin);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float2 RotateVector(in float2 vector, in float degree)
            => GcMath.RotateVector(vector, degree);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Round(in float value) => GcMath.Round(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Round(in double value) => GcMath.Round(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Save(in string key, BigInteger? value)
            => m_Context.Storage.Save(key, value.ToString());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Save(in string key, float? value)
            => m_Context.Storage.Save(key, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Save(in string key, int? value)
            => m_Context.Storage.Save(key, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Save(in string key, string value)
            => m_Context.Storage.Save(key, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SaveScreenshotAsync(in System.Action<string> onComplete = null)
            => m_Context.Storage.SaveScreenshotAsync(onComplete);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ScaleCoordinate(in float sx, in float sy)
            => m_Context.Graphics.ScaleCoordinate(new float2(sx, sy));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ScaleCoordinate(in float2 scaling)
            => m_Context.Graphics.ScaleCoordinate(scaling);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ScreenToCanvasPoint(in float2 screen, out float2 canvas)
            => m_Context.Graphics.ScreenToCanvasPoint(screen, out canvas);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ScreenToCanvasPoint(in float2 screen, out int2 canvas)
            => m_Context.Graphics.ScreenToCanvasPoint(screen, out canvas);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBackgroundColor(in float r, in float g, in float b)
            => m_Context.Graphics.BackgroundColor = new Color(r, g, b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBackgroundColor(in Color color)
            => m_Context.Graphics.BackgroundColor = color;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetColor(in float r, in float g, in float b, float a = 1f)
            => m_Context.Graphics.Color = new Color(r, g, b, a);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetColor(in Color color)
            => m_Context.Graphics.Color = color;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetColor(in Color color, in float alpha)
            => m_Context.Graphics.Color = new Color(color.r, color.g, color.b, alpha);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetColor(in byte r, in byte g, in byte b, byte a = 255)
            => m_Context.Graphics.Color = new Color32(r, g, b, a);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCoordinate(in float2x3 matrix)
            => m_Context.Graphics.CurrentCoordinate = matrix;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFont(in GcFont font)
            => m_Context.Graphics.Font = font;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFontSize(in int fontSize)
            => m_Context.Graphics.FontSize = fontSize;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFrameInterval(in double targetDeltaTime, in bool vSyncEnabled = true)
            => m_Context.Time.SetFrameInterval(targetDeltaTime, vSyncEnabled);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFrameRate(in int targetFrameRate, bool vSyncEnabled = true)
            => m_Context.Time.SetFrameRate(targetFrameRate, vSyncEnabled);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLineCap(in GcLineCap lineCap)
            => m_Context.Graphics.LineCap = lineCap;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLineWidth(in float lineWidth)
            => m_Context.Graphics.LineWidth = lineWidth;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRandomSeed(in uint seed) => GcMath.SetRandomSeed(seed);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRectAnchor(in GcAnchor anchor)
            => m_Context.Graphics.RectAnchor = anchor;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetResolution(in int width, in int height)
            => m_Context.Graphics.ChangeCanvasSize(new int2(width, height));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetSeed(in int seed) => GcMath.SetRandomSeed((uint)seed);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSoundLevel(in float decibel, in GcSoundTrack track = GcSoundTrack.Master)
            => m_Context.Sound.SetSoundLevel(decibel, track);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSoundVolume(in float volume, in GcSoundTrack track = GcSoundTrack.Master)
            => m_Context.Sound.SetSoundLevel(20f * math.log10(volume), track);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetStringAnchor(in GcAnchor anchor)
            => m_Context.Graphics.StringAnchor = anchor;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetStyle(in GcStyle style)
            => m_Context.Graphics.CurrentStyle = style;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetTapSensitivity(in float maxDuration, in float maxDistance)
            => m_Context.InputPointer.TapSettings = new GcTapSettings(maxDistance, maxDuration);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ShowScreenKeyboard()
            => m_Context.InputKey.ShowScreenKeyboard();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Sin(in float degree) => GcMath.Sin(degree);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Sqrt(in float value) => GcMath.Sqrt(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartGeolocationService(in float desiredAccuracy = 10, in float updateDistance = 10)
            => m_Context.InputGeolocation.StartGeolocationService(desiredAccuracy, updateDistance);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StopCameraImage(in GcCameraDevice camera)
            => m_Context.InputCamera.StopCameraImage(camera);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StopGeolocationService()
            => m_Context.InputGeolocation.StopGeolocationService();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StopSound(in GcSoundTrack track = GcSoundTrack.BGM1)
            => m_Context.Sound.StopSound(track);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SweepTest(in GcAABB @static, in float2 @dynamic, in float2 delta, out GcSweepResult hit)
            => @static.SweepTest(@dynamic, delta, out hit);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SweepTest(in GcAABB @static, in GcAABB @dynamic, in float2 delta, out GcSweepResult hit)
            => @static.SweepTest(@dynamic, delta, out hit);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TranslateCoordinate(in float tx, in float ty)
            => m_Context.Graphics.TranslateCoordinate(new float2(tx, ty));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TranslateCoordinate(in float2 translation)
            => m_Context.Graphics.TranslateCoordinate(translation);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int2 TryChangeCameraImageResolution(in GcCameraDevice camera, in GcResolution request)
            => m_Context.InputCamera.TryChangeCameraImageResolution(camera, request);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetAccelerationEvent(int i, out GcAccelerationEvent e)
            => m_Context.InputAcceleration.TryGetAccelerationEvent(i, out e);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetAccelerationEvents(out NativeArray<GcAccelerationEvent>.ReadOnly array, out int count)
            => m_Context.InputAcceleration.TryGetAccelerationEvents(out array, out count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetActor(in int i, out GcActor actor)
        {
            if (m_CurrentScene != null)
            {
                return m_CurrentScene.TryGetActor(i, out actor);
            }
            actor = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetActor<T>(in int i, out T actor) where T : GcActor
        {
            if (m_CurrentScene != null)
            {
                return m_CurrentScene.TryGetActor<T>(i, out actor);
            }
            actor = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetActorList<T>(out ReadOnlyActorList<T> list) where T : GcActor
        {
            if (m_CurrentScene != null)
            {
                return m_CurrentScene.TryGetActorList(out list);
            }
            list = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetCameraImage(out GcCameraDevice camera)
            => m_Context.InputCamera.TryGetCameraImage(out camera);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetCameraImage(in string deviceName, out GcCameraDevice camera)
            => m_Context.InputCamera.TryGetCameraImage(deviceName, out camera);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetCameraImageAll(out ReadOnlyCollection<GcCameraDevice> array)
            => m_Context.InputCamera.TryGetCameraImageAll(out array);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetCameraImageRotation(in GcCameraDevice camera, out float degree)
            => m_Context.InputCamera.TryGetCameraImageRotation(camera, out degree);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetCameraImageSize(in GcCameraDevice camera, out int2 resolution)
            => m_Context.InputCamera.TryGetCameraImageSize(camera, out resolution);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetGeolocationEvent(out GcGeolocationEvent data)
            => m_Context.InputGeolocation.TryGetGeolocationEvent(out data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetKeyEvent(in char key, out GcKeyEvent e)
        {
            if (key.TryGetKeyCode(out var code))
            {
                return m_Context.InputKey.TryGetKeyEvent(code, out e);
            }
            e = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetKeyEvent(in KeyCode key, out GcKeyEvent e)
            => m_Context.InputKey.TryGetKeyEvent(key, out e);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetKeyEventArray(out NativeArray<GcKeyEvent>.ReadOnly array, out int count)
            => m_Context.InputKey.TryGetKeyEventArray(out array, out count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetKeyEventArray(in GcKeyEventPhase phase, out NativeArray<GcKeyEvent>.ReadOnly array, out int count)
            => m_Context.InputKey.TryGetKeyEventArray(phase, out array, out count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetKeyTrace(in char key, out GcKeyTrace trace)
        {
            if (key.TryGetKeyCode(out var code))
            {
                return m_Context.InputKey.TryGetKeyTrace(code, out trace);
            }
            trace = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetKeyTrace(in KeyCode key, out GcKeyTrace trace)
            => m_Context.InputKey.TryGetKeyTrace(key, out trace);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetKeyTraceArray(out NativeArray<GcKeyTrace>.ReadOnly array, out int count)
            => m_Context.InputKey.TryGetKeyTraceArray(out array, out count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetKeyTraceArray(in GcKeyEventPhase phase, out NativeArray<GcKeyTrace>.ReadOnly array, out int count)
            => m_Context.InputKey.TryGetKeyTraceArray(phase, out array, out count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GcAvailability TryGetOnlineImage(in string url, out Texture2D texture)
            => m_Context.Network.TryGetOnlineImage(url, out texture);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetOnlineImageSize(in string url, out int2 size)
            => m_Context.Network.TryGetOnlineImageSize(url, out size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GcAvailability TryGetOnlineSound(in string url, out AudioClip clip)
            => m_Context.Network.TryGetOnlineSound(url, out clip);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GcAvailability TryGetOnlineSound(in string url, in AudioType type, out AudioClip clip)
            => m_Context.Network.TryGetOnlineSound(url, type, out clip);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GcAvailability TryGetOnlineText(in string url, out string str)
            => m_Context.Network.TryGetOnlineText(url, out str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetPointerEvent(in int i, out GcPointerEvent e)
            => m_Context.InputPointer.TryGetPointerEvent(i, out e);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetPointerEvent(in GcPointerEventPhase phase, in int i, out GcPointerEvent e)
            => m_Context.InputPointer.TryGetPointerEvent(phase, i, out e);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetPointerEventArray(out NativeArray<GcPointerEvent>.ReadOnly array, out int count)
            => m_Context.InputPointer.TryGetPointerEventArray(out array, out count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetPointerEventArray(in GcPointerEventPhase phase, out NativeArray<GcPointerEvent>.ReadOnly array, out int count)
            => m_Context.InputPointer.TryGetPointerEventArray(phase, out array, out count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetPointerTapPoint(in int i, out float2 point)
            => m_Context.InputPointer.TryGetPointerTapPoint(i, out point);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetPointerTapPointArray(out NativeArray<float2>.ReadOnly array, out int count)
            => m_Context.InputPointer.TryGetPointerTapPointArray(out array, out count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetPointerTrace(in int i, out GcPointerTrace history)
            => m_Context.InputPointer.TryGetPointerTrace(i, out history);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetPointerTrace(in GcPointerEventPhase phase, in int i, out GcPointerTrace trace)
            => m_Context.InputPointer.TryGetPointerTrace(phase, i, out trace);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetPointerTraceArray(out NativeArray<GcPointerTrace>.ReadOnly array, out int count)
            => m_Context.InputPointer.TryGetPointerTraceArray(out array, out count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetPointerTraceArray(in GcPointerEventPhase phase, out NativeArray<GcPointerTrace>.ReadOnly array, out int count)
            => m_Context.InputPointer.TryGetPointerTraceArray(phase, out array, out count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetScreenKeyboardArea(out GcAABB area)
            => m_Context.InputKey.TryGetScreenKeyboardArea(out area);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryLoad(in string key, out BigInteger value)
            => m_Context.Storage.TryLoad(key, out string text)
            && BigInteger.TryParse(text, out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryLoad(in string key, out float value)
            => m_Context.Storage.TryLoad(key, out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryLoad(in string key, out int value)
            => m_Context.Storage.TryLoad(key, out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryLoad(in string key, out string value)
            => m_Context.Storage.TryLoad(key, out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveActor(in GcActor actor)
            => m_CurrentScene?.TryRemoveActor(actor) ?? false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnpauseSound(in GcSoundTrack track = GcSoundTrack.BGM1)
            => m_Context.Sound.UnpauseSound(track);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnregisterScene<T>() where T : GcScene
        {
            var key = typeof(T);
            if (m_SceneDict.TryGetValue(key, out var scene))
            {
                m_SceneDict.Remove(key);

                if (m_CurrentScene == scene)
                {
                    m_SceneLeaveFlag = true;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnregisterScene(in GcScene scene)
        {
            if (m_SceneDict.ContainsValue(scene))
            {
                m_SceneDict.Remove(scene.GetType());

                if (m_CurrentScene == scene)
                {
                    m_SceneLeaveFlag = true;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int UpdateCameraDevice()
            => m_Context.InputCamera.UpdateCameraDevice();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void WriteScreenImage(in string _)
            => m_Context.Storage.SaveScreenshotAsync();
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal GcProxy(in BehaviourBase behaviour)
        {
            m_Context = new GcContext(behaviour);
            m_SceneDict = new Dictionary<System.Type, GcScene>();

            GcScene.Inject(this);
            GcActor.Inject(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void DrawCurrentScene() => m_CurrentScene?.Draw();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnAterDraw()
        {
            if (m_SceneLeaveFlag)
            {
                m_CurrentScene?.LeaveScene();
                m_CurrentScene = null;
                m_SceneLeaveFlag = false;
            }

            foreach (var engine in m_Context.EngineArray)
            {
                engine.OnAfterDraw();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnBeforeUpdate(in System.DateTimeOffset now)
        {
            foreach (var engine in m_Context.EngineArray)
            {
                engine.OnBeforeUpdate(now);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnDisable() => m_Context.Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnEnable() => m_Context.Graphics?.Init();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        #endregion
    }
}
