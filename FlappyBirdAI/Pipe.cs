using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlappyBirdAI
{
    public class Pipe : Sprite
    {
        public float BottomPipe { get; private set; }
        private float pipeGap;
        private float speed;
        private float yStartPos;

        public Pipe(Vector2 position, float scale, Texture2D texture, float speed, int pipeHeightMin, int pipeHeightMax, int gap, float yStartPos)
            : base(texture, position, Color.White, SpriteEffects.None, scale, 0, new Rectangle(0, 0, texture.Width, texture.Height), Vector2.Zero)
        {
            this.speed = speed;
            RandomizePipe(pipeHeightMin, pipeHeightMax, gap);
            pipeGap = gap;
            this.yStartPos = yStartPos;
        }

        private void RandomizePipe(int pipeHeightMin, int pipeHeightMax, int gap)
        {
            Random random = new Random();
            BottomPipe = random.Next(pipeHeightMin, pipeHeightMax);
        }

        public float GetXPos()
        {
            return position.X;
        }

        public void Update()
        {
            position = new Vector2(position.X -= speed, position.Y);
        }
    }
}
