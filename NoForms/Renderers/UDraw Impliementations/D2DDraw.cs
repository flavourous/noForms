using System;
using System.Collections.Generic;
using System.Text;
using SharpDX.DirectWrite;
using SharpDX.Direct2D1;
using NoForms.Common;

namespace NoForms.Renderers
{
    
    class D2DDraw : IUnifiedDraw
    {
        // FIXME this should go somewere in the d2d renderelements...
        private static SharpDX.DirectWrite.Factory _dwFact;
        public static SharpDX.DirectWrite.Factory dwFact
        {
            get
            {
                if (_dwFact == null)
                    _dwFact = new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Shared);
                return _dwFact;
            }
        }
        private static SharpDX.WIC.ImagingFactory _wicFact;
        public static SharpDX.WIC.ImagingFactory wicFact
        {
            get
            {
                if (_wicFact == null)
                    _wicFact = new SharpDX.WIC.ImagingFactory();
                return _wicFact;
            }
        }

        D2D_RenderElements realRenderer;
        public D2DDraw(D2D_RenderElements els)
        {
            realRenderer = els;
        }
        public void PushAxisAlignedClip(Rectangle clipRect, bool ignoreRenderOffset)
        {
            var cr = clipRect;
            if (ignoreRenderOffset) cr -= new Point(realRenderer.renderTarget.Transform.M31, realRenderer.renderTarget.Transform.M32);
            realRenderer.renderTarget.PushAxisAlignedClip(cr, AntialiasMode.PerPrimitive);
        }
        public void PopAxisAlignedClip()
        {
            realRenderer.renderTarget.PopAxisAlignedClip();
        }
        public void SetRenderOffset(Point offset)
        {
            var rtt = realRenderer.renderTarget.Transform;
            rtt = new SharpDX.Matrix3x2(rtt.M11, rtt.M12, rtt.M21, rtt.M22, offset.X, offset.Y);
            realRenderer.renderTarget.Transform = rtt;
        }
        public void Clear(Color color)
        {
            realRenderer.renderTarget.Clear(color);
        }
        public void FillPath(UPath path, UBrush brush)
        {
            realRenderer.renderTarget.FillGeometry(CreatePath(path), CreateBrush(brush));
        }
        public void DrawPath(UPath path, UBrush brush, UStroke stroke)
        {
            realRenderer.renderTarget.DrawGeometry(CreatePath(path), CreateBrush(brush), stroke.strokeWidth, CreateStroke(stroke));
        }
        public void DrawBitmap(UBitmap bitmap, float opacity, UInterp interp, Rectangle source, Rectangle destination)
        {
            realRenderer.renderTarget.DrawBitmap(CreateBitmap(bitmap), destination, opacity, Translate(interp), source);
        }
        public void FillEllipse(Point center, float radiusX, float radiusY, UBrush brush)
        {
            realRenderer.renderTarget.FillEllipse(new Ellipse(D2DTr.tr(center), radiusX, radiusY), CreateBrush(brush));
        }
        public void DrawEllipse(Point center, float radiusX, float radiusY, UBrush brush, UStroke stroke)
        {
            realRenderer.renderTarget.DrawEllipse(new Ellipse(D2DTr.tr(center), radiusX, radiusY), CreateBrush(brush), stroke.strokeWidth, CreateStroke(stroke));
        }
        public void DrawLine(Point start, Point end, UBrush brush, UStroke stroke)
        {
            realRenderer.renderTarget.DrawLine(D2DTr.tr(start), D2DTr.tr(end), CreateBrush(brush), stroke.strokeWidth, CreateStroke(stroke));
        }
        public void DrawRectangle(Rectangle rect, UBrush brush, UStroke stroke)
        {
            realRenderer.renderTarget.DrawRectangle(rect, CreateBrush(brush), stroke.strokeWidth, CreateStroke(stroke));
        }
        public void FillRectangle(Rectangle rect, UBrush brush)
        {
            realRenderer.renderTarget.FillRectangle(rect, CreateBrush(brush));
        }
        public void DrawRoundedRectangle(Rectangle rect, float radX, float radY, UBrush brush, UStroke stroke)
        {
            var rr = new RoundedRectangle()
            {
                Rect = rect,
                RadiusX = radX,
                RadiusY = radY
            };
            realRenderer.renderTarget.DrawRoundedRectangle(rr, CreateBrush(brush), stroke.strokeWidth, CreateStroke(stroke));
        }
        public void FillRoundedRectangle(Rectangle rect, float radX, float radY, UBrush brush)
        {
            var rr = new RoundedRectangle()
            {
                Rect = rect,
                RadiusX = radX,
                RadiusY = radY
            };
            realRenderer.renderTarget.FillRoundedRectangle(rr, CreateBrush(brush));
        }
        public void DrawText(UText textObject, Point location, UBrush defBrush, UTextDrawOptions opt, bool clientRendering)
        {
            var tl = CreateTextElements(textObject);

            foreach (var ord in textObject.SafeGetStyleRanges)
            {
                if (ord.bgOverride != null)
                {
                    foreach (var r in HitTextRange(ord.start, ord.length, location, textObject))
                        FillRectangle(r, ord.bgOverride);
                }
            }

            if (clientRendering)
            {
                // set default foreground brush
                tl.textRenderer.defaultEffect.fgBrush = CreateBrush(defBrush);

                // Draw the text (foreground & background in the client renderer)
                tl.textLayout.Draw(new Object[] { location, textObject }, tl.textRenderer, location.X, location.Y);
            }
            else
            {
                // Use D2D implimentation of text layout rendering
                realRenderer.renderTarget.DrawTextLayout(D2DTr.tr(location), tl.textLayout, CreateBrush(defBrush));
            }
        }

        // Text Measuring
        public UTextHitInfo HitPoint(Point hitPoint, UText text)
        {
            var textLayout = CreateTextElements(text).textLayout;
            SharpDX.Bool trailing, inside;
            var htm = textLayout.HitTestPoint(hitPoint.X, hitPoint.Y, out trailing, out inside);
            return new UTextHitInfo()
            {
                charPos = htm.TextPosition,
                leading = hitPoint.X > htm.Left + htm.Width / 2,
                isText = htm.IsText
            };
        }

        public Point HitText(int pos, bool trailing, UText text)
        {
            var textLayout = CreateTextElements(text).textLayout;
            float hx, hy;
            var htm = textLayout.HitTestTextPosition(pos, trailing, out hx, out hy);
            return new Point(hx, hy);
        }

        public IEnumerable<Rectangle> HitTextRange(int start, int length, Point offset, UText text)
        {
            var textLayout = CreateTextElements(text).textLayout;
            foreach (var htm in textLayout.HitTestTextRange(start, length, offset.X, offset.Y))
                yield return new Rectangle(htm.Left, htm.Top, htm.Width, htm.Height);
        }

        public UTextInfo GetTextInfo(UText text)
        {
            var textLayout = CreateTextElements(text).textLayout;
            return NewTextInfo(textLayout);
        }

        public UTextInfo NewTextInfo(TextLayout textLayout)
        {
            UTextInfo ret = new UTextInfo();
            // get size of the render
            float minHeight = 0;
            ret.numLines = 0;
            ret.lineLengths = new int[textLayout.Metrics.LineCount];
            ret.lineNewLineLength = new int[textLayout.Metrics.LineCount];
            int i = 0;
            foreach (var tlm in textLayout.GetLineMetrics())
            {
                minHeight += tlm.Height;
                ret.numLines++;
                ret.lineLengths[i] = tlm.Length;
                ret.lineNewLineLength[i] = tlm.NewlineLength;
                i++;
            }
            ret.minSize = new Size(textLayout.DetermineMinWidth(), minHeight);
            return ret;
        }

        // Element Creators
        // FIXME how to avoid switching on type, and allowing mixing of UBrush etc between renderes at runtime?  Factory method on specific class wouldnt work...
        Brush CreateBrush(UBrush b)
        {
            return b.Retreive<D2D_RenderElements>(new NoCacheDelegate(() => CreateNewBrush(b))) as Brush;
        }
        Brush CreateNewBrush(UBrush b)
        {
            Brush ret;
            if (b is USolidBrush)
            {
                // FIXME support brushProperties
                USolidBrush sb = b as USolidBrush;
                ret= new SolidColorBrush(realRenderer.renderTarget, sb.color);
            }
            else if (b is ULinearGradientBrush)
            {
                // FIXME advanced features not used
                var lb = b as ULinearGradientBrush;
                LinearGradientBrush lgb = new LinearGradientBrush(realRenderer.renderTarget,
                new LinearGradientBrushProperties()
                {
                    StartPoint = D2DTr.tr(lb.point1),
                    EndPoint = D2DTr.tr(lb.point2)
                },
                new GradientStopCollection(realRenderer.renderTarget,
                new GradientStop[] {
                    new GradientStop() { Color = lb.color1, Position = 0f },
                    new GradientStop() { Color = lb.color2, Position = 1f } },
                    Gamma.StandardRgb,
                    ExtendMode.Clamp)
                );
                ret =  lgb;
            }
            else throw new NotImplementedException();
            // Extra invalidation condition besides colors changeing etc as handled by the ObjStore descendants:  renderTarget disposed (eg resize).
            realRenderer.renderTarget.Disposed += new EventHandler<EventArgs>((o, e) => b.Invalidate());
            return ret;
        }

        Bitmap CreateBitmap(UBitmap b)
        {
            return b.Retreive<D2D_RenderElements>(new NoCacheDelegate(() => CreateNewBitmap(b))) as Bitmap;
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
            SharpDX.WIC.Bitmap wbm = new SharpDX.WIC.Bitmap(wicFact, bm, SharpDX.WIC.BitmapAlphaChannelOption.UseAlpha);
            var bmd =  Bitmap.FromWicBitmap(realRenderer.renderTarget, wbm);
            realRenderer.renderTarget.Disposed += new EventHandler<EventArgs>((o, e) => b.Invalidate());
            return bmd;
        }

        BitmapInterpolationMode Translate(UInterp interp)
        {
            switch (interp)
            {
                case UInterp.Linear: return BitmapInterpolationMode.Linear;
                case UInterp.Nearest: return BitmapInterpolationMode.NearestNeighbor;
                default: throw new NotImplementedException();
            }
        }

        StrokeStyle CreateStroke(UStroke s)
        {
            return s.Retreive<D2D_RenderElements>(new NoCacheDelegate(() => CreateNewStroke(s))) as StrokeStyle;
        }
        StrokeStyle CreateNewStroke(UStroke s)
        {
            StrokeStyle ret;
            var sp = new StrokeStyleProperties()
            {
                DashCap = Translate(s.dashCap),
                EndCap = Translate(s.endCap),
                StartCap = Translate(s.startCap),
                LineJoin = Translate(s.lineJoin),
                DashStyle = Translate(s.dashStyle),
                MiterLimit = s.mitreLimit,
                DashOffset = s.offset
            };
            if (s.dashStyle == StrokeType.custom)
                ret =  new StrokeStyle(realRenderer.renderTarget.Factory, sp, s.custom);
            else
                ret = new StrokeStyle(realRenderer.renderTarget.Factory, sp);
            realRenderer.renderTarget.Disposed += new EventHandler<EventArgs>((o, e) => s.Invalidate());
            return ret;
        }

        CapStyle Translate(StrokeCaps sc)
        {
            switch (sc)
            {
                case (StrokeCaps.flat): return CapStyle.Flat;
                case (StrokeCaps.round): return CapStyle.Round;
                case (StrokeCaps.triangle): return CapStyle.Triangle;
                default: return CapStyle.Flat;
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

        Geometry CreatePath(UPath p)
        {
            return p.Retreive<D2D_RenderElements>(new NoCacheDelegate(() => CreateNewGeometry(p))) as Geometry;
        }
        Geometry CreateNewGeometry(UPath p)
        {
            var pg = new PathGeometry(realRenderer.renderTarget.Factory);
            var gs = pg.Open();
            foreach (var f in p.figures)
            {
                gs.BeginFigure(D2DTr.tr(f.startPoint), f.filled ? FigureBegin.Filled : FigureBegin.Hollow);
                foreach (var gb in f.geoElements) AppendGeometry(gs, gb);
                gs.EndFigure(f.open ? FigureEnd.Open : FigureEnd.Closed);
            }
            gs.Close();
            return pg;
        }

        // FIXME another example of type switching.  Cant put d2d code on other side of bridge in eg UGeometryBase abstraction
        //       and also, using a factory to create the UGeometryBase will make d2d specific versions.
        // IDEA  This is possible:  Use factory (DI?) and check the Uxxx instances when passed to this type of class, and recreate
        //       a portion of the Uxxx if it doesnt match D2D?  Factory could be configured for d2d/ogl etc.  This links in with cacheing
        //       code too? Not sure if this will static compile :/ thats kinda the point...  we'd need the Uxxx.ICreateStuff to be a specific
        //       D2D interface...could subclass... would check if(ICreateStuff is D2DCreator) as d2dcreator else Icreatestuff=new d2dcreator...

        void AppendGeometry(GeometrySink sink, UGeometryBase geo)
        {
            if (geo is UArc)
            {
                UArc arc = geo as UArc;
                sink.AddArc(new ArcSegment()
                {
                    SweepDirection = arc.sweepClockwise ? SweepDirection.Clockwise : SweepDirection.CounterClockwise,
                    RotationAngle = -arc.rotation,
                    ArcSize = arc.reflex ? ArcSize.Large : ArcSize.Small,
                    Point = D2DTr.tr(arc.endPoint),
                    Size = D2DTr.tr(arc.arcSize)
                });
            }
            else if (geo is ULine)
            {
                ULine line = geo as ULine;
                sink.AddLine(D2DTr.tr(line.endPoint));
            }
            else if (geo is UBeizer)
            {
                UBeizer beizer = geo as UBeizer;
                sink.AddBezier(new BezierSegment()
                {
                    Point1 = D2DTr.tr(beizer.controlPoint1),
                    Point2 = D2DTr.tr(beizer.controlPoint2),
                    Point3 = D2DTr.tr(beizer.endPoint)
                });
            }
            else throw new NotImplementedException();
        }

        public D2D_TextElements CreateTextElements(UText t)
        {
            return t.Retreive<D2D_RenderElements>(new NoCacheDelegate(() => CreateNewTextElements(t))) as D2D_TextElements;
        }
        public D2D_TextElements CreateNewTextElements(UText t)
        {
            var textLayout = new TextLayout(dwFact, t.text, new TextFormat(
                dwFact,
                t.font.name,
                t.font.bold ? FontWeight.Bold : FontWeight.Normal,
                t.font.italic ? FontStyle.Italic : FontStyle.Normal,
                TranslateFontSize(t.font.size))
            {
                ParagraphAlignment = Translate(t.valign),
                TextAlignment = Translate(t.halign),
                WordWrapping = t.wrapped ? WordWrapping.Wrap : WordWrapping.NoWrap
            }, t.width, t.height);

            // Set font ranges... textLayout just created, dont worry about any leftover ranges.
            foreach (var sr in t.SafeGetStyleRanges)
            {
                var tr = new TextRange(sr.start, sr.length);
                if (sr.fontOverride != null)
                {
                    UFont ft = (UFont)sr.fontOverride;
                    textLayout.SetFontFamilyName(ft.name, tr);
                    textLayout.SetFontSize(TranslateFontSize(ft.size), tr);
                    textLayout.SetFontStyle(ft.italic ? FontStyle.Italic : FontStyle.Normal, tr);
                    textLayout.SetFontWeight(ft.bold ? FontWeight.Bold : FontWeight.Normal, tr);
                }
                if (sr.fgOverride != null || sr.bgOverride != null)
                {
                    ClientTextEffect cte = new ClientTextEffect();
                    if (sr.fgOverride != null)
                        cte.fgBrush = CreateBrush(sr.fgOverride);
                    if (sr.bgOverride != null)
                        cte.bgBrush = CreateBrush(sr.bgOverride);
                    textLayout.SetDrawingEffect(cte, tr);
                }
            }

            // Set renderer with a default brush
            var def = new USolidBrush() { color = new Color(0) };
            var textRenderer = new D2D_ClientTextRenderer(realRenderer.renderTarget, new ClientTextEffect() { fgBrush = CreateBrush(def) });

            var ret = new D2D_TextElements(textLayout, textRenderer);
            realRenderer.renderTarget.Disposed += new EventHandler<EventArgs>((o, e) => t.Invalidate());
            return ret;
        }
        float TranslateFontSize(float pt)
        {
            // a dip is 1/96 inches.  a pt is 1/72 inches.
            return (96f / 72f) * pt;
        }
        ParagraphAlignment Translate(UVAlign v)
        {
            switch (v)
            {
                case UVAlign.Top: return ParagraphAlignment.Near;
                case UVAlign.Middle: return ParagraphAlignment.Center;
                case UVAlign.Bottom: return ParagraphAlignment.Far;
                default: return ParagraphAlignment.Near;
            }
        }
        TextAlignment Translate(UHAlign h)
        {
            switch (h)
            {
                case UHAlign.Left: return TextAlignment.Leading;
                case UHAlign.Center: return TextAlignment.Center;
                case UHAlign.Right: return TextAlignment.Trailing;
                default: return TextAlignment.Leading;
            }
        }
    }
        public class D2D_TextElements : IDisposable
    {
        public TextLayout textLayout;
        public D2D_ClientTextRenderer textRenderer;

        public D2D_TextElements(TextLayout textLayout, D2D_ClientTextRenderer textRenderer)
        {
            this.textLayout = textLayout;
            this.textRenderer = textRenderer;
        }

        public void Dispose()
        {
            textRenderer.Dispose();
            textLayout.Dispose();
        }
    }
    public class ClientTextEffect : SharpDX.ComObject
    {
        public Brush fgBrush;
        public Brush bgBrush;
    }
    public class D2D_ClientTextRenderer : TextRendererBase
    {
        internal ClientTextEffect defaultEffect;
        RenderTarget rt;
        public D2D_ClientTextRenderer(RenderTarget rt, ClientTextEffect defaultEffect)
        {
            this.rt = rt;
            this.defaultEffect = defaultEffect;
        }

        public override SharpDX.Result DrawGlyphRun(object clientDrawingContext, float baselineOriginX, float baselineOriginY, MeasuringMode measuringMode, GlyphRun glyphRun, GlyphRunDescription glyphRunDescription, SharpDX.ComObject clientDrawingEffect)
        {
            var cce = (ClientTextEffect)clientDrawingEffect;
            var args = (Object[])clientDrawingContext;
            var ofs = (Point)args[0];
            var ut = (UText)args[1];

            Point origin = new Point(baselineOriginX, baselineOriginY);

            var fgb = cce == null ? defaultEffect.fgBrush : cce.fgBrush;
            rt.DrawGlyphRun(D2DTr.tr(origin), glyphRun, fgb, MeasuringMode.Natural);

            return SharpDX.Result.Ok;
        }
    }
}
