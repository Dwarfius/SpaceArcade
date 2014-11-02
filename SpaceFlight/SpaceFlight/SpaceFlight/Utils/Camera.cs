using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceFlight.Game;

namespace SpaceFlight
{
    public class Camera
    {
        public Matrix transform;
        public Matrix viewTransform;
        public Viewport viewport;
        float scale = 1;
        public float Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                if (scale < 0.3f)
                    scale = 0.3f;
                if (scale > 2)
                    scale = 2;
            }
        }

        Vector2 center;

        public Camera(Viewport viewport)
        {
            this.viewport = viewport;
        }

        public void Update(Player obj)
        {
            center = new Vector2(obj.Rec.X + obj.Rec.Width / 2, obj.Rec.Y + obj.Rec.Height / 2);
            transform = Matrix.CreateTranslation(new Vector3(-center.X, -center.Y, 0)) * 
                        Matrix.CreateScale(new Vector3(scale, scale, 0)) *
                        Matrix.CreateRotationZ(0) *
                        Matrix.CreateTranslation(new Vector3(viewport.Width/2, viewport.Height/2, 0));
            
            viewTransform = Matrix.CreateTranslation(new Vector3(viewport.X/2, viewport.Y/2, 0));
        }
    }
}