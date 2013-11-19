using System;
using System.Collections.Generic;
using NoForms.Renderers;

namespace NoForms.Controls
{
    public class TextLabel : Templates.Containable
    {
        public Object tag = null;
        public USolidBrush background = new USolidBrush() { color = new Color(0, 0, 0, 0) };
        public UBrush foreground = new USolidBrush() { color = new Color(0) };

        public bool autosizeX = false, autosizeY = false;
        public UText textData = new UText("", UHAlign_Enum.Center, UVAlign_Enum.Middle, true, 0, 0)
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
        public override void DrawBase(IRenderType ra)
        {
            ra.uDraw.FillRectangle(DisplayRectangle, background);
            ra.uDraw.DrawText(textData,DisplayRectangle.Location, foreground, UTextDrawOptions_Enum.None);
            
            int nLines;
            Size minSize = textData.TextMinSize(out nLines);
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
