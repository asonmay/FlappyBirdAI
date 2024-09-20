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
    public class Bird : Sprite
    {
        public NeuralNetwork Network { get; set; }
        public double Fitness { get; private set; }
        public bool isDead { get; private set; }

        private float yVelocity;
        private float gravity;
        private float jumpPower;

        public Bird(NeuralNetwork network, Vector2 position, float scale, Texture2D texture, float yVelocity, float gravity, float jumpPower)
            : base(texture, position, Color.White, SpriteEffects.None, scale, 0, new Rectangle(0, 0, texture.Width, texture.Height), Vector2.Zero)
        {
            Network = network;
            this.yVelocity = yVelocity;
            this.gravity = gravity;
            isDead = false;
            this.jumpPower = jumpPower;
        }

        private float Filter(double value)
        {
            if(value < 0.5)
            {
                return 0f;
            }
            else
            {
                return 1f;
            }
        }

        public void Update(double[] inputs, GameTime gameTime)
        {
            float raw = (float)Network.Compute(inputs)[0];
            float filtered = Filter(raw);
            if(filtered == 1)
            {
                yVelocity += jumpPower;
            }
            yVelocity -= gravity;
            position = new Vector2(position.X, position.Y + yVelocity);
            Fitness += gameTime.ElapsedGameTime.TotalMilliseconds;
        }
    }
}
