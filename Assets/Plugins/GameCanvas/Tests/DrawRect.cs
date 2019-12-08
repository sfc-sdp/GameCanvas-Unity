
namespace GameCanvas.Tests
{
    public sealed class DrawRect : GameBase
    {
        public override void DrawGame()
        {
            gc.ClearScreen();

            gc.SetColor(255, 0, 0);
            gc.FillRect(0, 0, 100, gc.CanvasHeight);

            var y = 0;
            for (var i = 0; i < 10; ++i)
            {
                y += i * 20;
                gc.SetColor(i * 6, i * 12, i * 25, 256 - i * 10);
                gc.FillRect(0, y, i * 50 + 50, i * 15 + 15);
            }

            gc.SetColor(0, 255, 0);
            gc.FillRect(0, 550, 100, 250);
        }
    }
}
