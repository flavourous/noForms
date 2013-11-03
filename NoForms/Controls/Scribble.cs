using System;
using System.Collections.Generic;
using SharpDX.Direct2D1;

namespace NoForms.Controls
{
    public class Scribble : Templates.Containable
    {
        public override void DrawBase<RenderType>(RenderType renderArgument)
        {
            if (renderArgument is RenderTarget)
            {
                var rt = renderArgument as RenderTarget;
                if (!d2dinit) Init(rt);
                Draw(rt);
            }
            else throw new System.Exception("Only d2d pls 4 me");
        }
        bool d2dinit = false;
        public delegate void scribble(RenderTarget rt, SolidColorBrush scb);
        public event scribble draw;
        void Draw(RenderTarget rt)
        {
            if (draw != null)
                draw(rt, scb);
            
        }
        SolidColorBrush scb;
        void Init(RenderTarget rt)
        {
            scb = new SolidColorBrush(rt, new NoForms.Color(1f, .9f, .2f, .2f));
            d2dinit = true;
        }
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
