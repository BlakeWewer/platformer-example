using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlatformLibrary;

namespace PlatformerExample
{
    /// <summary>
    /// A class representing a platform
    /// </summary>
    public class Platform : IBoundable
    {
        /// <summary>
        /// The platform's bounds
        /// </summary>
        public BoundingRectangle bounds;

        /// <summary>
        /// The platform's sprite
        /// </summary>
        Sprite sprite;

        /// <summary>
        /// The number of times the sprite is repeated (tiled) in the platform
        /// </summary>
        int tileCount;

        /// <summary>
        /// The bounding rectangle of the platform
        /// </summary>
        public BoundingRectangle Bounds => bounds;

        public Vector2 Velocity;
        public Vector2 Position;

        Random random = new Random();

        /// <summary>
        /// Constructs a new platform
        /// </summary>
        /// <param name="bounds">The platform's bounds</param>
        /// <param name="sprite">The platform's sprite</param>
        public Platform(BoundingRectangle bounds, Sprite sprite)
        {
            this.bounds = bounds;
            Position = new Vector2(bounds.X, bounds.Y);
            this.sprite = sprite;
            tileCount = (int)bounds.Width / sprite.Width;
            Velocity.X = (float)random.NextDouble();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            if(Position.X < 0 || Position.X > 400 - bounds.Width)
            {
                Velocity.X *= -1;
            }

            Position += (float)gameTime.ElapsedGameTime.TotalMilliseconds * Velocity;
            bounds.X = Position.X;
            bounds.Y = Position.Y;            
        }

        /// <summary>
        /// Draws the platform
        /// </summary>
        /// <param name="spriteBatch">The spriteBatch to render to</param>
        public void Draw(SpriteBatch spriteBatch)
        {
#if VISUAL_DEBUG
            VisualDebugging.DrawRectangle(spriteBatch, bounds, Color.Green);
#endif
            for (int i = 0; i < tileCount; i++)
            {
                sprite.Draw(spriteBatch, new Vector2(bounds.X + i * sprite.Width, bounds.Y), Color.Purple);
            }

        }
    }
}
