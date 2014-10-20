using System;
using NoForms;
using NoForms.Common;
using GlyphRunLib;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace NoForms.Renderers.DotNet
{
    public class SDG_RenderElements : IRenderElements
    {
        public SDG_RenderElements(System.Drawing.Graphics gr)
        {
            graphics = gr;
        }
        public System.Drawing.Graphics graphics { get; internal set; }
    }
 

    /// <summary>
    /// SDG stands for system.drawing.graphics
    /// </summary>
    public class SDGDraw : IUnifiedDraw
    {
        SDG_RenderElements realRenderer;
        GlyphRun<Font> glyphRunner;
        public SDGDraw(SDG_RenderElements els)
        {
            realRenderer = els;
            glyphRunner = new GlyphRun<Font>(
                (s, f) => SDGTr.tr(realRenderer.graphics.MeasureString(s, f, PointF.Empty, StringFormat.GenericTypographic)), // measurer
                uf => Translate(uf) // font translator (from ufont to the template)
            );
        }
        Stack<System.Drawing.Region> PushedClips = new Stack<System.Drawing.Region>();
        public void PushAxisAlignedClip(NoForms.Common.Rectangle clipRect, bool ignoreRenderOffset)
        {
            // This probabbly pushes an empty clip...
            if (PushedClips.Count == 0) PushedClips.Push(realRenderer.graphics.Clip);

            var cr = clipRect;
            if (ignoreRenderOffset) cr -= new NoForms.Common.Point(realRenderer.graphics.Transform.OffsetX, realRenderer.graphics.Transform.OffsetY);
            var cr2 = new System.Drawing.Region(SDGTr.trF(clipRect));
            realRenderer.graphics.Clip = cr2;
            PushedClips.Push(cr2);
        }
        public void PopAxisAlignedClip()
        {
            PushedClips.Pop();
            realRenderer.graphics.Clip = PushedClips.Peek();
        }
        public void SetRenderOffset(NoForms.Common.Point offset)
        {
            var ox = realRenderer.graphics.Transform.OffsetX;
            var oy = realRenderer.graphics.Transform.OffsetY;
            realRenderer.graphics.Transform.Translate(offset.X - ox, offset.Y - oy);
        }
        public void FillPath(UPath path, UBrush brush)
        {
            realRenderer.graphics.FillPath(CreateBrush(brush), CreatePath(path));
        }
        public void DrawPath(UPath path, UBrush brush, UStroke stroke)
        {
            realRenderer.graphics.DrawPath(CreatePen(brush,stroke), CreatePath(path));
        }
        public void DrawBitmap(UBitmap bitmap, float opacity, UInterp interp, NoForms.Common.Rectangle source, NoForms.Common.Rectangle destination)
        {
            var bm = CreateBitmap(bitmap);
            if(opacity<1.0f)
                for(int x =0;x<bm.Width;x++)
                    for(int y =0;y< bm.Height;y++)
                        bm.SetPixel(x,y,System.Drawing.Color.FromArgb((int)(opacity*255), bm.GetPixel(x,y)));

            var previousInterp = realRenderer.graphics.CompositingQuality;
            realRenderer.graphics.CompositingQuality = Translate(interp);
            realRenderer.graphics.DrawImage(bm, SDGTr.trF(destination), SDGTr.trF(source), GraphicsUnit.Pixel);
            realRenderer.graphics.CompositingQuality = previousInterp;
        }
        public void FillEllipse(NoForms.Common.Point center, float radiusX, float radiusY, UBrush brush)
        {
            realRenderer.graphics.FillEllipse(CreateBrush(brush), center.X - radiusX, center.Y - radiusY, radiusX * 2, radiusY * 2);
        }
        public void DrawEllipse(NoForms.Common.Point center, float radiusX, float radiusY, UBrush brush, UStroke stroke)
        {
            realRenderer.graphics.DrawEllipse(CreatePen(brush, stroke), center.X - radiusX, center.Y - radiusY, radiusX * 2, radiusY * 2);
        }
        public void DrawLine(NoForms.Common.Point start, NoForms.Common.Point end, UBrush brush, UStroke stroke)
        {
            realRenderer.graphics.DrawLine(CreatePen(brush,stroke),SDGTr.trF(start), SDGTr.trF(end));
        }
        public void DrawRectangle(NoForms.Common.Rectangle rect, UBrush brush, UStroke stroke)
        {
            realRenderer.graphics.DrawRectangle(CreatePen(brush, stroke), rect.left, rect.top, rect.width, rect.height);
        }
        public void FillRectangle(NoForms.Common.Rectangle rect, UBrush brush)
        {
            realRenderer.graphics.FillRectangle(CreateBrush(brush), rect.left, rect.top, rect.width, rect.height);
        }
        public void DrawRoundedRectangle(NoForms.Common.Rectangle rect, float radX, float radY, UBrush brush, UStroke stroke)
        {
            var rr = ShapeHelpers.RoundedRectangle(rect, radX, radY);
            realRenderer.graphics.DrawPath(CreatePen(brush, stroke), rr);
        }
        public void FillRoundedRectangle(NoForms.Common.Rectangle rect, float radX, float radY, UBrush brush)
        {
            var rr = ShapeHelpers.RoundedRectangle(rect, radX, radY);
            realRenderer.graphics.FillPath(CreateBrush(brush), rr);
        }
        public void DrawText(UText textObject, NoForms.Common.Point location, UBrush defBrush, UTextDrawOptions opt, bool clientRendering)
        {
            var tl = glyphRunner.GetSDGTextInfo(textObject);

            foreach (var glyphrun in tl.glyphRuns)
            {
                var style = glyphrun.run.drawStyle;
                UFont font = style != null ? (style.fontOverride ?? textObject.font) : textObject.font;
                FontStyle fs = (font.bold ? FontStyle.Bold: 0) | (font.italic ? FontStyle.Italic: 0);
                var sdgFont = Translate(font);
                UBrush brsh = style != null ? (style.fgOverride ?? defBrush) : defBrush;
                if (style != null && style.bgOverride != null)
                    FillRectangle(new NoForms.Common.Rectangle(glyphrun.location, glyphrun.run.runSize), style.bgOverride);
                realRenderer.graphics.DrawString(glyphrun.run.content, sdgFont, CreateBrush(brsh), SDGTr.trF(location + glyphrun.location), StringFormat.GenericTypographic);
            }
        }

        // WARNING this all assumes that char rects and lines are "tightly packed", except  differing font sizes
        //         on same line, which get "baselined".  Is this what really happens?  What about line,word and char spacing adjustments?
        //         dirty way would be to adjust char rects, but does SDG do that? (does it even support that?)
        // Text Measuring - isText tells you if you actually hit a part of the string...
        public UTextHitInfo HitPoint(NoForms.Common.Point hitPoint, UText text)
        {
            // Grab a ref to the sdgtextinfo
            var ti = glyphRunner.GetSDGTextInfo(text);
            return glyphRunner.HitPoint(ti, hitPoint);
        }

        public NoForms.Common.Point HitText(int pos, bool trailing, UText text)
        {
            // Grab a ref to the sdgtextinfo
            var ti = glyphRunner.GetSDGTextInfo(text);
            return glyphRunner.HitText(ti, pos, trailing);
        }

        public IEnumerable<NoForms.Common.Rectangle> HitTextRange(int start, int length, NoForms.Common.Point offset, UText text)
        {
            // Grab a ref to the sdgtextinfo
            var ti = glyphRunner.GetSDGTextInfo(text);
            return glyphRunner.HitTextRange(ti, start, length, offset);
        }

        // FIXME Cache!
        public UTextInfo GetTextInfo(UText text)
        {
            var sti = glyphRunner.GetSDGTextInfo(text);
            return new UTextInfo()
            {
                minSize = sti.minSize,
                lineLengths = sti.lineLengths.ToArray(),
                lineNewLineLength = sti.newLineLengths.ToArray(),
                numLines = sti.lineLengths.Count
            };
        }

        Font Translate(UFont font)
        {
            return new Font(font.name, font.size, (font.italic ? FontStyle.Italic : 0) | (font.bold ? FontStyle.Bold : 0));
        }

        // Element Creators
        // FIXME how to avoid switching on type, and allowing mixing of UBrush etc between renderes at runtime?  Factory method on specific class wouldnt work...
        Brush CreateBrush(UBrush b)
        {
            return b.Retreive<SDG_RenderElements>(new NoCacheDelegate(() => CreateNewBrush(b))) as Brush;
        }
        Brush CreateNewBrush(UBrush b)
        {
            // FIXME can use PathGradientBrush, d2d just has gradientbrush...
            Brush ret;
            if (b is USolidBrush)
            {
                // FIXME support brushProperties
                USolidBrush sb = b as USolidBrush;
                ret = new SolidBrush(SDGTr.tr(sb.color));
            }
            else if (b is ULinearGradientBrush)
            {
                var lb = b as ULinearGradientBrush;
                ret = new LinearGradientBrush(SDGTr.trF(lb.point1), SDGTr.trF(lb.point2), SDGTr.tr(lb.color1), SDGTr.tr(lb.color2));
            }
            else throw new NotImplementedException();
            return ret;
        }

        CompositingQuality Translate(UInterp ui)
        {
            switch (ui)
            {
                default: case UInterp.Linear: return CompositingQuality.AssumeLinear;
                case UInterp.Nearest: return CompositingQuality.HighSpeed;
            }
        }

        Bitmap CreateBitmap(UBitmap b)
        {
            return b.Retreive<SDG_RenderElements>(new NoCacheDelegate(() => CreateNewBitmap(b))) as Bitmap;
        }
        Bitmap CreateNewBitmap(UBitmap b)
        {
            System.Drawing.Bitmap bm;
            if (b.bitmapData != null)
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream(b.bitmapData);
                bm = new System.Drawing.Bitmap(ms);
            }
            else bm = new System.Drawing.Bitmap(b.bitmapFile);
            return bm;
        }

        Pen CreatePen(UBrush b, UStroke s)
        {
            return s.Retreive<SDG_RenderElements>(new NoCacheDelegate(() => CreateNewPen(b, s))) as Pen;
        }
        Pen CreateNewPen(UBrush b, UStroke s)
        {
            // Link to invalidation of brush, once.
            //  the cache of the pen is stored in the stroke.
            NoFormsAction inval = delegate { };
            inval = () =>
            {
                s.Invalidate();
                b.invalidated -= inval;
            };
            b.invalidated += inval;

            // FIXME advanced features not used!
            Pen ret;
            ret = new Pen(CreateBrush(b));
            ret.DashCap = Translate1(s.dashCap);
            ret.EndCap = Translate2(s.endCap);
            ret.StartCap = Translate2(s.startCap);
            ret.LineJoin = Translate(s.lineJoin);
            ret.DashStyle = Translate(s.dashStyle);
            ret.MiterLimit = s.mitreLimit;
            ret.DashOffset = s.offset;

            // FIXME maybe not right for compatibility with other renderes?
            ret.Alignment = PenAlignment.Center;

            if (s.dashStyle == StrokeType.custom)
                ret.DashPattern = s.custom;

            return ret;
        }

        DashCap Translate1(StrokeCaps sc)
        {
            switch (sc)
            {
                case (StrokeCaps.flat): return DashCap.Flat;
                case (StrokeCaps.round): return DashCap.Round;
                case (StrokeCaps.triangle): return DashCap.Triangle;
                default: return DashCap.Flat;
            }
        }
        LineCap Translate2(StrokeCaps sc)
        {
            switch (sc)
            {
                case (StrokeCaps.flat): return LineCap.Flat;
                case (StrokeCaps.round): return LineCap.Round;
                case (StrokeCaps.triangle): return LineCap.Triangle;
                default: return LineCap.Flat;
            }
        }
        LineJoin Translate(StrokeJoin sj)
        {
            switch (sj)
            {
                case (StrokeJoin.bevel): return LineJoin.Bevel;
                case (StrokeJoin.mitre): return LineJoin.Miter;
                case (StrokeJoin.round): return LineJoin.Round;
                default: return LineJoin.Bevel;
            }
        }
        DashStyle Translate(StrokeType st)
        {
            switch (st)
            {
                case (StrokeType.solid): return DashStyle.Solid;
                case (StrokeType.custom): return DashStyle.Custom;
                case (StrokeType.dash): return DashStyle.Dash;
                case (StrokeType.dashdot): return DashStyle.DashDot;
                case (StrokeType.dashdotdot): return DashStyle.DashDotDot;
                case (StrokeType.dot): return DashStyle.Dot;
                default: return DashStyle.Solid;
            }
        }

        GraphicsPath CreatePath(UPath p)
        {
            return p.Retreive<SDG_RenderElements>(new NoCacheDelegate(() => CreateNewPath(p))) as GraphicsPath;
        }
        GraphicsPath CreateNewPath(UPath p)
        {
            var gp = new GraphicsPath();
            foreach (var f in p.figures)
            {
                // FIXME hollow/filled not used here.
                gp.StartFigure();
                NoForms.Common.Point current = f.startPoint;
                foreach (var gb in f.geoElements) AppendGeometry(ref gp, gb, current);
                gp.CloseFigure();
            }
            return gp;
        }

        // FIXME another example of type switching.  Cant put d2d code on other side of bridge in eg UGeometryBase abstraction
        //       and also, using a factory to create the UGeometryBase will make d2d specific versions.
        // IDEA  This is possible:  Use factory (DI?) and check the Uxxx instances when passed to this type of class, and recreate
        //       a portion of the Uxxx if it doesnt match D2D?  Factory could be configured for d2d/ogl etc.  This links in with cacheing
        //       code too? Not sure if this will static compile :/ thats kinda the point...  we'd need the Uxxx.ICreateStuff to be a specific
        //       D2D interface...could subclass... would check if(ICreateStuff is D2DCreator) as d2dcreator else Icreatestuff=new d2dcreator...
        void AppendGeometry(ref GraphicsPath path, UGeometryBase geo, NoForms.Common.Point start) // ref GraphicsPath will allow us to recreate it if we want with new.
        {
            if (geo is UArcBase)
            {
                System.Drawing.PointF[] pts;
                if (geo is UArc)
                {
                    UArc arc = geo as UArc;
                    pts = (geo.Retreive<SDGDraw>(() =>
                    {
                        var elInput = new EllipseLib.Ellipse_Input(start.X, start.Y, arc.endPoint.X, arc.endPoint.Y, arc.arcSize.width, arc.arcSize.height, arc.rotation);
                        var elSolution = new List<EllipseLib.Ellipse_Output>(EllipseLib.Ellipse.Get_X0Y0(elInput)).ToArray();
                        EllipseLib.Ellipse.SampleArc(elInput, elSolution, arc.reflex, arc.sweepClockwise, arc.resolution, out pts);
                        return new disParr(pts);
                    }) as disParr).pts;
                }
                //else if (geo is UEasyArc)
                //{
                //    var arc = geo as UEasyArc;
                //    pts = (geo.Retreive<SDGDraw>(() =>
                //    {
                //        return new disParr(new List<System.Drawing.PointF>(EllipseLib.EasyEllipse.Generate(new EllipseLib.EasyEllipse.EasyEllipseInput()
                //        {
                //            rotation = arc.rotation,
                //            start_x = start.X,
                //            start_y = start.Y,
                //            rx = arc.arcSize.width,
                //            ry = arc.arcSize.height,
                //            t1 = arc.startAngle,
                //            t2 = arc.endAngle,
                //            resolution = arc.resolution
                //        })).ToArray());
                //    }) as disParr).pts;
                //}
                else throw new NotImplementedException();

                // clone the data
                List<PointF> opts = new List<PointF>(path.PointCount > 0 ? path.PathPoints : new PointF[0]);
                List<byte> otyps = new List<byte>(path.PointCount > 0 ? path.PathTypes : new byte[0]);

                // do the types
                if (otyps.Count == 0 || otyps[otyps.Count - 1] != (byte)PathPointType.Start)
                {
                    otyps.Add((byte)PathPointType.Start);
                    opts.Add(SDGTr.trF(start));
                }
                for (int i = 0; i < pts.Length; i++)
                    otyps.Add((byte)PathPointType.Line); // try to interpolate a bit?

                // append new data
                opts.AddRange(pts);

                // Replace the path via reference
                path = new GraphicsPath(opts.ToArray(), otyps.ToArray(), path.FillMode);
            }
            else if (geo is ULine)
            {
                ULine line = geo as ULine;
                path.AddLine(SDGTr.trF(start), SDGTr.trF(line.endPoint));
            }
            else if (geo is UBeizer)
            {
                UBeizer beizer = geo as UBeizer;
                path.AddBezier(SDGTr.trF(start), SDGTr.trF(beizer.controlPoint1), SDGTr.trF(beizer.controlPoint2), SDGTr.trF(beizer.endPoint));
            }
            else throw new NotImplementedException();
        }

        // Drawing Helpers
        static class ShapeHelpers
        {
            public static GraphicsPath RoundedRectangle(NoForms.Common.Rectangle r, float rx, float ry)
            {
                // FIXME surely this is slow? we need some special caching for GDI?
                var gp = new GraphicsPath();
                gp.StartFigure();
                float x = r.left;
                float y = r.top;
                float w = r.width;
                float h = r.height;
                gp.AddLine(x + rx, y, x + w - rx, y); // top line
                gp.AddArc(x + w - 2 * rx, y, 2 * rx, 2 * ry, 270, 90); // top-right corner
                gp.AddLine(x + w, y + ry, x + w, y + h - ry); // right line
                gp.AddArc(x + w - 2 * rx, y + h - 2 * ry, 2 * rx, 2 * ry, 0, 90);// bottom-right corner
                gp.AddLine(x + w - rx, y + h, x + rx, y + h); // bottom line
                gp.AddArc(x, y + h - 2 * ry, 2 * rx, 2 * ry, 90, 90); // bottom-left corner
                gp.AddLine(x, y + h - ry, x, y + ry); // left line
                gp.AddArc(x, y, 2 * rx, 2 * ry, 180, 90); // top-left corner
                gp.CloseFigure();
                return gp;
            }
        }
    }

}
