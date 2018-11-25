
namespace GameCanvas.Tests
{
    public sealed class PlaySound : GameBase
    {
        const int buttonNum = 4;
        readonly string[] buttonLabel = { "PLAY SE1", "PLAY SE2", "SET VOLUME 1  ", "SET VOLUME 100" };
        readonly bool[] buttonHit = { false, false, false, false };
        readonly int[][] buttonArea = {
            new[] { 20,  20, 440, 100 },
            new[] { 20, 140, 440, 100 },
            new[] { 20, 260, 440, 100 },
            new[] { 20, 380, 440, 100 }
        };

        public override void InitGame()
        {
            gc.SetResolution(512, 512);
            gc.SetFontSize(50);
        }

        public override void UpdateGame()
        {
            for (var i = 0; i < buttonNum; i++)
            {
                buttonHit[i] = gc.CheckHitRect(buttonArea[i][0], buttonArea[i][1], buttonArea[i][2], buttonArea[i][3], gc.GetPointerX(0), gc.GetPointerY(0), 1, 1);
                if (buttonHit[i] && gc.GetIsPointerBegan(0))
                {
                    switch (i)
                    {
                        case 0: gc.PlaySE(0); break;
                        case 1: gc.PlaySE(1); break;
                        case 2: gc.SetSoundVolume(1, ESoundTrack.SE); break;
                        case 3: gc.SetSoundVolume(100, ESoundTrack.SE); break;
                    }
                }
            }
        }

        public override void DrawGame()
        {
            gc.ClearScreen();

            for (var i = 0; i < buttonNum; i++)
            {
                if (buttonHit[i])
                {
                    gc.SetColor(20, 20, 20);
                    gc.DrawRect(buttonArea[i][0], buttonArea[i][1], buttonArea[i][2], buttonArea[i][3]);
                    gc.DrawCenterString(buttonLabel[i], buttonArea[i][0] + buttonArea[i][2] / 2, buttonArea[i][1] + 32);
                }
                else
                {
                    gc.SetColor(20, 20, 20);
                    gc.FillRect(buttonArea[i][0], buttonArea[i][1], buttonArea[i][2], buttonArea[i][3]);
                    gc.SetColor(240, 240, 240);
                    gc.DrawCenterString(buttonLabel[i], buttonArea[i][0] + buttonArea[i][2] / 2, buttonArea[i][1] + 32);
                }
            }
        }
    }
}
