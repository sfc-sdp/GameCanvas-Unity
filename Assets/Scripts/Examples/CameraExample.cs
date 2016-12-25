/*------------------------------------------------------------*/
/// <summary>GameCanvas Camera Example</summary>
/// <author>Seibe TAKAHASHI</author>
/// <remarks>
/// (c) 2015-2016 Smart Device Programming.
/// This software is released under the MIT License.
/// http://opensource.org/licenses/mit-license.php
/// </remarks>
/*------------------------------------------------------------*/
using GameCanvas;

public class CameraExample : GameBase
{
    int count;
    float cameraCenterX, cameraCenterY;
    float cameraX, cameraY, cameraAngle;
    float scale;

    override public void Start()
    {
        // カメラ映像入力を有効化します
        gc.StartCameraService();
        count = 0;
        cameraCenterX = gc.cameraImageWidth * 0.5f;
        cameraCenterY = gc.cameraImageHeight * 0.5f;

        // カメラが画面中央に大きく描画されるよう位置と拡縮率を計算します
        var scaleW = gc.screenWidth  / (float)gc.cameraImageWidth;
        var scaleH = gc.screenHeight / (float)gc.cameraImageHeight;
        if (scaleW > scaleH)
        {
            scale   = scaleW;
            cameraX = 0;
            cameraY = (gc.screenHeight - gc.cameraImageHeight * scale) * 0.5f;
        }
        else
        {
            scale   = scaleH;
            cameraX = (gc.screenWidth - gc.cameraImageWidth * scale) * 0.5f;
            cameraY = 0;
        }
    }

    public override void Calc()
    {
        ++count;

        if (count < 180)
        {
            cameraAngle = 0;
        }
        else if (count < 360)
        {
            cameraAngle = (count - 180) * 2;
        }
        else
        {
            count = 0;
            cameraAngle = 0;
        }
    }

    override public void Draw()
    {
        // 画面を白で塗りつぶします
        gc.ClearScreen();

        // カメラ映像を描画します
        gc.DrawCameraImageSRT(cameraX, cameraY, scale, scale, cameraAngle, cameraCenterX, cameraCenterY);
    }
}
