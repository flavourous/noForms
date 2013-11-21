using System;
using System.Collections.Generic;

namespace NoForms.Renderers
{
    public class UnifiedDraw // This is a combined replacement for 2d2RenderTarget, drawing.graphics etc
    {
        IRenderElements realRenderer;
        public UnifiedDraw(IRenderElements re)
        {
            realRenderer = re;
        }
        private UnifiedDraw()
        {
            throw new Exception("No. Wait. How?");
        }

        Dictionary<System.Drawing.Graphics, List<Rectangle>> SDRClips = new Dictionary<System.Drawing.Graphics, List<Rectangle>>();
        public void PushAxisAlignedClip(Rectangle clipRect)
        {
            if (realRenderer is D2D_RenderElements)
            {
                var rel = realRenderer as D2D_RenderElements;
                rel.renderTarget.PushAxisAlignedClip(clipRect, SharpDX.Direct2D1.AntialiasMode.PerPrimitive);
            }
            else if (realRenderer is SDG_RenderElements)
            {
                var rel = realRenderer as SDG_RenderElements;
                System.Drawing.Region rg = new System.Drawing.Region(Rectangle.Empty);
                SDRClips[rel.graphics].Add(clipRect);
                foreach (var cr in SDRClips[rel.graphics])
                    rg.Union(cr);
                rel.graphics.SetClip(rg, System.Drawing.Drawing2D.CombineMode.Replace);
            }
            else throw new Exception("Internal error, DrawLine cannot handle " + realRenderer.GetType().ToString());
        }
        public void PopAxisAlignedClip()
        {
            if (realRenderer is D2D_RenderElements)
            {
                var rel = realRenderer as D2D_RenderElements;
                rel.renderTarget.PopAxisAlignedClip();
            }
            else if (realRenderer is SDG_RenderElements)
            {
                var rel = realRenderer as SDG_RenderElements;

                if (SDRClips[rel.graphics].Count == 0)
                    throw new Exception("Tried to pop a clip rectangle, but there were none left!  You much match calls to pop and push equally.");

                System.Drawing.Region rg = new System.Drawing.Region(Rectangle.Empty);
                SDRClips[rel.graphics].RemoveAt(SDRClips[rel.graphics].Count-1);
                foreach (var cr in SDRClips[rel.graphics])
                    rg.Union(cr);

                rel.graphics.SetClip(rg, System.Drawing.Drawing2D.CombineMode.Replace);
            }
            else throw new Exception("Internal error, DrawLine cannot handle " + realRenderer.GetType().ToString());
        }

        public void Clear(Color color)
        {
            if (realRenderer is D2D_RenderElements)
            {
                var rel = realRenderer as D2D_RenderElements;
                rel.renderTarget.Clear(color);
            }
            else if (realRenderer is SDG_RenderElements)
            {
                var rel = realRenderer as SDG_RenderElements;
                rel.graphics.Clear(color);
            }
            else throw new Exception("Internal error, DrawLine cannot handle " + realRenderer.GetType().ToString());
        }

        public void FillPath(UPath path, UBrush brush)
        {
            if (realRenderer is D2D_RenderElements)
            {
                var rel = realRenderer as D2D_RenderElements;
                rel.renderTarget.FillGeometry(path.getD2D(rel.renderTarget.Factory), brush.GetD2D(rel.renderTarget));
            }
            else if (realRenderer is SDG_RenderElements)
            {
                throw new NotImplementedException("Dave, when you get here, good luck.");
                //var rel = realRenderer as SDG_RenderElements;
                //System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();

                //gp.AddArc(System.Drawing.Rectangle.Empty,/*startAngle*/,/*sweepAngle*/);
                //gp.AddBezier();
                //gp.AddBeziers();
                //gp.AddLine();
                //gp.AddLines();
            }
            else throw new Exception("Internal error, DrawGeometry cannot handle " + realRenderer.GetType().ToString());
        }
        
        public void DrawPath(UPath path, UBrush brush, UStroke stroke)
        {
            if (realRenderer is D2D_RenderElements)
            {
                var rel = realRenderer as D2D_RenderElements;
                rel.renderTarget.DrawGeometry(path.getD2D(rel.renderTarget.Factory), brush.GetD2D(rel.renderTarget), stroke.strokeWidth, stroke.Get_D2D(rel.renderTarget.Factory));
            }
            else if(realRenderer is SDG_RenderElements) 
            {
                throw new NotImplementedException("Dave, when you get here, good luck.");
                //var rel = realRenderer as SDG_RenderElements;
                //System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();

                //gp.AddArc(System.Drawing.Rectangle.Empty,/*startAngle*/,/*sweepAngle*/);
                //gp.AddBezier();
                //gp.AddBeziers();
                //gp.AddLine();
                //gp.AddLines();
            }
            else throw new Exception("Internal error, DrawGeometry cannot handle " + realRenderer.GetType().ToString());
        }
        public void DrawBitmap(UBitmap bitmap, float opacity, UInterp interp, Rectangle source, Rectangle destination)
        {
            if (realRenderer is D2D_RenderElements)
            {
                var rel = realRenderer as D2D_RenderElements;
                rel.renderTarget.DrawBitmap(bitmap.GetD2D(rel.renderTarget), destination, opacity, interp, source);
            }
            else throw new Exception("Internal error, DrawBitmap cannot handle " + realRenderer.GetType().ToString());
        }
        public void FillEllipse(Point center, float radiusX, float radiusY, UBrush brush)
        {
            if (realRenderer is D2D_RenderElements)
            {
                var rel = realRenderer as D2D_RenderElements;
                rel.renderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(center, radiusX, radiusY), brush.GetD2D(rel.renderTarget));
            }
            else throw new Exception("Internal error, FillEllipse cannot handle " + realRenderer.GetType().ToString());
        }
        public void DrawEllipse(Point center, float radiusX, float radiusY, UBrush brush, UStroke stroke)
        {
            if (realRenderer is D2D_RenderElements)
            {
                var rel = realRenderer as D2D_RenderElements;
                rel.renderTarget.DrawEllipse(new SharpDX.Direct2D1.Ellipse(center, radiusX, radiusY), brush.GetD2D(rel.renderTarget), stroke.strokeWidth, stroke.Get_D2D(rel.renderTarget.Factory));
            }
            else throw new Exception("Internal error, DrawEllipse cannot handle " + realRenderer.GetType().ToString());
        }
        public void DrawLine(Point start, Point end, UBrush brush, UStroke stroke) 
        {
            if (realRenderer is D2D_RenderElements)
            {
                var rel = realRenderer as D2D_RenderElements;
                rel.renderTarget.DrawLine(start, end, brush.GetD2D(rel.renderTarget), stroke.strokeWidth, stroke.Get_D2D(rel.renderTarget.Factory));
            }
            else if (realRenderer is SDG_RenderElements)
            {
                var rel = realRenderer as SDG_RenderElements;
                rel.graphics.DrawLine(stroke.Get_SysDraw(brush), start, end);
            }
            else throw new Exception("Internal error, DrawLine cannot handle " + realRenderer.GetType().ToString());
        }
        public void DrawRectangle(Rectangle rect, UBrush brush, UStroke stroke)
        {
            if (realRenderer is D2D_RenderElements)
            {
                var rel = realRenderer as D2D_RenderElements;
                rel.renderTarget.DrawRectangle(rect, brush.GetD2D(rel.renderTarget), stroke.strokeWidth, stroke.Get_D2D(rel.renderTarget.Factory));
            }
            else throw new Exception("Internal error, DrawRectangle cannot handle " + realRenderer.GetType().ToString());
        }
        public void FillRectangle(Rectangle rect, UBrush brush)
        {
            if (realRenderer is D2D_RenderElements)
            {
                var rel = realRenderer as D2D_RenderElements;
                rel.renderTarget.FillRectangle(rect, brush.GetD2D(rel.renderTarget));
            }
            else throw new Exception("Internal error, FillRectangle cannot handle " + realRenderer.GetType().ToString());
        }
        public void DrawRoundedRectangle(Rectangle rect, float radX, float radY, UBrush brush, UStroke stroke)
        {
            if (realRenderer is D2D_RenderElements)
            {
                var rel = realRenderer as D2D_RenderElements;
                var rr = new SharpDX.Direct2D1.RoundedRectangle() 
                {
                    Rect = rect,
                    RadiusX = radX,
                    RadiusY = radY
                };
                rel.renderTarget.DrawRoundedRectangle(rr, brush.GetD2D(rel.renderTarget), stroke.strokeWidth, stroke.Get_D2D(rel.renderTarget.Factory));
            }
            else throw new Exception("Internal error, DrawRectangle cannot handle " + realRenderer.GetType().ToString());
        }
        public void FillRoundedRectangle(Rectangle rect, float radX, float radY, UBrush brush)
        {
            if (realRenderer is D2D_RenderElements)
            {
                var rel = realRenderer as D2D_RenderElements;
                var rr = new SharpDX.Direct2D1.RoundedRectangle()
                {
                    Rect = rect,
                    RadiusX = radX,
                    RadiusY = radY
                };
                rel.renderTarget.FillRoundedRectangle(rr, brush.GetD2D(rel.renderTarget));
            }
            else throw new Exception("Internal error, DrawRectangle cannot handle " + realRenderer.GetType().ToString());
        }


        // FIXME this should go somewere in the d2d renderelements...
        private static SharpDX.DirectWrite.Factory _dwFact;
        public static SharpDX.DirectWrite.Factory dwFact
        {
            get
            {
                if(_dwFact == null)
                     _dwFact = new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Shared);
                return _dwFact;
            }
        }

        public void DrawText(UText textObject, Point location, UBrush brush, UTextDrawOptions_Enum opt)
        {
            UTextDrawOptions optStruct = opt;
            if (realRenderer is D2D_RenderElements)
            {
                var rel = realRenderer as D2D_RenderElements;
                rel.renderTarget.DrawTextLayout(location, textObject.GetD2D(dwFact), brush.GetD2D(rel.renderTarget), optStruct);
            }
            else throw new Exception("Internal error, DrawText cannot handle " + realRenderer.GetType().ToString());
        }
        public void MeasureText(UText textObject)
        {
            if (realRenderer is D2D_RenderElements)
            {
                var rel = realRenderer as D2D_RenderElements;
                var dummyForUpdatePurposesOnly = textObject.GetD2D(dwFact);
            }
            else throw new Exception("Internal error, MeasureText cannot handle " + realRenderer.GetType().ToString());
        }
    }

    public enum UTextDrawOptions_Enum { None = 1, Clip = 2, NoSnap = 4 };
    public struct UTextDrawOptions
    {
        UTextDrawOptions_Enum innerEnum;
        public static implicit operator UTextDrawOptions(UTextDrawOptions_Enum enumform)
        {
            return new UTextDrawOptions() { innerEnum = enumform };
        }
        public static implicit operator SharpDX.Direct2D1.DrawTextOptions(UTextDrawOptions me)
        {
            switch (me.innerEnum)
            {
                case UTextDrawOptions_Enum.None: return SharpDX.Direct2D1.DrawTextOptions.None;
                case UTextDrawOptions_Enum.Clip: return SharpDX.Direct2D1.DrawTextOptions.Clip;
                case UTextDrawOptions_Enum.NoSnap: return SharpDX.Direct2D1.DrawTextOptions.NoSnap;
                default: throw new NotImplementedException("UTextDrawOptions does not support specified option; " + me.innerEnum.ToString());
            }
        }
    }
    public enum UHAlign_Enum { Left, Center, Right };
    public enum UVAlign_Enum { Top, Middle, Bottom };
    public struct UHAlign
    {
        UHAlign_Enum innerEnum;
        public static implicit operator UHAlign(UHAlign_Enum enumform)
        {
            return new UHAlign() { innerEnum = enumform };
        }
        public static implicit operator SharpDX.DirectWrite.TextAlignment(UHAlign me)
        {
            switch (me.innerEnum)
            {
                case UHAlign_Enum.Left: return SharpDX.DirectWrite.TextAlignment.Leading;
                case UHAlign_Enum.Center: return SharpDX.DirectWrite.TextAlignment.Center;
                case UHAlign_Enum.Right: return SharpDX.DirectWrite.TextAlignment.Trailing;
                default: return SharpDX.DirectWrite.TextAlignment.Leading;
            }
        }
    }
    public struct UVAlign
    {
        UVAlign_Enum innerEnum;
        public static implicit operator UVAlign(UVAlign_Enum enumform)
        {
            return new UVAlign() { innerEnum = enumform };
        }
        public static implicit operator SharpDX.DirectWrite.ParagraphAlignment(UVAlign me)
        {
            switch (me.innerEnum)
            {
                case UVAlign_Enum.Top: return SharpDX.DirectWrite.ParagraphAlignment.Near;
                case UVAlign_Enum.Middle: return SharpDX.DirectWrite.ParagraphAlignment.Center;
                case UVAlign_Enum.Bottom: return SharpDX.DirectWrite.ParagraphAlignment.Far;
                default: return SharpDX.DirectWrite.ParagraphAlignment.Near;
            }
        }
    }
    public class UText 
    {
        // When -1, we need to recreate the buffered Path before returning it.
        // Otherwise 0 is sysdraw, 1 is d2d, 2 is ogl
        int storedType = -1;
        IDisposable storedText = new DumDis();
        void PropertyChanged()
        {
            storedType = -1; // reset validity of cached value
            hitPointCacheValid = hitTextCacheValid = hitTextRangeCacheValid = textInfoCacheValid = false;
        }

        private String _text;
        public String text
        {
            get { return _text; }
            set { _text = value; PropertyChanged(); }
        }
        private UFont _font;
        public UFont font
        {
            get { return _font; }
            set { _font = value; PropertyChanged(); }
        }
        private UHAlign _halign;
        public UHAlign halign
        {
            get { return _halign; }
            set { _halign = value; PropertyChanged(); }
        }
        private UVAlign _valign;
        public UVAlign valign
        {
            get { return _valign; }
            set { _valign = value; PropertyChanged(); }
        }
        private bool _wrapped;
        public bool wrapped
        {
            get { return _wrapped; }
            set { _wrapped = value; PropertyChanged(); }
        }
        private float _width;
        public float width
        {
            get { return _width; }
            set { _width = value; PropertyChanged(); }
        }
        private float _height;
        public float height 
        {
            get { return _height; }
            set { _height = value; PropertyChanged(); }
        }
        
        public UText(String text, UHAlign_Enum halign, UVAlign_Enum valign, bool isWrapped, float width, float height)
        {
            this.text = text;
            this.valign = valign;
            this.halign = halign;
            this.wrapped = isWrapped;
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// Gives hit info, like text position, given a point on the text. Usually used
        /// for mouse based caret movement and highlighting.
        /// </summary>
        /// <param name="hitPoint"></param>
        /// <returns></returns>
        /// 
        public UTextHitInfo HitPoint(Point hitPoint)
        {
            if (!hitPointCacheValid)
            {
                switch (storedType)
                {
                    case -1: throw new Exception("UText is not ready for measuring. GetXXX needs calling first.");
                    case 1: 
                        hitPointCache = HitPointD2D(hitPoint, storedText as SharpDX.DirectWrite.TextLayout);
                        break;
                    default: throw new NotImplementedException("Text type not supported by HitPoint");
                }
                hitPointCacheValid = true;
            }
            return hitPointCache;
        }
        bool hitPointCacheValid = false;
        UTextHitInfo hitPointCache;
        UTextHitInfo HitPointD2D(Point hitPoint, SharpDX.DirectWrite.TextLayout textLayout)
        {
            SharpDX.Bool trailing,inside;
            var htm = textLayout.HitTestPoint(hitPoint.X,hitPoint.Y, out trailing, out inside);
            return new UTextHitInfo()
            {
                charPos=htm.TextPosition,
                leading = hitPoint.X > htm.Left + htm.Width/2,
                isText = htm.IsText
            };
        }

        /// <summary>
        /// Gives the upper left (trailing) or upper right (leading) point of
        /// specified text position. Usually used for caret positioning.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public Point HitText(int pos, bool trailing)
        {
            if (!hitTextCacheValid)
            {
                switch (storedType)
                {
                    case -1: throw new Exception("UText is not ready for measuring. GetXXX needs calling first.");
                    case 1:
                        hitTextCache = HitTextD2D(storedText as SharpDX.DirectWrite.TextLayout, pos, trailing);
                        break;
                    default: throw new NotImplementedException("Text type not supported by HitText");
                }
                hitTextCacheValid = true;
            }
            return hitTextCache;
        }
        bool hitTextCacheValid = false;
        Point hitTextCache;
        Point HitTextD2D(SharpDX.DirectWrite.TextLayout textLayout, int pos, bool trailing)
        {
            float hx, hy;
            var htm = textLayout.HitTestTextPosition(pos, trailing, out hx, out hy);
            return new Point(hx, hy);
        }

        /// <summary>
        /// Gives collection of glyph dimensions. Usually used for highlighting text.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public IEnumerable<Rectangle> HitTextRange(int start, int length, Point offset)
        {
            if (!hitTextRangeCacheValid)
            {
                hitTextRangeCache.Clear();
                switch (storedType)
                {
                    case -1: throw new Exception("UText is not ready for measuring. GetXXX needs calling first.");
                    case 1:
                        foreach (var rect in HitTextRangeD2D(storedText as SharpDX.DirectWrite.TextLayout, start, length, offset))
                        {
                            yield return rect;
                            hitTextRangeCache.Add(rect);
                        }
                        break;
                    default: throw new NotImplementedException("Text type not supported by HitTextRange");
                }
                hitTextRangeCacheValid = true;
            }
            else
                foreach (var rect in hitTextRangeCache)
                    yield return rect;
        }
        bool hitTextRangeCacheValid = false;
        List<Rectangle> hitTextRangeCache = new List<Rectangle>(); 
        IEnumerable<Rectangle> HitTextRangeD2D(SharpDX.DirectWrite.TextLayout textLayout, int start, int length, Point offset)
        {
            foreach (var htm in textLayout.HitTestTextRange(start, length, offset.X, offset.Y))
                yield return new Rectangle(htm.Left, htm.Top, htm.Width, htm.Height);
        }

        public TextInfo GetTextInfo()
        {
            if (!textInfoCacheValid)
            {
                switch (storedType)
                {
                    case -1: throw new Exception("UText is not ready for measuring. GetXXX needs calling first.");
                    case 1: 
                        textInfoCache =  TextInfoD2D(storedText as SharpDX.DirectWrite.TextLayout);
                        break;
                    default: throw new NotImplementedException("Text type not supported by TextSize");
                }
                textInfoCacheValid = true;
            }
            return textInfoCache;
        }
        bool textInfoCacheValid = false;
        TextInfo textInfoCache;
        TextInfo TextInfoD2D(SharpDX.DirectWrite.TextLayout textLayout)
        {
            TextInfo ret = new TextInfo();

            // get size of the render
            float minHeight = 0;
            ret.numLines = 0;
            ret.lineLengths = new int[textLayout.Metrics.LineCount];
            ret.lineWrapped = new bool[textLayout.Metrics.LineCount];
            int i=0;
            foreach (var tlm in textLayout.GetLineMetrics())
            {
                minHeight += tlm.Height;
                ret.numLines++;
                ret.lineLengths[i] = tlm.Length;
                ret.lineWrapped[i] = tlm.TrailingWhitespaceLength > 0 && (i+1) < textLayout.Metrics.LineCount;
                i++;
            }
            ret.minSize = new Size(textLayout.DetermineMinWidth(), minHeight);

            return ret;
        }
        public class TextInfo
        {
            public Size minSize;
            public int numLines;
            public int[] lineLengths;
            public bool[] lineWrapped;
        }



        public SharpDX.DirectWrite.TextLayout GetD2D(SharpDX.DirectWrite.Factory dwFact)
        {
            if (storedType != 1)
            {
                storedText.Dispose();
                storedText = new SharpDX.DirectWrite.TextLayout(dwFact, text, new SharpDX.DirectWrite.TextFormat(
                    dwFact,
                    font.name,
                    font.bold ? SharpDX.DirectWrite.FontWeight.Bold : SharpDX.DirectWrite.FontWeight.Normal,
                    font.italic ? SharpDX.DirectWrite.FontStyle.Italic : SharpDX.DirectWrite.FontStyle.Normal,
                    font.size)
                {
                    ParagraphAlignment = valign,
                    TextAlignment = halign,
                    WordWrapping = wrapped ? SharpDX.DirectWrite.WordWrapping.Wrap : SharpDX.DirectWrite.WordWrapping.NoWrap
                }, width, height);
                storedType = 1;
            }
            return storedText as SharpDX.DirectWrite.TextLayout;
        }
    }
    public struct UFont
    {
        String _name;
        public String name { get { return _name; } }
        bool _bold;
        public bool bold { get { return _bold; } }
        bool _italic;
        public bool italic { get { return _italic; } }
        float _size;
        public float size { get { return _size; } }
        public UFont(String fontName, float fontSize, bool bold, bool italic)
        {
            _name = fontName;
            _bold = bold;
            _italic = italic;
            _size = fontSize;
        }
    }
    public struct UTextHitInfo
    {
        public int charPos;
        public bool leading;
        public bool isText;
    }

    public class UPath
    {
        // When -1, we need to recreate the buffered Path before returning it.
        // Otherwise 0 is sysdraw, 1 is d2d, 2 is ogl
        int storedType = -1;
        IDisposable storedPath = new DumDis();

        ObsCollection<UFigure> _figures = new ObsCollection<UFigure>();
        public ObsCollection<UFigure> figures { get { return _figures; } }

        public UPath()
        {
            _figures.collectionChanged += new System.Windows.Forms.MethodInvoker(() => storedType = -1);
        }

        public SharpDX.Direct2D1.Geometry getD2D(SharpDX.Direct2D1.Factory d2dfact)
        {
            if (storedType != 1)
            {
                var pg = new SharpDX.Direct2D1.PathGeometry(d2dfact);
                var gs = pg.Open();
                foreach (var f in figures)
                {
                    gs.BeginFigure(f.startPoint, f.filled ? SharpDX.Direct2D1.FigureBegin.Filled : SharpDX.Direct2D1.FigureBegin.Hollow);
                    foreach (var gb in f.geoElements) gb.AddMeTo(gs);
                    gs.EndFigure(f.open ? SharpDX.Direct2D1.FigureEnd.Open : SharpDX.Direct2D1.FigureEnd.Closed);
                }
                gs.Close();
                storedPath.Dispose();
                storedPath = pg;
                storedType = 1;
            }
            return storedPath as SharpDX.Direct2D1.Geometry;
        }
    }
    public class UFigure : IObservable
    {
        public event System.Windows.Forms.MethodInvoker collectionChanged;
        ObsCollection<UGeometryBase> _geoElements = new ObsCollection<UGeometryBase>();
        public ObsCollection<UGeometryBase> geoElements { get { return _geoElements; } }

        public UFigure(Point start, bool amIFilled, bool amIOpen)
        {
            startPoint = start;
            filled = amIFilled;
            open = amIOpen;
            _geoElements.collectionChanged += new System.Windows.Forms.MethodInvoker(
                () => { if (collectionChanged != null) collectionChanged(); }
                );
        }
        public Point startPoint;
        public bool filled;
        public bool open;
    }
    public class UGeometryBase 
    {
        
        public void AddMeTo(SharpDX.Direct2D1.GeometrySink geometrySink)
        {
            if (this is UArc)
            {
                UArc arc = this as UArc;
                geometrySink.AddArc(new SharpDX.Direct2D1.ArcSegment()
                {
                    SweepDirection = arc.sweepClockwise ? SharpDX.Direct2D1.SweepDirection.Clockwise : SharpDX.Direct2D1.SweepDirection.CounterClockwise,
                    RotationAngle = arc.rotation,
                    ArcSize = arc.reflex ? SharpDX.Direct2D1.ArcSize.Large : SharpDX.Direct2D1.ArcSize.Small,
                    Point = arc.endPoint,
                    Size = arc.arcSize
                });
            }
            if (this is ULine)
            {
                ULine line = this as ULine;
                geometrySink.AddLine(line.endPoint);
            }
            if (this is UBeizer)
            {
                UBeizer beizer = this as UBeizer;
                geometrySink.AddBezier(new SharpDX.Direct2D1.BezierSegment()
                {
                    Point1 = beizer.controlPoint1,
                    Point2 = beizer.controlPoint2,
                    Point3 = beizer.endPoint
                });
            }
        }
        public void AddMeTo(System.Drawing.Drawing2D.GraphicsPath graphicsPath)
        {
            throw new NotImplementedException("No not yet with SDG!");
        }
    }
    public class UArc : UGeometryBase
    {
        float _rotation = 0f;
        public float rotation
        {
            get { return _rotation; }
        }
        bool _sweepClockwise = true;
        public bool sweepClockwise
        {
            get { return _sweepClockwise; }
        }
        public UArc(bool reflex, Point endPoint, Size arcSize)
        {
            this.reflex = reflex;
            this.endPoint = endPoint;
            this.arcSize = arcSize;
        }
        public bool reflex { get; private set; }
        public Point endPoint { get; private set; }
        public Size arcSize { get; private set; }
    }
    public class ULine : UGeometryBase 
    {
        public ULine(Point endPoint)
        {
            this.endPoint = endPoint;
        }
        public Point endPoint { get; private set; }
    }
    public class UBeizer : UGeometryBase
    {
        public UBeizer(Point controlPoint1, Point controlPoint2, Point endPoint)
        {
            this.controlPoint1 = controlPoint1;
            this.controlPoint2 = controlPoint2;
            this.endPoint = endPoint;
        }
        public Point endPoint { get; private set; }
        public Point controlPoint1 { get; private set; }
        public Point controlPoint2 { get; private set; }
    }

    public enum eStrokeCaps { flat, round, triangle };
    public struct StrokeCaps
    {
        eStrokeCaps cap;
        public StrokeCaps(eStrokeCaps cap)
        {
            this.cap = cap;
        }
        public static implicit operator StrokeCaps(eStrokeCaps use)
        {
            return new StrokeCaps(use);
        }
        public static implicit operator SharpDX.Direct2D1.CapStyle(StrokeCaps me)
        {
            switch (me.cap)
            {
                case (eStrokeCaps.flat): return SharpDX.Direct2D1.CapStyle.Flat;
                case (eStrokeCaps.round): return SharpDX.Direct2D1.CapStyle.Round;
                case (eStrokeCaps.triangle): return SharpDX.Direct2D1.CapStyle.Triangle;
                default: return SharpDX.Direct2D1.CapStyle.Flat;
            }
        }
        public static implicit operator System.Drawing.Drawing2D.DashCap(StrokeCaps me)
        {
            switch (me.cap)
            {
                case (eStrokeCaps.flat): return System.Drawing.Drawing2D.DashCap.Flat;
                case (eStrokeCaps.round): return System.Drawing.Drawing2D.DashCap.Round;
                case (eStrokeCaps.triangle): return System.Drawing.Drawing2D.DashCap.Triangle;
                default: return System.Drawing.Drawing2D.DashCap.Flat;
            }
        }
        public static implicit operator System.Drawing.Drawing2D.LineCap(StrokeCaps me)
        {
            switch (me.cap)
            {
                case (eStrokeCaps.flat): return System.Drawing.Drawing2D.LineCap.Flat;
                case (eStrokeCaps.round): return System.Drawing.Drawing2D.LineCap.Round;
                case (eStrokeCaps.triangle): return System.Drawing.Drawing2D.LineCap.Triangle;
                default: return System.Drawing.Drawing2D.LineCap.Flat;
            }
        }
    }
    public enum eStrokeType { solid, custom, dash, dashdot, dashdotdot, dot };
    public struct StrokeType
    {
        eStrokeType type;
        public StrokeType(eStrokeType type)
        {
            this.type = type;
        }
        public static implicit operator StrokeType(eStrokeType use)
        {
            return new StrokeType(use);
        }
        public static implicit operator eStrokeType(StrokeType me)
        {
            return me.type;
        }
        public static implicit operator SharpDX.Direct2D1.DashStyle(StrokeType me)
        {
            switch (me.type)
            {
                case (eStrokeType.solid): return SharpDX.Direct2D1.DashStyle.Solid;
                case (eStrokeType.custom): return SharpDX.Direct2D1.DashStyle.Custom;
                case (eStrokeType.dash): return SharpDX.Direct2D1.DashStyle.Dash;
                case (eStrokeType.dashdot): return SharpDX.Direct2D1.DashStyle.DashDot;
                case (eStrokeType.dashdotdot): return SharpDX.Direct2D1.DashStyle.DashDotDot;
                case (eStrokeType.dot): return SharpDX.Direct2D1.DashStyle.Dot;
                default: return SharpDX.Direct2D1.DashStyle.Solid;
            }
        }
        public static implicit operator System.Drawing.Drawing2D.DashStyle(StrokeType me)
        {
            switch (me.type)
            {
                case (eStrokeType.solid): return System.Drawing.Drawing2D.DashStyle.Solid;
                case (eStrokeType.custom): return System.Drawing.Drawing2D.DashStyle.Custom;
                case (eStrokeType.dash): return System.Drawing.Drawing2D.DashStyle.Dash;
                case (eStrokeType.dashdot): return System.Drawing.Drawing2D.DashStyle.DashDot;
                case (eStrokeType.dashdotdot): return System.Drawing.Drawing2D.DashStyle.DashDotDot;
                case (eStrokeType.dot): return System.Drawing.Drawing2D.DashStyle.Dot;
                default: return System.Drawing.Drawing2D.DashStyle.Solid;
            }
        }
    }
    public enum eStrokeJoin { bevel, mitre, round };
    public struct StrokeJoin
    {
        eStrokeJoin join;
        public StrokeJoin(eStrokeJoin join)
        {
            this.join = join;
        }
        public static implicit operator StrokeJoin(eStrokeJoin use)
        {
            return new StrokeJoin(use);
        }
        public static implicit operator SharpDX.Direct2D1.LineJoin(StrokeJoin me)
        {
            switch (me.join)
            {
                case (eStrokeJoin.bevel): return SharpDX.Direct2D1.LineJoin.Bevel;
                case (eStrokeJoin.mitre): return SharpDX.Direct2D1.LineJoin.Miter;
                case (eStrokeJoin.round): return SharpDX.Direct2D1.LineJoin.Round;
                default: return SharpDX.Direct2D1.LineJoin.Bevel;
            }
        }
        public static implicit operator System.Drawing.Drawing2D.LineJoin(StrokeJoin me)
        {
            switch (me.join)
            {
                case (eStrokeJoin.bevel): return System.Drawing.Drawing2D.LineJoin.Bevel;
                case (eStrokeJoin.mitre): return System.Drawing.Drawing2D.LineJoin.Miter;
                case (eStrokeJoin.round): return System.Drawing.Drawing2D.LineJoin.Round;
                default: return System.Drawing.Drawing2D.LineJoin.Bevel;
            }
        }
    }

    /// <summary>
    /// Keeps a strokestyle, which singletons until properties are changed in which case it regenerates.
    /// </summary>
    public class UStroke
    {
        // When -1, we need to recreate the buffered strokestyle before returning it.
        // Otherwise 0 is sysdraw, 1 is d2d, 2 is ogl
        int storedType = -1;
        IDisposable storedStroke = new DumDis();

        // Stroke Properties
        float _strokeWidth = 1f;
        public float strokeWidth 
        { 
            get { return _strokeWidth; } 
            set 
            { 
                _strokeWidth = value; 
                storedType = -1; 
            } 
        }

        StrokeCaps _startCap = eStrokeCaps.flat;
        public StrokeCaps startCap
        {
            get { return _startCap; }
            set
            {
                _startCap = value;
                storedType = -1;
            }
        }

        StrokeCaps _endCap = eStrokeCaps.flat;
        public StrokeCaps endCap
        {
            get { return _endCap; }
            set
            {
                _endCap = value;
                storedType = -1;
            }
        }

        StrokeCaps _dashCap = eStrokeCaps.flat;
        public StrokeCaps dashCap
        {
            get { return _dashCap; }
            set
            {
                _dashCap = value;
                storedType = -1;
            }
        }

        float _offset = 0f;
        float offset
        {
            get { return _offset; }
            set
            {
                _offset = value;
                storedType = -1;
            }
        }

        StrokeType _dashStyle = eStrokeType.solid;
        public StrokeType dashStyle
        {
            get { return _dashStyle; }
            set
            {
                _dashStyle = value;
                storedType = -1;
            }
        }

        float[] _custom = new float[] { 1f, 1f };
        public float[] custom
        {
            get { return _custom; }
            set
            {
                _custom = value;
                storedType = -1;
            }
        }

        StrokeJoin _lineJoin = eStrokeJoin.mitre;
        public StrokeJoin lineJoin
        {
            get { return _lineJoin; }
            set
            {
                _lineJoin = value;
                storedType = -1;
            }
        }

        float _mitreLimit = 0f;
        public float mitreLimit
        {
            get { return _mitreLimit; }
            set
            {
                _mitreLimit = value;
                storedType = -1;
            }
        }

        public SharpDX.Direct2D1.StrokeStyle Get_D2D(SharpDX.Direct2D1.Factory d2dfact)
        {
            if (storedType != 1)
            {
                var sp = new SharpDX.Direct2D1.StrokeStyleProperties()
                {
                    DashCap = dashCap,
                    EndCap = endCap,
                    StartCap = startCap,
                    LineJoin = lineJoin,
                    MiterLimit = mitreLimit,
                    DashStyle = dashStyle,
                    DashOffset = offset
                };
                storedStroke.Dispose();
                if(dashStyle == eStrokeType.custom)
                    storedStroke = new SharpDX.Direct2D1.StrokeStyle(d2dfact, sp, custom);
                else
                    storedStroke = new SharpDX.Direct2D1.StrokeStyle(d2dfact, sp);
                storedType = 1;
            }
            return storedStroke as SharpDX.Direct2D1.StrokeStyle;
        }

        public System.Drawing.Pen Get_SysDraw(UBrush brush)
        {
            if (storedType != 0)
            {
                storedStroke.Dispose();
                storedStroke = new System.Drawing.Pen(brush.GetSDG(), strokeWidth)
                {
                    LineJoin = lineJoin,
                    MiterLimit = mitreLimit,
                    DashCap = dashCap,
                    StartCap = startCap,
                    EndCap = endCap,
                    DashStyle = dashStyle,
                    DashOffset = offset,
                    DashPattern = custom
                };
                storedType = 0;
            }
            return storedStroke as System.Drawing.Pen;
        }
    }

    class DumDis : IDisposable
    {
        public void Dispose()
        {
            // I do nothing but placehold :)
        }
    }

    public abstract class UBrush
    {
        // When -1, we need to recreate the buffered brush before returning it.
        // Otherwise 0 is sysdraw, 1 is d2d, 2 is ogl
        protected int storedType = -1;
        IDisposable storedBrush = new DumDis();

        public SharpDX.Direct2D1.Brush GetD2D(SharpDX.Direct2D1.RenderTarget rt)
        {
            if (storedType != 1)
            {
                storedBrush.Dispose();
                storedBrush = CreateD2D(rt);
                storedType = 1;
            }
            return storedBrush as SharpDX.Direct2D1.Brush;
        }
        public System.Drawing.Brush GetSDG() 
        {
            if (storedType != 0)
            {
                storedBrush.Dispose();
                storedBrush = CreateSDG();
                storedType = 0;
            }
            return storedBrush as System.Drawing.Brush;
        }
        
        protected abstract SharpDX.Direct2D1.Brush CreateD2D(SharpDX.Direct2D1.RenderTarget rt);
        protected abstract System.Drawing.Brush CreateSDG();
    }
    public class USolidBrush : UBrush
    {
        Color _color = new Color(1); // white... 
        public Color color { get { return _color; } set { _color = value; storedType = -1; } }

        protected override SharpDX.Direct2D1.Brush CreateD2D(SharpDX.Direct2D1.RenderTarget rt)
        {
            return new SharpDX.Direct2D1.SolidColorBrush(rt, color);
        }
        protected override System.Drawing.Brush CreateSDG()
        {
            return new System.Drawing.SolidBrush(color);
        }
    }
    public class ULinearGradientBrush : UBrush
    {
        Color _color1 = new Color(1);
        public Color color1 { get { return _color1; } set { _color1 = value; storedType = -1; } }
        Color _color2 = new Color(0);
        public Color color2 { get { return _color2; } set { _color2 = value; storedType = -1; } }

        Point _point1 = new Point(0, 0);
        public Point point1 { get { return _point1; } set { _point1 = value; storedType = -1; } }
        Point _point2 = new Point(0, 0);
        public Point point2 { get { return _point2; } set { _point2 = value; storedType = -1; } }

        protected override SharpDX.Direct2D1.Brush CreateD2D(SharpDX.Direct2D1.RenderTarget rt)
        {
            SharpDX.Direct2D1.LinearGradientBrush lgb = new SharpDX.Direct2D1.LinearGradientBrush(rt,
                new SharpDX.Direct2D1.LinearGradientBrushProperties()
                {
                    StartPoint = point1,
                    EndPoint = point2
                },
                new SharpDX.Direct2D1.GradientStopCollection(rt,
                    new SharpDX.Direct2D1.GradientStop[] { 
                        new SharpDX.Direct2D1.GradientStop() { Color = color1, Position = 0f },
                        new SharpDX.Direct2D1.GradientStop() { Color = color2, Position = 1f } },
                     SharpDX.Direct2D1.Gamma.StandardRgb,
                     SharpDX.Direct2D1.ExtendMode.Clamp)
                     );
            return lgb;
        }

        /// <summary>
        /// only capable of two points two colors
        /// </summary>
        /// <returns></returns>
        protected override System.Drawing.Brush CreateSDG()
        {
            var lgb = new System.Drawing.Drawing2D.LinearGradientBrush(point1, point2, color1, color2)
            {
                WrapMode = System.Drawing.Drawing2D.WrapMode.Clamp
            };
            return lgb;
        }
    }

    /// <summary>
    ///  instance a bitmap!
    /// </summary>
    public class UBitmap
    {
        static SharpDX.WIC.ImagingFactory wicFact = new SharpDX.WIC.ImagingFactory();
        byte[] bitmapData = null;
        String bitmapFile = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="storeInMemory">Only use if this resource will be accessed often by different renderers, saving disk access time but using more system memory.</param>
        public UBitmap(string filePath, bool storeInMemory = false)
        {
            if (storeInMemory) bitmapData = System.IO.File.ReadAllBytes(filePath);
            else bitmapFile = filePath;
        }
        private UBitmap()
        {
        }

        // When -1, we need to recreate the buffered bitmap before returning it.
        // Otherwise 0 is sysdraw, 1 is d2d, 2 is ogl
        protected int storedType = -1;
        IDisposable storedBitmap = new DumDis();

        public SharpDX.Direct2D1.Bitmap GetD2D(SharpDX.Direct2D1.RenderTarget rt) 
        {
            if(storedType != 1) 
            {
                storedBitmap.Dispose();
                System.Drawing.Bitmap bm;
                if (bitmapData != null)
                {
                    System.IO.MemoryStream ms = new System.IO.MemoryStream(bitmapData);
                    bm = new System.Drawing.Bitmap(ms);
                }
                else bm = new System.Drawing.Bitmap(bitmapFile);
                SharpDX.WIC.Bitmap wbm = new SharpDX.WIC.Bitmap(wicFact, bm, SharpDX.WIC.BitmapAlphaChannelOption.UseAlpha);
                storedBitmap = SharpDX.Direct2D1.Bitmap.FromWicBitmap(rt, wbm);
                storedType = 1;
            }
            return storedBitmap as SharpDX.Direct2D1.Bitmap;
        }
        public System.Drawing.Bitmap GetSDG()
        {
            if (storedType != 0)
            {
                storedBitmap.Dispose();
                if (bitmapData != null)
                {
                    System.IO.MemoryStream ms = new System.IO.MemoryStream(bitmapData);
                    storedBitmap = new System.Drawing.Bitmap(ms);
                }
                else storedBitmap = new System.Drawing.Bitmap(bitmapFile);
                storedType = 0;
            }
            return storedBitmap as System.Drawing.Bitmap;
        }
    }

    public enum UInterpModes { Linear, Nearest };
    public struct UInterp
    {
        UInterpModes mode;
        public UInterp(UInterpModes mode)
        {
            this.mode = mode;
        }
        public static implicit operator SharpDX.Direct2D1.BitmapInterpolationMode(UInterp me)
        {
            switch (me.mode)
            {
                case UInterpModes.Linear: return SharpDX.Direct2D1.BitmapInterpolationMode.Linear;
                case UInterpModes.Nearest: return SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor;
                default: return SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor;
            }
        }
        public static implicit operator System.Drawing.Drawing2D.InterpolationMode(UInterp me)
        {
            switch (me.mode)
            {
                case UInterpModes.Linear: return  System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                case UInterpModes.Nearest: return System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                default: return System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            }
        }
    }
    

    /// <summary>
    /// Blank interface, the backing to renderes are these only.
    /// </summary>
    public interface IRenderElements
    {
    }
    public class D2D_RenderElements : IRenderElements
    {
        public D2D_RenderElements(SharpDX.Direct2D1.RenderTarget rt)
        {
            renderTarget = rt;
        }
        public SharpDX.Direct2D1.RenderTarget renderTarget { get; internal set; }
    }
    public class SDG_RenderElements : IRenderElements
    {
        public SDG_RenderElements(System.Drawing.Graphics gr)
        {
            graphics = gr;
        }
        public System.Drawing.Graphics graphics { get; internal set; }
    }
    public class OGL2D_RenderElements : IRenderElements
    {
    }
}
