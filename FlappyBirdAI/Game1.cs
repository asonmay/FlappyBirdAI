using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NeuralNetworkLibrary;
using System;

namespace FlappyBirdAI
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Bird[] birds;
        private Pipe[] pipes;
        private Texture2D birdTexture;
        private Texture2D pipeTexture;
        private Texture2D background;
        private int generation;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
            graphics.PreferredBackBufferHeight = 800;
            graphics.PreferredBackBufferWidth = 800;
            graphics.ApplyChanges();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            ActivationFunction activationFunc = new ActivationFunction(ActivationAndErrorFunctions.TanH, ActivationAndErrorFunctions.TanHDerivative);
            ErrorFunction errorFunc = new ErrorFunction(ActivationAndErrorFunctions.MeanSquareError, ActivationAndErrorFunctions.MeanSquaredErrorDerivative);
            Random random = new Random();   

            pipeTexture = Content.Load<Texture2D>("pipe");
            Vector2 startingPos = new Vector2(GraphicsDevice.Viewport.Width, 0);
            pipes =
            [
                new Pipe(startingPos, startingPos, 0.5f, pipeTexture, 4, 150, 400, 225, 0),
                new Pipe(startingPos, new Vector2(startingPos.X * 1.5f + pipeTexture.Width / 4, 0), 0.5f, pipeTexture, 4, 150, 400, 225, 0)
            ];

            generation = 1;
            birdTexture = Content.Load<Texture2D>("bird");
            birds = new Bird[100];
            for(int i = 0; i < birds.Length; i++)
            {
                birds[i] = new Bird(new NeuralNetwork(activationFunc, errorFunc, 2, 4, 1), new Vector2(100, GraphicsDevice.Viewport.Height), 0.3f, birdTexture, 0, 0.4f, 8, TimeSpan.FromMilliseconds(250));
                birds[i].Network.Randomize(random, -10, 10);
            }
        }

        private bool TestNetworks(GameTime gametime)
        {
            UpdatePipes();
            return UpdateBirds(gametime);
        }

        private void UpdatePipes()
        {
            for(int i = 0; i < pipes.Length; i++)
            {
                pipes[i].Update();
            }
        }

        private bool UpdateBirds(GameTime gametime)
        {
            for(int i = 0; i < birds.Length; i++)
            {
                Pipe pipe = GetClosestPipe(birds[i]);
                birds[i].Update([Math.Abs(birds[i].Position.X - pipe.Position.X), pipe.YStartPos + pipe.BottomPipeHeight], gametime, pipe);
            }
            return AreAllDead();
        }

        private bool AreAllDead()
        {
            for(int i = 0; i < birds.Length; i++)
            {
                if (!birds[i].isDead)
                {
                    return false;
                }
            }
            return true;
        }

        private Pipe GetClosestPipe(Bird bird)
        {
            Pipe closest = pipes[0];
            for(int i = 1; i < pipes.Length; i++)
            {
                if (Math.Abs(pipes[i].Position.X - bird.Position.X) < Math.Abs(closest.Position.X - bird.Position.X))
                {
                    closest = pipes[i];
                }
            }
            return closest;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            TestNetworks(gameTime);

            base.Update(gameTime);
        }

        private void DrawPipes()
        {
            for(int i = 0; i < pipes.Length; i++)
            {
                pipes[i].Draw(spriteBatch);
            }
        }

        private void DrawBirds()
        {
            for (int i = 0; i < birds.Length; i++)
            {
                birds[i].Draw(spriteBatch);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            DrawPipes();
            DrawBirds();
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
