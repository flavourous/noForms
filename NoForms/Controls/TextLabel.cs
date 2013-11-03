using System;
using System.Collections.Generic;
using SharpDX.Direct2D1;
using SharpDX;
using SharpDX.DirectWrite;

namespace NoForms.Controls
{
    public class TextLabel : Templates.Containable
    {
        SharpDX.DirectWrite.Factory dwfact;
        public TextLabel(SharpDX.DirectWrite.Factory ineedawriteyfact) 
        {
            dwfact = ineedawriteyfact;
        }

        public event System.Windows.Forms.MethodInvoker clicked;
        public override void DrawBase<RenderType>(RenderType renderArgument)
        {
            if (renderArgument is RenderTarget)
            {
                Draw(renderArgument as RenderTarget);
            }
            else throw new NotImplementedException("No suportty anything but d2d");
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

        public Object tag = null;
        public String fontName = "Arial Black";
        public float fontSize = 15f;
        public Align textAlign = new Align() { horizontal = HAlign.center, vertical = VAlign.middle };
        public String text = "";
        void Draw(RenderTarget d2drt)
        {
            if (!d2dinit) Init(d2drt);

            var tl = getTextLayout();
            d2drt.FillRectangle(DisplayRectangle, bb);
            d2drt.DrawTextLayout(DisplayRectangle.Location, tl, tb);
            tl.Dispose();
        }

        TextLayout ll = null;
        TextLayout getTextLayout()
        {
            return ll=new TextLayout(dwfact, text, new TextFormat(dwfact, fontName, fontSize), DisplayRectangle.width, DisplayRectangle.height)
            {
                ParagraphAlignment = textAlign,
                TextAlignment = textAlign,
                WordWrapping = WordWrapping.Wrap
            };
        }

        public float getLineHeight()
        {
            if (ll == null) getTextLayout();
            return ll.GetLineMetrics()[0].Height;
        }
        bool d2dinit = false;
        SolidColorBrush tb, bb;
        void Init(RenderTarget d2drt)
        {
            bb = new SolidColorBrush(d2drt, new NoForms.Color(0,0,0,0));
            tb = new SolidColorBrush(d2drt, new NoForms.Color(0.15f));
            d2dinit = true;
        }

        public NoForms.Color foreColor
        {
            get
            {
                if (tb == null)
                    new NoForms.Color(0);
                return tb.Color;
            }
            set
            {
                if (tb != null)
                    tb.Color = value;
            }
        }
        public NoForms.Color backColor
        {
            get
            {
                if (bb == null)
                    new NoForms.Color(0);
                return bb.Color;
            }
            set
            {
                if (bb != null)
                    bb.Color = value;
            }
        }

    }

}
