using System;
using NoForms;
using NoForms.Renderers;
using NoForms.ComponentBase;
using Common;

namespace NoFormsSDK
{
    public class MoveHandle : BasicContainer
    {
        NoForm controlled;
        public MoveHandle(NoForm MoveControl)
        {
            controlled = MoveControl;
            Cursor = Common.Cursors.SizeAll;
            controlled.controller.MouseMove += MoveMove;
        }

        USolidBrush fore = new USolidBrush() { color = new Color(1.0f, 0.8f, 0.8f, 0.8f) };
        USolidBrush back = new USolidBrush() { color = new Color(1.0f, 0f, 0f, 0f) };
        UStroke stroke = new UStroke() { strokeWidth = 2f };
        // Render methody
        public override void Draw(IDraw ra)
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

        // Mousey
        bool movin = false;
        Point deltaLoc;
        public void MoveMove(Point location)
        {
            if (movin)
            {
                var usepos = controlled.controller.MouseScreenLocation;
                float dx = usepos.X - deltaLoc.X;
                float dy = usepos.Y - deltaLoc.Y;
                deltaLoc = usepos;
                controlled.Location = new Point(controlled.Location.X + dx, controlled.Location.Y + dy);
            }
        }
        public override void MouseUpDown(Point location, MouseButton mb, ButtonState mbs, bool inComponent, bool amClipped)
        {
            if (mbs == ButtonState.DOWN && inComponent && !amClipped && Util.AmITopZOrder(this,location))
            {
                deltaLoc = controlled.controller.MouseScreenLocation;
                controlled.window.CaptureMouse = true;
                movin = true;
            }
            if (mbs == ButtonState.UP && movin)
                movin = false;
        }
    }
}
