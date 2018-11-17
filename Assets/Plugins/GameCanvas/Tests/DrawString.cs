
namespace GameCanvas.Tests
{
    public sealed class DrawString : GameBase
    {
        public override void DrawGame()
        {
            gc.ClearScreen();

            var y = 0;
            for (var i = 0; i < 10; ++i)
            {
                y += i * 20;
                gc.SetColor(i * 6, i * 12, i * 25);
                gc.SetFontSize(i * 15 + 1);
                gc.DrawString("123あいう壱弐参", 0, y);
            }
        }
    }
}
