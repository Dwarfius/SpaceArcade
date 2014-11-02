using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceFlight.Utils;

namespace SpaceFlight.Game
{
    public class SpaceItem : GameObj
    {
        public Item item;
        
        public SpaceItem(Rectangle frame, Vector2 vel, Texture2D text, float rotation)
            : base(frame, vel, Vector2.Zero, text, rotation, 0)
        {
            //for items the maximum aproach speed is the following
            maxVelocity = new Vector2(20, 20);
            curRotation = (float)Math.PI / 180;
            Type = GameObjectType.SpaceItem;
        }

        public override void Update(GameTime gameTime)
        {
            #region Movement
            Game1 del = Game1.GetDelegate();

            //try to get to the player
            float dist = 999999;
            float distLimit = 150 * 150;

            if(del.unitController.player != null)
                dist = DistSqr(del.unitController.player);

            if (dist < distLimit && del.unitController.player.inv.FreeCount > 0)
            {
                int xDir = Math.Sign(CenterPos.X - del.unitController.player.CenterPos.X);
                int yDir = Math.Sign(CenterPos.Y - del.unitController.player.CenterPos.Y);
                Vel = maxVelocity * new Vector2(-xDir, -yDir) * (distLimit / dist);
            }
            else
            {
                //finding the nearest with a free slot
                Ship nearest = null;
                foreach (Ship ship in del.unitController.enemies)
                {
                    float d = DistSqr(ship);
                    if (d < dist && ship.inv.FreeCount > 0)
                    {
                        nearest = ship;
                        dist = d;
                    }
                }

                if (nearest != null)
                {
                    int xDir = Math.Sign(CenterPos.X - nearest.CenterPos.X);
                    int yDir = Math.Sign(CenterPos.Y - nearest.CenterPos.Y);
                    Vel = maxVelocity * new Vector2(-xDir, -yDir) * (distLimit / dist);
                }
            }

            Pos += Vel * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Rec = new Rectangle((int)Pos.X, (int)Pos.Y, Rec.Width, Rec.Height);

            Rot += curRotation;
            #endregion

            RecalculateBoundingRect();
        }

#warning It still bugs out
    }
}
