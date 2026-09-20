/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2026 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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

        public GcLocationService Location { get; }
        public GcCameraService Camera { get; }
        public GcNetworkService Network => m_Context.Network;
        public bool Contains(in GcRect rect, in GcPoint point) => GcHitTest.Contains(rect, point, RectAnchor, CurrentCoordinate);
        public void Drag(GcDrag drag, ref GcRect rect)
        {
            if (drag == null) throw new System.ArgumentNullException(nameof(drag));
            drag.Update(Pointers, ref rect, RectAnchor, CurrentCoordinate);
        }
        public void DrawImage(GcImageRequest image) => DrawImage(image, 0, 0);
        public void DrawImage(GcImageRequest image, float x, float y, float rotation = 0)
        {
            if (image == null) throw new System.ArgumentNullException(nameof(image));
            if (image.Status == GcRequestState.Succeeded && image.Texture != null)
                m_Context.Graphics.DrawTexture(image.Texture, new float2(x, y), rotation);
        }
        public void DrawImage(GcImageRequest image, in GcPoint position, float rotation = 0)
            => DrawImage(image, position.X, position.Y, rotation);
        public void DrawImage(GcImageRequest image, float x, float y, float width, float height, float rotation = 0)
            => DrawImage(image, GcRect.FromDegrees(x, y, width, height, rotation));
        public void DrawImage(GcImageRequest image, in GcRect rect)
        {
            if (image == null) throw new System.ArgumentNullException(nameof(image));
            if (image.Status == GcRequestState.Succeeded && image.Texture != null)
                m_Context.Graphics.DrawTexture(image.Texture, rect);
        }
        public bool PlaySound(GcSoundRequest sound, GcSoundTrack track = GcSoundTrack.BGM1, bool loop = false)
        {
            if (sound == null) throw new System.ArgumentNullException(nameof(sound));
            if (sound.Status != GcRequestState.Succeeded || sound.Clip == null) return false;
            m_Context.Sound.PlaySound(sound.Clip, track, loop); return true;
        }
        public GcAccelerationService Acceleration { get; }

        readonly GcContext m_Context;
        readonly Dictionary<System.Type, GcScene> m_SceneDict;

        GcScene? m_CurrentScene;
        GcScene? m_NextScene;
        object? m_NextSceneState;
        bool m_SceneEnterFlag;
        bool m_SceneLeaveFlag;
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        /// <inheritdoc/>
        public Color BackgroundColor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.BackgroundColor;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.Graphics.BackgroundColor = value; }
        }

        /// <inheritdoc/>
        public Color BorderColor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.BorderColor;
        }

        /// <inheritdoc/>
        public GcAABB CanvasAABB
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GcAABB.WH(m_Context.Graphics.CanvasSize);
        }

        /// <inheritdoc/>
        public float2 CanvasCenter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (float2)m_Context.Graphics.CanvasSize * 0.5f;
        }

        /// <inheritdoc/>
        public int CanvasHeight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.CanvasSize.y;
        }

        /// <inheritdoc/>
        public GcResolution CanvasResolution
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(m_Context.Graphics.CanvasSize, new() { denominator = 1, numerator = (uint)m_Context.Time.TargetFrameRate });
        }

        /// <inheritdoc/>
        public int2 CanvasSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.CanvasSize;
        }

        /// <inheritdoc/>
        public int CanvasWidth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.CanvasSize.x;
        }

        /// <inheritdoc/>
        public int CircleResolution
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.CircleResolution;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.Graphics.CircleResolution = value; }
        }

        /// <inheritdoc/>
        public Color Color
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.Color;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.Graphics.Color = value; }
        }

        /// <inheritdoc/>
        public Color ColorAqua
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(0.5f, 0.5f, 1f);
        }

        /// <inheritdoc/>
        public Color ColorBlack
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(0f, 0f, 0f);
        }

        /// <inheritdoc/>
        public Color ColorBlue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(0f, 0f, 1f);
        }

        /// <inheritdoc/>
        public Color ColorCyan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(0f, 1f, 1f);
        }

        /// <inheritdoc/>
        public Color ColorGray
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(0.5f, 0.5f, 0.5f);
        }

        /// <inheritdoc/>
        public Color ColorGreen
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(0f, 1f, 0f);
        }

        /// <inheritdoc/>
        public Color ColorPurple
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(1f, 0f, 1f);
        }

        /// <inheritdoc/>
        public Color ColorRed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(1f, 0f, 0f);
        }

        /// <inheritdoc/>
        public Color ColorWhite
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(1f, 1f, 1f);
        }

        /// <inheritdoc/>
        public Color ColorYellow
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(1f, 1f, 0f);
        }

        /// <inheritdoc/>
        public CoordinateScope CoordinateScope
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.CoordinateScope;
        }

        /// <inheritdoc/>
        public float CornerRadius
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.CornerRadius;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.Graphics.CornerRadius = value; }
        }

        /// <inheritdoc/>
        public float2x3 CurrentCoordinate
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.CurrentCoordinate;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.Graphics.CurrentCoordinate = value; }
        }

        /// <inheritdoc/>
        public int CurrentFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentFrame;
        }

        /// <inheritdoc/>
        public GcStyle CurrentStyle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.CurrentStyle;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.Graphics.CurrentStyle = value; }
        }

        /// <inheritdoc/>
        public System.DateTimeOffset CurrentTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime;
        }

        /// <inheritdoc/>
        public int CurrentTimeDay
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime.Day;
        }

        /// <inheritdoc/>
        public System.DayOfWeek CurrentTimeDayOfWeek
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime.DayOfWeek;
        }

        /// <inheritdoc/>
        public int CurrentTimeHour
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime.Hour;
        }

        /// <inheritdoc/>
        public int CurrentTimeMillisecond
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime.Millisecond;
        }

        /// <inheritdoc/>
        public int CurrentTimeMinute
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime.Minute;
        }

        /// <inheritdoc/>
        public int CurrentTimeMonth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime.Month;
        }

        /// <inheritdoc/>
        public int CurrentTimeSecond
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime.Second;
        }

        /// <inheritdoc/>
        public long CurrentTimestamp
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTimestamp;
        }

        /// <inheritdoc/>
        public int CurrentTimeYear
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.CurrentTime.Year;
        }

        /// <inheritdoc/>
        public int DeviceScreenHeight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.DeviceScreenSize.y;
        }

        /// <inheritdoc/>
        public int2 DeviceScreenSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.DeviceScreenSize;
        }

        /// <inheritdoc/>
        public int DeviceScreenWidth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.DeviceScreenSize.x;
        }

        /// <inheritdoc/>
        public GcFont Font
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.Font;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.Graphics.Font = value; }
        }

        /// <inheritdoc/>
        public int FontSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.FontSize;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.Graphics.FontSize = value; }
        }

        /// <inheritdoc/>
        public bool IsScreenKeyboardSupported
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputKey.IsScreenKeyboardSupported;
        }

        /// <inheritdoc/>
        public bool IsScreenKeyboardVisible
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputKey.IsScreenKeyboardVisible;
        }

        /// <inheritdoc/>
        public bool IsTouchPressureSupported
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputPointer.IsTouchPressureSupported;
        }

        /// <inheritdoc/>
        public bool IsTouchSupported
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputPointer.IsTouchSupported;
        }

        /// <inheritdoc/>
        public GcPointer Pointer => m_Context.InputPointer.Pointer;
        /// <inheritdoc/>
        public GcReadOnlyList<GcPointer> Pointers => m_Context.InputPointer.Pointers;
        /// <inheritdoc/>
        public GcReadOnlyList<GcPointerEvent> PointerEvents => m_Context.InputPointer.PointerEvents;

        /// <inheritdoc/>
        public GcLineCap LineCap
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.LineCap;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.Graphics.LineCap = value; }
        }

        /// <inheritdoc/>
        public float LineWidth
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.LineWidth;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.Graphics.LineWidth = value; }
        }

        /// <inheritdoc/>
        public System.DateTimeOffset NowTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.NowTime;
        }

        /// <inheritdoc/>
        public GcAnchor RectAnchor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.RectAnchor;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.Graphics.RectAnchor = value; }
        }

        /// <inheritdoc/>
        public GcAnchor StringAnchor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.StringAnchor;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.Graphics.StringAnchor = value; }
        }

        /// <inheritdoc/>
        public StyleScope StyleScope
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Graphics.StyleScope;
        }

        /// <inheritdoc/>
        public GcTapSettings TapSettings
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.InputPointer.TapSettings;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { m_Context.InputPointer.TapSettings = value; }
        }

        /// <inheritdoc/>
        public double TargetFrameInterval
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.TargetFrameInterval;
        }

        /// <inheritdoc/>
        public int TargetFrameRate
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.TargetFrameRate;
        }

        /// <inheritdoc/>
        public float TimeSincePrevFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.TimeSincePrevFrame;
        }

        /// <inheritdoc/>
        public double TimeSinceStartup
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.TimeSinceStartup;
        }

        /// <inheritdoc/>
        public bool VSyncEnabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Context.Time.VSyncEnabled;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Abs(in float value) => GcMath.Abs(value);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Abs(in int value) => GcMath.Abs(value);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddActor(in GcActor actor)
            => m_CurrentScene?.AddActor(actor);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AlmostSame(in float a, in float b) => GcMath.AlmostSame(a, b);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AlmostZero(in float value) => GcMath.AlmostZero(value);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Atan2(in float x, in float y) => GcMath.Atan2(new float2(x, y));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Atan2(in float2 v) => GcMath.Atan2(v);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float CalcStringHeight(in string str)
            => m_Context.Graphics.CalcStringHeight(str);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float2 CalcStringSize(in string str)
            => m_Context.Graphics.CalcStringSize(str);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float CalcStringWidth(in string str)
            => m_Context.Graphics.CalcStringWidth(str);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CanvasToScreenPoint(in float2 canvas, out float2 screen)
            => m_Context.Graphics.CanvasToScreenPoint(canvas, out screen);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CanvasToScreenPoint(in float2 canvas, out int2 screen)
            => m_Context.Graphics.CanvasToScreenPoint(canvas, out screen);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeBorderColor(in float r, in float g, in float b)
            => m_Context.Graphics.ChangeBorderColor(new Color(r, g, b));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeBorderColor(in Color color)
            => m_Context.Graphics.ChangeBorderColor(color);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeCanvasSize(in int width, in int height)
            => m_Context.Graphics.ChangeCanvasSize(new int2(width, height));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeCanvasSize(in int2 size)
            => m_Context.Graphics.ChangeCanvasSize(size);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeScene<T>(object? state = null) where T : GcScene
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

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Clamp(in float value, in float min, in float max)
            => GcMath.Clamp(value, min, max);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearCoordinate()
            => m_Context.Graphics.ClearCoordinate();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearScreen()
            => m_Context.Graphics.ClearScreen();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearSound()
            => m_Context.Sound.ClearSound();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearStyle()
            => m_Context.Graphics.ClearStyle();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Cos(in float degree) => GcMath.Cos(degree);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T CreateActor<T>() where T : GcActor, new()
            => m_CurrentScene?.CreateActor<T>() ?? throw new System.InvalidOperationException();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Cross(in float2 a, in float2 b) => GcMath.Cross(a, b);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CrossTest(in GcLine a, in GcLine b)
            => a.Intersects(b);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CrossTest(in GcLine a, in GcLine b, out float2 intersection)
            => a.Intersects(b, out intersection);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Dot(in float2 a, in float2 b) => GcMath.Dot(a, b);

        /// <inheritdoc/>
        public void DrawCamera() => DrawCamera(0, 0);
        /// <inheritdoc/>
        public void DrawCamera(in GcPoint position, float rotation = 0) => DrawCamera(position.X, position.Y, rotation);
        /// <inheritdoc/>
        public void DrawCamera(in float2 position, float rotation = 0) => DrawCamera(position.x, position.y, rotation);
        /// <inheritdoc/>
        public void DrawCamera(float x, float y, float rotation = 0)
            => DrawCamera(new GcRect(x, y, Camera.Width, Camera.Height) { Rotation = rotation });
        /// <inheritdoc/>
        public void DrawCamera(float x, float y, float width, float height, float rotation = 0)
            => DrawCamera(new GcRect(x, y, width, height) { Rotation = rotation });
        /// <inheritdoc/>
        public void DrawCamera(in GcRect rect)
        {
            var texture = Camera.Texture;
            if (texture == null || Camera.Width == 0 || Camera.Height == 0) return;
            var frame = Camera.Frame;
            var matrix = Engine.GcInputCameraEngine.CalcCameraMatrix(new float2(frame.Width, frame.Height), frame.Rotation, frame.Mirrored, RectAnchor);
            matrix = GcAffine.FromTRS(rect.Position, rect.Radian, rect.Size / new float2(Camera.Width, Camera.Height)).Mul(matrix);
            m_Context.Graphics.DrawTexture(texture, matrix);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawCircle()
            => m_Context.Graphics.DrawCircle();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawCircle(in float x, in float y, in float radius)
            => m_Context.Graphics.DrawCircle(new GcCircle(x, y, radius));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawCircle(in float2 position, in float radius)
            => m_Context.Graphics.DrawCircle(new GcCircle(position, radius));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawCircle(in GcCircle circle)
            => m_Context.Graphics.DrawCircle(circle);

        /// <summary>Assets/Resからの相対パスで画像を描きます。</summary>
        public void DrawImage(string path, float x, float y, float rotation = 0)
        {
            if (GcAssets.TryGetImage(path, out var image)) DrawImage(image, x, y, rotation);
            else DrawMissingImage(path, new GcRect(x, y, 64, 64, math.radians(rotation)));
        }
        /// <summary>画像を指定した領域へ拡大・縮小します。rotationは矩形の回転へ加える度数です。</summary>
        public void DrawImage(string path, in GcRect rect, float rotation = 0)
        {
            var area = rect; area.Radian += math.radians(rotation);
            if (GcAssets.TryGetImage(path, out var image)) DrawImage(image, area);
            else DrawMissingImage(path, area);
        }
        /// <summary>キャンバス座標に画像を描きます。</summary>
        public void DrawImage(string path, in GcPoint position, float rotation = 0)
            => DrawImage(path, position.X, position.Y, rotation);
        /// <summary>キャンバス座標に画像を描きます。</summary>
        public void DrawImage(in GcImage image, in GcPoint position, float rotation = 0)
            => DrawImage(image, position.X, position.Y, rotation);
        /// <summary>キャンバス座標に文字を描きます。</summary>
        public void DrawString(string text, in GcPoint position, float rotation = 0)
            => DrawString(text, position.X, position.Y, rotation);

        void DrawMissingImage(string path, in GcRect area)
        {
            GcAssets.ReportMissingImage(path);
            using (StyleScope)
            {
                SetStringAnchor(RectAnchor); SetColor(255, 0, 255); FillRect(area);
                SetColor(0, 0, 0); SetFontSize(16);
                DrawString(path ?? "<null>", area.Position.x, area.Position.y);
            }
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawImage(in GcImage image)
            => m_Context.Graphics.DrawImage(image);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawImage(in GcImage image, in float x, in float y, float rotation = 0f)
            => m_Context.Graphics.DrawImage(image, new float2(x, y), rotation);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawImage(in GcImage image, in float x, in float y, in float width, in float height, float rotation = 0f)
            => m_Context.Graphics.DrawImage(image, new GcRect(x, y, width, height, math.radians(rotation)));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawImage(in GcImage image, in float2 position, float rotation = 0f)
            => m_Context.Graphics.DrawImage(image, position, rotation);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawImage(in GcImage image, in GcRect rect, float rotation = 0)
            => m_Context.Graphics.DrawImage(image, rect, rotation);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawLine()
            => m_Context.Graphics.DrawLine();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawLine(in float2 begin, in float2 end)
            => m_Context.Graphics.DrawLine(GcLine.Segment(begin, end));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawLine(in float x0, in float y0, in float x1, in float y1)
            => m_Context.Graphics.DrawLine(GcLine.Segment(new float2(x0, y0), new float2(x1, y1)));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawLine(in GcLine line)
            => m_Context.Graphics.DrawLine(line);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRect()
            => m_Context.Graphics.DrawRect();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRect(in float x, in float y, in float width, in float height, float rotation = 0f)
            => m_Context.Graphics.DrawRect(new GcRect(x, y, width, height, math.radians(rotation)));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRect(in float2 position, in float2 size, float rotation = 0f)
            => m_Context.Graphics.DrawRect(new GcRect(position, size, math.radians(rotation)));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRect(in GcRect rect)
            => m_Context.Graphics.DrawRect(rect);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRoundedRect()
            => m_Context.Graphics.DrawRoundedRect();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRoundedRect(in float x, in float y, in float width, in float height, float rotation = 0f)
            => m_Context.Graphics.DrawRoundedRect(new GcRect(x, y, width, height, math.radians(rotation)));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRoundedRect(in float x, in float y, in float width, in float height, float cornerRadius, float rotation = 0f)
            => m_Context.Graphics.DrawRoundedRect(new GcRect(x, y, width, height, math.radians(rotation)), cornerRadius);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRoundedRect(in float2 position, in float2 size, float rotation = 0f)
            => m_Context.Graphics.DrawRoundedRect(new GcRect(position, size, math.radians(rotation)));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRoundedRect(in float2 position, in float2 size, float cornerRadius, float rotation = 0f)
            => m_Context.Graphics.DrawRoundedRect(new GcRect(position, size, math.radians(rotation)), cornerRadius);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRoundedRect(in GcRect rect)
            => m_Context.Graphics.DrawRoundedRect(rect);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawRoundedRect(in GcRect rect, float cornerRect)
            => m_Context.Graphics.DrawRoundedRect(rect, cornerRect);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawString(in string str)
            => m_Context.Graphics.DrawString(str);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawString(in string str, in float x, in float y, float rotation = 0f)
            => m_Context.Graphics.DrawString(str, new float2(x, y), rotation);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawString(in string str, in float x, in float y, in float width, in float height, float rotation = 0f)
            => m_Context.Graphics.DrawString(str, new GcRect(x, y, width, height, math.radians(rotation)));

        /// <inheritdoc/>
        public void DrawString(in string str, in float2 position, float rotation = 0f)
            => m_Context.Graphics.DrawString(str, position, rotation);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawString(in string str, in GcRect rect, float rotation = 0)
            => m_Context.Graphics.DrawString(str, rect, rotation);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawTexture(in Texture texture)
            => m_Context.Graphics.DrawTexture(texture);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawTexture(in Texture texture, in float2 position, float rotation = 0f)
            => m_Context.Graphics.DrawTexture(texture, position, rotation);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawTexture(in Texture texture, in float x, in float y, in float width, in float height, float rotation = 0f)
            => m_Context.Graphics.DrawTexture(texture, new GcRect(x, y, width, height, math.radians(rotation)));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawTexture(in Texture texture, in GcRect rect)
            => m_Context.Graphics.DrawTexture(texture, rect);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawTexture(in Texture texture, in float2x3 matrix)
            => m_Context.Graphics.DrawTexture(texture, matrix);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EraseSavedDataAll()
            => m_Context.Storage.EraseSavedDataAll();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillCircle()
            => m_Context.Graphics.FillCircle();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillCircle(in float x, in float y, in float radius)
            => m_Context.Graphics.FillCircle(new GcCircle(x, y, radius));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillCircle(in float2 position, in float radius)
            => m_Context.Graphics.FillCircle(new GcCircle(position, radius));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillCircle(in GcCircle circle)
            => m_Context.Graphics.FillCircle(circle);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillRect()
            => m_Context.Graphics.FillRect();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillRect(in float x, in float y, in float width, in float height, float rotation = 0f)
            => m_Context.Graphics.FillRect(new GcRect(x, y, width, height, math.radians(rotation)));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillRect(in float2 position, in float2 size, float rotation = 0f)
            => m_Context.Graphics.FillRect(new GcRect(position, size, math.radians(rotation)));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillRect(in GcRect rect)
            => m_Context.Graphics.FillRect(rect);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillRoundedRect()
            => m_Context.Graphics.FillRoundedRect();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillRoundedRect(in float x, in float y, in float width, in float height, float rotation = 0f)
            => m_Context.Graphics.FillRoundedRect(new GcRect(x, y, width, height, math.radians(rotation)));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillRoundedRect(in float x, in float y, in float width, in float height, float cornerRadius, float rotation = 0f)
            => m_Context.Graphics.FillRoundedRect(new GcRect(x, y, width, height, math.radians(rotation)), cornerRadius);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillRoundedRect(in float2 position, in float2 size, float rotation = 0f)
            => m_Context.Graphics.FillRoundedRect(new GcRect(position, size, math.radians(rotation)));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillRoundedRect(in float2 position, in float2 size, float cornerRadius, float rotation = 0f)
            => m_Context.Graphics.FillRoundedRect(new GcRect(position, size, math.radians(rotation)), cornerRadius);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillRoundedRect(in GcRect rect)
            => m_Context.Graphics.FillRoundedRect(rect);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillRoundedRect(in GcRect rect, float cornerRadius)
            => m_Context.Graphics.FillRoundedRect(rect, cornerRadius);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GcActor? GetActor()
            => (m_CurrentScene != null) && m_CurrentScene.TryGetActor(0, out var actor) ? actor : null;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? GetActor<T>() where T : GcActor
            => (m_CurrentScene != null) && m_CurrentScene.TryGetActor<T>(0, out var actor) ? actor : null;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetActorCount()
            => m_CurrentScene?.GetActorCount() ?? 0;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetActorCount<T>() where T : GcActor
            => m_CurrentScene?.GetActorCount<T>() ?? 0;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetImageHeight(in GcImage image) => image.m_Size.y;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int2 GetImageSize(in GcImage image) => image.m_Size;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetImageWidth(in GcImage image) => image.m_Size.x;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetSoundLevel(GcSoundTrack track = GcSoundTrack.Master)
            => m_Context.Sound.GetSoundLevel(track);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetSoundVolume(GcSoundTrack track = GcSoundTrack.Master)
            => math.pow(10, m_Context.Sound.GetSoundLevel(track) * 0.05f);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HideScreenKeyboard()
            => m_Context.InputKey.HideScreenKeyboard();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HitTest(in GcAABB a, in GcAABB b)
            => a.Overlaps(b);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HitTest(in GcAABB aabb, in float2 point)
            => aabb.Contains(point);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HitTest(in GcCircle circle1, in GcCircle circle2)
            => circle1.Overlaps(circle2);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HitTest(in GcCircle circle, in float2 point)
            => circle.Contains(point);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GcKeyState Key(GcKey key) => m_Context.InputKey.Key(key);
        public GcReadOnlyList<GcKeyEvent> KeyEvents => m_Context.InputKey.KeyEvents;
        public GcReadOnlyList<GcPoint> Taps => m_Context.InputPointer.Taps;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPlayingSound(GcSoundTrack track = GcSoundTrack.BGM1)
            => m_Context.Sound.IsPlayingSound(track);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Max(in float a, in float b) => GcMath.Max(a, b);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Min(in float a, in float b) => GcMath.Min(a, b);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PauseSound(GcSoundTrack track = GcSoundTrack.BGM1)
            => m_Context.Sound.PauseSound(track);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PlaySE(in GcSound sound)
            => m_Context.Sound.PlaySound(sound, GcSoundTrack.SE, false);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PlaySE(in AudioClip clip)
            => m_Context.Sound.PlaySound(clip, GcSoundTrack.SE);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PlaySound(in GcSound sound, GcSoundTrack track = GcSoundTrack.BGM1, bool loop = false)
            => m_Context.Sound.PlaySound(sound, track, loop);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PlaySound(in AudioClip clip, GcSoundTrack track = GcSoundTrack.BGM1, bool loop = false)
            => m_Context.Sound.PlaySound(clip, track, loop);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PopCoordinate()
            => m_Context.Graphics.PopCoordinate();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PopStyle()
            => m_Context.Graphics.PopStyle();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushCoordinate()
            => m_Context.Graphics.PushCoordinate();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushStyle()
            => m_Context.Graphics.PushStyle();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Random() => GcMath.Random();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Random(in int min, in int maxExclusive) => GcMath.Random(min, maxExclusive);

        /// <inheritdoc/>
        public int Random(int maxExclusive) => GcMath.Random(maxExclusive);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Random(in float min, in float maxExclusive) => GcMath.Random(min, maxExclusive);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RebuildFontTexture()
            => m_Context.Graphics.RebuildFontTexture();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterScene<T>() where T : GcScene, new()
            => m_SceneDict.Add(typeof(T), new T());

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterScene(in GcScene scene)
            => m_SceneDict.Add(scene.GetType(), scene);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveActorAll()
            => m_CurrentScene?.RemoveActorAll();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Repeat(in float value, in float max) => GcMath.Repeat(value, max);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RotateCoordinate(in float rotation)
            => m_Context.Graphics.RotateCoordinate(rotation);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RotateCoordinate(in float rotation, in float originX, in float originY)
            => m_Context.Graphics.RotateCoordinate(rotation, new float2(originX, originY));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RotateCoordinate(in float rotation, in float2 origin)
            => m_Context.Graphics.RotateCoordinate(rotation, origin);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float2 RotateVector(in float2 vector, in float degree)
            => GcMath.RotateVector(vector, degree);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Round(in float value) => GcMath.Round(value);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Round(in double value) => GcMath.Round(value);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Save(in string key, float? value)
            => m_Context.Storage.Save(key, value);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Save(in string key, int? value)
            => m_Context.Storage.Save(key, value);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Save(in string key, string? value)
            => m_Context.Storage.Save(key, value);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SaveScreenshotAsync(System.Action<string?>? onComplete = null)
            => m_Context.Storage.SaveScreenshotAsync(onComplete);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ScaleCoordinate(in float sx, in float sy)
            => m_Context.Graphics.ScaleCoordinate(new float2(sx, sy));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ScaleCoordinate(in float2 scaling)
            => m_Context.Graphics.ScaleCoordinate(scaling);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ScreenToCanvasPoint(in float2 screen, out float2 canvas)
            => m_Context.Graphics.ScreenToCanvasPoint(screen, out canvas);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ScreenToCanvasPoint(in float2 screen, out int2 canvas)
            => m_Context.Graphics.ScreenToCanvasPoint(screen, out canvas);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBackgroundColor(int r, int g, int b)
            => m_Context.Graphics.BackgroundColor = new GcColor(r, g, b).ToUnity();

        public void SetBackgroundColor(in GcColor color)
            => m_Context.Graphics.BackgroundColor = color.ToUnity();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBackgroundColor(in Color color)
            => m_Context.Graphics.BackgroundColor = color;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetColor(in GcColor color)
            => m_Context.Graphics.Color = color.ToUnity();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetColor(in Color color)
            => m_Context.Graphics.Color = color;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetColor(in Color color, in float alpha)
            => m_Context.Graphics.Color = new Color(color.r, color.g, color.b, alpha);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetColor(int r, int g, int b, int a = 255)
            => SetColor(new GcColor(r, g, b, a));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCoordinate(in float2x3 matrix)
            => m_Context.Graphics.CurrentCoordinate = matrix;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFont(in GcFont font)
            => m_Context.Graphics.Font = font;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFontSize(in int fontSize)
            => m_Context.Graphics.FontSize = fontSize;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFrameInterval(in double targetDeltaTime, bool vSyncEnabled = true)
            => m_Context.Time.SetFrameInterval(targetDeltaTime, vSyncEnabled);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFrameRate(in int targetFrameRate, bool vSyncEnabled = true)
            => m_Context.Time.SetFrameRate(targetFrameRate, vSyncEnabled);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLineCap(in GcLineCap lineCap)
            => m_Context.Graphics.LineCap = lineCap;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLineWidth(in float lineWidth)
            => m_Context.Graphics.LineWidth = lineWidth;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetRandomState() => GcMath.GetRandomState();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRandomSeed(in uint seed) => GcMath.SetRandomSeed(seed);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRandomSeedByIndex(in uint index) => GcMath.SetRandomSeedByIndex(index);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRectAnchor(in GcAnchor anchor)
            => m_Context.Graphics.RectAnchor = anchor;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSoundLevel(in float decibel, GcSoundTrack track = GcSoundTrack.Master)
            => m_Context.Sound.SetSoundLevel(decibel, track);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSoundVolume(in float volume, GcSoundTrack track = GcSoundTrack.Master)
            => m_Context.Sound.SetSoundLevel(20f * math.log10(volume), track);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetStringAnchor(in GcAnchor anchor)
            => m_Context.Graphics.StringAnchor = anchor;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetStyle(in GcStyle style)
            => m_Context.Graphics.CurrentStyle = style;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ShowScreenKeyboard()
            => m_Context.InputKey.ShowScreenKeyboard();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Sin(in float degree) => GcMath.Sin(degree);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Sqrt(in float value) => GcMath.Sqrt(value);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StopSound(GcSoundTrack track = GcSoundTrack.BGM1)
            => m_Context.Sound.StopSound(track);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SweepTest(in GcAABB @static, in float2 @dynamic, in float2 delta, out GcSweepResult hit)
            => @static.SweepTest(@dynamic, delta, out hit);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SweepTest(in GcAABB @static, in GcAABB @dynamic, in float2 delta, out GcSweepResult hit)
            => @static.SweepTest(@dynamic, delta, out hit);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TranslateCoordinate(in float tx, in float ty)
            => m_Context.Graphics.TranslateCoordinate(new float2(tx, ty));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TranslateCoordinate(in float2 translation)
            => m_Context.Graphics.TranslateCoordinate(translation);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetActor(in int i, [NotNullWhen(true)] out GcActor? actor)
        {
            if (m_CurrentScene != null)
            {
                return m_CurrentScene.TryGetActor(i, out actor);
            }
            actor = null;
            return false;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetActor<T>(in int i, [NotNullWhen(true)] out T? actor) where T : GcActor
        {
            if (m_CurrentScene != null)
            {
                return m_CurrentScene.TryGetActor<T>(i, out actor);
            }
            actor = null;
            return false;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetActorAll<T>(out System.ReadOnlySpan<T> actors) where T : GcActor
        {
            if (m_CurrentScene != null)
            {
                return m_CurrentScene.TryGetActorAll(out actors);
            }
            actors = default;
            return false;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetScreenKeyboardArea(out GcAABB area)
            => m_Context.InputKey.TryGetScreenKeyboardArea(out area);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryLoad(in string key, out float value)
            => m_Context.Storage.TryLoad(key, out value);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryLoad(in string key, out int value)
            => m_Context.Storage.TryLoad(key, out value);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryLoad(in string key, [NotNullWhen(true)] out string? value)
            => m_Context.Storage.TryLoad(key, out value);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveActor(in GcActor actor)
            => m_CurrentScene?.TryRemoveActor(actor) ?? false;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnpauseSound(GcSoundTrack track = GcSoundTrack.BGM1)
            => m_Context.Sound.UnpauseSound(track);

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        #endregion
        #region 公開関数（廃止）
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal GcProxy(in BehaviourBase behaviour, GcCameraService? camera = null)
        {
            m_Context = new GcContext(behaviour);
            Location = new GcLocationService(behaviour);
            Camera = camera ?? new GcCameraService();
            Acceleration = new GcAccelerationService();
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
            Location.Tick();
            Camera.Tick();
            Acceleration.Tick();
            foreach (var engine in m_Context.EngineArray)
            {
                engine.OnBeforeUpdate(now);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnDisable() { Camera.SetPaused(true); Location.Stop(); Acceleration.SetPaused(true); Acceleration.Dispose(); m_Context.Dispose(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnEnable() { Camera.SetPaused(false); Acceleration.SetPaused(false); m_Context.Graphics?.Init(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnFocus(bool focus)
        {
            m_Context.InputPointer.SetFocused(focus);
            m_Context.InputKey.SetFocused(focus);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnPause()
        {
            Network.CancelAll();
            Camera.SetPaused(true);
            Location.Stop();
            Acceleration.SetPaused(true);
            m_Context.InputPointer.SetPaused(true);
            m_Context.InputKey.SetPaused(true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnUnpause()
        {
            Camera.SetPaused(false);
            Acceleration.SetPaused(false);
            m_Context.Graphics.RebuildFontTexture();
            m_Context.InputPointer.SetPaused(false);
            m_Context.InputKey.SetPaused(false);
        }

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
