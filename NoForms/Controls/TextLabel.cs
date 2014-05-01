using System;
using System.Collections.Generic;
using NoForms.Renderers;

namespace NoForms.Controls
{
    public class TextLabel : Abstract.BasicContainer
    {
        public Object tag = null;
        public USolidBrush background = new USolidBrush() { color = new Color(0, 0, 0, 0) };
        public UBrush foreground = new USolidBrush() { color = new Color(0) };

        public bool autosizeX = false, autosizeY = false;
        public UText textData = new UText("", UHAlign.Center, UVAlign.Middle, true, 0, 0)
        {
            font = new UFont("Arial Black", 15f, false, false),
        };

        public TextLabel() : base() 
        {
            SizeChanged += new Action<NoForms.Size>(TextLabel_SizeChanged);
        }

        void TextLabel_SizeChanged(Size obj)
        {
            textData.width = obj.width;
            textData.height = obj.height;
        }

        public event System.Windows.Forms.MethodInvoker clicked;
        public override void Draw(IRenderType ra)
        {
            ra.uDraw.FillRectangle(DisplayRectangle, background);
            ra.uDraw.DrawText(textData,DisplayRectangle.Location, foreground, UTextDrawOptions.None,false);
            
            var ti = ra.uDraw.GetTextInfo(textData);
            int nLines=ti.numLines;
            Size minSize = ti.minSize;
            if ((autosizeX && minSize.width != Size.width) || (autosizeY && minSize.height != Size.height))
                Size = new Size(autosizeX ? minSize.width : Size.width, autosizeY ? minSize.height : Size.height);
        }

        public override void MouseUpDown(System.Windows.Forms.MouseEventArgs mea, MouseButtonState mbs, bool inComponent, bool amClipped)
        {
            if (inComponent && mbs == MouseButtonState.UP && mea.Button == System.Windows.Forms.MouseButtons.Left && clicked != null)
                clicked();
        }

        bool overed = false;
        public override void MouseMove(System.Drawing.Point location, bool inComponent, bool amClipped)
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
