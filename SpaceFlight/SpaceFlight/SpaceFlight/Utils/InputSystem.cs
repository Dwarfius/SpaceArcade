using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace SpaceFlight.Utils
{
    public enum MouseButton
    {
        Left, Middle, Right
    }
    public class InputSystem
    {
        KeyboardState kbState, oldKbState;
        MouseState mState, oldMState;

        public InputSystem()
        {
        }

        public void Update()
        {
            oldKbState = kbState;
            oldMState = mState;
            kbState =  Keyboard.GetState();
            mState = Mouse.GetState();
        }

        //KEYBOARD
        public bool IsKeyDown(Keys key)
        {
            return kbState.IsKeyDown(key);
        }

        public bool IsKeyUp(Keys key)
        {
            return kbState.IsKeyUp(key);
        }

        public bool IsKeyPressed(Keys key)
        {
            return kbState.IsKeyDown(key) && oldKbState.IsKeyUp(key);
        }

        public bool IsKeyReleased(Keys key)
        {
            return kbState.IsKeyUp(key) && oldKbState.IsKeyDown(key);
        }

        //MOUSE
        public bool IsMouseBtnDown(MouseButton btn)
        {
            if (btn == MouseButton.Left)
                return mState.LeftButton == ButtonState.Pressed;
            else if (btn == MouseButton.Middle)
                return mState.MiddleButton == ButtonState.Pressed;
            else if (btn == MouseButton.Right)
                return mState.RightButton == ButtonState.Pressed;

            return false;
        }

        public bool IsMouseBtnUp(MouseButton btn)
        {
            if (btn == MouseButton.Left)
                return mState.LeftButton == ButtonState.Released;
            else if (btn == MouseButton.Middle)
                return mState.MiddleButton == ButtonState.Released;
            else if (btn == MouseButton.Right)
                return mState.RightButton == ButtonState.Released;

            return true;
        }

        public bool IsMouseBtnPressed(MouseButton btn)
        {
            if (btn == MouseButton.Left)
                return mState.LeftButton == ButtonState.Pressed && oldMState.LeftButton == ButtonState.Released;
            else if (btn == MouseButton.Middle)
                return mState.MiddleButton == ButtonState.Pressed && oldMState.MiddleButton == ButtonState.Released;
            else if (btn == MouseButton.Right)
                return mState.RightButton == ButtonState.Pressed && oldMState.RightButton == ButtonState.Released;

            return false;
        }

        public bool IsMouseBtnReleased(MouseButton btn)
        {
            if (btn == MouseButton.Left)
                return mState.LeftButton == ButtonState.Released && oldMState.LeftButton == ButtonState.Pressed;
            else if (btn == MouseButton.Middle)
                return mState.MiddleButton == ButtonState.Released && oldMState.MiddleButton == ButtonState.Pressed;
            else if (btn == MouseButton.Right)
                return mState.RightButton == ButtonState.Released && oldMState.RightButton == ButtonState.Pressed;

            return true;
        }

        public Vector2 MousePos()
        {
            return new Vector2(mState.X, mState.Y);
        }

        public bool ScrollWheelValueChanged()
        {
            return mState.ScrollWheelValue != oldMState.ScrollWheelValue;
        }

        public int ScrollWheelValueChange()
        {
            return mState.ScrollWheelValue - oldMState.ScrollWheelValue;
        }

        public int ScrollWheelValue()
        {
            return mState.ScrollWheelValue;
        }
    }
}