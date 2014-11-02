using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceFlight.Utils;

namespace SpaceFlight.Game
{
#warning Make this used
    public enum MunitionsType
    {
        Bullet, Rocket, Laser
    }

    public class Munitions : GameObj
    {
        //MunitionsType munitionsType;
        float timedLife = 7.5f;
        float dmgVal;

        public Munitions(Rectangle rec, Vector2 vel, Vector2 maxVel, Texture2D text, float rotation, float damage)
            : base(rec, vel, Vector2.Zero, text, rotation, 0)
        {
            dmgVal = damage;
            Type = GameObjectType.Munitions;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            timedLife -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timedLife <= 0)
                Destroy();
        }
    }
}
