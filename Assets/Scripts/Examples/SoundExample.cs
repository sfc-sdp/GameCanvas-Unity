/**
 * GameCanvas Sound Example
 * 
 * Copyright (c) 2016 Seibe TAKAHASHI
 * 
 * This software is released under the MIT License.
 * http://opensource.org/licenses/mit-license.php
 */
using GameCanvas;

public class SoundExample : GameBase
{
    const int BALL_MAX = 10;

    int count;
    int[] ballX;
    int[] ballY;

    override public void Start()
    {
        count = 0;
        ballX = new int[BALL_MAX];
        ballY = new int[BALL_MAX];
    }

    override public void Calc()
    {
        // タップした時だけ処理する
        if (gc.isTap)
        {
            // 10個未満の時
            if (count < BALL_MAX)
            {
                // 0番目の音声を再生する
                gc.PlaySE(0);

                // タップしたところにボールを置く
                ballX[count] = gc.touchX - 12;
                ballY[count] = gc.touchY - 12;
                count++;
            }

            // 10個以上の時
            else
            {
                // 1番目の音声を再生する
                gc.PlaySE(1);

                // 全てのボールを消す
                count = 0;
            }
        }
    }

    override public void Draw()
    {
        // 画面を白で塗りつぶします
        gc.ClearScreen();

        // 背景画像を描画します
        gc.DrawImage(0, 0, 0);

        // ボールを描画します
        for (int i = 0; i < count; ++i)
        {
            gc.DrawImage(1, ballX[i], ballY[i]);
        }
    }
}
