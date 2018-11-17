
namespace GameCanvas.Tests
{
    public sealed class DrawLine : GameBase
    {
        public override void DrawGame()
        {
            gc.ClearScreen();

            var y = 0;
            for (var i = 0; i < 10; ++i)
            {
                y += i * 20;
                gc.SetColor(i * 6, i * 12, i * 25);
                gc.DrawLine(i * 20, y, 100 + i * 40, y + i * 30);
            }
        }
    }
}
