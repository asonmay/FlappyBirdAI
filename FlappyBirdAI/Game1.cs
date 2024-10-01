using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NeuralNetworkLibrary;
using System;
using System.Linq;
using System.Net;
using System.Security.Principal;

namespace FlappyBirdAI
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Pipe[] pipes;
        private Texture2D birdTexture;
        private Texture2D pipeTexture;
        private Texture2D background;
        private int generation;
        private double mutationRate;
        private int speed;
        private GeneticTrainer<Bird> trainer;

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

            ResetPipes();

            generation = 1;
            birdTexture = Content.Load<Texture2D>("bird");

            Bird[] birds = new Bird[100];
            for(int i = 0; i < birds.Length; i++)
            {
                birds[i] = new Bird(new NeuralNetwork(activationFunc, errorFunc, 2, 4, 1), new Vector2(100, random.Next(50,600)), 0.3f, birdTexture, 0, 0.4f, 8, TimeSpan.FromMilliseconds(250));
                birds[i].Network.Randomize(random, 0, 2);
            }
            speed = 10;
            mutationRate = 0.5;

            trainer = new GeneticTrainer<Bird>(-10, 10,mutationRate, birds);
        }

        private void ResetPipes()
        {
            Vector2 startingPos = new Vector2(GraphicsDevice.Viewport.Width, 0);
            pipes =
            [
                new Pipe(startingPos, startingPos, 0.5f, pipeTexture, 4, 150, 400, 225, 0),
                new Pipe(startingPos, new Vector2(startingPos.X * 1.5f + pipeTexture.Width / 4, 0), 0.5f, pipeTexture, 4, 150, 400, 225, 0)
            ];
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
            for(int i = 0; i < trainer.Networks.Length; i++)
            {
                Pipe pipe = GetClosestPipe(trainer.Networks[i]);
                trainer.Networks[i].Update([Math.Abs(trainer.Networks[i].Position.X - pipe.Position.X), Math.Abs(pipe.YStartPos + pipe.BottomPipeHeight - trainer.Networks[i].Position.Y)], gametime, pipe, GraphicsDevice.Viewport);
            }
            return AreAllDead();
        }

        private bool AreAllDead()
        {
            for(int i = 0; i < trainer.Networks.Length; i++)
            {
                if (!trainer.Networks[i].isDead)
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

        private void SwapWeights(Neuron one, Neuron two)
        {
            for(int i = 0; i < one.Dendrites.Length; i++)
            {
                one.Dendrites[i].Weight = two.Dendrites[i].Weight;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            for(int j = 0; j < speed; j++)
            {
                if (TestNetworks(gameTime))
                {
                    trainer.Train(new Random());
                    ResetPipes();
                    ResetBirds();
                }
            }

            base.Update(gameTime);
        }

        private void ResetBirds()
        {
            for (int i = 0; i < trainer.Networks.Length; i++)
            {
                trainer.Networks[i].Reset();
            }
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
            for (int i = 0; i < trainer.Networks.Length; i++)
            {
                if (!trainer.Networks[i].isDead)
                {
                    trainer.Networks[i].Draw(spriteBatch);
                }
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
