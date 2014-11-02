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
    public enum GameObjectType
    {
        Player,
        Enemy,
        Munitions,              
        SpaceItem,
        Other
    }

    public class GameObjContr
    {
        public ProgressBar healthBar;
        public ProgressBar invBar;

        public Player player;
        public List<Ship> enemies = new List<Ship>();
        public List<GameObj> other = new List<GameObj>();
        public List<Munitions> munitions = new List<Munitions>();
        public List<SpaceItem> spaceItems = new List<SpaceItem>();

        public List<GameObj>[,] grid;
        static int zoneSize = 32;
        const int levelSize = 4096;

        Game1 game;
        Texture2D text;
        StringBuilder stringBuilder;
        List<GameObj> scheduledForRemoval = new List<GameObj>();

        public GameObjContr()
        {
            game = Game1.GetDelegate();
            text = new Texture2D(game.GraphicsDevice, 1, 1);
            text.SetData<Color>(new [] { Color.Red });

            //the healthbar is being drawn in the Game1 class, the section of drawing local elements
            stringBuilder = new StringBuilder();
            int width = 140, height = 50;
            Color startC = Color.Green;
            startC.A = (byte)(0.8f * 255);
            Color endC = Color.Red;
            endC.A = (byte)(0.8f * 255);
            healthBar = new ProgressBar(new Rectangle(game.GraphicsDevice.Viewport.Bounds.Width / 2 - width / 2, 0, width, height), startC, endC);
            healthBar.Font = game.baseFont;

            Color constC = Color.LightGray;
            constC.A = (byte)(0.8f * 255);
            invBar = new ProgressBar(new Rectangle(game.GraphicsDevice.Viewport.Bounds.Width / 2 - width / 2, game.GraphicsDevice.Viewport.Height-height, width, height), constC);
            invBar.Font = game.baseFont;
        }

        //handles the global unit update routine and also check for the collisions
        public void Update(GameTime gameTime, InputSystem input)
        {
            RemoveDestroyed();

            GridUpdate();

            HandleCollisions();

            if(player != null)
                player.Update(gameTime, input);

            foreach (Ship obj in enemies)
                obj.Update(gameTime);
            foreach (GameObj obj in other)
                obj.Update(gameTime);
            foreach (Munitions obj in munitions)
                obj.Update(gameTime);
            foreach (SpaceItem obj in spaceItems)
                obj.Update(gameTime);

            if (player != null)
            {
                healthBar.Value = player.Health / player.Hull.MaxHealth;
                stringBuilder.Clear();
                stringBuilder.AppendFormat("{0}/{1}", player.Health.ToString(), player.Hull.MaxHealth.ToString());
                healthBar.Title = stringBuilder.ToString();

                invBar.Value = player.inv.Count() * 1.0f / player.inv.Capacity;
                stringBuilder.Clear();
                stringBuilder.AppendFormat("{0}/{1}", player.inv.Count(), player.inv.Capacity);
                invBar.Title = stringBuilder.ToString();
            }
        }

        public void Draw(SpriteBatch batch)
        {
            if(player != null)
                player.Draw(batch);
            
            foreach (Ship obj in enemies)
                obj.Draw(batch);
            foreach (GameObj obj in other)
                obj.Draw(batch);
            foreach (Munitions obj in munitions)
                obj.Draw(batch);
            foreach (SpaceItem obj in spaceItems)
                obj.Draw(batch);

            int count = enemies.Count + other.Count + munitions.Count + spaceItems.Count;
            batch.DrawString(Game1.GetDelegate().baseFont, "Entities: " + count.ToString(), player.CenterPos - Vector2.One * 30, Color.White); 
        }
        
        //The idea is that every object is stored in it's one zone. The zones are used to reduce the amount if collision checks:
        //Objects only in the same zone can collide.
        void GridUpdate()
        {
            int zones = levelSize / zoneSize;
            grid = new List<GameObj>[zones, zones];

            for (int j = 0; j < zones; j++)
                for (int i = 0; i < zones; i++)
                    grid[i, j] = new List<GameObj>(); //I'll put every drawable object inside the grid

            int x = (int)MathHelper.Clamp(player.CenterPos.X / zoneSize, 0, zones - 1);
            int y = (int)MathHelper.Clamp(player.CenterPos.Y / zoneSize, 0, zones - 1);
            grid[x, y].Add(player);

            GameObj obj;
            for (int i = 0; i < enemies.Count; i++)
            {
                obj = enemies[i];
                x = (int)MathHelper.Clamp(obj.CenterPos.X / zoneSize, 0, zones - 1);
                y = (int)MathHelper.Clamp(obj.CenterPos.Y / zoneSize, 0, zones - 1);
                grid[x, y].Add(obj);
            }

            for (int i = 0; i < munitions.Count; i++)
            {
                obj = munitions[i];
                x = (int)MathHelper.Clamp(obj.CenterPos.X / zoneSize, 0, zones - 1);
                y = (int)MathHelper.Clamp(obj.CenterPos.Y / zoneSize, 0, zones - 1);
                grid[x, y].Add(obj);
            }

            for (int i = 0; i < other.Count; i++)
            {
                obj = other[i];
                x = (int)MathHelper.Clamp(obj.CenterPos.X / zoneSize, 0, zones - 1);
                y = (int)MathHelper.Clamp(obj.CenterPos.Y / zoneSize, 0, zones - 1);
                grid[x, y].Add(obj);
            }

            for (int i = 0; i < spaceItems.Count; i++)
            {
                obj = spaceItems[i];
                x = (int)MathHelper.Clamp(obj.CenterPos.X / zoneSize, 0, zones - 1);
                y = (int)MathHelper.Clamp(obj.CenterPos.Y / zoneSize, 0, zones - 1);
                grid[x, y].Add(obj);
            }
        }

        void HandleCollisions()
        {
            //first, munitions collisions
            GameObj obstacle;
            foreach (Munitions mun in munitions)
            {
                if (Intersects(mun, GameObjectType.Player, out obstacle))
                {
                    player.Health--;
                    mun.Destroy();
                }
                else if (Intersects(mun, GameObjectType.Enemy, out obstacle))
                {
                    (obstacle as Ship).Health--;
                    mun.Destroy();
                }
            }

            //then, item colissions
            foreach(SpaceItem item in spaceItems)
            {
                if (Intersects(item, GameObjectType.Player, out obstacle))
                {
                    (obstacle as Player).inv.Add(item.item);
                    item.Destroy();
                } 
                else if (Intersects(item, GameObjectType.Enemy, out obstacle))
                {
                    (obstacle as Ship).inv.Add(item.item);
                    item.Destroy();
                }
            }
        }

        #region Intersection Checks
        bool Intersects(GameObj obj, GameObjectType type, out GameObj obstacle)
        {
            //I need to determine, in which grid zone is the current object, that way I can make a lot less intersection checks
            int x = (int)MathHelper.Clamp(obj.CenterPos.X / zoneSize, 0, levelSize / zoneSize - 1);
            int y = (int)MathHelper.Clamp(obj.CenterPos.Y / zoneSize, 0, levelSize / zoneSize - 1);
            List<GameObj> group = Game1.GetDelegate().unitController.grid[x, y];

            //iterating through every object in teh zone
            foreach (GameObj elem in group)
            {
                if (elem.Type == type && obj != elem && Intersects(obj, elem))
                {
                    obstacle = elem;
                    return true;
                }
            }
            obstacle = default(GameObj);
            return false;
        }

        bool Intersects(GameObj obj1, GameObj obj2)
        {
            //if the bounding rects intersect
            if (obj1.BoundingRect.Intersects(obj2.BoundingRect))
            {
                //getting the global transform matrix
                Matrix transformAToB = obj1.Matrix * Matrix.Invert(obj2.Matrix);

                //getting data usde for collision tests
                Color[] dataA = obj1.colorData;
                Color[] dataB = obj2.colorData;
                Rectangle recA = obj1.BoundingRect;
                Rectangle recB = obj2.BoundingRect;

                //finding the bounds of rectangle intersection
                int top = Math.Max(recA.Top, recB.Top);
                int bottom = Math.Min(recA.Bottom, recB.Bottom);
                int left = Math.Max(recA.Left, recB.Left);
                int right = Math.Min(recA.Right, recB.Right);

#warning Not precise - sometimes flies halfway through the object; Maybe reimplement the way sprites are displayed using matrix transforms?
                //checking per pixel collision
                for (int y = top; y < bottom; y++)
                {
                    for (int x = left; x < right; x++)
                    {
                        //transforming in to local space
                        Vector2 posA = new Vector2(x - recA.Left, y - recA.Top);
                        Vector2 posB = Vector2.Transform(posA, transformAToB);

                        int indA = (int)Math.Round(posA.X + posA.Y * obj1.ColorSize.X);
                        int indB = (int)Math.Round(posB.X + posB.Y * obj2.ColorSize.X);

                        if(indA < 0 || indA >= dataA.Length || indB < 0 || indB >= dataB.Length) //making sure it's in range
                            return false;

                        Color A = dataA[indA];
                        Color B = dataB[indB];
                        if (A.A != 0 && B.A != 0) //collision happens only if both pixels are opaque
                            return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region Delayed Removal
        public void ScheduleForRemoval(GameObj obj)
        {
            scheduledForRemoval.Add(obj);
        }

        void RemoveDestroyed()
        {
            int count = scheduledForRemoval.Count;
            for (int i = 0; i < count; i++)
            {
                GameObj obj = scheduledForRemoval[i];
                if (obj is Ship)
                    enemies.Remove(obj as Ship);
                else if (obj is Munitions)
                    munitions.Remove(obj as Munitions);
                else if (obj is SpaceItem)
                    spaceItems.Remove(obj as SpaceItem);
            }
        }
        #endregion
    }
}
