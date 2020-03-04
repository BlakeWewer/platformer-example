using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using PlatformLibrary;

namespace PlatformerExample
{
    /// <summary>
    /// An enumeration of possible player animation states
    /// </summary>
    enum PlayerAnimState
    {
        Idle,
        JumpingLeft,
        JumpingRight,
        WalkingLeft,
        WalkingRight,
        FallingLeft,
        FallingRight
    }

    /// <summary>
    /// An enumeration of possible player veritcal movement states
    /// </summary>
    public enum VerticalMovementState
    {
        OnGround,
        Jumping,
        Falling
    }

    /// <summary>
    /// A class representing the player
    /// </summary>
    public class Player
    {
        // The speed of the walking animation
        const int FRAME_RATE = 300;

        // The duration of a player's jump, in milliseconds
        const int JUMP_TIME = 500;

        // The player sprite frames
        Sprite[] frames;

        // The currently rendered frame
        int currentFrame = 0;

        // The player's animation state
        PlayerAnimState animationState = PlayerAnimState.Idle;

        // The player's speed
        int speed = 3;

        KeyboardState newKS = Keyboard.GetState();
        KeyboardState oldKS;
        

        // The player's vertical movement state
        public VerticalMovementState verticalState = VerticalMovementState.OnGround;

        // A timer for jumping
        TimeSpan jumpTimer;

        // A timer for animations
        TimeSpan animationTimer;

        // The currently applied SpriteEffects
        SpriteEffects spriteEffects = SpriteEffects.None;

        // The color of the sprite
        Color color = Color.White;

        // The origin of the sprite (centered on its feet)
        Vector2 origin = new Vector2(10, 21);

        /// <summary>
        /// Gets and sets the position of the player on-screen
        /// </summary>
        public Vector2 Position;

        public BoundingRectangle Bounds => new BoundingRectangle((Position - 1.8f * origin).X + 9f, (Position - 1.8f * origin).Y + 30f, 20, 10);

        /// <summary>
        /// Constructs a new player
        /// </summary>
        /// <param name="frames">The sprite frames associated with the player</param>
        public Player(IEnumerable<Sprite> frames, Vector2 startingPosition)
        {
            this.frames = frames.ToArray();
            animationState = PlayerAnimState.WalkingLeft;
            Position = startingPosition;
        }

        /// <summary>
        /// Updates the player, applying movement and physics
        /// </summary>
        /// <param name="gameTime">The GameTime object</param>
        public void Update(GameTime gameTime)
        {
            oldKS = newKS;
            newKS = Keyboard.GetState();

            // Vertical movement
            switch(verticalState)
            {
                case VerticalMovementState.OnGround:
                    if(newKS.IsKeyDown(Keys.Space) && !oldKS.IsKeyDown(Keys.Space))
                    {
                        verticalState = VerticalMovementState.Jumping;
                        jumpTimer = new TimeSpan(0);
                    }
                    break;
                case VerticalMovementState.Jumping:
                    jumpTimer += gameTime.ElapsedGameTime;
                    // Simple jumping with platformer physics
                    Position.Y -= (1000 / (float)jumpTimer.TotalMilliseconds);
                    if (jumpTimer.TotalMilliseconds >= JUMP_TIME) verticalState = VerticalMovementState.Falling;
                    break;
                case VerticalMovementState.Falling:
                    Position.Y += speed*2;
                    // TODO: This needs to be replaced with collision logic
                    if (Position.Y > 650)
                    {
                        Position.Y = 650;
                    }
                    break;
            }
            

            // Horizontal movement
            if (newKS.IsKeyDown(Keys.Left))
            {
                if (verticalState == VerticalMovementState.Jumping || verticalState == VerticalMovementState.Falling) 
                    animationState = PlayerAnimState.JumpingLeft;
                else animationState = PlayerAnimState.WalkingLeft;
                Position.X -= speed;
            }
            else if(newKS.IsKeyDown(Keys.Right))
            {
                if (verticalState == VerticalMovementState.Jumping || verticalState == VerticalMovementState.Falling)
                    animationState = PlayerAnimState.JumpingRight;
                else animationState = PlayerAnimState.WalkingRight;
                Position.X += speed;
            }
            else
            {
                animationState = PlayerAnimState.Idle;
            }
            if (Position.X < 20)
            {
                float delta = 20 - Position.X;
                Position.X += delta;
            }
            if (Position.X > 380)
            {
                float delta = 380 - Position.X;
                Position.X += delta;
            }

            // Apply animations
            switch (animationState)
            {
                case PlayerAnimState.Idle:
                    currentFrame = 0;
                    animationTimer = new TimeSpan(0);
                    break;

                case PlayerAnimState.JumpingLeft:
                    spriteEffects = SpriteEffects.FlipHorizontally;
                    currentFrame = 7;
                    break;

                case PlayerAnimState.JumpingRight:
                     spriteEffects = SpriteEffects.None;
                    currentFrame = 7;
                    break;

                case PlayerAnimState.WalkingLeft:
                    animationTimer += gameTime.ElapsedGameTime;
                    spriteEffects = SpriteEffects.FlipHorizontally;
                    // Walking frames are 9 & 10
                    if(animationTimer.TotalMilliseconds > FRAME_RATE * 2)
                    {
                        animationTimer = new TimeSpan(0);
                    }
                    currentFrame = (int)Math.Floor(animationTimer.TotalMilliseconds / FRAME_RATE) + 9;
                    break;

                case PlayerAnimState.WalkingRight:
                    animationTimer += gameTime.ElapsedGameTime;
                    spriteEffects = SpriteEffects.None;
                    // Walking frames are 9 & 10
                    if (animationTimer.TotalMilliseconds > FRAME_RATE * 2)
                    {
                        animationTimer = new TimeSpan(0);
                    }
                    currentFrame = (int)Math.Floor(animationTimer.TotalMilliseconds / FRAME_RATE) + 9;
                    break;

            }
        }

        public void CheckForPlatformCollision(IEnumerable<IBoundable> platforms)
        {
            Debug.WriteLine($"Checking collisions against {platforms.Count()} platforms");
            if (verticalState != VerticalMovementState.Jumping)
            {
                verticalState = VerticalMovementState.Falling;
                foreach (IBoundable platform in platforms)
                {
                    if (Bounds.CollidesWith(platform.Bounds))
                    {
                        Position.Y = platform.Bounds.Y - 1;
                        verticalState = VerticalMovementState.OnGround;
                    }
                }
            }
        }

        /// <summary>
        /// Render the player sprite.  Should be invoked between 
        /// SpriteBatch.Begin() and SpriteBatch.End()
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch to use</param>
        public void Draw(SpriteBatch spriteBatch)
        {
#if VISUAL_DEBUG 
            VisualDebugging.DrawRectangle(spriteBatch, Bounds, Color.Red);
#endif
            frames[currentFrame].Draw(spriteBatch, Position, color, 0, origin, 2, spriteEffects, 1);
        }

    }
}
