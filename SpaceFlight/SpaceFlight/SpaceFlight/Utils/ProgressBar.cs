using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceFlight.Utils
{
    public class ProgressBar
    {
        public float Value
        {
            get { return val; }
            set 
            {
                val = value;
                filRect = new Rectangle(Frame.X, Frame.Y, (int)(Frame.Width * val), Frame.Height);
            }
        }

        public Rectangle Frame;
        public String Title = "";
        public SpriteFont Font = null;

        Texture2D text;
        Color startC, endC;
        Rectangle filRect;
        float val;

        public ProgressBar(Rectangle frame, Color color) 
        {
            text = new Texture2D(Game1.GetDelegate().GraphicsDevice, 1, 1);
            text.SetData<Color>(new [] { Color.White });

            startC = color;
            endC = color;
            Frame = frame;
        }

        public ProgressBar(Rectangle frame, Color startColor, Color endColor) : this(frame, startColor)
        {
            endC = endColor;
        }

        public void Draw(SpriteBatch batch)
        {
            if (startC != endC)
            {
                int dR = 0, dG = 0, dB = 0, dA = 0;
                dR = (int)((endC.R - startC.R) * val);
                dG = (int)((endC.G - startC.G) * val);
                dB = (int)((endC.B - startC.B) * val);
                dA = (int)((endC.A - startC.A) * val);
                Color c = new Color(endC.R - dR, endC.G - dG, endC.B - dB, endC.A - dA);
                batch.Draw(text, filRect, c);
            }
            else
                batch.Draw(text, filRect, startC);

            if(Title.Length > 0)
            {
                Vector2 size = Font.MeasureString(Title);
                if (size.X < Frame.Width && size.Y < Frame.Height)
                    batch.DrawString(Font, Title, new Vector2(Frame.Center.X - size.X / 2, Frame.Center.Y - size.Y / 2), Color.White);
            }
        }

        public void Dispose()
        {
            text.Dispose();
        }

        public void Resize(DisplayMode oldMode)
        {
            Viewport port = Game1.GetDelegate().GraphicsDevice.Viewport;
            float x = port.Width * Frame.X * 1.0f / oldMode.Width;
            float y = port.Height * Frame.Y * 1.0f / oldMode.Height;
            float width = port.Width * Frame.Width * 1.0f / oldMode.Width;
            float height = port.Height * Frame.Height * 1.0f / oldMode.Height;
            Frame = new Rectangle((int)x, (int)y, (int)width, (int)height);
        }
    }
}
