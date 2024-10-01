using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NeuralNetworkLibrary;

namespace FlappyBirdAI
{
    public class Bird : Sprite, INeuralNetwork
    {
        public NeuralNetwork Network { get; set; }
        public double Fitness { get; set; }
        public bool isDead { get; set; }
        private TimeSpan jumpTimer;
        private TimeSpan jumpInBetween;

        private float yVelocity;
        private readonly float gravity;
        private readonly float jumpPower;

        public Bird(NeuralNetwork network, Vector2 position, float scale, Texture2D texture, float yVelocity, float gravity, float jumpPower, TimeSpan jumpInBetween)
            : base(texture, position, Color.White, SpriteEffects.None, scale, 0, new Rectangle(0, 0, texture.Width, texture.Height), Vector2.Zero)
        {
            Network = network;
            this.yVelocity = yVelocity;
            this.gravity = gravity;
            isDead = false;
            this.jumpPower = jumpPower;
            this.jumpInBetween = jumpInBetween;
            jumpTimer = TimeSpan.Zero;
        }

        public void Reset()
        {
            Random random = new Random();
            isDead = false;
            Position = new Vector2(100, random.Next(50, 500));
            yVelocity = 0;
        }

        public void Update(double[] inputs, GameTime gameTime, Pipe pipe, Viewport viewport)
        {
            if (!isDead)
            {
                float raw = (float)Network.Compute(inputs)[0];
                jumpTimer += gameTime.ElapsedGameTime;
                if (raw > 0 && jumpTimer.Milliseconds >= jumpInBetween.Milliseconds)
                {
                    yVelocity += jumpPower;
                    jumpTimer = TimeSpan.Zero;
                }

                yVelocity -= gravity;
                Position = new Vector2(Position.X, Position.Y - yVelocity);
                Fitness += gameTime.ElapsedGameTime.TotalMilliseconds;
                

                if (Position.Y <= 0 || Position.Y > viewport.Height)
                {
                    isDead = true;
                    Fitness -= 1000;
                }
                else if(pipe.BottomPipe.Intersects(Hitbox) || pipe.TopPipe.Intersects(Hitbox))
                {
                    isDead = true;
                }
            }
        }
    }
}
