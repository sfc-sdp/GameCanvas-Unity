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
    int cameraX, cameraY;
    float scale;

    override public void Start()
    {
        // カメラ映像入力を有効化します
        gc.StartCameraService();

        // カメラが画面中央に大きく描画されるよう位置と拡縮率を計算します
        var scaleW = gc.screenWidth  / (float)gc.cameraImageWidth;
        var scaleH = gc.screenHeight / (float)gc.cameraImageHeight;
        if (scaleW > scaleH)
        {
            scale   = scaleW;
            cameraX = 0;
            cameraY = (int)(gc.screenHeight * (scaleH - scaleW));
        }
        else
        {
            scale   = scaleH;
            cameraX = (int)(gc.screenWidth * (scaleW - scaleH));
            cameraY = 0;
        }
    }

    override public void Draw()
    {
        // 画面を白で塗りつぶします
        gc.ClearScreen();

        // カメラ映像を描画します
        gc.DrawScaledCameraImage(cameraX, cameraY, scale, scale);
    }
}
