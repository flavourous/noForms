using System;
using NoForms.ComponentBase;
using NoForms;
using NoForms.Renderers;
using NoForms.Common;

namespace NoFormsSDK
{
    public class SizeHandle : Container
    {
        NoForm controlled;
        public Direction ResizeMode = Direction.NONE;
        public SizeHandle(NoForm MoveControl)
        {
            controlled = MoveControl;
            MoveControl.controller.MouseMove += ResizeMove;
        }

        // Render methody
        public UBrush background = new USolidBrush() { color = new Color(0) };
        public UBrush foreground = new USolidBrush() { color = new Color(1) };
        public UStroke lineStroke = new UStroke() { strokeWidth = 2f };
        public override void Draw(IDraw renderArg, Region dirty)
        {
            if (invisible) return;
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
        Point deltaLoc;
        Point defosit;
        public void ResizeMove(Point location)
        {
            Point loc = controlled.controller.MouseScreenLocation;
            if (sizin)
            {
                float dx = loc.X - deltaLoc.X;
                float dy = loc.Y - deltaLoc.Y;
                deltaLoc = loc;

                switch (ResizeMode)
                {
                    case Direction.NORTH:
                        dy = -dy; dx=0;
                        break;
                    case Direction.SOUTH:
                        dx = 0;
                        break;
                    case Direction.EAST:
                        dy = 0;
                        break;
                    case Direction.WEST:
                        dx = -dx; dy=0;
                        break;
                    case Direction.NORTH | Direction.EAST:
                        dy = -dy;
                        break;
                    case Direction.NORTH | Direction.WEST:
                        dy = -dy; dx=-dx;
                        break;
                    case Direction.SOUTH | Direction.EAST:
                        break;
                    case Direction.SOUTH | Direction.WEST:
                        dx = -dx;
                        break;
                    default:
                        dx = dy = 0;
                        break;
                }

                if ((dx < 0 && defosit.X > 0) || (dx > 0 && defosit.X < 0))
                {
                    // Process X defosit
                    bool defX = defosit.X > 0;
                    defosit.X += dx;
                    dx = 0;
                    if (defX != defosit.X > 0)
                    {
                        dx = defosit.X;
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
                        dy = defosit.Y;
                        defosit.Y = 0;
                    }
                }

                float neww = controlled.ReqSize.width + dx;
                float newh = controlled.ReqSize.height + dy;

                if (neww > controlled.MaxSize.width)
                {
                    defosit.X += neww - controlled.MaxSize.width;
                    neww = controlled.MaxSize.width;
                }
                if (neww < controlled.MinSize.width)
                {
                    defosit.X += neww - controlled.MinSize.width;
                    neww = controlled.MinSize.width;
                }
                if (newh > controlled.MaxSize.height)
                {
                    defosit.Y += newh - controlled.MaxSize.height;
                    newh = controlled.MaxSize.height;
                }
                if (newh < controlled.MinSize.height)
                {
                    defosit.Y += newh - controlled.MinSize.height;
                    newh = controlled.MinSize.height;
                }

                float newx = (ResizeMode & Direction.WEST) == Direction.WEST ? controlled.Location.X - (neww - controlled.ReqSize.width) : controlled.Location.X;
                float newy = (ResizeMode & Direction.NORTH) == Direction.NORTH ? controlled.Location.Y - (newh - controlled.ReqSize.height) : controlled.Location.Y;

                controlled.Location = new Point((int)newx, (int)newy);
                controlled.Size = new Size((int)neww, (int)newh);
            }
        }
        public override void MouseUpDown(Point location, MouseButton mb, ButtonState mbs, bool inComponent, bool amClipped)
        {
            if (mbs == ButtonState.DOWN && inComponent && !amClipped && IComponent_Util.AmITopZOrder(this, location))
            {
                defosit = new Point(0, 0);
                deltaLoc = controlled.controller.MouseScreenLocation;
                sizin = true;
                controlled.window.CaptureMouse = true;
            }
            if (mbs == ButtonState.UP && sizin)
                sizin = false;
        }

        bool invisible = false;
        public static void AddEdgeResize(NoForm target, int zIndex, float thickness, float cornerSize)
        {
            SizeHandle n, s, e, w, ne, nw, se, sw;
            n = new SizeHandle(target) { ZIndex = zIndex, ResizeMode = Direction.NORTH, invisible=true};
            s = new SizeHandle(target) { ZIndex = zIndex, ResizeMode = Direction.SOUTH, invisible = true };
            e = new SizeHandle(target) { ZIndex = zIndex, ResizeMode = Direction.EAST, invisible = true };
            w = new SizeHandle(target) { ZIndex = zIndex, ResizeMode = Direction.WEST, invisible = true };
            ne = new SizeHandle(target) { ZIndex = zIndex + 1, ResizeMode = Direction.NORTH | Direction.EAST, invisible = true };
            nw = new SizeHandle(target) { ZIndex = zIndex + 1, ResizeMode = Direction.NORTH | Direction.WEST, invisible = true };
            se = new SizeHandle(target) { ZIndex = zIndex + 1, ResizeMode = Direction.SOUTH | Direction.EAST, invisible = true };
            sw = new SizeHandle(target) { ZIndex = zIndex + 1, ResizeMode = Direction.SOUTH | Direction.WEST, invisible = true };

            // cursors
            s.Cursor = n.Cursor = NoForms.Common.Cursors.SizeNS;
            e.Cursor = w.Cursor = NoForms.Common.Cursors.SizeWE;
            ne.Cursor = sw.Cursor = NoForms.Common.Cursors.SizeNESW;
            se.Cursor = nw.Cursor = NoForms.Common.Cursors.SizeNWSE;

            Action<Size> sc = null;
            sc = new Action<Size>(size =>
            {
                float wd = size.width;
                float hi = size.height;
                float th = thickness;
                float cs = cornerSize;
                n.DisplayRectangle = new Rectangle(0, 0, wd, th);
                s.DisplayRectangle = new Rectangle(0, hi - th, wd, th);
                e.DisplayRectangle = new Rectangle(wd - th, 0, th, hi);
                w.DisplayRectangle = new Rectangle(0, 0, th, hi);
                ne.DisplayRectangle = new Rectangle(wd - cs, 0, cs, cs);
                nw.DisplayRectangle = new Rectangle(0, 0, cs, cs);
                se.DisplayRectangle = new Rectangle(wd - cs, hi - cs, cs, cs);
                sw.DisplayRectangle = new Rectangle(0, hi - cs, cs, cs);
            });
            target.SizeChanged += sc;
            sc(target.Size);
            target.components.Add(n);
            target.components.Add(s);
            target.components.Add(e);
            target.components.Add(w);
            target.components.Add(ne);
            target.components.Add(nw);
            target.components.Add(se);
            target.components.Add(sw);
        }
    }
}
