/**
 * GameCanvas Accelerator Example
 * 
 * Copyright (c) 2016 Seibe TAKAHASHI
 * 
 * This software is released under the MIT License.
 * http://opensource.org/licenses/mit-license.php
 */
 using GameCanvas;

public class AcceleratorExample : GameBase
{
    float ballX;
    float ballY;
    float ballSpeedX;
    float ballSpeedY;
    float ballWidth;
    float ballHeight;

    override public void Start()
    {
        // ボールの大きさを取得します
        ballWidth = gc.GetImageWidth(1);
        ballHeight = gc.GetImageHeight(1);

        // 画面の中心にボールを置きます
        ballX = (gc.screenWidth - ballWidth) / 2;
        ballY = (gc.screenHeight - ballHeight) / 2;

        // ボールの初速はゼロ（静止）にします
        ballSpeedX = 0;
        ballSpeedY = 0;
    }

    override public void Calc()
    {
        // ボールの速度に端末の加速度センサーの値を加算します
        ballSpeedX += gc.acceX;
        ballSpeedY += gc.acceY;

        // ボールの速度を元に位置を更新します
        ballX += ballSpeedX;
        ballY += ballSpeedY;

        if (ballX <= 0)
        {
            // ボールが画面左に到達したら
            // X軸の速度を反転させ、少し弱めます
            ballX = 0;
            ballSpeedX *= -0.5f;
        }

        else if (ballX >= gc.screenWidth - ballWidth)
        {
            // ボールが画面右に到達したら
            // X軸の速度を反転させ、少し弱めます
            ballX = gc.screenWidth - ballWidth;
            ballSpeedX *= -0.5f;
        }

        if (ballY < 0)
        {
            // ボールが画面上に到達したら
            // Y軸の速度を反転させ、少し弱めます
            ballY = 0;
            ballSpeedY *= -0.5f;
        }

        else if (ballY > gc.screenHeight - ballHeight)
        {
            // ボールが画面下に到達したら
            // Y軸の速度を反転させ、少し弱めます
            ballY = gc.screenHeight - ballHeight;
            ballSpeedY *= -0.5f;
        }
    }

    override public void Draw()
    {
        // 画面を白で塗りつぶします
        gc.ClearScreen();

        // 背景画像を描画します
        gc.DrawImage(0, 0, 0);

        // ボールを描画します
        gc.DrawImage(1, ballX, ballY);
    }
}
