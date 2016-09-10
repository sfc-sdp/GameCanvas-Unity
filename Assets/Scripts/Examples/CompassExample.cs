/**
 * GameCanvas Compass Example
 * 
 * Copyright (c) 2016 Seibe TAKAHASHI
 * 
 * This software is released under the MIT License.
 * http://opensource.org/licenses/mit-license.php
 */
using GameCanvas;

public class CompassExample : GameBase
{
    override public void Start()
    {
        // 地磁気センサーを有効化します
        gc.isCompassEnabled = true;
    }

    override public void Draw()
    {
        // 画面を白で塗りつぶします
        gc.ClearScreen();

        // 背景画像を描画します
        gc.DrawImage(0, 0, 0);

        // ボールを描画します
        gc.DrawRotatedImage(1, 308, 228, gc.compass, 12, 12);
    }
}
