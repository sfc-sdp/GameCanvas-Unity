
namespace GameCanvas.Tests
{
    public sealed class DrawRect : GameBase
    {
        public override void DrawGame()
        {
            gc.ClearScreen();

            var y = 0;
            for (var i = 0; i < 10; ++i)
            {
                y += i * 20;
                gc.SetColor(i * 6, i * 12, i * 25, 256 - i * 10);
                gc.SetFontSize(i * 15 + 1);
                gc.FillRect(0, y, i * 50 + 50, i * 15 + 15);
            }
        }
    }
}
