using UnityEngine;

namespace GameCanvas
{
    public class GameBase : MonoBehaviour
    {
        protected GameCanvas gc;

        private void Awake()
        {
            gc = GameCanvas.Instance;
        }

        private void Start()
        {
            initGame();
        }

        private void Update()
        {
            updateGame();
        }

        private void Draw()
        {
            drawGame();
        }

        private void OnDestroy()
        {
            finalGame();
        }

        public virtual void initGame() { }

        public virtual void updateGame() { }

        public virtual void drawGame() { }

        public virtual void finalGame() { }
    }
}
