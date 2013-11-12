using System;
using SharpDX.Direct2D1;
using SysRect = System.Drawing.Rectangle;

namespace NoForms.Controls
{
    public class MoveHandle : Templates.Containable
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
        public override void FocusChange(bool focus)
        {
        }

        public override void KeyPress(char c)
        {
        } 

        // Render methody
        public override void DrawBase(IRenderType renderArg)
        {
            if (renderArg is RenderTarget)
            {
                RenderTarget d2dtarget = renderArg as RenderTarget;
                Draw(d2dtarget);
            }
            else
            {
                throw new NotImplementedException("Render type " + renderArg.ToString() + " not supported");
            }
        }

        // Direct2D Support
        public virtual void Draw(RenderTarget d2dtarget)
        {
            d2dtarget.FillRectangle(DisplayRectangle, new SolidColorBrush(d2dtarget, new SharpDX.Color4(0f, 0f, 0f, 1f)));
            var scb = new SolidColorBrush(d2dtarget,new SharpDX.Color4(0.8f,0.8f,0.8f,1.0f));
            float epad = 3; float arrSq = 3; float thickness = 2f;
            d2dtarget.DrawLine(aof(new SharpDX.DrawingPointF(epad, DisplayRectangle.height / 2)), aof(new SharpDX.DrawingPointF(DisplayRectangle.width - epad, DisplayRectangle.height / 2)), scb, thickness);

            d2dtarget.DrawLine(aof(new SharpDX.DrawingPointF(epad, DisplayRectangle.height / 2)), aof(new SharpDX.DrawingPointF(epad + arrSq, DisplayRectangle.height / 2 + arrSq)), scb, thickness);
            d2dtarget.DrawLine(aof(new SharpDX.DrawingPointF(epad, DisplayRectangle.height / 2)), aof(new SharpDX.DrawingPointF(epad + arrSq, DisplayRectangle.height / 2 - arrSq)), scb, thickness);
            d2dtarget.DrawLine(aof(new SharpDX.DrawingPointF(DisplayRectangle.width - epad - arrSq, DisplayRectangle.height / 2 + arrSq)), aof(new SharpDX.DrawingPointF(DisplayRectangle.width - epad, DisplayRectangle.height / 2)), scb, thickness);
            d2dtarget.DrawLine(aof(new SharpDX.DrawingPointF(DisplayRectangle.width - epad - arrSq, DisplayRectangle.height / 2 - arrSq)), aof(new SharpDX.DrawingPointF(DisplayRectangle.width - epad, DisplayRectangle.height / 2)), scb, thickness);

            d2dtarget.DrawLine(aof(new SharpDX.DrawingPointF(DisplayRectangle.width / 2, epad)), aof(new SharpDX.DrawingPointF(DisplayRectangle.width / 2, DisplayRectangle.height - epad)), scb, thickness);

            d2dtarget.DrawLine(aof(new SharpDX.DrawingPointF(DisplayRectangle.width / 2, epad)), aof(new SharpDX.DrawingPointF(DisplayRectangle.width / 2 - arrSq, epad + arrSq)), scb, thickness);
            d2dtarget.DrawLine(aof(new SharpDX.DrawingPointF(DisplayRectangle.width / 2, epad)), aof(new SharpDX.DrawingPointF(DisplayRectangle.width / 2 + arrSq, epad + arrSq)), scb, thickness);
            d2dtarget.DrawLine(aof(new SharpDX.DrawingPointF(DisplayRectangle.width / 2 - arrSq, DisplayRectangle.height - epad - arrSq)), aof(new SharpDX.DrawingPointF(DisplayRectangle.width / 2, DisplayRectangle.height - epad)), scb, thickness);
            d2dtarget.DrawLine(aof(new SharpDX.DrawingPointF(DisplayRectangle.width / 2 + arrSq, DisplayRectangle.height - epad - arrSq)), aof(new SharpDX.DrawingPointF(DisplayRectangle.width / 2, DisplayRectangle.height - epad)), scb, thickness);
            scb.Dispose();
        }
        SharpDX.DrawingPointF aof(SharpDX.DrawingPointF dp)
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
                int dx = location.X - deltaLoc.X;
                int dy = location.Y - deltaLoc.Y;
                deltaLoc = location;
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
