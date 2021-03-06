﻿using System;
using System.Collections.Generic;
using NoForms.Renderers;
using NoForms;
using NoForms.ComponentBase;
using NoForms.Common;

namespace NoFormsSDK
{
    public class TextLabel : Container
    {
        public Object tag = null;
        public USolidBrush background = new USolidBrush() { color = new Color(0, 0, 0, 0) };
        public UBrush foreground = new USolidBrush() { color = new Color(0) };

        public bool autosizeX = false, autosizeY = false;
        public UText textData = new UText("", UHAlign.Center, UVAlign.Middle, true, 0, 0)
        {
            font = new UFont("Arial Black", 12f, false, false),
        };

        public TextLabel() : base() 
        {
            SizeChanged += new Action<Size>(TextLabel_SizeChanged);
        }

        void TextLabel_SizeChanged(Size obj)
        {
            textData.width = obj.width;
            textData.height = obj.height;
        }

        public event VoidAction clicked;
        public override void Draw(IDraw ra, Region dirty)
        {
            ra.uDraw.FillRectangle(DisplayRectangle, background);
            ra.uDraw.DrawText(textData,DisplayRectangle.Location, foreground, UTextDrawOptions.None,false);
            
            var ti = ra.uDraw.GetTextInfo(textData);
            int nLines=ti.numLines;
            Size minSize = ti.minSize;
            if ((autosizeX && minSize.width != Size.width) || (autosizeY && minSize.height != Size.height))
                Size = new Size(autosizeX ? minSize.width : Size.width, autosizeY ? minSize.height : Size.height);
        }

        public override void MouseUpDown(Point location, MouseButton mb, ButtonState mbs, bool inComponent, bool amClipped)
        {
            if (inComponent && mbs == ButtonState.UP && mb == MouseButton.LEFT && clicked != null)
                clicked();
        }

        bool overed = false;
        public override void MouseMove(Point location, bool inComponent, bool amClipped)
        {
            base.MouseMove(location, inComponent, amClipped);
            if (inComponent && overed == false)
            {
                overed = true;
                if (MouseHover != null)
                    MouseHover(true);
            }
            if (overed && !inComponent)
            {
                overed = false;
                if (MouseHover != null)
                    MouseHover(false);
            }
        }
        public event Action<bool> MouseHover;
    }
}
