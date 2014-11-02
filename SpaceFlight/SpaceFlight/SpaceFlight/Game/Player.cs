using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SpaceFlight.Utils;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpaceFlight.Game
{
    public class Player : Ship
    {
        public Player(Rectangle rec) : base(rec)
        {
            Type = GameObjectType.Player;
        }

        public void Update(GameTime gameTime, InputSystem input)
        {
            Game1 gameDel = Game1.GetDelegate();

            #region Apply Effect
            for (int i = 0; i < effects.Count; i++)
            {
                ItemEffect effect = effects[i];
                effect.timedLife -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (effect.timedLife <= 0)
                {
                    effect.endAction();
                    effects.RemoveAt(i);
                }
                else
                {
                    effects[i] = effect;
                }
            }
            #endregion

            #region Rotation
            //since they are drawn in different transforms, each must be transformed using appropriate transforms
            Vector2 transformedMousePosition = Vector2.Transform(gameDel.mousePosition, gameDel.localTransform);
            Vector2 localCenter = Vector2.Transform(new Vector2(Rec.Center.X, Rec.Center.Y), gameDel.globalTransform);
            if ((transformedMousePosition.X != localCenter.X) && (transformedMousePosition.Y != localCenter.Y))
            {
                float targetAngle = (float)Math.Atan2(transformedMousePosition.Y-localCenter.Y, transformedMousePosition.X - localCenter.X);
                float angleToPlayer = MathHelper.WrapAngle(targetAngle - Rot);
                if (eng != null)
                    rotationDelta = MathHelper.Clamp(angleToPlayer, -eng.maxRotSpeed, eng.maxRotSpeed);
                else
                    rotationDelta = 0;
                Rot += rotationDelta * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            #endregion

            float cosA = (float)Math.Cos(Rot);
            float sinA = (float)Math.Sin(Rot);

            #region Moving
            if (eng != null)
            {
                int keyPressedX = 1, keyPressedY = 1;
                if (input.IsKeyDown(Keys.W))
                {
                    Vel += new Vector2(0, -10) * friction;
                    keyPressedY = 0;
                }
                if (input.IsKeyDown(Keys.S))
                {
                    Vel += new Vector2(0, 10) * friction;
                    keyPressedY = 0;
                }
                if (input.IsKeyDown(Keys.A))
                {
                    Vel += new Vector2(-10, 0) * friction;
                    keyPressedX = 0;
                }
                if (input.IsKeyDown(Keys.D))
                {
                    Vel += new Vector2(10, 0) * friction;
                    keyPressedX = 0;
                }
                if (keyPressedX == 1 || keyPressedY == 1)
                    Vel -= new Vector2(10 * keyPressedX, 10 * keyPressedY) * new Vector2(Math.Sign(Vel.X), Math.Sign(Vel.Y)) * friction;

                //truncating excess velocity
                float x = Vel.X, y = Vel.Y;
                if (Math.Abs(Vel.X) > Math.Abs(eng.maxSpeed))
                    x = Math.Sign(Vel.X) * eng.maxSpeed;
                if (Math.Abs(Vel.Y) > Math.Abs(eng.maxSpeed))
                    y = Math.Sign(Vel.Y) * eng.maxSpeed;
                Vel = new Vector2(x, y);
            }
            else
            {
                Vel -= new Vector2(Math.Sign(Vel.X), Math.Sign(Vel.Y)) * friction;
            }

            Pos += Vel * (float)gameTime.ElapsedGameTime.TotalSeconds;
            int w = Rec.Width, h = Rec.Height;
            Rec = new Rectangle((int)Pos.X, (int)Pos.Y, w, h);
            #endregion

            #region Actions
            if (input.IsMouseBtnDown(MouseButton.Left))
            {
                for (int i = 0; i < weapons.Length; i++)
                {
                    if (weapons[i] != null && weapons[i].cooldown <= 0)
                    {
                        FireWeapon(weapons[i], cosA, sinA);
                    }
                }
            }

            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null && weapons[i].cooldown > 0)
                    weapons[i].cooldown -= gameTime.ElapsedGameTime.TotalSeconds;
            }
            #endregion

            RecalculateBoundingRect();
        }

        public override void Destroy()
        {
            Game1.GetDelegate().GameOver();
        }
    }
}
