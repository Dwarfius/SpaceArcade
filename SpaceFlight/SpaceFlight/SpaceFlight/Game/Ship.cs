using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceFlight.Utils;

namespace SpaceFlight.Game
{
    public class Engine
    {
        public Item item;
        public float maxSpeed;
        public float maxRotSpeed; //deg
        public Texture2D text;
        public Engine(Item i)
        {
            item = i;
            text = item.Image;
            maxSpeed = (float)Convert.ToDouble(item.values["MaxSpeed"]);
            maxRotSpeed = MathHelper.ToRadians(Convert.ToInt32(item.values["MaxRotation"]));
        }
    }
    
    public class Weapon
    {
        public Item item;
        public float dmg;
        public double weaponCd; //cost of fire
        public double cooldown; //actual cooldown
        public Texture2D text;
        public Weapon(Item i)
        {
            item = i;
            text = Game1.GetDelegate().textures[item.values["ItemText"]];
            weaponCd = Convert.ToDouble(item.values["WeaponCD"]);
            cooldown = 0;
            dmg = (float)Convert.ToDouble(item.values["Damage"]);
        }
    }

    public class Hull
    {
        public Item item;
        public string eqSlots;
        public int cargoCap;
        public float health;
        public float MaxHealth;
        public Texture2D text;
        public Hull(Item i)
        {
            item = i;
            text = item.Image;
            health = MaxHealth = (float)Convert.ToDouble(item.values["MaxHP"]);
            cargoCap = Convert.ToInt32(item.values["CargoCap"]);
            eqSlots = item.values["EquipSlots"];
        }

        int weapSlots = -1;
        public int WeaponSlots()
        {
            if (weapSlots == -1)
            {
                string[] info = eqSlots.Split(':', ',');
                weapSlots = Convert.ToInt16(info[(int)SlotType.Weapon + 1]);
            }

            return weapSlots;
        }
    }

    public struct ItemEffect
    {
        public Action endAction;
        public float timedLife;
    }

    public enum EnemyState
    {
        Aggresive, Passive
    }

    public class Ship : GameObj
    {
        public EnemyState state;
        public virtual float Health
        {
            get { return hull.health; }
            set
            {
                hull.health = value;
                if (hull.health <= 0)
                    Destroy();
            }
        }
        public Hull Hull 
        {
            get { return hull; }
            set
            {
                hull = value;
                colorData = new Color[hull.text.Width * hull.text.Height];
                hull.text.GetData<Color>(colorData);
                ColorSize = new Vector2(hull.text.Width, hull.text.Height);
                weapons = new Weapon[hull.WeaponSlots()];
            }
        }
        public Engine eng = null;
        public Weapon[] weapons = null; //weapon slots
        public Inventory inv = null;
        public List<ItemEffect> effects;
        
        public override Texture2D Text { get { return hull.text; } }

        Hull hull;

        protected float rotationDelta;
        protected const float friction = 0.1f;

        public Ship(Rectangle rec) : base(rec, Vector2.Zero, Vector2.Zero, null, 0, 0)
        {
            effects = new List<ItemEffect>();
            Rec = rec;
            Pos = new Vector2(Rec.X, Rec.Y);
            Type = GameObjectType.Enemy;
        }

        public override void Update(GameTime gameTime)
        {
            float angleToPlayer = MathHelper.Pi / 18;
            bool shouldFire = false;

            #region AI Decision
#warning Add AI decision making
            GameObj target = Game1.GetDelegate().unitController.player;
            #endregion

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

            #region Movement
            if (target != null && state == EnemyState.Aggresive)
            {
                float d = DistSqr(target);
                shouldFire = d < 90000;
                Vector2 heading = target.CenterPos - CenterPos;
                if (d > 10000 && d < 160000)
                {
                    //I'll take that the max acceleration rate is 10, need to change later
                    float a = 10;

                    Vector2 dist = new Vector2();
                    dist.X = MathHelper.Clamp(heading.X, -a, a);
                    dist.Y = MathHelper.Clamp(heading.Y, -a, a);
                    Vel += dist;
                }

                float targetAngle = (float)Math.Atan2(heading.Y, heading.X); 
                angleToPlayer = MathHelper.WrapAngle(targetAngle - Rot);
                rotationDelta = angleToPlayer;
            }

            Vel -= new Vector2(Math.Sign(Vel.X), Math.Sign(Vel.Y)); //applying deceleration
            Vector2 v = new Vector2(); //can't change a field of a property directly
            v.X = MathHelper.Clamp(Vel.X, -eng.maxSpeed, eng.maxSpeed);
            v.Y = MathHelper.Clamp(Vel.Y, -eng.maxSpeed, eng.maxSpeed);
            Vel = v;

            Pos += Vel * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Rec = new Rectangle((int)Pos.X, (int)Pos.Y, Rec.Width, Rec.Height);

            rotationDelta = MathHelper.Clamp(rotationDelta, -eng.maxRotSpeed, eng.maxRotSpeed);
            Rot += rotationDelta * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Rot = MathHelper.WrapAngle(Rot);
            #endregion

            #region Actions
            float cosA = (float)Math.Cos(Rot);
            float sinA = (float)Math.Sin(Rot);

            //fire only if there's a 10 degree angle difference between perfect direction and current
            bool canFire = Math.Abs(angleToPlayer) < MathHelper.Pi / 18 && shouldFire;
            for (int i = 0; i < weapons.Length; i++)
            {
                Weapon weap = weapons[i];
                if (weap != null && weap.cooldown > 0) //cooldown
                    weap.cooldown -= gameTime.ElapsedGameTime.TotalSeconds;
                if (weap != null && canFire && weap.cooldown <= 0) //firing
                    FireWeapon(weap, cosA, sinA);
            }
            #endregion

            RecalculateBoundingRect();
        }

        public override void Draw(SpriteBatch batch)
        {
            Vector2 center = new Vector2(Rec.Width, Rec.Height) / 2;
            batch.Draw(hull.text, Rec.CenterV(), null, Color.White, Rot, center, 1, SpriteEffects.None, 0); //finaly fixed, damn it!
            Utilities.DrawRect(BoundingRect, Color.Green);
        }

        public bool IsSlotEmpty(SlotType type, int slot)
        {
            if (type == SlotType.Hull)
                return hull == null;
            else if (type == SlotType.Engine)
                return eng == null;
            else if (type == SlotType.Weapon)
                return weapons[slot] == null;
            return true;
        }

        protected void FireWeapon(Weapon weap, float cosA, float sinA)
        {
            int width = weap.text.Width, height = weap.text.Height;
            Vector2 spawnPosition = new Vector2(Rec.Center.X + 30 * cosA - width / 2, Rec.Center.Y + 30 * sinA - height / 2);
            Rectangle frame = new Rectangle((int)spawnPosition.X, (int)spawnPosition.Y, width, height);
            Game1.GetDelegate().unitController.munitions.Add(new Munitions(frame, new Vector2(100 * cosA, 100 * sinA), new Vector2(10, 10), weap.text, Rot, weap.dmg));
            weap.cooldown = weap.weaponCd;
        }

        public override void Destroy()
        {
            base.Destroy();
            for (int i = 0; i < inv.Capacity; i++)
            {
                Item item = inv[i];
                if (item != null)
                {
                    Rectangle spawnRec = new Rectangle(Rec.Center.X, Rec.Center.Y, 20, 20);
                    SpaceItem si = new SpaceItem(spawnRec, new Vector2(10, 10), item.Image, 0);
                    si.item = item;
                    Game1.GetDelegate().unitController.spaceItems.Add(si);
                }
            }
        }
    }
}