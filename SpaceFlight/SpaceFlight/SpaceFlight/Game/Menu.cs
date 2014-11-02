using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using SpaceFlight.Utils;
using Microsoft.Xna.Framework.Content;

namespace SpaceFlight.Game
{
    class Menu  //the idea of this class is to encapsulate the drawing and functions of the menu UI
    {
        List<Label> lbls = new List<Label>();
        List<Button> btns = new List<Button>();
        GraphicsDevice graphics;
        GraphicsDeviceManager graphMan;
        Dictionary<String, Texture2D> textures = new Dictionary<string, Texture2D>();
        List<DisplayMode> dispModes = GraphicsAdapter.DefaultAdapter.SupportedDisplayModes.ToList();

        int dmInd, oldDmInd;
        bool resChanged = false;

        public Menu(ContentManager Content, GraphicsDevice graphicsDevice, GraphicsDeviceManager graphicsManager)
        {
            graphics = graphicsDevice;
            graphMan = graphicsManager;

            textures.Add("MouseText", Content.Load<Texture2D>("mouse"));
            textures.Add("PlayText", Content.Load<Texture2D>("playBtn"));
            textures.Add("HighlightText", Content.Load<Texture2D>("HighlightBtn"));
            textures.Add("ActiveText", Content.Load<Texture2D>("ActiveBtn"));

            ToMain(); //creates all the mainmenu buttons
        }

        public void Update(InputSystem input)
        {
            foreach (Button btn in btns.ToArray())
            {
                btn.Update(input);
            }
        }

        public void Draw(SpriteBatch batch)
        {
            batch.Begin();

            foreach(Button btn in btns)
            {
                btn.Draw(batch);
            }

            Vector2 mPos = Game1.GetDelegate().mousePosition;
            Vector2 mSize = new Vector2(15, 20);
            batch.Draw(textures["MouseText"], new Rectangle((int)(mPos.X), (int)(mPos.Y), (int)mSize.X, (int)mSize.Y), Color.White); 

            batch.End();
        }

        void SettingPressed()
        {
            btns.Clear();

            Button resolutionBtn = new Button(new Rectangle(100, 100, 300, 50), textures["PlayText"]);
            resolutionBtn.HighlightTexture = textures["HighlightText"];
            resolutionBtn.Method = new Action(ChangeResolution);
            for (int i=0; i<dispModes.Count; i++)
            {
                if (graphMan.PreferredBackBufferWidth == dispModes[i].Width && graphMan.PreferredBackBufferHeight == dispModes[i].Height && 
                    graphMan.PreferredBackBufferFormat == dispModes[i].Format)
                {
                    dmInd = i;
                    oldDmInd = dmInd;
                    break;
                }
            }
            resolutionBtn.Title = String.Format("{0}:{1}", dispModes[dmInd].Width, dispModes[dmInd].Height);
            resolutionBtn.Font = Game1.GetDelegate().baseFont;
            btns.Add(resolutionBtn);

            Button fullScreenBtn = new Button(new Rectangle(100, 170, 200, 50), textures["PlayText"]);
            fullScreenBtn.HighlightTexture = textures["HighlightText"];
            fullScreenBtn.Method = new Action(FullScreenBtn);
            fullScreenBtn.Title = (graphMan.IsFullScreen) ? "Go Windowed" : "Go Fullscreen";
            fullScreenBtn.Font = Game1.GetDelegate().baseFont;
            btns.Add(fullScreenBtn);

            Button apply = new Button(new Rectangle(100, graphics.Viewport.Height - 100, 100, 50), textures["PlayText"]);
            apply.HighlightTexture = textures["HighlightText"];
            apply.Title = "Apply";
            apply.Font = Game1.GetDelegate().baseFont;
            apply.Method = new Action(ApplyChanges);
            btns.Add(apply);
        }

        void FullScreenBtn()
        {
            graphMan.IsFullScreen = !graphMan.IsFullScreen;
            graphMan.ApplyChanges();

            Game1.GetDelegate().camera.viewport = Game1.GetDelegate().GraphicsDevice.Viewport;

            btns[1].Title = (graphMan.IsFullScreen) ? "Go Windowed" : "Go Fullscreen";
        }

        void ChangeResolution()
        {
            dmInd++;
            if (dmInd > dispModes.Count / 2 - 1)
                dmInd = 0;
            btns[0].Title = String.Format("{0}:{1}", dispModes[dmInd].Width, dispModes[dmInd].Height);

            resChanged = !(graphMan.PreferredBackBufferWidth == dispModes[dmInd].Width && graphMan.PreferredBackBufferHeight == dispModes[dmInd].Height &&
                    graphMan.PreferredBackBufferFormat == dispModes[dmInd].Format); //check if resolution changed to a different than the current one       
        }

        void ApplyChanges()
        {
            //apply resolution
            if (resChanged)
            {
                graphMan.PreferredBackBufferFormat = dispModes[dmInd].Format;
                graphMan.PreferredBackBufferHeight = dispModes[dmInd].Height;
                graphMan.PreferredBackBufferWidth = dispModes[dmInd].Width;
                graphMan.ApplyChanges();

                DisplayMode oldMode = dispModes[oldDmInd];
                foreach (Button btn in btns)
                {
                    float x = Game1.GetDelegate().GraphicsDevice.Viewport.Width * btn.Frame.X * 1.0f / oldMode.Width;
                    float y = Game1.GetDelegate().GraphicsDevice.Viewport.Height * btn.Frame.Y * 1.0f / oldMode.Height;
                    float width = Game1.GetDelegate().GraphicsDevice.Viewport.Width * btn.Frame.Width * 1.0f / oldMode.Width;
                    float height = Game1.GetDelegate().GraphicsDevice.Viewport.Height * btn.Frame.Height * 1.0f / oldMode.Height;
                    btn.Frame = new Rectangle((int)x, (int)y, (int)width, (int)height);
                }

                Game1.GetDelegate().ResizeForViewportChange(oldMode);

                Game1.GetDelegate().camera.viewport = Game1.GetDelegate().GraphicsDevice.Viewport;

                resChanged = false;
            }
            ToMain();
        }

        public void ToMain()
        {
            btns.Clear();

            int width = 100, height = 50, offset = height + 20;
            Vector2 center = new Vector2(graphics.Viewport.Width, graphics.Viewport.Height) / 2;

            Button playBtn = new Button(new Rectangle((int)center.X - width / 2, (int)center.Y - height / 2, width, height), textures["PlayText"]);
            playBtn.HighlightTexture = textures["HighlightText"];
            playBtn.ActiveTexture = textures["ActiveText"];
            playBtn.Method = new Action(PlayPressed);
            playBtn.Title = (Game1.GetDelegate().gameRunning)?"Continue":"Play";
            playBtn.Font = Game1.GetDelegate().baseFont;
            playBtn.Hotkey = Keys.P;
            btns.Add(playBtn);

            Button settingBtn = new Button(new Rectangle((int)center.X - width / 2, (int)center.Y - height / 2 + offset, width, height), textures["PlayText"]);
            settingBtn.HighlightTexture = textures["HighlightText"];
            settingBtn.ActiveTexture = textures["ActiveText"];
            settingBtn.Method = new Action(SettingPressed);
            settingBtn.Title = "Settings";
            settingBtn.Font = Game1.GetDelegate().baseFont;
            settingBtn.Hotkey = Keys.S;
            btns.Add(settingBtn);

            Button exitBtn = new Button(new Rectangle((int)center.X - width / 2, (int)center.Y - height / 2 + 2 * offset, width, height), textures["PlayText"]);
            exitBtn.HighlightTexture = textures["HighlightText"];
            exitBtn.ActiveTexture = textures["ActiveText"];
            exitBtn.Method = new Action(ExitPressed);
            exitBtn.Title = "Exit game";
            exitBtn.Font = Game1.GetDelegate().baseFont;
            exitBtn.Hotkey = Keys.Escape;
            btns.Add(exitBtn);
        }

        void PlayPressed()
        {
            Game1 del = Game1.GetDelegate();

            if (!del.gameRunning)
                del.InitWorld();

            del.gameRunning = true;
            del.gameState = Game1.GameState.Game;
            btns[0].Title = "Continue";

            del = null;
        }

        void ExitPressed()
        {
            Game1.GetDelegate().Exit();
        }

        public void Dispose()
        {
            btns.Clear();
            
            foreach (Texture2D text in textures.Values)
                text.Dispose();
            
            textures.Clear();
        }
    }
}
