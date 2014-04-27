using System;
using NoForms;
using NoForms.Renderers;

namespace NoForms.Controls
{
    public class MoveHandle : Abstract.BasicContainer
    {
        NoForm controlled;
        public MoveHandle(NoForm MoveControl)
        {
            controlled = MoveControl;
            MoveControl.MouseMoved += new NoForm.MouseMoveEventHandler(MoveMove);
        }

        public override void KeyDown(System.Windows.Forms.Keys key)
        {
        }
        public override void KeyUp(System.Windows.Forms.Keys key)
        {
        }

        public override void KeyPress(char c)
        {
        }

        USolidBrush fore = new USolidBrush() { color = new Color(1.0f, 0.8f, 0.8f, 0.8f) };
        USolidBrush back = new USolidBrush() { color = new Color(1.0f, 0f, 0f, 0f) };
        UStroke stroke = new UStroke() { strokeWidth = 2f };
        // Render methody
        public override void Draw(IRenderType ra)
        {
            ra.uDraw.FillRectangle(DisplayRectangle,back);
            
            float epad = 3; float arrSq = 3;
            ra.uDraw.DrawLine(aof(new Point(epad, DisplayRectangle.height / 2)), aof(new Point(DisplayRectangle.width - epad, DisplayRectangle.height / 2)), fore, stroke);

            ra.uDraw.DrawLine(aof(new Point(epad, DisplayRectangle.height / 2)), aof(new Point(epad + arrSq, DisplayRectangle.height / 2 + arrSq)), fore, stroke);
            ra.uDraw.DrawLine(aof(new Point(epad, DisplayRectangle.height / 2)), aof(new Point(epad + arrSq, DisplayRectangle.height / 2 - arrSq)), fore, stroke);
            ra.uDraw.DrawLine(aof(new Point(DisplayRectangle.width - epad - arrSq, DisplayRectangle.height / 2 + arrSq)), aof(new Point(DisplayRectangle.width - epad, DisplayRectangle.height / 2)), fore, stroke);
            ra.uDraw.DrawLine(aof(new Point(DisplayRectangle.width - epad - arrSq, DisplayRectangle.height / 2 - arrSq)), aof(new Point(DisplayRectangle.width - epad, DisplayRectangle.height / 2)), fore, stroke);

            ra.uDraw.DrawLine(aof(new Point(DisplayRectangle.width / 2, epad)), aof(new Point(DisplayRectangle.width / 2, DisplayRectangle.height - epad)), fore, stroke);

            ra.uDraw.DrawLine(aof(new Point(DisplayRectangle.width / 2, epad)), aof(new Point(DisplayRectangle.width / 2 - arrSq, epad + arrSq)), fore, stroke);
            ra.uDraw.DrawLine(aof(new Point(DisplayRectangle.width / 2, epad)), aof(new Point(DisplayRectangle.width / 2 + arrSq, epad + arrSq)), fore, stroke);
            ra.uDraw.DrawLine(aof(new Point(DisplayRectangle.width / 2 - arrSq, DisplayRectangle.height - epad - arrSq)), aof(new Point(DisplayRectangle.width / 2, DisplayRectangle.height - epad)), fore, stroke);
            ra.uDraw.DrawLine(aof(new Point(DisplayRectangle.width / 2 + arrSq, DisplayRectangle.height - epad - arrSq)), aof(new Point(DisplayRectangle.width / 2, DisplayRectangle.height - epad)), fore, stroke);
        }

        Point aof(Point dp)
        {
            dp.X += DisplayRectangle.left;
            dp.Y += DisplayRectangle.top;
            return dp;
        }

        // GDI Support
        public virtual void Draw(System.Drawing.Graphics graphics)
        {
        }

        // Mousey
        bool movin = false;
        System.Drawing.Point deltaLoc;
        public override void MouseMove(System.Drawing.Point location, bool inComponent, bool amClipped)
        {
            // move event added in constructor
        }
        public void MoveMove(System.Drawing.Point location)
        {
            if (movin)
            {
                var usepos = System.Windows.Forms.Cursor.Position;
                int dx = usepos.X - deltaLoc.X;
                int dy = usepos.Y - deltaLoc.Y;
                deltaLoc = usepos;
                controlled.Location = new Point(controlled.Location.X + dx, controlled.Location.Y + dy);
            }
        }
        public override void MouseUpDown(System.Windows.Forms.MouseEventArgs mea, MouseButtonState mbs, bool inComponent, bool amClipped)
        {
            if (mbs == MouseButtonState.DOWN && inComponent)
            {
                deltaLoc = System.Windows.Forms.Cursor.Position;
                movin = true;
            }
            if (mbs == MouseButtonState.UP && movin)
                movin = false;
        }
    }
}
