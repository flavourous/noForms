using System;
using System.Collections.Generic;
using System.Text;
using SharpDX.Direct2D1;

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
        public void PushAxisAlignedClip(Rectangle clipRect)
        {
            realRenderer.renderTarget.PushAxisAlignedClip(clipRect, SharpDX.Direct2D1.AntialiasMode.PerPrimitive);
        }
        public void PopAxisAlignedClip()
        {
            realRenderer.renderTarget.PopAxisAlignedClip();
        }
        public void Clear(Color color)
        {
            realRenderer.renderTarget.Clear(color);
        }
        public void FillPath(UPath path, UBrush brush)
        {
            realRenderer.renderTarget.FillGeometry(path.getD2D(realRenderer.renderTarget), CreateBrush(brush));
        }
        public void DrawPath(UPath path, UBrush brush, UStroke stroke)
        {
            realRenderer.renderTarget.DrawGeometry(path.getD2D(realRenderer.renderTarget), CreateBrush(brush), stroke.strokeWidth, CreateStroke(stroke));
        }
        public void DrawBitmap(UBitmap bitmap, float opacity, UInterp interp, Rectangle source, Rectangle destination)
        {
            realRenderer.renderTarget.DrawBitmap(CreateBitmap(bitmap), destination, opacity, Translate(interp), source);
        }
        public void FillEllipse(Point center, float radiusX, float radiusY, UBrush brush)
        {
            realRenderer.renderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(center, radiusX, radiusY), CreateBrush(brush));
        }
        public void DrawEllipse(Point center, float radiusX, float radiusY, UBrush brush, UStroke stroke)
        {
            realRenderer.renderTarget.DrawEllipse(new SharpDX.Direct2D1.Ellipse(center, radiusX, radiusY), CreateBrush(brush), stroke.strokeWidth, CreateStroke(stroke));
        }
        public void DrawLine(Point start, Point end, UBrush brush, UStroke stroke)
        {
            realRenderer.renderTarget.DrawLine(start, end, CreateBrush(brush), stroke.strokeWidth, CreateStroke(stroke));
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
            var rr = new SharpDX.Direct2D1.RoundedRectangle()
            {
                Rect = rect,
                RadiusX = radX,
                RadiusY = radY
            };
            realRenderer.renderTarget.DrawRoundedRectangle(rr, CreateBrush(brush), stroke.strokeWidth, CreateStroke(stroke));
        }
        public void FillRoundedRectangle(Rectangle rect, float radX, float radY, UBrush brush)
        {
            var rr = new SharpDX.Direct2D1.RoundedRectangle()
            {
                Rect = rect,
                RadiusX = radX,
                RadiusY = radY
            };
            realRenderer.renderTarget.FillRoundedRectangle(rr, CreateBrush(brush));
        }
        public void DrawText(UText textObject, Point location, UBrush defBrush, UTextDrawOptions opt, bool clientRendering)
        {
            var tl = textObject.GetD2D(dwFact, realRenderer.renderTarget);

            foreach (var ord in textObject.SafeGetStyleRanges)
            {
                if (ord.bgOveride != null)
                {
                    foreach (var r in textObject.HitTextRange(ord.start, ord.length, location))
                        FillRectangle(r, ord.bgOveride);
                }
            }

            if (clientRendering)
            {
                // set default foreground brush
                tl.textRenderer.defaultEffect.fgBrush = defBrush;

                // Draw the text (foreground & background in the client renderer)
                tl.textLayout.Draw(new Object[] { location, textObject }, tl.textRenderer, location.X, location.Y);
            }
            else
            {
                // Use D2D implimentation of text layout rendering
                realRenderer.renderTarget.DrawTextLayout(location, tl.textLayout, defCreateBrush(brush));
            }
        }

        // Text Measuring
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
                var lb = b as ULinearGradientBrush;
                SharpDX.Direct2D1.LinearGradientBrush lgb = new SharpDX.Direct2D1.LinearGradientBrush(realRenderer.renderTarget,
                new SharpDX.Direct2D1.LinearGradientBrushProperties()
                {
                    StartPoint = lb.point1,
                    EndPoint = lb.point2
                },
                new SharpDX.Direct2D1.GradientStopCollection(realRenderer.renderTarget,
                new SharpDX.Direct2D1.GradientStop[] {
                    new SharpDX.Direct2D1.GradientStop() { Color = lb.color1, Position = 0f },
                    new SharpDX.Direct2D1.GradientStop() { Color = lb.color2, Position = 1f } },
                    SharpDX.Direct2D1.Gamma.StandardRgb,
                    SharpDX.Direct2D1.ExtendMode.Clamp)
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
            var bmd =  SharpDX.Direct2D1.Bitmap.FromWicBitmap(realRenderer.renderTarget, wbm);
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
            var sp = new SharpDX.Direct2D1.StrokeStyleProperties()
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
        
    }
}
