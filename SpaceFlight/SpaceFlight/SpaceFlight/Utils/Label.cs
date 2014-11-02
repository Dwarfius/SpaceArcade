using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceFlight.Utils
{
    public enum Alignment
    {
        Left, Center, Right
    }
    public class Label
    {
        public Alignment Align;
        public String Title;
        public Rectangle Frame;
        public SpriteFont Font;
        public Color TextColor;
        public Texture2D background = null;

        public Label(Rectangle frame, String title, SpriteFont font)
        {
            Title = title;
            Frame = frame;
            Font = font;
            TextColor = Color.White;
            Align = Alignment.Center;
        }

        public void Draw(SpriteBatch batch)
        {
            if(background != null)
                batch.Draw(background, Frame, Color.White);

            Vector2 drawPos;
            Vector2 stringSize = Font.MeasureString(Title);
            if (Align == Alignment.Center)
                drawPos = new Vector2(Frame.X + Frame.Width / 2 - stringSize.X / 2, Frame.Y + Frame.Height / 2 - stringSize.Y / 2);
            else if (Align == Alignment.Left)
                drawPos = new Vector2(Frame.X + 5, Frame.Y + Frame.Height / 2 - stringSize.Y / 2);
            else //Align == Right
                drawPos = new Vector2(Frame.Right - 5 - stringSize.X, Frame.Y + Frame.Height / 2 - stringSize.Y / 2);

            batch.DrawString(Font, Title, drawPos, Color.White);
        }
    }
}
