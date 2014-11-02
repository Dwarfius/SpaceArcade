using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceFlight
{
    public struct Particle
    {
        public static Texture2D img;

        public Vector2 pos, force;
        public float life;

        Color color;

        public Particle(Vector2 position, Vector2 initialForce, float maxLife)
        {
            if (img == null)
            {
                img = new Texture2D(Game1.GetDelegate().GraphicsDevice, 1, 1);
                img.SetData<Color>(new[] { Color.White });
            }

            pos = position;
            force = initialForce;
            Random rnd = new Random();
            color = Color.FromNonPremultiplied(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);
            life = maxLife;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(img, pos, color);
        }
    }
}
