using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceFlight.Utils;
using SpaceFlight.Game;

namespace SpaceFlight
{
    public class GameObj
    {
        public Vector2 maxVelocity;
        public float curRotation;

        public Color[] colorData { get; set; }
        public Vector2 Pos { get; set; }
        public Vector2 Vel { get; set; }
        public Rectangle Rec { get; set; }
        public float Rot { get; set; }
        public virtual Texture2D Text { get { return texture; } }
        public GameObjectType Type { get; set; }
        public Rectangle BoundingRect;
        public Vector2 CenterPos { get { return Rec.CenterV(); } }
        public Matrix Matrix;
        public Vector2 ColorSize { get; set; }

        Texture2D texture;

        /// <summary>
        /// Create game object at position with preset velocity, maximum velocity, texture and rotation.
        /// </summary>
        /// <param name="rec">Object position and size</param>
        /// <param name="vel">Starting velocity</param>
        /// <param name="maxVel">Maximum possible velocity</param>
        /// <param name="text">Texture</param>
        /// <param name="rot">Starting rotation</param>
        /// <param name="rotdx">Rotation differential</param>
        public GameObj(Rectangle rec, Vector2 vel, Vector2 maxVel, Texture2D text, float rot, float rotdx)
        {
            Rec = rec;
            Pos = new Vector2(rec.X, rec.Y);
            Vel = vel;
            maxVelocity = maxVel;
            texture = text;
            Rot = rot;
            curRotation = rotdx;
            if (texture != null)
            {
                colorData = new Color[texture.Width * texture.Height];
                texture.GetData<Color>(colorData);
                ColorSize = new Vector2(texture.Width, texture.Height);
            }
            Type = GameObjectType.Other;
        }

        public virtual void Update(GameTime gameTime)
        {
            #region Movement
            Pos += Vel * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Rec = new Rectangle((int)Pos.X, (int)Pos.Y, Rec.Width, Rec.Height);

            Rot += curRotation;
            #endregion

            RecalculateBoundingRect();
        }

        public virtual void Draw(SpriteBatch batch)
        {
            Vector2 center = new Vector2(Rec.Width, Rec.Height) / 2;
            batch.Draw(texture, Rec.CenterV(), null, Color.White, Rot, center, 1, SpriteEffects.None, 0); //finaly fixed, damn it!
            Utilities.DrawRect(BoundingRect, Color.Green);
        }

        public virtual void Destroy()
        {
            Game1.GetDelegate().particles.Add(new ParticleController(Rec.CenterV(), 0.5f));
            Game1.GetDelegate().unitController.ScheduleForRemoval(this);
        }

        public void RecalculateBoundingRect()
        {
            //creating a global matrix
            Matrix = Matrix.CreateTranslation(new Vector3(-Rec.Width / 2, -Rec.Height / 2, 0)) * 
                     Matrix.CreateRotationZ(Rot) * 
                     Matrix.CreateTranslation(new Vector3(Rec.CenterV(), 0));

            // Get all four corners in local space
            Vector2 leftTop = new Vector2(0, 0);
            Vector2 rightTop = new Vector2(Rec.Width, 0);
            Vector2 leftBottom = new Vector2(0, Rec.Height);
            Vector2 rightBottom = new Vector2(Rec.Width, Rec.Height);

            // Transform all four corners into work space
            Vector2.Transform(ref leftTop, ref Matrix, out leftTop);
            Vector2.Transform(ref rightTop, ref Matrix, out rightTop);
            Vector2.Transform(ref leftBottom, ref Matrix, out leftBottom);
            Vector2.Transform(ref rightBottom, ref Matrix, out rightBottom);

            // Find the minimum and maximum extents of the rectangle in world space
            Vector2 min = Vector2.Min(Vector2.Min(leftTop, rightTop),
                                      Vector2.Min(leftBottom, rightBottom));
            Vector2 max = Vector2.Max(Vector2.Max(leftTop, rightTop),
                                      Vector2.Max(leftBottom, rightBottom));

            // Return as a rectangle
            BoundingRect = new Rectangle((int)min.X, (int)min.Y,
                                 (int)(max.X - min.X), (int)(max.Y - min.Y));
        }

        #region Utility methods
        public float DistSqr(GameObj obj)
        {
            return Vector2.DistanceSquared(CenterPos, obj.CenterPos);
        }

        public float Dist(GameObj obj)
        {
            return Vector2.Distance(CenterPos, obj.CenterPos);
        }
        #endregion
    }
}