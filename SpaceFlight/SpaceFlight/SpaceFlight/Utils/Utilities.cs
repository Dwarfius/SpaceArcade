using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceFlight.Utils
{
    //This class is the source for different extensions and utility methods
    public static class Utilities
    {
        struct Drawable { public Rectangle r; public Color c;}

        static Texture2D texture;
        static List<Drawable> drawables = new List<Drawable>();

        public static void Init(GraphicsDevice dev)
        {
            texture = new Texture2D(dev, 1, 1);
            texture.SetData<Color>(new[] { Color.White });
        }

        public static Vector2 CenterV(this Rectangle r)
        {
            return new Vector2(r.Center.X, r.Center.Y);
        }

        public static void DrawRect(Rectangle rec, Color color, int lineWidth = 1)
        {
            drawables.Add(new Drawable() { r = new Rectangle(rec.Left, rec.Top, rec.Width, lineWidth), c = color });
            drawables.Add(new Drawable() { r = new Rectangle(rec.Left, rec.Top, lineWidth, rec.Height), c = color });
            drawables.Add(new Drawable() { r = new Rectangle(rec.Left, rec.Bottom, rec.Width, lineWidth), c = color });
            drawables.Add(new Drawable() { r = new Rectangle(rec.Right, rec.Top, lineWidth, rec.Height), c = color });
        }

        public static void Draw(SpriteBatch batch)
        {
            foreach (Drawable drawable in drawables)
                batch.Draw(texture, drawable.r, drawable.c);

            drawables.Clear();
        }
    }
}
