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
        private Bird[] birds;
        private Pipe[] pipes;
        private Texture2D birdTexture;
        private Texture2D pipeTexture;
        private Texture2D background;
        private int generation;
        private double mutationRate;

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
            birds = new Bird[100];
            for(int i = 0; i < birds.Length; i++)
            {
                birds[i] = new Bird(new NeuralNetwork(activationFunc, errorFunc, 2, 4, 1), new Vector2(100, random.Next(50,600)), 0.3f, birdTexture, 0, 0.4f, 8, TimeSpan.FromMilliseconds(250));
                birds[i].Network.Randomize(random, 0, 2);
            }

            mutationRate = 0.1;
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
            for(int i = 0; i < birds.Length; i++)
            {
                Pipe pipe = GetClosestPipe(birds[i]);
                birds[i].Update([Math.Abs(birds[i].Position.X - pipe.Position.X), pipe.YStartPos + pipe.BottomPipeHeight], gametime, pipe, GraphicsDevice.Viewport);
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

        public void Crossover(NeuralNetwork winner, NeuralNetwork loser, Random random)
        {
            for (int i = 0; i < winner.Layers.Length; i++)
            {
                Layer winLayer = winner.Layers[i];
                Layer childLayer = loser.Layers[i];

                int cutPoint = random.Next(winLayer.Neurons.Length); 
                bool flip = random.Next(2) == 0; 

                for (int j = (flip ? 0 : cutPoint); j < (flip ? cutPoint : winLayer.Neurons.Length); j++)
                {
                    Neuron winNeuron = winLayer.Neurons[j];
                    Neuron childNeuron = childLayer.Neurons[j];

                    SwapWeights(childNeuron, winNeuron);
                    childNeuron.Bias = winNeuron.Bias;
                }
            }
        }

        private void SwapWeights(Neuron one, Neuron two)
        {
            for(int i = 0; i < one.Dendrites.Length; i++)
            {
                one.Dendrites[i].Weight = two.Dendrites[i].Weight;
            }
        }

        public void Mutate(NeuralNetwork net, Random random, double mutationRate)
        {
            foreach (Layer layer in net.Layers)
            {
                foreach (Neuron neuron in layer.Neurons)
                {
                    for (int i = 0; i < neuron.Dendrites.Length; i++)
                    {
                        if (random.Next(2) == 0)
                        {
                            neuron.Dendrites[i].Weight += mutationRate;
                        }
                        else
                        {
                            neuron.Dendrites[i].Weight -= mutationRate; 
                        }
                    }

                    if (random.Next(2) == 0)
                    {
                        neuron.Bias += mutationRate;
                    }
                    else
                    {
                        neuron.Bias -= mutationRate;
                    }
                }
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if(TestNetworks(gameTime))
            {
                Random random = new Random();
                Array.Sort(birds, (a, b) => b.Fitness.CompareTo(a.Fitness));

                int start = (int)(birds.Length * 0.1);
                int end = (int)(birds.Length * 0.9);

                for (int i = start; i < end; i++)
                {
                    Crossover(birds[random.Next(start)].Network, birds[i].Network, random);
                    Mutate(birds[i].Network, random, mutationRate);
                }

                for (int i = end; i < birds.Length; i++)
                {
                    birds[i].Network.Randomize(random, 0, 2);
                }
                ResetPipes();
                for(int i = 0; i < birds.Length; i++)
                {
                    birds[i].Position = new Vector2(100, random.Next(50, 500));
                    birds[i].ResetBird();
                }
            }
            Window.Title = pipes[0].Position.X.ToString();

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
                if (!birds[i].isDead)
                {
                    birds[i].Draw(spriteBatch);
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
