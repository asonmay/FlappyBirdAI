using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlappyBirdAI
{
    public class Sprite
    {
        protected Texture2D texture;
        public Vector2 Position { get; set; }
        protected Color color;
        protected SpriteEffects effects;
        protected float scale;
        protected float rotation;
        protected Rectangle sourceRectangle;
        protected Vector2 origin;
        protected Rectangle Hitbox
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, sourceRectangle.Width, sourceRectangle.Height);
            }
        }

        public Sprite (Texture2D texture, Vector2 position, Color color, SpriteEffects effects, float scale, float rotation, Rectangle sourceRectangle, Vector2 origin)
        {
            this.texture = texture;
            this.Position = position;
            this.color = color;
            this.scale = scale;
            this.sourceRectangle = sourceRectangle;
            this.rotation = rotation;
            this.origin = origin;
            this.effects = effects;
        }

        public virtual void Draw(SpriteBatch sp)
        {
            sp.Draw(texture, Position, sourceRectangle, color, rotation, origin, scale, effects, 1);
        }
    }
}
