/**
 * GameCanvas Serialize Example
 * 
 * Copyright (c) 2016 Seibe TAKAHASHI
 * 
 * This software is released under the MIT License.
 * http://opensource.org/licenses/mit-license.php
 */
using GameCanvas;

public class SerializeExample : GameBase
{
    int num;
    float colorR, colorG, colorB;

    override public void Start()
    {
        // セーブデータを読み込む
        num = gc.LoadAsInt("random");
        colorR = gc.LoadAsNumber("red");
        colorG = gc.LoadAsNumber("green");
        colorB = gc.LoadAsNumber("blue");

        // もし保存していなければ
        if (num == 0)
        {
            // ランダムな値を設定します
            num = gc.Random(1, 100);
            colorR = gc.Random();
            colorG = gc.Random();
            colorB = gc.Random();

            // セーブデータを書き込みます
            gc.SaveAsInt("random", num);
            gc.SaveAsNumber("red", colorR);
            gc.SaveAsNumber("green", colorG);
            gc.SaveAsNumber("blue", colorB);
            gc.WriteDataToStorage();
        }
    }

    override public void Calc()
    {
        // 画面をタップするたびに
        if (gc.isTap)
        {
            // ランダムな値を設定します
            num = gc.Random(1, 100);
            colorR = gc.Random();
            colorG = gc.Random();
            colorB = gc.Random();

            // セーブデータを書き込みます
            gc.SaveAsInt("random", num);
            gc.SaveAsNumber("red", colorR);
            gc.SaveAsNumber("green", colorG);
            gc.SaveAsNumber("blue", colorB);
            gc.WriteDataToStorage();
        }
    }

    override public void Draw()
    {
        // 画面を1色で塗りつぶします
        gc.SetColor(colorR, colorG, colorB);
        gc.FillRect(0, 0, gc.screenWidth, gc.screenHeight);

        // 数字を描画します
        gc.SetColor(0, 0, 0);
        gc.DrawString(201, 201, num.ToString());
        gc.SetColor(1, 1, 1);
        gc.DrawString(200, 200, num.ToString());
    }
}
