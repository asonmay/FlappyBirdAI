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
                birds[i] = new Bird(new NeuralNetwork(activationFunc, errorFunc, 2, 4, 1), new Vector2(100, random.Next(50,600)), 0.3f, birdTexture, 0, 0.7f, 10, TimeSpan.FromMilliseconds(200));
                birds[i].Network.Randomize(random, -5, 5);
            }
            
            speed = 1;
            mutationRate = 0.03;

            trainer = new GeneticTrainer<Bird>(-3, 3,mutationRate, birds);
        }

        private void ResetPipes()
        {
            Vector2 startingPos = new Vector2(GraphicsDevice.Viewport.Width, 0);
            pipes =
            [
                new Pipe(startingPos, startingPos, 0.5f, pipeTexture, 5, 75, 300, 240),
                new Pipe(startingPos, new Vector2(startingPos.X * 1.5f + pipeTexture.Width / 4, 0), 0.5f, pipeTexture, 5, 75, 300, 240)
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

        private double GetNoramlizeValue(double min, double max, double nMin, double nMax, double current)
        {
            return (current - min) / (max - min) * (nMax - nMin) + nMin;
        }

        private bool UpdateBirds(GameTime gametime)
        {
            for(int i = 0; i < trainer.Networks.Length; i++)
            {
                Pipe pipe = GetClosestPipe(trainer.Networks[i]);
                Bird current = trainer.Networks[i];
                double input1 = GetNoramlizeValue(0, pipe.BottomPipeHeight, -1, 1, Math.Abs(pipe.YStartPos + pipe.BottomPipeHeight - current.Position.Y + current.Hitbox.Height));
                double input2 = GetNoramlizeValue(0, GraphicsDevice.Viewport.Width / 2, -1, 1, Math.Abs(current.Position.X + current.Hitbox.Width - pipe.Position.X));
                trainer.Networks[i].Update([input1, input2], gametime, pipe, GraphicsDevice.Viewport);
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

            if(Keyboard.GetState().IsKeyDown(Keys.D1))
            {
                speed = 1;
            }
            else if(Keyboard.GetState().IsKeyDown(Keys.D5))
            {
                speed = 5;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D9))
            {
                speed = 10;
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
                    if(i < 100)
                    {
                        trainer.Networks[i].Draw(spriteBatch);
                    }
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
