using System;
using SharpDX.Direct2D1;
using NoForms.Renderers;


namespace NoForms.Controls
{
    public class SizeHandle : Abstract.BasicContainer
    {
        NoForm controlled;
        public SizeHandle(NoForm MoveControl)
        {
            controlled = MoveControl;
            MoveControl.MouseMoved += new NoForm.MouseMoveEventHandler(ResizeMove);
        }

        // Render methody
        public UBrush background = new USolidBrush() { color = new Color(0) };
        public UBrush foreground = new USolidBrush() { color = new Color(1) };
        public UStroke lineStroke = new UStroke() { strokeWidth = 2f };
        public override void Draw(IRenderType renderArg)
        {
            renderArg.uDraw.FillRectangle(DisplayRectangle, background);
            float epad = 3;
            renderArg.uDraw.DrawLine(aof(new Point(1 * DisplayRectangle.width / 5, DisplayRectangle.height - epad)), aof(new Point(DisplayRectangle.width - epad, 1 * DisplayRectangle.height / 5)), foreground, lineStroke);
            renderArg.uDraw.DrawLine(aof(new Point(2 * DisplayRectangle.width / 5, DisplayRectangle.height - epad)), aof(new Point(DisplayRectangle.width - epad, 2 * DisplayRectangle.height / 5)), foreground, lineStroke);
            renderArg.uDraw.DrawLine(aof(new Point(3 * DisplayRectangle.width / 5, DisplayRectangle.height - epad)), aof(new Point(DisplayRectangle.width - epad, 3 * DisplayRectangle.height / 5)), foreground, lineStroke);
        }

        Point aof(Point dp)
        {
            dp.X += DisplayRectangle.left;
            dp.Y += DisplayRectangle.top;
            return dp;
        }
        
        // Mousey
        bool sizin = false;
        System.Drawing.Point deltaLoc;
        public override void MouseMove(System.Drawing.Point location, bool inComponent, bool amClipped)
        {
            
        }
        Point defosit;

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
                deltaLoc = mea.Location;
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
