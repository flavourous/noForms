using System;
using System.Collections.Generic;
using SharpDX.Direct2D1;
using NoForms.Renderers;

namespace NoForms.Controls
{
    public class Scribble : Templates.Containable
    {
        public override void DrawBase(IRenderType renderArgument)
        {
            draw(renderArgument.uDraw, tehBrush, tehStroke);
        }
        USolidBrush tehBrush = new USolidBrush() { color = new Color(1)};
        UStroke tehStroke = new UStroke(); // use defaylt;
        public delegate void scribble(UnifiedDraw uDraw, USolidBrush tehBrush, UStroke strk);
        public event scribble draw = delegate { };
        
        System.Windows.Forms.Cursor pCurs = null;
        void OnpleaseChangeCursor(System.Windows.Forms.Cursor c)
        {
            if (pleaseChangeCursor != null)
                pleaseChangeCursor(c);
        }
        public event Action<System.Windows.Forms.Cursor> pleaseChangeCursor;
        public override void MouseMove(System.Drawing.Point location, bool inComponent, bool amClipped)
        {
            if (inComponent && pCurs == null)
            {
                pCurs = System.Windows.Forms.Cursor.Current;
                OnpleaseChangeCursor(System.Windows.Forms.Cursors.Hand);
            }
            if (pCurs != null && !inComponent)
            {
                OnpleaseChangeCursor(pCurs);
                pCurs = null;
            }
        }
        public delegate void ClickDelegate(Point loc);
        public event ClickDelegate Clicked;
        bool downed = false;
        public override void MouseUpDown(System.Windows.Forms.MouseEventArgs mea, MouseButtonState mbs, bool inComponent, bool amClipped)
        {
            if (mbs == MouseButtonState.DOWN && inComponent && mea.Button == System.Windows.Forms.MouseButtons.Left)
                downed = true;
            if (mbs == MouseButtonState.UP && inComponent && mea.Button == System.Windows.Forms.MouseButtons.Left)
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
