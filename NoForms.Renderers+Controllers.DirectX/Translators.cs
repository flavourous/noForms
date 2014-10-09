using System;

namespace NoForms.Common
{
    public class D2DTr
    {
        static public SharpDX.DrawingSizeF tr(Size s) { return new SharpDX.DrawingSizeF(s.width, s.height); }
        static public SharpDX.DrawingPointF tr(Point p) { return new SharpDX.DrawingPointF(p.X, p.Y); }
        static public SharpDX.DrawingRectangleF tr(Rectangle r) { return new SharpDX.DrawingRectangleF(r.left, r.top, r.width, r.height); }
        static public SharpDX.Color tr(Color c) { return new SharpDX.Color(c.r, c.g, c.b, c.a); }
    }
}
