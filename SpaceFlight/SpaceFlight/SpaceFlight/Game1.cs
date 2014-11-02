using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SpaceFlight.Game;
using SpaceFlight.Utils;

namespace SpaceFlight
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public enum GameState
        {
            Game, Paused, Menu
        }

        //important
        public GameState gameState;

        Menu menu;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Camera camera;

        InputSystem input;

        //fps counter
        int frames = 0;
        float elapsedTime = 0;
        int fps = 0;

        //generic
        float reticuleRotation = 0;

        public GameObjContr unitController;

        public SpriteFont baseFont;

        public Vector2 mousePosition;
        public Vector2 prePausePos;

        public Matrix localTransform;
        public Matrix globalTransform;

        public List<ParticleController> particles;
        public bool gameRunning = false;

        public Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        public Dictionary<string, Item> items = new Dictionary<string, Item>();

        static Game1 del;

        static public Game1 GetDelegate()
        {
            return del;
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            del = this;

            List<DisplayMode> dispModes = GraphicsAdapter.DefaultAdapter.SupportedDisplayModes.ToList();
            foreach(DisplayMode disp in dispModes)
            {
                if (disp.Width == 640)
                {
                    graphics.PreferredBackBufferHeight = disp.Height;
                    graphics.PreferredBackBufferWidth = disp.Width;
                    graphics.PreferredBackBufferFormat = disp.Format;
                    break;
                }
            }
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            camera = new Camera(GraphicsDevice.Viewport);

            input = new InputSystem();

            mousePosition = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height) / 2;
            Mouse.SetPosition((int)mousePosition.X, (int)mousePosition.Y);

            gameState = GameState.Menu;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            LoadTextures();

            LoadItems();

            Utilities.Init(GraphicsDevice);

            menu = new Menu(Content, GraphicsDevice, graphics);
        }

        protected override void UnloadContent()
        {
            foreach (Texture2D t in textures.Values)
                t.Dispose();
            menu.Dispose();
            if(Particle.img != null)
                Particle.img.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            //fps
            elapsedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (elapsedTime >= 1000.0f)
            {
                fps = frames;
                frames = 0;
                elapsedTime = 0;
            }

            input.Update();
            
            mousePosition = input.MousePos();

            if (gameState == GameState.Game)
            {
                //actual updates come here
                unitController.Update(gameTime, input);
                foreach (ParticleController part in particles.ToList<ParticleController>())
                    part.Update(gameTime);

                reticuleRotation += (float)Math.PI / 180;
                if (reticuleRotation >= Math.PI * 2)
                    reticuleRotation = 0;

                //updating the view camera
                if (input.ScrollWheelValueChanged())
                    camera.Scale += 0.05f * (input.ScrollWheelValueChange()/120.0f);

                if(unitController.player != null)
                    camera.Update(unitController.player);

                localTransform = camera.viewTransform;
                globalTransform = camera.transform;

                if (input.IsKeyPressed(Keys.Tab))
                {
                    gameState = GameState.Paused;
                    prePausePos = mousePosition;
                }
                if (input.IsKeyPressed(Keys.Escape))
                    gameState = GameState.Menu;
            }
            else if (gameState == GameState.Paused)
            {
                unitController.player.inv.Update(input);
                if (input.IsKeyPressed(Keys.Tab))
                    gameState = GameState.Game;
            }
            else if (gameState == GameState.Menu)
                menu.Update(input);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //fps
            frames++;

            GraphicsDevice.Clear(Color.Black);
            if (gameState == GameState.Game || gameState == GameState.Paused)
            {
                //World Drawing (Global)
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, globalTransform);

                unitController.Draw(spriteBatch);
                foreach (ParticleController part in particles)
                    part.Draw(spriteBatch);

                Utilities.Draw(spriteBatch); //for global debug drawing

                spriteBatch.End();
                
                //UI Drawing (Local)
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, localTransform);

                spriteBatch.DrawString(baseFont, string.Format("FPS:{0}", fps), Vector2.Zero, Color.White);
                if (unitController.player != null)
                {
                    unitController.healthBar.Draw(spriteBatch);
                    unitController.invBar.Draw(spriteBatch);
                }

                Vector2 spriteOrigin = new Vector2(textures["Reticule"].Width / 2, textures["Reticule"].Height / 2);
                Vector2 drawPos = (gameState == GameState.Paused) ? prePausePos : mousePosition;
                spriteBatch.Draw(textures["Reticule"], drawPos, null, Color.White, reticuleRotation, spriteOrigin, 1f, SpriteEffects.None, 0);

                if (gameState == GameState.Paused)
                {
                    //fade the background a bit
                    spriteBatch.Draw(textures["PauseBg"], new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.White);

                    //Draw the inventory
                    unitController.player.inv.Draw(spriteBatch);

                    Rectangle dispRec = new Rectangle((int)mousePosition.X, (int)mousePosition.Y, 15, 20);
                    spriteBatch.Draw(textures["MouseText"], dispRec, Color.White);
                }

                Utilities.Draw(spriteBatch); //for local

                spriteBatch.End();
            }
            else if (gameState == GameState.Menu)
            {
                menu.Draw(spriteBatch);
            }

            base.Draw(gameTime);
        }

        public void ResizeForViewportChange(DisplayMode oldMode)
        {
            if (unitController != null)
            {
                unitController.healthBar.Resize(oldMode);
                unitController.invBar.Resize(oldMode);

                unitController.player.inv.Resize();
            }
        }

        void LoadTextures()
        {
            baseFont = Content.Load<SpriteFont>("BaseFont");

            textures.Add("Reticule", Content.Load<Texture2D>("Reticule"));
            textures.Add("Placeholder", Content.Load<Texture2D>("placeholder"));
            textures.Add("munitions", Content.Load<Texture2D>("munitions"));
            textures.Add("weap0", Content.Load<Texture2D>("weap0"));
            textures.Add("hull0", Content.Load<Texture2D>("hull0"));
            textures.Add("eng0", Content.Load<Texture2D>("eng0"));
            textures.Add("Asteroid", Content.Load<Texture2D>("Asteroid"));
            textures.Add("EmptySlot", Content.Load<Texture2D>("empty"));
            textures.Add("heal0", Content.Load<Texture2D>("heal0"));
            textures.Add("rapidFire0", Content.Load<Texture2D>("rapidFire0"));
            textures.Add("MouseText", Content.Load<Texture2D>("mouse"));

            textures.Add("PauseBg", new Texture2D(GraphicsDevice, 1, 1));
            textures["PauseBg"].SetData<Color>(new [] { new Color(0, 0, 0, 0.75f) });
        }

        void LoadItems()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("Content\\items.xml");
            XmlNodeList list = doc.GetElementsByTagName("Item");
            int c = 0;
            foreach(XmlNode node in list)
            {
                c++;
                Item i = new Item();

                foreach (XmlNode child in node)
                    i.values.Add(child.Name, child.InnerText);

                if(textures.ContainsKey(i.ID))
                    i.Image = textures[i.ID];
                else
                    i.Image = textures["hull0"];

                items.Add(i.ID, i);
                System.Diagnostics.Debug.WriteLine("Item " + c + ": " + i.ToString());
            }
        }

        //the world loading. This method is called from the Menu class when the player presses Play button.
        public void InitWorld()
        {
            particles = new List<ParticleController>();

            unitController = new GameObjContr();
            Hull h = new Hull(items["hull0"]);
            unitController.player = new Player(new Rectangle(1000, 1000, h.text.Width, h.text.Height));
            unitController.player.Hull = h;
            unitController.player.weapons[0] = new Weapon(items["weap0"]);
            unitController.player.weapons[1] = new Weapon(items["weap0"]);
            unitController.player.eng = new Engine(items["eng0"]);
            unitController.player.inv = new Inventory(unitController.player.Hull, unitController.player);

            Random rnd = new Random();
            for (int i = 0; i < 1; i++)
            {
                h = new Hull(items["hull"]);
                Ship en = new Ship(new Rectangle(rnd.Next(500, 1500), rnd.Next(500, 1500), h.text.Width, h.text.Height));
                en.Hull = h;
                en.weapons[0] = new Weapon(items["weap0"]);
                en.eng = new Engine(items["eng0"]);
                en.inv = new Inventory(en.Hull, en);
                if(rnd.Next(100) >= 50)
                    en.inv.Add(items.ElementAt(rnd.Next(2)).Value);
                unitController.enemies.Add(en);
            }
        }

        public void GameOver()
        {
            unitController.player = null;
            gameRunning = false;
            menu.ToMain();
            gameState = GameState.Menu;
        }
    }
}