using System;
using System.Collections.Generic;
using SharpDX.Direct2D1;
using NoForms.Renderers;
using Common;

namespace NoForms.Controls
{
    public class Scribble : Abstract.BasicContainer
    {
        public override void Draw(IDraw renderArgument)
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
        public override void MouseUpDown(Point location, MouseButton mb, ButtonState mbs, bool inComponent, bool amClipped)
        {
            if (mbs == ButtonState.DOWN && inComponent && !amClipped && mb == MouseButton.LEFT)
                downed = true;
            if (mbs == ButtonState.UP && inComponent && !amClipped && mb == MouseButton.LEFT)
            {
                if (downed)
                { // clicked
                    if (Clicked != null)
                        foreach (ClickDelegate cd in Clicked.GetInvocationList())
                            cd(location);
                }
                downed = false;
            }
        }
    }
}
