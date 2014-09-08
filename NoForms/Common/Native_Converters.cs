using System;
using System.Collections.Generic;
using System.Text;
using c = System.Windows.Forms.Cursors;
using NoForms.Common;

namespace NoForms
{
    class SDGTr
    {
        static public System.Drawing.SizeF trF(Size s) { return new System.Drawing.SizeF(s.width, s.height); }
        static public System.Drawing.Size trI(Size s) { return new System.Drawing.Size((int)s.width, (int)s.height); }
        static public System.Drawing.PointF trF(Point p) { return new System.Drawing.PointF(p.X, p.Y); }
        static public System.Drawing.Point trI(Point p) { return new System.Drawing.Point((int)p.X, (int)p.Y); }
        static public System.Drawing.RectangleF trF(Rectangle r) { return new System.Drawing.RectangleF(r.left, r.top, r.width, r.height); }
        static public System.Drawing.Color tr(Color c) 
        {
            byte a = (byte)Math.Round(c.a * 255);
            byte r = (byte)Math.Round(c.r * 255);
            byte g = (byte)Math.Round(c.g * 255);
            byte b = (byte)Math.Round(c.b * 255);
            return System.Drawing.Color.FromArgb(a, r, g, b);
        }
        static public Point tr(System.Drawing.Point p) { return new Point(p.X, p.Y); }
        static public Size tr(System.Drawing.Size s) { return new Size(s.Width, s.Height); }
        static public Size tr(System.Drawing.SizeF s) { return new Size(s.Width, s.Height); }
    }
    class D2DTr
    {
        static public SharpDX.DrawingSizeF tr(Size s) { return new SharpDX.DrawingSizeF(s.width, s.height); }
        static public SharpDX.DrawingPointF tr(Point p) { return new SharpDX.DrawingPointF(p.X, p.Y); }
        static public SharpDX.DrawingRectangleF tr(Rectangle r) { return new SharpDX.DrawingRectangleF(r.left, r.top, r.width, r.height); }
        static public SharpDX.Color tr(Color c) { return new SharpDX.Color(c.r, c.g, c.b, c.a); }
    }
    class WFTr
    {
        public static System.Windows.Forms.Cursor Translate(Cursors cur)
        {
            switch (cur)
            {
                case Cursors.AppStarting: return c.AppStarting;
                case Cursors.Arrow: return c.Arrow;
                case Cursors.Cross: return c.Cross;
                default:
                case Cursors.Default: return c.Default;
                case Cursors.Hand: return c.Hand;
                case Cursors.Help: return c.Help;
                case Cursors.HSplit: return c.HSplit;
                case Cursors.IBeam: return c.IBeam;
                case Cursors.No: return c.No;
                case Cursors.NoMove2D: return c.NoMove2D;
                case Cursors.NoMoveHoriz: return c.NoMoveHoriz;
                case Cursors.NoMoveVert: return c.NoMoveVert;
                case Cursors.PanEast: return c.PanEast;
                case Cursors.PanNE: return c.PanNE;
                case Cursors.PanNorth: return c.PanNorth;
                case Cursors.PanNW: return c.PanNW;
                case Cursors.PanSE: return c.PanSE;
                case Cursors.PanSouth: return c.PanSouth;
                case Cursors.PanSW: return c.PanSW;
                case Cursors.PanWest: return c.PanWest;
                case Cursors.SizeAll: return c.SizeAll;
                case Cursors.SizeNESW: return c.SizeNESW;
                case Cursors.SizeNS: return c.SizeNS;
                case Cursors.SizeNWSE: return c.SizeNWSE;
                case Cursors.SizeWE: return c.SizeWE;
                case Cursors.UpArrow: return c.UpArrow;
                case Cursors.VSplit: return c.VSplit;
                case Cursors.WaitCursor: return c.WaitCursor;
            }
        }
        public static Cursors Translate(System.Windows.Forms.Cursor cur)
        {
            if (cur == c.AppStarting) return Cursors.AppStarting;
            if (cur == c.Arrow) return Cursors.Arrow;
            if (cur == c.Cross) return Cursors.Cross;
            if (cur == c.Default) return Cursors.Default;
            if (cur == c.Hand) return Cursors.Hand;
            if (cur == c.Help) return Cursors.Help;
            if (cur == c.HSplit) return Cursors.HSplit;
            if (cur == c.IBeam) return Cursors.IBeam;
            if (cur == c.No) return Cursors.No;
            if (cur == c.NoMove2D) return Cursors.NoMove2D;
            if (cur == c.NoMoveHoriz) return Cursors.NoMoveHoriz;
            if (cur == c.NoMoveVert) return Cursors.NoMoveVert;
            if (cur == c.PanEast) return Cursors.PanEast;
            if (cur == c.PanNE) return Cursors.PanNE;
            if (cur == c.PanNorth) return Cursors.PanNorth;
            if (cur == c.PanNW) return Cursors.PanNW;
            if (cur == c.PanSE) return Cursors.PanSE;
            if (cur == c.PanSouth) return Cursors.PanSouth;
            if (cur == c.PanSW) return Cursors.PanSW;
            if (cur == c.PanWest) return Cursors.PanWest;
            if (cur == c.SizeAll) return Cursors.SizeAll;
            if (cur == c.SizeNESW) return Cursors.SizeNESW;
            if (cur == c.SizeNS) return Cursors.SizeNS;
            if (cur == c.SizeNWSE) return Cursors.SizeNWSE;
            if (cur == c.SizeWE) return Cursors.SizeWE;
            if (cur == c.UpArrow) return Cursors.UpArrow;
            if (cur == c.VSplit) return Cursors.VSplit;
            if (cur == c.WaitCursor) return Cursors.WaitCursor;
            return Cursors.Default;
        }
        public static MouseButton Translate(System.Windows.Forms.MouseButtons mb)
        {
            // FIXME where da buttons?
            switch (mb)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    return MouseButton.LEFT;
                case System.Windows.Forms.MouseButtons.Middle:
                    break;
                case System.Windows.Forms.MouseButtons.None:
                    break;
                case System.Windows.Forms.MouseButtons.Right:
                    return MouseButton.RIGHT;
                case System.Windows.Forms.MouseButtons.XButton1:
                    break;
                case System.Windows.Forms.MouseButtons.XButton2:
                    break;
                default:
                    return MouseButton.NONE;
            }
            return MouseButton.NONE;
        }
    }
}
