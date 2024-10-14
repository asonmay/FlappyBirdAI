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
        public float BottomPipeHeight { get; private set; }
        public float YStartPos { get; private set; }
        public Rectangle BottomPipe { get; private set; }
        public Rectangle TopPipe { get; private set; }

        private readonly float pipeGap;
        private readonly float speed;
        private readonly int pipeHeightMin;
        private readonly int pipeHeightMax;
        private Vector2 startingPos;

        public Pipe(Vector2 startPos, Vector2 position, float scale, Texture2D texture, float speed, int pipeHeightMin, int pipeHeightMax, int gap)
            : base(texture, position, Color.White, SpriteEffects.None, scale, 0, new Rectangle(0, 0, texture.Width, texture.Height), Vector2.Zero)
        {
            this.speed = speed;
            pipeGap = gap;
            startingPos = startPos;
            this.pipeHeightMin = pipeHeightMin;
            this.pipeHeightMax = pipeHeightMax;
            RandomizePipe();
        }

        private void RandomizePipe()
        {
            Random random = new Random();
            BottomPipeHeight = random.Next(pipeHeightMin, pipeHeightMax);
        }

        public void Update()
        {
            Position = new Vector2(Position.X - speed, Position.Y);
            BottomPipe = new Rectangle((int)Position.X, (int)(BottomPipeHeight - (texture.Height * scale)), (int)(texture.Width * scale), (int)(texture.Height * scale));
            TopPipe = new Rectangle((int)Position.X, (int)(BottomPipeHeight + pipeGap), (int)(texture.Width * scale), (int)(texture.Height * scale));

            if (Position.X + (texture.Width * scale) < 0)
            {
                RandomizePipe();
                Position = startingPos;
            }
        }

        public override void Draw(SpriteBatch sp)
        {
            sp.Draw(texture, new Vector2(Position.X, BottomPipeHeight), sourceRectangle, Color, rotation, new Vector2(0, texture.Height), scale, SpriteEffects.None, 1);
            sp.Draw(texture, new Vector2(Position.X, BottomPipeHeight + pipeGap), sourceRectangle, Color, rotation, Vector2.Zero, scale, SpriteEffects.FlipVertically, 1);
        }
    }
}
