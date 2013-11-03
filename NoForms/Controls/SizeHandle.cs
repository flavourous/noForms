using System;
using SharpDX.Direct2D1;
using SysRect = System.Drawing.Rectangle;

namespace NoForms.Controls
{
    public class SizeHandle : Templates.Containable
    {
        NoForm controlled;
        public SizeHandle(NoForm MoveControl)
        {
            controlled = MoveControl;
            MoveControl.MouseMoved+= new NoForm.MouseMoveEventHandler(ResizeMove);
        }

        public override void KeyPress(char c)
        {
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

        // Render methody
        public override void DrawBase<RenderType>(RenderType renderArg)
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
            float epad = 3; float thickness = 2f;
            d2dtarget.DrawLine(aof(new SharpDX.DrawingPointF(1*DisplayRectangle.width / 5, DisplayRectangle.height - epad)), aof(new SharpDX.DrawingPointF(DisplayRectangle.width - epad, 1*DisplayRectangle.height / 5)), scb, thickness);
            d2dtarget.DrawLine(aof(new SharpDX.DrawingPointF(2*DisplayRectangle.width / 5, DisplayRectangle.height - epad)), aof(new SharpDX.DrawingPointF(DisplayRectangle.width - epad, 2*DisplayRectangle.height / 5)), scb, thickness);
            d2dtarget.DrawLine(aof(new SharpDX.DrawingPointF(3*DisplayRectangle.width / 5, DisplayRectangle.height - epad)), aof(new SharpDX.DrawingPointF(DisplayRectangle.width - epad, 3*DisplayRectangle.height / 5)), scb, thickness);
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
        bool sizin = false;
        System.Drawing.Point deltaLoc;
        public override void MouseMove(System.Drawing.Point location, bool inComponent, bool amClipped)
        {
            
        }
        Point defosit;

        public void TestResize(int dx, int dy)
        {
            sizin = true;
            defosit = new System.Drawing.Point(0, 0);
            deltaLoc = System.Windows.Forms.Cursor.Position;
            sizin = true;
            controlled.theForm.Capture = true;
            ResizeMove(new System.Drawing.Point(deltaLoc.X + dx, deltaLoc.Y+dy));
            controlled.theForm.Capture = false;
            sizin = false;
        }

        public void ResizeMove(System.Drawing.Point location)
        {
            if (sizin)
            {
                int dx = location.X - deltaLoc.X;
                int dy = location.Y - deltaLoc.Y;
                deltaLoc = location;

                if ((dx < 0 && defosit.X > 0) || (dx > 0 && defosit.X < 0))
                {
                    // Process X defosit
                    bool defX = defosit.X > 0;
                    defosit.X += dx;
                    dx = 0;
                    if (defX != defosit.X > 0)
                    {
                        dx = (int)defosit.X;
                        defosit.X = 0;
                    }
                }
                if ((dy < 0 && defosit.Y > 0) || (dy > 0 && defosit.Y < 0))
                {
                    // Process Y defosit
                    bool defY = defosit.Y > 0;
                    defosit.Y += dy;
                    dy = 0;
                    if (defY != defosit.Y > 0)
                    {
                        dy = (int)defosit.Y;
                        defosit.Y = 0;
                    }
                }

                float newx = controlled.Size.width + (float)dx;
                float newy = controlled.Size.height + (float)dy;
                controlled.Size = new Size(newx,newy);

                if (newx != controlled.Size.width)
                {
                    // Oh, we need to add to the x defosit
                    defosit.X += newx - controlled.Size.width;
                }
                if (newy != controlled.Size.height)
                {
                    // Oh, we need to add to the y defosit
                    defosit.Y += newy - controlled.Size.height;
                }
            }
        }
        public override void MouseUpDown(System.Windows.Forms.MouseEventArgs mea, MouseButtonState mbs, bool inComponent, bool amClipped)
        {
            if (mbs == MouseButtonState.DOWN && inComponent)
            {
                defosit = new System.Drawing.Point(0, 0);
                deltaLoc = System.Windows.Forms.Cursor.Position;
                sizin = true;
                controlled.theForm.Capture = true;
            }
            if (mbs == MouseButtonState.UP && sizin)
                sizin = false;
        }
        public void MouseUpDownGlob(MouseButtonState mbs)
        {
            
        }
    }
}
