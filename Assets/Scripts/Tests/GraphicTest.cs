/*------------------------------------------------------------*/
/// <summary>GameCanvas GraphicTest</summary>
/// <author>Seibe TAKAHASHI</author>
/// <remarks>
/// (c) 2015-2017 Smart Device Programming.
/// This software is released under the MIT License.
/// http://opensource.org/licenses/mit-license.php
/// </remarks>
/*------------------------------------------------------------*/
using GameCanvas;
using UnityEngine;
using UnityEngine.Assertions;

public class GraphicTest : GameBase
{
    private const int canvasW = 720;
    private const int canvasH = 1280;
    private const int buttonW = 177;
    private const int buttonH = 100;
    private const int buttonX1 = 4 * 0;
    private const int buttonX2 = 4 * 1 + buttonW * 1;
    private const int buttonX3 = 4 * 2 + buttonW * 2;
    private const int buttonX4 = 4 * 3 + buttonW * 3;
    private const int buttonY = canvasH - buttonH - 4;
    private const float centerX = canvasW * 0.5f;
    private const float centerY = canvasH * 0.5f - buttonH - 8;

    private bool isTapButton1 = false;
    private bool isTapButton2 = false;
    private bool isTapButton3 = false;
    private bool isTapButton4 = false;
    private bool isTouchButton1 = false;
    private bool isTouchButton2 = false;
    private bool isTouchButton3 = false;
    private bool isTouchButton4 = false;

    private int mode = 1;
    private int count = 0;
    private float wave = 0f;

    override public void Start()
    {
        gc.SetResolution(canvasW, canvasH);
        gc.StartCameraService();
    }

    override public void Calc()
    {
        ++count;
        if (count == int.MaxValue) count = 0;
        wave = (gc.Sin(count) + 1f) * 0.5f;

        isTapButton1 = gc.isTap && gc.CheckHitRect(gc.touchX, gc.touchY, 1, 1, buttonX1, buttonY, buttonW, buttonH);
        isTapButton2 = gc.isTap && gc.CheckHitRect(gc.touchX, gc.touchY, 1, 1, buttonX2, buttonY, buttonW, buttonH);
        isTapButton3 = gc.isTap && gc.CheckHitRect(gc.touchX, gc.touchY, 1, 1, buttonX3, buttonY, buttonW, buttonH);
        isTapButton4 = gc.isTap && gc.CheckHitRect(gc.touchX, gc.touchY, 1, 1, buttonX4, buttonY, buttonW, buttonH);

        isTouchButton1 = gc.isTouch && gc.CheckHitRect(gc.touchX, gc.touchY, 1, 1, buttonX1, buttonY, buttonW, buttonH);
        isTouchButton2 = gc.isTouch && gc.CheckHitRect(gc.touchX, gc.touchY, 1, 1, buttonX2, buttonY, buttonW, buttonH);
        isTouchButton3 = gc.isTouch && gc.CheckHitRect(gc.touchX, gc.touchY, 1, 1, buttonX3, buttonY, buttonW, buttonH);
        isTouchButton4 = gc.isTouch && gc.CheckHitRect(gc.touchX, gc.touchY, 1, 1, buttonX4, buttonY, buttonW, buttonH);

        if (isTapButton1) mode = 1;
        if (isTapButton2) mode = 2;
        if (isTapButton3) mode = 3;
        if (isTapButton4) mode = 4;
    }

    override public void Draw()
    {
        gc.ClearScreen();

        gc.SetColor(0.8f, 0.8f, 0.8f);
        gc.SetFontSize(32);
        gc.DrawString(10, 15, mode.ToString(), -128);

        switch (mode)
        {
            case 1:
                _DrawLines();
                break;

            case 2:
                _DrawCirclesAndRects();
                break;

            case 3:
                _DrawImages();
                break;

            case 4:
                gc.DrawCameraImage(10, 70);
                break;
        }

        _DrawFooter();
    }

    private void _DrawLines()
    {
        gc.SetLineWidth(1);
        for (var i = 0; i < 40; ++i)
        {
            var lineY = 70 + i * 5;
            var lineW = gc.Sin(4.5f + i * 9 + count * 1.33f) * (canvasW - 100) * 0.5f;
            gc.SetColorHSV(i * 0.01f, 1f, wave);
            gc.DrawLine(centerX, lineY, centerX + lineW, lineY);
        }
        gc.SetLineWidth(4);
        for (var i = 0; i < 40; ++i)
        {
            var lineY = 270 + i * 10;
            var lineW = gc.Sin(4.5f + i * 9 + count * 1.33f) * (canvasW - 100) * 0.5f;
            gc.SetColorHSV(0.4f + i * 0.01f, 1f, wave);
            gc.DrawLine(centerX, lineY, centerX + lineW, lineY);
        }
        gc.SetLineWidth(8);
        for (var i = 0; i < 15; ++i)
        {
            var lineY = 670 + i * 20;
            var lineW = gc.Sin(4.5f + i * 9 + count * 1.33f) * (canvasW - 100) * 0.5f;
            gc.SetColorHSV(0.8f + i * 0.01f, 1f, wave);
            gc.DrawLine(centerX, lineY, centerX + lineW, lineY);
        }
        gc.SetLineWidth(16);
        for (var i = 0; i < 5; ++i)
        {
            var lineY = 970 + i * 30;
            var lineW = gc.Sin(139.5f + i * 9 + count * 1.33f) * (canvasW - 100) * 0.5f;
            gc.SetColorHSV(0.95f + i * 0.01f, 1f, wave);
            gc.DrawLine(centerX, lineY, centerX + lineW, lineY);
        }
    }

    private void _DrawCirclesAndRects()
    {
        for (var i = 0; i < 10; ++i)
        {
            var rectX = 10 + 71 * i;
            var circleX = 40.5f + 71 * i;

            gc.SetColorHSV(i * 0.1f, 1f, wave);
            gc.SetLineWidth(i + 1f);
            gc.DrawRect(rectX, 141, 61, 61);
            gc.DrawRotatedRect(rectX + 30.5f, 313.5f, 21.5f, 21.5f, count * 3);
            gc.DrawRotatedRect(rectX + 9, 434, 43, 43, count * 3, 21.5f, 21.5f);
            gc.DrawCircle(circleX, 597.5f, 30.5f);
        }

        for (var i = 0; i < 10; ++i)
        {
            var rectX = 10 + 71 * i;
            var circleX = 40.5f + 71 * i;

            gc.SetColorHSV(i * 0.1f, 1f, wave);
            gc.FillRect(rectX, 70, 61, 61);
            gc.FillRotatedRect(rectX + 30.5f, 242.5f, 21.5f, 21.5f, count * 3);
            gc.FillRotatedRect(rectX + 9, 363, 43, 43, count * 3, 21.5f, 21.5f);
            gc.FillCircle(circleX, 526.5f, 30.5f);

            gc.SetColor(0f, 0f, 0f, 0.15f);
            for (var j = 0; j < 6; ++j)
            {
                gc.FillRect(rectX, 212 + 71 * j, 61, 61, -1);
            }
        }

        gc.SetColor(0.6f, 0.6f, 0.6f);
        gc.FillRect(10, 638, 432.6f, 432.6f);
        gc.SetColor(0.5f, 0.5f, 0.5f);
        gc.FillRect(442.6f, 638, 267.4f, 267.4f);
        gc.SetColor(0.4f, 0.4f, 0.4f);
        gc.FillRect(544.8f, 905.4f, 165.2f, 165.2f);
        gc.SetColor(0.3f, 0.3f, 0.3f);
        gc.FillRect(442.6f, 968.4f, 102.2f, 102.2f);
        gc.SetColor(0.2f, 0.2f, 0.2f);
        gc.FillRect(442.6f, 905.4f, 63, 63);
        gc.SetColor(0.1f, 0.1f, 0.1f);
        gc.FillRect(505.6f, 905.4f, 39.2f, 63);

        gc.SetColor(0.7f, 0.7f, 0.7f);
        gc.FillCircle(226.3f, 854.3f, 213.6f);
        gc.SetColor(0.6f, 0.6f, 0.6f);
        gc.FillCircle(576.3f, 771.7f, 133.7f);
        gc.SetColor(0.5f, 0.5f, 0.5f);
        gc.FillCircle(627.4f, 988, 82.6f);
        gc.SetColor(0.4f, 0.4f, 0.4f);
        gc.FillCircle(493.7f, 1019.5f, 51.1f);
        gc.SetColor(0.3f, 0.3f, 0.3f);
        gc.FillCircle(474.1f, 936.9f, 31.5f);
    }

    private void _DrawImages()
    {
        gc.SetColor(0.1f, 0.1f, 0.1f);
        gc.FillRect(10, 70, 700, 510);
        gc.FillRect(10, 600, 700, 510);

        gc.DrawImage(0, 40, 85);

        gc.DrawImage(1, 60, 105);
        gc.DrawRotatedImage(2, 106, 105, wave * 360, 12, 12);
        gc.DrawScaledImage(1, 156, 105, 2f, 2f);
        gc.DrawImageSRT(2, 228, 105, 2f, 2f, wave * 360, 24, 24);

        var wave2 = 1f - wave;
        gc.DrawClippedImage(0, 40, 615, 0, 640 * wave, 480 * wave, 0);
        gc.DrawClippedImage(0, 40 + 640 * wave2, 615 + 480 * wave2, 480 * wave2, 0, 0, 640 * wave2);
    }

    private void _DrawFooter()
    {
        gc.SetColor(0.1f, 0.1f, 0.1f);
        gc.FillRect(0, canvasH - buttonH - 8, canvasW, buttonH + 8);

        _DrawButton(buttonX1, buttonY, buttonW, buttonH, "LINE"  , isTouchButton1);
        _DrawButton(buttonX2, buttonY, buttonW, buttonH, "CIRCLE\n RECT", isTouchButton2);
        _DrawButton(buttonX3, buttonY, buttonW, buttonH, "IMAGE" , isTouchButton3);
        _DrawButton(buttonX4, buttonY, buttonW, buttonH, "CAMERA", isTouchButton4);
    }

    private void _DrawButton(float x, float y, float w, float h, string str, bool isTouch)
    {
        if (isTouch)
        {
            gc.SetColor(0.25f, 0.25f, 0.25f);
        }
        else
        {
            gc.SetColor(0.15f, 0.15f, 0.15f);
        }
        gc.FillRect(x, y, w, h);

        var lines = str.Split('\n');
        var numBreak = lines.Length - 1;
        if (numBreak > 0)
        {
            var strLen = 0f;
            foreach (var line in lines)
            {
                strLen = strLen < line.Length ? line.Length : strLen;
            }

            var strX = x + (w - strLen * 20) * 0.5f;
            var strY = y + (h - 20 - 33 * numBreak) * 0.5f;
            gc.SetColor(1f, 1f, 1f);
            gc.SetFontSize(20f);
            gc.DrawMultiLineString(strX, strY, str);
        }
        else
        {
            var strX = x + (w - str.Length * 20) * 0.5f;
            var strY = y + (h - 20) * 0.5f;
            gc.SetColor(1f, 1f, 1f);
            gc.SetFontSize(20f);
            gc.DrawString(strX, strY, str);
        }
    }
}
