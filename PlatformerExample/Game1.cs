using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System;
using PlatformLibrary;

namespace PlatformerExample
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        enum ViewState
        {
            IDLE,
            DEAD,
        }
        ViewState view = ViewState.IDLE;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteSheet sheet;
        Tileset tileset;
        Tilemap tilemap;
        Player player;
        List<Platform> platforms;
        Platform curPlatform;
        AxisList world;
        KeyboardState oldKS;
        KeyboardState newKS;
        float deathY = 600;
        Random random = new Random();
        SpriteFont font;
        int score = 0;
        Texture2D death;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            platforms = new List<Platform>();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            graphics.PreferredBackBufferWidth = 400;
            graphics.PreferredBackBufferHeight = 600;
            graphics.ApplyChanges();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
#if VISUAL_DEBUG
            VisualDebugging.LoadContent(Content);
#endif
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("font");
            death = Content.Load<Texture2D>("death");

            // TODO: use this.Content to load your game content here
            var t = Content.Load<Texture2D>("spritesheet");
            sheet = new SpriteSheet(t, 21, 21, 3, 2);

            // Create the player with the corresponding frames from the spritesheet
            var playerFrames = from index in Enumerable.Range(19, 30) select sheet[index];


            // Load the level
            tilemap = Content.Load<Tilemap>("level1");
            Vector2 x = tilemap.GetStartingPosition();
            player = new Player(playerFrames, new Vector2(350, 375));

            platforms.Add(new Platform(new BoundingRectangle(280, 500, 84, 21), sheet[1]));
            platforms.Add(new Platform(new BoundingRectangle(80, 300, 105, 21), sheet[1]));
            platforms.Add(new Platform(new BoundingRectangle(80, 100, 105, 21), sheet[1]));
            curPlatform = platforms[0];

            // Add the platforms to the axis list
            world = new AxisList();
            foreach (Platform platform in platforms)
            {
                world.AddGameObject(platform);
            }

            tileset = Content.Load<Tileset>("tiledspritesheet");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            player.Update(gameTime);
            if (player.Position.Y > deathY)
            {
                view = ViewState.DEAD;
            }

            // Check for platform collisions
            var platformQuery = world.QueryRange(player.Bounds.X, player.Bounds.X + player.Bounds.Width);
            player.CheckForPlatformCollision(platforms);
            foreach (Platform p in platforms)
            {
                p.Update(gameTime);
                if (player.Position.Y + 100 < p.Position.Y && player.verticalState == VerticalMovementState.OnGround)
                {
                    PlatformShift(p);
                    return;
                }
                Debug.WriteLine($"{p.Position}\t{p.Velocity}");
            }
            curPlatform = platforms[0];
            curPlatform.Velocity = Vector2.Zero;
            base.Update(gameTime);
        }

        public void PlatformShift(Platform p)
        {
            deathY = p.bounds.Y;
            platforms.Add(new Platform(new BoundingRectangle((float)random.Next(0, 400 - (int)p.bounds.Width), platforms[2].Position.Y - 200,
                                                            p.Bounds.Width, p.Bounds.Height),
                                                        sheet[1]));
            score++;
            p.bounds.X = -1000;
            platforms.Remove(p);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            switch (view)
            {
                case ViewState.IDLE:
                    spriteBatch.Begin();
                    //Draw the tilemap
                    tilemap.Draw(spriteBatch);
                    spriteBatch.DrawString(font, "Score: " + score.ToString(), new Vector2(10, 10), Color.Black);
                    spriteBatch.End();

                    var offset = new Vector2(curPlatform.Position.X, graphics.GraphicsDevice.Viewport.Height - 100) - curPlatform.Position;
                    var t = Matrix.CreateTranslation(offset.X, offset.Y, 0);
                    spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, t);

                    // Draw the platforms 
                    foreach (Platform platform in platforms)
                    {
                        platform.Draw(spriteBatch);
                    }
                    Debug.WriteLine($"{platforms.Count()} Platforms rendered");

                    // Draw the player
                    player.Draw(spriteBatch);

                    spriteBatch.End();
                    break;
                case ViewState.DEAD:
                    spriteBatch.Begin();
                    spriteBatch.Draw(death, new Rectangle(0, 0, 400, 400), Color.White);
                    spriteBatch.DrawString(font, "Score: " + score.ToString(), new Vector2(150, 500), Color.Black);
                    spriteBatch.End();
                    break;
            }



            base.Draw(gameTime);
        }
    }
}
