using System;
using SysRect = System.Drawing.Rectangle;
using System.Windows.Forms;

namespace NoForms.Controls.Templates
{
    public abstract class Focusable : IComponent
    {
        static class FocusManager
        {
            static Object focusLock = new object();
            static Focusable focused;
            public static void FocusSet(Focusable setFocus, bool focus)
            {
                lock (focusLock)
                {
                    // Focus change from one to another
                    if (focused != null && focus && focused != setFocus)
                    {
                        focused._focus = false;
                        focused = setFocus;
                    }
                    // Focus being set to nothing
                    if (focused == setFocus && !focus)
                    {
                        focused._focus = false;
                        focused = null;
                    }
                    // focus set from nothing to something
                    if (focused == null && focus)
                    {
                        focused = setFocus;
                    }
                    setFocus._focus = focus;
                }
            }
        }

        // Focusable bits...
        private bool _focus = false;
        public bool focus
        {
            get { return _focus; }
            set
            {
                FocusManager.FocusSet(this, value);
                FocusChange(value);
            }
        }

        // Only focusable components can recieve keys...
        // Key Events, dont insist you handle them...
        public virtual void KeyPress(char c)
        {
        }
        public virtual void KeyDown(System.Windows.Forms.Keys key)
        {
        }
        public virtual void KeyUp(System.Windows.Forms.Keys key)
        {
        }
        public virtual void FocusChange(bool focus)
        {
        }

        // Mousey, dont force to impliment
        public virtual void MouseMove(System.Drawing.Point location, bool inComponent, bool amClipped)
        {
        }
        public virtual void MouseUpDown(MouseEventArgs mea, MouseButtonState mbs, bool inComponent, bool amClipped)
        {
        }

        // Left from implimentation
        public abstract IContainer Parent { get; set; }
        public abstract Rectangle DisplayRectangle { get; set; }
        public abstract bool visible { get; set; }
        public abstract void RecalculateDisplayRectangle();
        public abstract void RecalculateLocation();
        public abstract void DrawBase(IRenderType renderArgument);
    }
}
