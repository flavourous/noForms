﻿using System;
using System.Collections.Generic;
using System.Text;
using NoForms.Common;
using OTK = OpenTK;
using OpenTK.Graphics.OpenGL;

namespace NoForms.Renderers.OpenTK
{
    public class OpenTK_RenderElements : IRenderElements
    {
        public OpenTK_RenderElements(OTK.Graphics.IGraphicsContext gc, int fd,int td,int fw,int tw)
        {
            graphicsContext = gc;
            FBO_Draw = fd;
            FBO_Window = fw;
            T2D_Draw = td;
            T2D_Window = tw;
        }
        public OTK.Graphics.IGraphicsContext graphicsContext { get; internal set; }
        public int FBO_Draw { get; internal set; }
        public int T2D_Draw { get; internal set; }
        public int FBO_Window { get; internal set; }
        public int T2D_Window { get; internal set; }
    }
    public class OTKDraw : IUnifiedDraw
    {
        OpenTK_RenderElements renderer;
        public OTKDraw(OpenTK_RenderElements rel)
        {
            renderer = rel;
        }

        Stack<Rectangle> Clips = new Stack<Rectangle>();
        public void PushAxisAlignedClip(Rectangle clipRect, bool ignoreRenderOffset)
        {
            var cr = clipRect;
            OTK.Matrix4 cm = new OTK.Matrix4();
            GL.LoadMatrix(ref cm);
            if (ignoreRenderOffset) cr -= new Point(cm.M31, cm.M32);

            Clips.Push(cr);
            ClipStackViewport();
        }

        public void PopAxisAlignedClip()
        {
            Clips.Pop();
            ClipStackViewport();
        }

        void ClipStackViewport()
        {
            Rectangle fcr = new Rectangle(); bool started = false;
            foreach (var cr in Clips)
            {
                if (!started)
                {
                    started = true;
                    fcr = cr;
                }
                else
                {
                    var l = Math.Max(fcr.left, cr.left);
                    var t = Math.Max(fcr.top, cr.top);
                    var r = Math.Min(fcr.right, cr.right);
                    var b = Math.Min(fcr.bottom, cr.bottom);
                    fcr = new Rectangle(new Point(l, t), new Point(r, b), true);
                }
                if (fcr.width <= 0 || fcr.height <= 0)
                {
                    fcr = new Rectangle(0, 0, 0, 0);
                    break;
                }
            }
            // set up ortho
            GL.Viewport((int)fcr.left, (int)fcr.top, (int)fcr.width, (int)fcr.height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(fcr.left, fcr.top, fcr.width, fcr.height, 0.0, 1.0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        public void SetRenderOffset(Point renderOffset)
        {
            GL.Translate(new OTK.Vector3(renderOffset.X, renderOffset.Y, 0));
        }

        public void FillPath(UPath path, UBrush brush)
        {
            foreach (var f in path.figures)
            {
                GL.Begin(PrimitiveType.Polygon);
                setCoordColor(brush, f.startPoint.X, f.startPoint.Y);
                GL.Vertex2(f.startPoint.X, f.startPoint.Y);
                Point st = f.startPoint;
                foreach (var g in f.geoElements)
                    PlotGeoElement(ref st, g, brush);
                setCoordColor(brush, f.startPoint.X, f.startPoint.Y);
                GL.Vertex2(f.startPoint.X, f.startPoint.Y);
                GL.End();
            }
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
            System.Drawing.PointF[] pts = new System.Drawing.PointF[0];
            var lst = start;
            if (g is ULine)
            {
                var l = g as ULine;
                pts = new System.Drawing.PointF[] { new System.Drawing.PointF(l.endPoint.X, l.endPoint.Y) };
            }
            else if (g is UBeizer)
            {
                UBeizer bez = g as UBeizer;
                var p1 = new System.Drawing.PointF(start.X, start.Y);
                var p2 = new System.Drawing.PointF(bez.controlPoint1.X, bez.controlPoint1.Y);
                var p3 = new System.Drawing.PointF(bez.controlPoint2.X, bez.controlPoint2.Y);
                var p4 = new System.Drawing.PointF(bez.endPoint.X, bez.endPoint.Y);
                pts = (g.Retreive<OTKDraw>(() => new disParr(Common.Bezier.Get2D(p1, p2, p3, p4, bez.resolution))) as disParr).pts;
            }
            else if (g is UArc)
            {
                UArc arc = g as UArc;
                pts = (g.Retreive<OTKDraw>(() =>
                {
                    var elInput = new GeometryLib.Ellipse_Input(lst.X, lst.Y, arc.endPoint.X, arc.endPoint.Y, arc.arcSize.width, arc.arcSize.height, arc.rotation);
                    var elSolution = new List<GeometryLib.Ellipse_Output>(GeometryLib.Ellipse.Get_X0Y0(elInput)).ToArray();
                    GeometryLib.Ellipse.SampleArc(elInput, elSolution, arc.reflex, arc.sweepClockwise, arc.resolution, out pts);
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


            else throw new NotImplementedException();
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

        void LoadBitmapIntoTexture(int texture, byte[] data)
        {
            // Bind texture
            GL.BindTexture(TextureTarget.Texture2D, texture);

            // Load Data
            using (var ms = new System.IO.MemoryStream(data))
            {
                var sdb = new System.Drawing.Bitmap(ms);
                var sdbd = sdb.LockBits(new System.Drawing.Rectangle(0, 0, sdb.Width, sdb.Height), 
                    System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, sdbd.Width, sdbd.Height, 0,
                    PixelFormat.Bgra, PixelType.UnsignedByte, sdbd.Scan0);

                sdb.UnlockBits(sdbd);
            }

            // do the mimmapthing FIXME for older cards? :/ mmmm 
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            // Unbind Texture
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        class distex : IDisposable
        {
            public distex()
            {
                tex = GL.GenTexture();
            }
            public int tex { get; private set; }
            public void Dispose()
            {
                GL.DeleteTexture(tex);
            }
        }

        public void DrawBitmap(UBitmap bitmap, float opacity, UInterp interp, Rectangle source, Rectangle destination)
        {
            var rd = bitmap.Retreive<OTKDraw>(() =>
                {
                    var bd = bitmap.bitmapData ?? System.IO.File.ReadAllBytes(bitmap.bitmapFile);
                    var dt = new distex();
                    LoadBitmapIntoTexture(dt.tex, bd);
                    return dt;
                }) as distex;
            GL.BindTexture(TextureTarget.Texture2D, rd.tex);
            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(source.left, source.top);
            GL.Vertex2(destination.left, destination.top);
            GL.TexCoord2(source.right, source.top);
            GL.Vertex2(destination.right, destination.top);
            GL.TexCoord2(source.right, source.bottom);
            GL.Vertex2(destination.right, destination.bottom);
            GL.TexCoord2(source.left, source.bottom);
            GL.Vertex2(destination.left, destination.bottom);
            GL.End();
            GL.BindTexture(TextureTarget.Texture2D, 0); // default texture.
        }

        public void FillEllipse(Point center, float radiusX, float radiusY, UBrush brush)
        {
            GL.Begin(PrimitiveType.Polygon);
            foreach(var pt in GeometryLib.EllipseUtil.GetEllipse(radiusX, radiusX, .1f))
            {
                float x = pt.X + center.X;
                float y = pt.Y + center.Y;
                setCoordColor(brush,x,y);
                GL.Vertex2(x,y);
            }
            GL.End();
        }

        public void DrawEllipse(Point center, float radiusX, float radiusY, UBrush brush, UStroke stroke)
        {
            GL.Begin(PrimitiveType.LineLoop);
            foreach(var pt in GeometryLib.EllipseUtil.GetEllipse(radiusX, radiusX, .1f))
            {
                float x = pt.X + center.X;
                float y = pt.Y + center.Y;
                setCoordColor(brush, x, y);
                GL.Vertex2(x, y);
            }
            GL.End();
        }

        public void DrawLine(Point start, Point end, UBrush brush, UStroke stroke)
        {
            GL.Begin(PrimitiveType.Quads);
            setCoordColor(brush, start.X, start.Y);
            GL.Vertex2(start.X, start.Y);
            setCoordColor(brush, end.X, end.Y);
            GL.Vertex2(end.X, end.Y);
            GL.End();
        }

        public void DrawRectangle(Rectangle rect, UBrush brush, UStroke stroke)
        {
            GL.Begin(PrimitiveType.LineLoop);
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
            GL.Begin(PrimitiveType.LineLoop);
            RRPoints(rect, radX, radY, brush);
            GL.End();
        }
        public void FillRoundedRectangle(Rectangle rect, float radX, float radY, UBrush brush)
        {
            GL.Begin(PrimitiveType.Polygon);
            RRPoints(rect, radX, radY, brush);
            GL.End();
        }
        void RRPoints(Rectangle rect, float radX, float radY, UBrush b)
        {
            // top left arc
            ARCPoints(rect.left, rect.top + radY, 90f, 180f, radX, radY, b);
            // line to top right arc start
            setCoordColor(b, rect.right - radX, rect.top);
            GL.Vertex2(rect.right - radX, rect.top);
            //top right arc
            ARCPoints(rect.right - radX, rect.top, 90f, 0f, radX, radY, b);
            // line top bottom right arc
            setCoordColor(b, rect.right, rect.bottom - radY);
            GL.Vertex2(rect.right, rect.bottom - radY);
            //bottom right arc
            ARCPoints(rect.right, rect.bottom - radY, 0f, -90f, radX, radY, b);
            //line to bottom left arc
            setCoordColor(b, rect.left + radX, rect.bottom);
            GL.Vertex2(rect.left + radX, rect.bottom);
            //bottom left arc
            ARCPoints(rect.left + radX, rect.bottom, -90f, -180f, radX, radY, b);
            // line back to top left arc
            setCoordColor(b, rect.left, rect.top + radY);
            GL.Vertex2(rect.left, rect.top + radY);
        }
        void ARCPoints(float sx, float sy, float t1, float t2, float rx, float ry, UBrush b)
        {
            var e_tl = GeometryLib.EasyEllipse.Generate(new GeometryLib.EasyEllipse.EasyEllipseInput()
            {
                rx = rx,
                ry = ry,
                rotation = 0,
                resolution = .1f,
                start_x = sx,
                start_y = sy,
                t1 = t1,
                t2 = t2
            });
            foreach (var pt in e_tl)
            {
                setCoordColor(b, pt.X, pt.Y);
                GL.Vertex2(pt.X, pt.Y);
            }
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
