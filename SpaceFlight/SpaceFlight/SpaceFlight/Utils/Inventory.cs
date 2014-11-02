using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SpaceFlight.Game;

namespace SpaceFlight.Utils
{
    public enum SlotType
    {
        Generic = 9, Weapon = 2, Engine = 1, Hull = 0
    }

    public class Item
    {
        public Texture2D Image;
        public Dictionary<string, string> values = new Dictionary<string, string>();

        #region Properties
        public String ID
        {
            get { return values["ID"]; }
            set { values["ID"] = value; }
        }
        public String Name
        {
            get { return values["Name"]; }
            set { values["Name"] = value; }
        }
        public String Description
        {
            get { return values["Description"]; }
            set { values["Description"] = value; }
        }
        public SlotType Type
        {
            get { return (SlotType)Convert.ToInt16(values["SlotType"]); }
            set { values["SlotType"] = value.ToString(); }
        }
        #endregion

        public override string ToString()
        {
            return ID + "(" + Type +")";
        }
    }

    public class Inventory
    {
        public int FreeCount { get { return items.Length - Count(); } }
        public int Capacity { get { return items.Length; } }

        static Texture2D empty;

        object owner;
        Item[] items;

        int columns;
        Button[] btns;
        Button[][] equip;

        List<Label> infoLbls = new List<Label>();

        public Inventory(Hull hull, object own, int cols = 10)
        {
            owner = own;
            items = new Item[hull.cargoCap];
            equip = Parse(hull.eqSlots);
            empty = Game1.GetDelegate().textures["EmptySlot"];
            columns = cols;
            if (own is Player)
                Resize();
        }

        #region Inventory Maintenance and Accessors
        public Item this[int index]
        {
            get { return items[index]; }
            set
            {
                items[index] = value;
                if (items[index] != null)
                {
                    btns[index].Texture = items[index].Image;
                    btns[index].Method = MethodForItem(value, index);
                }
                else
                {
                    btns[index].Texture = empty;
                    btns[index].Method = null;
                    btns[index].HighlightTexture = null;
                }
            }
        }

        public Item Find(string id)
        {
            for (int j = 0; j < items.GetLength(0); j++)
                if (items[j].ID == id)
                    return items[j];
            return null;
        }

        public Item[] FindAll(string id)
        {
            Item[] res = null;
            int j = 0;
            //check all the elements
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].ID == id)
                {
                    //if we actually find one, we should return something
                    if (res == null)
                        res = new Item[items.Length];
                    res[j++] = items[i];
                }
            }
            //truncating the excess slots
            if (res != null)
            {
                Item[] copy = new Item[j + 1];
                res.CopyTo(copy, 0);
                res = copy;
            }
            return res;
        }

        public bool Add(Item item)
        {
            if (item == null)
                return true;
            if (Count() < items.Length)
            {
                int index = FreeSlot();
                items[index] = item;
                if (owner is Player)
                {
                    btns[index].Texture = item.Image;
                    btns[index].Method = MethodForItem(item, index);
                }
                return true;
            }
            return false;
        }

        public void Remove(Item item)
        {
            for (int i = 0; i < items.GetLength(0); i++)
            {
                if (items[i] == item)
                {
                    items[i] = null;
                    if (owner is Player)
                    {
                        btns[i].Method = null;
                        btns[i].Texture = empty;
                    }
                    return;
                }
            }
        }

        public void RemoveAll(Item item)
        {
            for (int i = 0; i < items.GetLength(0); i++)
            {
                if (items[i] == item)
                {
                    items[i] = null;
                    if (owner is Player)
                    {
                        btns[i].Method = null;
                        btns[i].Texture = empty;
                    }
                }
            }
        }

        public Item[] SetCapacity(int cap)
        {
            //array of items that don't fit
            Item[] ret = null;

            int count = Count();
            if (count > cap)
            {
                ret = new Item[count - cap];
                for (int i = count - 1; i < items.Length; i++)
                    ret[i - count + 1] = items[i];
            }
            Item[] newArr = new Item[cap];
            for (int i = 0; i < cap; i++)
                newArr[i] = items[i];

            items = newArr;
            Resize();
            return ret;
        }

        public bool Exists(Item item)
        {
            for (int i = 0; i < items.Length; i++)
                if (items[i] == item)
                    return true;
            return false;
        }

        public int Count()
        {
            int res = 0;
            for (int i = 0; i < items.Length; i++)
                if (items[i] != null) res++;
            return res;
        }

        public void Resize()
        {
            float startX = Game1.GetDelegate().GraphicsDevice.Viewport.Width / 8;
            float startY = Game1.GetDelegate().GraphicsDevice.Viewport.Height / 5;

            float size = startY / 2;
            const int offset = 5;

            int highest = 0;
            for (int j = 0; j < equip.Length; j++)
            {
                for (int i = 0; i < equip[j].Length; i++)
                {
                    int x = (int)(startX + (size*2) * j);
                    int y = (int)(startY + size * i);
                    if (i > highest)
                        highest = i;
                    int s = (int)(size - offset);
                    Rectangle rec = new Rectangle(x,y,s,s);
                    equip[j][i] = new Button(rec, empty);
                    if(!(owner as Ship).IsSlotEmpty((SlotType)j, i))
                        equip[j][i].Method = Unequip((SlotType)j, i);
                }
            }

            startY += (highest+1) * size + offset;

            Rectangle[] rects = new Rectangle[items.Length];
            btns = new Button[items.Length];

            int rows = (int)Math.Ceiling(items.Length / (float)columns);
            for (int j = 0; j < rows; j++)
            {
                for (int i = 0; i < columns; i++)
                {
                    int index = j * columns + i;
                    if (index > items.Length - 1)
                        break;
                    else
                    {
                        float x = startX + i * size;
                        float y = startY + j * size;
                        rects[index] = new Rectangle((int)x, (int)y, (int)size - offset, (int)size - offset);
                    }
                }
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (btns[i] != null)
                    btns[i].Frame = rects[i];
                else
                    btns[i] = new Button(rects[i], empty);
            }

            if (infoLbls.Count == 0) //no items present
            {
                int y = (int)(rows * size + startY + 20);
                Texture2D text = new Texture2D(Game1.GetDelegate().GraphicsDevice, 1, 1); //creating a texture for both labels
                text.SetData<Color>(new [] { Color.Blue });

                Label lbl1 = new Label(new Rectangle((int)startX, y, (int)startX * 6, 20), "", Game1.GetDelegate().baseFont);
                lbl1.background = text;
                infoLbls.Add(lbl1);

                int height = Game1.GetDelegate().GraphicsDevice.Viewport.Height - (y + 30);
                Label lbl2 = new Label(new Rectangle((int)startX, y + 30, (int)startX * 6, height), "", Game1.GetDelegate().baseFont);
                lbl2.background = text;
                infoLbls.Add(lbl2);
            }
            else
            {
                int y = (int)(rows * size + startY + 20);
                infoLbls[0].Frame.X = (int)startX;
                infoLbls[0].Frame.Y = y;
                infoLbls[0].Frame.Width = (int)startX * 6;
                infoLbls[1].Frame.X = (int)startX;
                infoLbls[1].Frame.Y = y + 30;
                infoLbls[1].Frame.Width = (int)startX * 6;
            }
        }
        #endregion

        #region Update and Draw
        public void Update(InputSystem input)
        {
            infoLbls[0].Title = ""; //resetting both of the bottom information labels
            infoLbls[1].Title = "";

            Player p = (owner as Player);
            for (int j = 0; j < equip.Length; j++ )
            {
                for (int i = 0; i < equip[j].Length; i++)
                {
                    Button b = equip[j][i];
                    if (j == (int)SlotType.Engine)
                    {
                        if (p.eng != null)
                            b.Texture = p.eng.item.Image;
                        else
                            b.Texture = empty;
                    }
                    else if (j == (int)SlotType.Hull)
                    {
                        if (p.Hull != null)
                            b.Texture = p.Hull.item.Image;
                        else
                            b.Texture = null;
                    }
                    else if (j == (int)SlotType.Weapon)
                    {
                        if (p.weapons[i] != null)
                            b.Texture = p.weapons[i].item.Image;
                        else
                            b.Texture = empty;
                    }
                    b.Update(input);
                }
            }

            //updating the inventory buttons
            bool flag = false;
            for (int i = 0; i < items.Length; i++)
            {
                if (flag)
                    break;

                btns[i].Update(input);
                if (btns[i].State == UIButtonState.Highlighted && items[i] != null)
                {
                    infoLbls[0].Title = items[i].Name;
                    infoLbls[1].Title = items[i].Description;
                    flag = true;
                }
            }
        }

        public void Draw(SpriteBatch batch)
        {
            for (int j = 0; j < equip.Length; j++)
                for (int i = 0; i < equip[j].Length; i++)
                    equip[j][i].Draw(batch);
            for (int i = 0; i < items.Length; i++)
                btns[i].Draw(batch);
            for (int i = 0; i < infoLbls.Count; i++)
                infoLbls[i].Draw(batch);
        }
        #endregion

        #region Internal Methods
        //returns an index of a free slot
        int FreeSlot()
        {
            for (int i = 0; i < items.Length; i++)
                if (items[i] == null)
                    return i;
            return -1;
        }

        Button[][] Parse(string s)
        {
            string[] info = s.Split(':', ',');
            int slotTypes = Convert.ToInt32(info[0]);
            Button[][] res = new Button[slotTypes][];
            for (int i = 0; i < slotTypes; i++)
                res[i] = new Button[Convert.ToInt32(info[i + 1])];

            return res;
        }

        Action MethodForItem(Item item, int btnIndex)
        {
            string id = item.ID;
            if (id.Contains("heal"))
            {
                float amount = (float)Convert.ToDouble(item.values["Amount"]);
                return delegate
                {
                    (owner as Ship).Health += amount;
                    this[btnIndex] = null; 
                };
            }
            else if (id.Contains("rapidFire"))
            {
                float amount = (float)Convert.ToDouble(item.values["Amount"]);
                float life = (float)Convert.ToDouble(item.values["Time"]);
                return delegate
                {
                    for (int i = 0; i < (owner as Ship).weapons.Length; i++)
                    {
                        (owner as Ship).weapons[i].weaponCd += amount;
                    }
                    ItemEffect effect = new ItemEffect();
                    effect.endAction = delegate 
                    {
                        for (int i = 0; i < (owner as Ship).weapons.Length; i++)
                        {
                            (owner as Ship).weapons[i].weaponCd -= amount;
                        }
                    };
                    effect.timedLife = life;
                    (owner as Ship).effects.Add(effect);
                    this[btnIndex] = null;
                };
            }
            else if (id.Contains("eng"))
            {
                return delegate { (owner as Player).eng = new Engine(item); Remove(item); };
            }
            else if (id.Contains("weap"))
            {
                return delegate 
                {
                    for (int i = 0; i < (owner as Ship).weapons.Length; i++)
                    {
                        if ((owner as Player).weapons[i] == null)
                        {
                            (owner as Player).weapons[i] = new Weapon(item);
                            break;
                        }
                    }
                    Remove(item); 
                };
            }
            else
                return null;
        }

        //a dirty hack, I need to save the slotType and slot at the moment of creating a function, so I can't do return delegate { Unequip(...); };
        //because that gets the variables at the moment of cycle 
        Action Unequip(SlotType slotType, int slot)
        {
            return delegate
            {
                Item item = null;
                if (slotType == SlotType.Engine)
                {
                    item = (owner as Player).eng.item;
                    (owner as Player).eng = null;
                }
                else if (slotType == SlotType.Weapon)
                {
                    item = (owner as Player).weapons[slot].item;
                    (owner as Player).weapons[slot] = null;
                }

                this.Add(item);
            };
        }
        #endregion
    }
}