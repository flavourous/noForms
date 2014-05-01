using System;
using System.Collections.Generic;
using SharpDX.Direct2D1;
using NoForms.Renderers;

namespace NoForms.Controls
{
    public class Scribble : Abstract.BasicContainer
    {
        public override void Draw(IRenderType renderArgument)
        {
            draw(renderArgument.uDraw, tehBrush, tehStroke);
        }
        USolidBrush tehBrush = new USolidBrush() { color = new Color(1)};
        UStroke tehStroke = new UStroke(); // use defaylt;
        public delegate void scribble(IUnifiedDraw uDraw, USolidBrush tehBrush, UStroke strk);
        public event scribble draw = delegate { };
        
        public delegate void ClickDelegate(Point loc);
        public event ClickDelegate Clicked;
        bool downed = false;
        public override void MouseUpDown(System.Windows.Forms.MouseEventArgs mea, MouseButtonState mbs, bool inComponent, bool amClipped)
        {
            if (mbs == MouseButtonState.DOWN && inComponent && !amClipped && mea.Button == System.Windows.Forms.MouseButtons.Left)
                downed = true;
            if (mbs == MouseButtonState.UP && inComponent && !amClipped && mea.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (downed)
                { // clicked
                    if (Clicked != null)
                        foreach (ClickDelegate cd in Clicked.GetInvocationList())
                            cd(mea.Location);
                }
                downed = false;
            }
        }
    }
}
