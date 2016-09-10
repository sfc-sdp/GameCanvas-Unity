/**
 * GameCanvas Gyroscope Example
 * 
 * Copyright (c) 2016 Seibe TAKAHASHI
 * 
 * This software is released under the MIT License.
 * http://opensource.org/licenses/mit-license.php
 */
using GameCanvas;

public class GyroscopeExample : GameBase
{
    const int NUM_BALL = 3;

    float[] ballX;
    float[] ballY;
    float[] ballDegree;

    override public void Start()
    {
        // ジャイロスコープを有効化します
        gc.isGyroEnabled = true;

        // ボール3つ分の変数を用意します
        ballX = new float[NUM_BALL];
        ballY = new float[NUM_BALL];
        ballDegree = new float[NUM_BALL];

        // 画面左上にボールを並べます
        for (int i = 0; i < NUM_BALL; ++i)
        {
            ballX[i] = 48 + i * 64;
            ballY[i] = 48;
            ballDegree[i] = 0;
        }
    }

    override public void Calc()
    {
        // ボールの回転角度にジャイロスコープの値を加算します
        ballDegree[0] += gc.gyroX;
        ballDegree[1] += gc.gyroY;
        ballDegree[2] += gc.gyroZ;
    }

    override public void Draw()
    {
        // 画面を白で塗りつぶします
        gc.ClearScreen();

        // 背景画像を描画します
        gc.DrawImage(0, 0, 0);

        // ボールを3つ描画します
        for (int i = 0; i < NUM_BALL; ++i)
        {
            gc.DrawRotatedImage(i % 2 + 1, ballX[i], ballY[i], ballDegree[i], 12, 12);
        }
    }
}
