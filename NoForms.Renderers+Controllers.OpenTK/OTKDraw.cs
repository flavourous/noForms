using System;
using System.Collections.Generic;
using System.Text;
using NoForms.Common;
using OTK = OpenTK;
using OpenTK.Graphics.OpenGL;

namespace NoForms.Renderers.OpenTK
{
    public class OpenTK_RenderElements : IRenderElements
    {
        public OpenTK_RenderElements(OTK.Graphics.IGraphicsContext gc)
        {
            graphicsContext = gc;
        }
        public OTK.Graphics.IGraphicsContext graphicsContext { get; internal set; }
    }
    public class OTKDraw : IUnifiedDraw
    {
        public void PushAxisAlignedClip(Rectangle clipRect, bool ignoreRenderOffset)
        {
            //throw new NotImplementedException();
        }

        public void PopAxisAlignedClip()
        {
            //throw new NotImplementedException();
        }

        public void SetRenderOffset(Point renderOffset)
        {
            throw new NotImplementedException();
        }

        public void FillPath(UPath path, UBrush brush)
        {
            throw new NotImplementedException();
        }

        public void DrawPath(UPath path, UBrush brush, UStroke stroke)
        {
            foreach (var f in path.figures)
            {
                GL.Begin(PrimitiveType.Lines);
                setCoordColor(brush, f.startPoint.X, f.startPoint.Y);
                GL.Vertex2(f.startPoint.X, f.startPoint.Y);
                Point st = f.startPoint;
                foreach (var g in f.geoElements)
                    PlotGeoElement(ref st, g, brush);
                if (!f.open)
                {
                    setCoordColor(brush, f.startPoint.X, f.startPoint.Y);
                    GL.Vertex2(f.startPoint.X, f.startPoint.Y);
                }
                GL.End();
            }
        }

        
        void PlotGeoElement(ref Point start, UGeometryBase g, UBrush b)
        {
            if (g is ULine)
            {
                var l = g as ULine;
                setCoordColor(b, l.endPoint.X, l.endPoint.Y);
                GL.Vertex2(l.endPoint.X, l.endPoint.Y); // end last line here
                GL.Vertex2(l.endPoint.X, l.endPoint.Y); // begin new line here possibly
                start = l.endPoint;
            }
            else if (g is UBeizer)
            {
            }
            else if (g is UArcBase)
            {
                System.Drawing.PointF[] pts = new System.Drawing.PointF[0];
                var lst = start;

                if (g is UArc)
                {
                    UArc arc = g as UArc;
                    pts = (g.Retreive<OTKDraw>(() =>
                    {
                        var elInput = new EllipseLib.Ellipse_Input(lst.X, lst.Y, arc.endPoint.X, arc.endPoint.Y, arc.arcSize.width, arc.arcSize.height, arc.rotation);
                        var elSolution = new List<EllipseLib.Ellipse_Output>(EllipseLib.Ellipse.Get_X0Y0(elInput)).ToArray();
                        EllipseLib.Ellipse.SampleArc(elInput, elSolution, arc.reflex, arc.sweepClockwise, arc.resolution, out pts);
                        return new disParr(pts);
                    }) as disParr).pts;
                }
                //else if (g is UEasyArc)
                //{
                //    var arc = g as UEasyArc;
                //    pts = (g.Retreive<OTKDraw>(() =>
                //        {
                //            return new disParr(new List<System.Drawing.PointF>(EllipseLib.EasyEllipse.Generate(new EllipseLib.EasyEllipse.EasyEllipseInput()
                //                                {
                //                                    rotation = arc.rotation,
                //                                    start_x = lst.X,
                //                                    start_y = lst.Y,
                //                                    rx = arc.arcSize.width,
                //                                    ry = arc.arcSize.height,
                //                                    t1 = arc.startAngle,
                //                                    t2 = arc.endAngle,
                //                                    resolution = arc.resolution
                //                                })).ToArray());
                //        }) as disParr).pts;
                //}

                Point? sp = null;
                foreach (var p in pts)
                {
                    if (sp == null) sp = new Point(p.X, p.Y);
                    setCoordColor(b, p.X, p.Y);
                    GL.Vertex2(p.X, p.Y); // end last line here
                    GL.Vertex2(p.X, p.Y); // begin new line here possibly
                }
                if (sp != null) start = sp.Value;
            }
            else throw new NotImplementedException();
        }

        public void DrawBitmap(UBitmap bitmap, float opacity, UInterp interp, Rectangle source, Rectangle destination)
        {
            throw new NotImplementedException();
        }

        public void FillEllipse(Point center, float radiusX, float radiusY, UBrush brush)
        {
            throw new NotImplementedException();
        }

        public void DrawEllipse(Point center, float radiusX, float radiusY, UBrush brush, UStroke stroke)
        {
            throw new NotImplementedException();
        }

        public void DrawLine(Point start, Point end, UBrush brush, UStroke stroke)
        {
            //throw new NotImplementedException();
        }

        public void DrawRectangle(Rectangle rect, UBrush brush, UStroke stroke)
        {
            throw new NotImplementedException();
        }

        public void FillRectangle(Rectangle rect, UBrush brush)
        {
            GL.Begin(PrimitiveType.Quads);
            setCoordColor(brush, rect.left, rect.top);
            GL.Vertex2(rect.left, rect.top);
            setCoordColor(brush, rect.right, rect.top);
            GL.Vertex2(rect.right, rect.top);
            setCoordColor(brush, rect.right, rect.bottom);
            GL.Vertex2(rect.right, rect.bottom);
            setCoordColor(brush, rect.left, rect.bottom);
            GL.Vertex2(rect.left, rect.bottom);
            GL.End();
        }

        void setCoordColor(UBrush b, float x, float y)
        {
            if (b is USolidBrush)
            {
                var usb = b as USolidBrush;
                GL.Color4(usb.color.r, usb.color.g, usb.color.b, usb.color.a);
            }
            else if (b is ULinearGradientBrush)
            {
                var ulgb = b as ULinearGradientBrush;
                float axisAngle = pointsanglex(ulgb.point1, ulgb.point2);

                float cp1, cp2;
                Color cv1, cv2;
                cp1 = angleaxisxinterp(axisAngle, ulgb.point1.X, ulgb.point1.Y);
                cp2 = angleaxisxinterp(axisAngle, ulgb.point2.X, ulgb.point2.Y);
                cv1 = ulgb.color1; cv2 = ulgb.color2;
                if (cp1 > cp2)
                {
                    cp1 = angleaxisxinterp(axisAngle, ulgb.point2.X, ulgb.point2.Y);
                    cp2 = angleaxisxinterp(axisAngle, ulgb.point1.X, ulgb.point1.Y);
                    cv1 = ulgb.color2; cv2 = ulgb.color1;
                }
                hcolorfour(x, y, axisAngle, cp1, cp2, cv1, cv2);
            }
        }
        void hcolorfour(float x, float y, float a, float cp1, float cp2, Color cv1, Color cv2)
        {
            float iv = onedinterp(angleaxisxinterp(a,x, y), cp1, cp2);
            GL.Color4(
                    cv1.r * (1 - iv) + cv2.r * iv,
                    cv1.g * (1 - iv) + cv2.g * iv,
                    cv1.b * (1 - iv) + cv2.b * iv,
                    cv1.a * (1 - iv) + cv2.a * iv);
        }
        // The angle that p1 to p2 makes with the x axis
        float pointsanglex(Point p1, Point p2)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            float dh = (float)Math.Sqrt(dx * dx + dy * dy);
            return (float)Math.Acos(dx / dh);
        }
        // transforms x,y point into distance along axis at angle with xaxis
        float angleaxisxinterp(float a, float x, float y)
        {
            return (float)(Math.Cos(a) * x + Math.Sin(a) * y);
        }
        // retuns [0,1] for distance of x between x1 and x2, clamped.
        float onedinterp(float x, float x1, float x2)
        {
            return x < x1 ? 0 : x > x2 ? 1 : (x - x1) / (x2 - x1);
        }

        public void DrawRoundedRectangle(Rectangle rect, float radX, float radY, UBrush brush, UStroke stroke)
        {
            throw new NotImplementedException();
        }

        public void FillRoundedRectangle(Rectangle rect, float radX, float radY, UBrush brush)
        {
            throw new NotImplementedException();
        }

        public void DrawText(UText textObject, Point location, UBrush defBrush, UTextDrawOptions opt, bool clientRendering)
        {
            throw new NotImplementedException();
        }

        public UTextHitInfo HitPoint(Point hitPoint, UText text)
        {
            throw new NotImplementedException();
        }

        public Point HitText(int pos, bool trailing, UText text)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Rectangle> HitTextRange(int start, int length, Point offset, UText text)
        {
            throw new NotImplementedException();
        }

        public UTextInfo GetTextInfo(UText text)
        {
            throw new NotImplementedException();
        }
    }
}
