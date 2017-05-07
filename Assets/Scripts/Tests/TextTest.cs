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

public class TextTest : GameBase
{
    private const int canvasW = 720;
    private const int canvasH = 1280;

    private readonly char[] chars = "GameCanvas".ToCharArray();

    private string[] text;
    private float[] textX;
    private float[] textY;
    private float[] fontSize;
    private char[][] builder;

    private int column;
    private int count;

    override public void Start()
    {
        gc.SetResolution(canvasW, canvasH);

        var a = 10f;
        var b = 0f;
        for (column = 0; b < canvasH; ++column)
        {
            a += column;
            b += a * 1.2f;
        }

        UnityEngine.Debug.Log(column);

        text = new string[column];
        textX = new float[column];
        textY = new float[column];
        fontSize = new float[column];
        builder = new char[column][];

        a = 10f;
        b = 0f;
        for (var i = 0; i < column; ++i)
        {
            a += i;

            fontSize[i] = a;
            textY[i] = b;

            var len = UnityEngine.Mathf.CeilToInt(canvasW / a) + 1;
            builder[i] = new char[len];
            for (var j = 0; j < len; ++j)
            {
                builder[i][j] = chars[j % 10];
            }
            text[i] = new string(builder[i]);

            b += a * 1.2f;
        }
    }

    override public void Calc()
    {
        ++count;
        if (count == 300) count = 0;

        var a = count % 30 == 0;
        var b = count / 30;
        var c = (count / 30f) - b;

        for (var i = 0; i < column; ++i)
        {
            if (a)
            {
                var l = builder[i].Length - 1;
                for (var j = 0; j < l; ++j)
                {
                    builder[i][j] = builder[i][j + 1];
                }
                builder[i][l] = chars[(b + l) % 10];
                text[i] = new string(builder[i]);
            }

            textX[i] = -c * fontSize[i];
        }
    }

    override public void Draw()
    {
        gc.ClearScreen();

        gc.SetColor(0.2f, 0.2f, 0.2f);
        for (var i = 0; i < column; ++i)
        {
            gc.SetColorHSV((float)i / column, 1f, 0.6f);
            gc.SetFontSize(fontSize[i]);
            gc.DrawString(textX[i], textY[i], text[i]);
        }
    }
}
