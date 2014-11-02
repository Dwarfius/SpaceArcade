using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpaceFlight.Utils
{
    public enum UIButtonState
    {
        None, Highlighted, Pressed
    }

    class Button
    {
        Texture2D dispText;
        public Texture2D Texture;
        public Texture2D HighlightTexture = null;
        public Texture2D ActiveTexture = null;
        public String Title = "";
        public SpriteFont Font = null;
        public Keys Hotkey = 0;
        public Action Method = null;
        public Rectangle Frame;
        UIButtonState state;
        public UIButtonState State
        {
            get { return state; }
        }

        public Button(Rectangle Rect, Texture2D text)
        {
            Frame = Rect;
            Texture = text;
            dispText = Texture;
        }

        public void Update(InputSystem input)
        {
            Game1 game = Game1.GetDelegate();
            bool isInRect = game.mousePosition.X >= Frame.Left && game.mousePosition.X <= Frame.Right &&
                          game.mousePosition.Y >= Frame.Top && game.mousePosition.Y <= Frame.Bottom;

            if (isInRect)
            {
                if (input.IsMouseBtnDown(MouseButton.Left))
                    state = UIButtonState.Pressed;
                else
                    state = UIButtonState.Highlighted;

                if (input.IsMouseBtnDown(MouseButton.Left) && ActiveTexture != null)
                    dispText = ActiveTexture;
                else if (HighlightTexture != null)
                    dispText = HighlightTexture;
                else
                    dispText = Texture;
            }
            else
            {
                dispText = Texture;
                state = UIButtonState.None;
            }

            if (input.IsKeyPressed(Hotkey) || (isInRect && input.IsMouseBtnReleased(MouseButton.Left)))
            {
                if (Method != null)
                    Method();
            }
            game = null;
        }

        public void Draw(SpriteBatch batch)
        {
            batch.Draw(dispText, Frame, Color.White);
            if (Title.Length > 0)
            {
                Vector2 size = Font.MeasureString(Title);
                //if the size of the string is to big to fit, display ...
                if (size.X > Frame.Width || size.Y > Frame.Height)
                    batch.DrawString(Font, "...", new Vector2(Frame.Center.X - size.X / 2, Frame.Center.Y - size.Y / 2), Color.White);
                else
                    batch.DrawString(Font, Title, new Vector2(Frame.Center.X - size.X / 2, Frame.Center.Y - size.Y / 2), Color.White);
            }
        }
    }
}
