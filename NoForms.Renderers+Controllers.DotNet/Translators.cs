using System;

namespace NoForms.Common
{
    public class SDGTr
    {
        static public System.Drawing.SizeF trF(Size s) { return new System.Drawing.SizeF(s.width, s.height); }
        static public System.Drawing.Size trI(Size s) { return new System.Drawing.Size((int)s.width, (int)s.height); }
        static public System.Drawing.PointF trF(Point p) { return new System.Drawing.PointF(p.X, p.Y); }
        static public System.Drawing.Point trI(Point p) { return new System.Drawing.Point((int)p.X, (int)p.Y); }
        static public System.Drawing.RectangleF trF(Rectangle r) { return new System.Drawing.RectangleF(r.left, r.top, r.width, r.height); }
        static public System.Drawing.Rectangle trI(Rectangle r) { return new System.Drawing.Rectangle((int)r.left, (int)r.top, (int)r.width, (int)r.height); }
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
}
