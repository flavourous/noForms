using System;
using System.Text;
using System.Collections.Generic;


namespace NoForms.Common
{
    public enum WindowBorderStyle { Sizable, Fixed, NoBorder };
    public delegate void NoFormsAction();
    public class WindowCreateOptions
    {
        public WindowCreateOptions(bool showInTaskbar, WindowBorderStyle windowBorderStyle)
        {
            this.showInTaskbar = showInTaskbar;
            this.windowBorderStyle = windowBorderStyle;
        }
        public bool showInTaskbar { get; private set; }
        public WindowBorderStyle windowBorderStyle { get; private set; }
    }

    [Flags]
    public enum Direction { NONE = 0, NORTH = 1, SOUTH = 2, EAST = 4, WEST = 8 }; // bitmask

    public enum MouseButton { NONE, LEFT, RIGHT };
    public enum ButtonState { NONE, DOWN, UP };

    public struct Color
    {
        public float a, r, g, b;
        public Color(float a, float r, float g, float b)
        {
            this.a = a;
            this.r = r;
            this.g = g;
            this.b = b;
        }
        public Color(float grey)
        {
            this.a = 1;
            r = g = b = grey;
        }

        public Color Scale(float signFactor)
        {
            return new Color(
                a,
                r + (signFactor > 0 ? (1.0f - r) * signFactor : r * signFactor),
                g + (signFactor > 0 ? (1.0f - g) * signFactor : g * signFactor),
                b + (signFactor > 0 ? (1.0f - b) * signFactor : b * signFactor)
                );
        }
        public Color Scale(float fr, float fg, float fb)
        {
            return new Color(a, r * fr, g * fg, b * fb);
        }
        public Color Add(float ar, float ag, float ab)
        {
            return new Color(a, fb(r + ar), fb(g + ag), fb(b + ab));
        }
        public Color Add(float ar, float ag, float ab, float baseVal)
        {
            return new Color(a, fb(r + ar / baseVal), fb(g + ag / baseVal), fb(b + ab / baseVal));
        }
        float fb(float inp)
        {
            if (inp > 1) inp = 1;
            if (inp < 0) inp = 0;
            return inp;
        }

        //public static implicit operator Color(SharpDX.Color4 input)
        //{
        //    return new Color(input.Alpha, input.Red, input.Green, input.Blue);
        //}
        //public static implicit operator SharpDX.Color4(Color input)
        //{
        //    return new SharpDX.Color4(input.r, input.g, input.b, input.a);
        //}
        //public static implicit operator SharpDX.Color(Color input)
        //{
        //    return new SharpDX.Color(input.r, input.g, input.b, input.a);
        //}
        //public static implicit operator System.Drawing.Color(Color input)
        //{
        //    int a = (int)Math.Round(input.a * 255);
        //    int r = (int)Math.Round(input.r * 255);
        //    int g = (int)Math.Round(input.g * 255);
        //    int b = (int)Math.Round(input.b * 255);
        //    return System.Drawing.Color.FromArgb(a, r, g, b);
        //}
        //public static implicit operator Color(System.Drawing.Color input)
        //{
        //    return new Color(input.A / 255f, input.R / 255f, input.G / 255f, input.B / 255f);
        //}
    }

    public struct Size
    {
        public static Size Empty { get { return new Size(0, 0); } }
        public Size(float width, float height)
        {
            this.width = width;
            this.height = height;
        }
        public float height;
        public float width;

        public override bool Equals(object obj)
        {
            if (!(obj is Size)) return false;
            Size s = (Size)obj;
            if (!s.width.Equals(width)) return false;
            if (!s.height.Equals(height)) return false;
            return true;
        }
        public override int GetHashCode()
        {
            int hc = 17;
            hc = hc * 31 + height.GetHashCode();
            hc = hc * 31 + width.GetHashCode();
            return hc;
        }

        //public static implicit operator System.Drawing.Size(Size me)
        //{
        //    return new System.Drawing.Size((int)(me.width + .5f), (int)(me.height + .5f));
        //}
        //public static implicit operator Size(System.Drawing.Size you)
        //{
        //    return new Size(you.Width, you.Height);
        //}
        //public static implicit operator SharpDX.DrawingSizeF(Size me)
        //{
        //    return new SharpDX.DrawingSizeF(me.width, me.height);
        //}
        //public static implicit operator Size(System.Drawing.SizeF me)
        //{
        //    return new Size(me.Width, me.Height);
        //}
        //public static implicit operator System.Drawing.SizeF(Size me)
        //{
        //    return new System.Drawing.SizeF(me.width, me.height);
        //}
    }
    public struct Point
    {
        public static Point Zero { get { return new Point(0, 0); } }
        public Point(float x, float y)
        {
            X = x;
            Y = y;
        }
        public float X;
        public float Y;

        public override string ToString()
        {
            return String.Format("({0},{1})", X, Y);
        }

        public static Point operator -(Point me, Point other)
        {
            return new Point(me.X - other.X, me.Y - other.Y);
        }
        public static Point operator +(Point me, Point other)
        {
            return new Point(me.X + other.X, me.Y + other.Y);
        }

        //public static implicit operator System.Drawing.PointF(Point me)
        //{
        //    return new System.Drawing.PointF(me.X, me.Y);
        //}
        //public static implicit operator Point(System.Drawing.PointF you)
        //{
        //    return new Point(you.X, you.Y);
        //}

        //public static implicit operator Point(System.Drawing.Point you)
        //{
        //    return new Point(you.X, you.Y);
        //}
        //public static implicit operator SharpDX.DrawingPointF(Point me)
        //{
        //    return new SharpDX.DrawingPointF(me.X, me.Y);
        //}
        //public static implicit operator System.Drawing.Point(Point me)
        //{
        //    return new System.Drawing.Point((int)(me.X + .5f), (int)(me.Y + .5f));
        //}
    }
    public struct Thickness
    {
        public Thickness(float amt)
        {
            this.top = this.left = this.right = this.bottom = amt;
        }
        public Thickness(float left, float top, float right, float bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }
        public float left;
        public float top;
        public float right;
        public float bottom;
    }
    public struct Rectangle
    {
        /// <summary>
        /// combines bounds of the rects
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public Rectangle Combine(Rectangle other)
        {
            return new Rectangle(
                new Point(
                    Math.Min(left,other.left),
                    Math.Min(top,other.top)
                ),
                new Point(
                    Math.Max(right,other.right),
                    Math.Max(bottom,other.bottom)
                )
            );
        }

        public bool Intersects(Rectangle r)
        {
            return r.left < right && r.right > left && r.top < bottom && r.bottom > top;
        }
        public bool Contains(Point pt)
        {
            if (pt.X >= left && pt.X <= right)
                if (pt.Y >= top && pt.Y <= bottom)
                    return true;
            return false;
        }
        public override string ToString()
        {
            return "(" + left + "," + top + "; " + width + "x" + height + ")";
        }

        public static Rectangle operator -(Rectangle me, Point subby)
        {
            return new Rectangle(me.left - subby.X, me.top - subby.Y, me.width, me.height);
        }

        public static Rectangle Empty
        {
            get
            {
                return new Rectangle(0, 0, 0, 0);
            }
        }

        public Rectangle(Point p, Size s)
        {
            Location = p;
            Size = s;
        }

        public Rectangle(float x, float y, float width, float height)
        {
            Location = new Point(x, y);
            Size = new Size(width, height);
        }

        public Rectangle(Point p1, Point p2, bool allowNegative = false)
        {
            float x1 = p1.X;
            float x2 = p2.X;
            float y1 = p1.Y;
            float y2 = p2.Y;

            float x, y, w, h;

            if (allowNegative)
            {
                x = x1;
                y = y1;
                w = x2 - x1;
                h = y2 - y1;
            }
            else
            {
                x = x1 > x2 ? x2 : x1;
                y = y1 > y2 ? y2 : y1;
                w = Math.Abs(x1 - x2);
                h = Math.Abs(y1 - y2);
            }

            Location = new Point(x, y);
            Size = new Size(w, h);
        }

        public float left
        {
            get { return Location.X; }
            set { Location = new Point(value, top); }
        }
        public float right
        {
            get { return Location.X + Size.width; }
            set { width = value - left; }
        }
        public float top
        {
            get { return Location.Y; }
            set { Location = new Point(left, value); }
        }
        public float bottom
        {
            get { return Location.Y + Size.height; }
            set { height = value - top; }
        }
        public float width
        {
            get { return Size.width; }
            set { Size = new Size(value, Size.height); }
        }
        public float height
        {
            get { return Size.height; }
            set { Size = new Size(Size.width, value); }
        }

        public Size Size;
        public Point Location;

        public Rectangle Inflated(Thickness amount)
        {
            return new Rectangle(left - amount.left, top - amount.top, width + amount.left + amount.right, height + amount.top + amount.bottom);
        }
        public Rectangle Deflated(Thickness amount)
        {
            Rectangle ret = new Rectangle(left, top, width, height);
            ret.left += amount.left;
            ret.top += amount.top;
            ret.width -= amount.right + amount.left;
            ret.height -= amount.bottom + amount.top;
            return ret;
        }

        

        // implicits
        //public static implicit operator SharpDX.RectangleF(Rectangle me)
        //{
        //    return new SharpDX.RectangleF(me.left, me.top, me.right, me.bottom);
        //}
        //public static implicit operator System.Drawing.RectangleF(Rectangle me)
        //{
        //    return new System.Drawing.RectangleF(me.left, me.top, me.width, me.height);
        //}
    }

    // struct for the assignment semantics
    public class Region
    {
        StringBuilder sb = new StringBuilder();
        public override string ToString()
        {
            sb.Length = 0;
            foreach (var r in rects)
                sb.AppendFormat("{0:f1},{1:f1} {2:f1}x{3:f1}   ", r.left, r.top, r.width, r.height);
            return sb.ToString();
        }
        public Region()
        {
            rects = new List<Rectangle>();
        }
        public Region(Region toCopy)
        {
            rects = new List<Rectangle>(toCopy.rects);
        }
        List<Rectangle> rects;
        public IEnumerable<Rectangle> AsRectangles()
        {
            return rects;
        }
        public void Add(Rectangle r) 
        {
            foreach (var er in rects)
                if (er.left <= r.left && er.right >= r.right && er.top <= r.top && er.bottom >= r.bottom)
                    return; // because this one is completely contained by an existing one!

            for (int i = 0; i < rects.Count; i++)
            {
                var rr = rects[i];
                if (r.left <= rr.left && r.right >= rr.right && r.top <= rr.top && r.bottom >= rr.bottom)
                    rects.RemoveAt(i--); // because existing one is completely contained by this new one we will add!
            }

            rects.Add(r);

            // Shattering logic...clever but not good enough.
            //IEnumerable<Rectangle> toadd = new Rectangle[] { r };
            //foreach(var er in rects)
            //    toadd = MultiExclude(er, toadd);
            //rects.AddRange(toadd); 
        }
        static List<Rectangle> MultiExclude(Rectangle excl, IEnumerable<Rectangle> from)
        {
            List<Rectangle> lr = new List<Rectangle>();
            foreach (var r in from)
                foreach (var rr in Exclude(excl, r))
                    lr.Add(rr);
            return lr;
        }
        public static List<Rectangle> Exclude(Rectangle exclusion, Rectangle from)
        {

            //   EX CONTAINED BY FRM         
            //                               
            //   frm (outside)               
            //   -------------------------   
            //   |1a  |2a           |3a  |   
            //   |    |             |    |   
            //   |    |             |    |   
            //   |  1b|           2b|  3b|   
            //   |----|-------------|----|   
            //   |4a  |ex (inside)  |5a  |   
            //   |    |             |    |   
            //   |    |             |    |   
            //   |    |             |    |   
            //   |    |             |    |   
            //   |    |             |    |   
            //   |    |             |    |   
            //   |    |             |    |   
            //   |  4b|             |  5b|   
            //   |----|-------------|----|   
            //   |6a  |7a           |8a  |   
            //   |    |             |    |   
            //   |    |             |    |   
            //   |    |             |    |   
            //   |  6b|           7b|  8b|   
            //   -------------------------   

            // Try and create the 8 regions (corner and sides)
            // if out of range, it wont make sense so dont create.

            // FIXME there only need to be 4 regions infact.  so many optimises possible here...needed?

            // These constrain the exclusion rect to within the from rect, when there is axial intersection.
            float ex_top = exclusion.top < from.top && from.top < exclusion.bottom ? from.top : exclusion.top;
            float ex_bot = exclusion.bottom > from.bottom && from.bottom > exclusion.top ? from.bottom : exclusion.bottom;
            float ex_lft = exclusion.left < from.left && from.left < exclusion.right ? from.left : exclusion.left;
            float ex_rht = exclusion.right > from.right && from.right > exclusion.left ? from.right : exclusion.right;

            // FIXME less object spawning please
            List<Rectangle> lr = new List<Rectangle>();
            Rectangle nr;
            if ((nr = new Rectangle(new Point(from.left, from.top), new Point(ex_lft, ex_top), true)).width > 0 && nr.height > 0) lr.Add(nr);
            if ((nr = new Rectangle(new Point(ex_lft, from.top), new Point(ex_rht, ex_top), true)).width > 0 && nr.height > 0) lr.Add(nr);
            if ((nr = new Rectangle(new Point(ex_rht, from.top), new Point(from.right, ex_top), true)).width > 0 && nr.height > 0) lr.Add(nr);
            if ((nr = new Rectangle(new Point(from.left, ex_top), new Point(ex_lft, ex_bot), true)).width > 0 && nr.height > 0) lr.Add(nr);
            if ((nr = new Rectangle(new Point(ex_rht, ex_top), new Point(from.right, ex_bot), true)).width > 0 && nr.height > 0) lr.Add(nr);
            if ((nr = new Rectangle(new Point(from.left, ex_bot), new Point(ex_lft, from.bottom), true)).width > 0 && nr.height > 0) lr.Add(nr);
            if ((nr = new Rectangle(new Point(ex_lft, ex_bot), new Point(ex_lft, ex_bot), true)).width > 0 && nr.height > 0) lr.Add(nr);
            if ((nr = new Rectangle(new Point(ex_rht, ex_bot), new Point(from.right, from.bottom), true)).width > 0 && nr.height > 0) lr.Add(nr);
            return lr;
        }
        public void Reset() { rects.Clear(); }
        public bool Intersects(Rectangle r)
        {
            foreach (var rr in rects)
                if (rr.Intersects(r))
                    return true;
            return false;
        }
        public bool IsEmpty { get { return rects.Count == 0; } }
    }

    public class UStyleRange : IObservable
    {
        int _start;
        public int start { get { return _start; } set { _start = value; changed(); } }
        int _length;
        public int length { get { return _length; } set { _length = value; changed(); } }
        UFont? _fontOverride;
        public UFont? fontOverride { get { return _fontOverride; } set { _fontOverride = value; changed(); } }
        UBrush _fgOverride;
        public UBrush fgOverride { get { return _fgOverride; } set { _fgOverride = value; changed(); } }
        UBrush _bgOverride;
        public UBrush bgOverride { get { return _bgOverride; } set { _bgOverride = value; changed(); } }

        public UStyleRange(int start, int length, UFont? font, UBrush foreground, UBrush background)
        {
            _start = start;
            _length = length;
            _fontOverride = font;
            _fgOverride = foreground;
            _bgOverride = background;
        }
        public UStyleRange(int start, int length, UStyleRange cloneFrom)
        {
            _fontOverride = cloneFrom.fontOverride;
            _bgOverride = cloneFrom.bgOverride;
            _fgOverride = cloneFrom.fgOverride;
        }
        public event VoidAction changed;
    }
    // Cached base object for drawing objects
    public class DDis : IDisposable { public void Dispose() { } }
    public delegate IDisposable NoCacheDelegate();
    public abstract class ObjStore : IDisposable, IObservable
    {
        Type storedType = null; // nothing stored to begin with
        bool storedValid = false;
        IDisposable storedObject = new DDis(); // nothing stored...
        Object validationLock = new Object(); // FIXME actually, it's up to framework not to destroy the Retrieved value until it's done with.
        public event VoidAction changed = delegate { };
        public void Invalidate()
        {
            lock (validationLock)
            {
                storedValid = false;
                changed();
            }
        }
        Object[] myExtraInvals = new object[0];
        bool ExtraCheck(Object[] einval)
        {
            if (einval.Length != myExtraInvals.Length)
                return false;

            for (int i = 0; i < einval.Length; i++)
                if (!einval[i].Equals(myExtraInvals[i]))
                    return false;

            return true;
        }
        // just get cached one
        public Object Retreive()
        {
            return storedObject;
        }
        public Object Retreive<RetreiverType>(NoCacheDelegate noCacheAction, params Object[] extraInval) where RetreiverType : class
        {
            lock (validationLock)
            {
                if (!storedValid || !ExtraCheck(extraInval) || storedType != typeof(RetreiverType))
                {
                    Dispose();
                    storedObject = noCacheAction();
                    storedValid = true;
                    storedType = typeof(RetreiverType);
                }
                return storedObject;
            }
        }
        public void Dispose()
        {
            storedObject.Dispose();
        }


    }

    // Text options
    public enum UTextDrawOptions { None = 1, Clip = 2, NoSnap = 4 };
    public enum UHAlign { Left, Center, Right };
    public enum UVAlign { Top, Middle, Bottom };
    public class UText : ObjStore
    {
        Object becauseWeNeedThreadSafty = new object();

        private String _text;
        public String text { get { return _text; } set { _text = value; Invalidate(); } }
        private UFont _font;
        public UFont font { get { return _font; } set { _font = value; Invalidate(); } }
        private UHAlign _halign;
        public UHAlign halign { get { return _halign; } set { _halign = value; Invalidate(); } }
        private UVAlign _valign;
        public UVAlign valign { get { return _valign; } set { _valign = value; Invalidate(); } }
        private bool _wrapped;
        public bool wrapped { get { return _wrapped; } set { _wrapped = value; Invalidate(); } }
        private float _width;
        public float width { get { return _width; } set { _width = value; Invalidate(); } }
        private float _height;
        public float height { get { return _height; } set { _height = value; Invalidate(); } }

        /// <summary>
        /// applied in order
        /// </summary>
        private ObsCollection<UStyleRange> _styleRanges = new ObsCollection<UStyleRange>();
        public ObsCollection<UStyleRange> styleRanges { get { return _styleRanges; } }
        void _styleRanges_collectionChanged() { Invalidate(); }
        public IEnumerable<UStyleRange> SafeGetStyleRanges
        { // "best" effort.  should lock. FIXME
            get
            {
                for (int i = 0; i < styleRanges.Count; i++)
                {
                    // Grab this volatile element
                    UStyleRange sr = null;
                    try { sr = styleRanges[i]; }
                    catch { continue; }
                    if (sr == null) continue;
                    yield return sr;
                }
            }
        }

        public UText(String text, UHAlign halign, UVAlign valign, bool isWrapped, float width, float height)
        {
            this.text = text;
            this.valign = valign;
            this.halign = halign;
            this.wrapped = isWrapped;
            this.width = width;
            this.height = height;
            _styleRanges.changed += new VoidAction(_styleRanges_collectionChanged);
        }
    }


    public class UTextInfo
    {
        public Size minSize = new Size(0, 0);
        public int numLines = 0;
        public int[] lineLengths = new int[0];
        public int[] lineNewLineLength = new int[0];
    }

    public struct UFont
    {
        String _name;
        public String name { get { return _name; } } bool _bold;
        public bool bold { get { return _bold; } } bool _italic;
        public bool italic { get { return _italic; } } float _size;
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
        public UTextHitInfo(int charPos, bool leading, bool isText)
        {
            this.charPos = charPos;
            this.leading = leading;
            this.isText = isText;
        }
        public int charPos;
        public bool leading;
        public bool isText;
    }
    public class UPath : ObjStore
    {
        ObsCollection<UFigure> _figures = new ObsCollection<UFigure>();
        public ObsCollection<UFigure> figures { get { return _figures; } }
        public UPath()
        {
            _figures.changed += new VoidAction(Invalidate);
        }
    }
    public class UFigure : IObservable
    {
        public event VoidAction changed = delegate { };
        ObsCollection<UGeometryBase> _geoElements = new ObsCollection<UGeometryBase>();
        public ObsCollection<UGeometryBase> geoElements { get { return _geoElements; } }
        public UFigure(Point start, bool amIFilled, bool amIOpen)
        {
            _startPoint = start;
            _filled = amIFilled;
            _open = amIOpen;
            _geoElements.changed += new VoidAction(changed);
        }
        Point _startPoint;
        public Point startPoint { get { return _startPoint; } set { _startPoint = value; changed(); } }
        bool _filled;
        public bool filled { get { return _filled; } set { _filled = value; changed(); } }
        bool _open;
        public bool open { get { return _open; } set { _open = value; changed(); } }
    }
    public abstract class UGeometryBase : ObjStore { }
    public class disParr : IDisposable
    {
        public disParr(System.Drawing.PointF[] pts) { this.pts = pts; }
        public System.Drawing.PointF[] pts;
        public void Dispose() { }
    }
    //public class UEasyArc : UArcBase
    //{
    //    public UEasyArc(float startAngle, float endAngle, Size arcSize, bool reflex, bool clockwise, float rotation, float resolution)
    //    {
    //        this.startAngle = startAngle;
    //        this.endAngle = endAngle;
    //        this.arcSize = arcSize;
    //        this.rotation = rotation;
    //        this.resolution = resolution;
    //    }
    //    float _startAngle;
    //    public float startAngle { get { return _startAngle; } private set { _startAngle = value; Invalidate(); } }
    //    float _endAngle;
    //    public float endAngle { get { return _endAngle; } private set { _endAngle = value; Invalidate(); } }

    //}
    public abstract class UArcBase : UGeometryBase
    {
        float _rotation;
        public float rotation { get { return _rotation; } protected set { _rotation = value; Invalidate(); } }
        Size _arcSize;
        public Size arcSize { get { return _arcSize; } protected set { _arcSize = value; Invalidate(); } }
        float _resolution = 0.1f;
        public float resolution { get { return _resolution; } set { _resolution = value; Invalidate(); } }
    }
    public class UArc : UArcBase
    {
        public UArc(Point endPoint, Size arcSize, bool reflex, bool clockwise, float rotation)
        {
            this.reflex = reflex;
            this.sweepClockwise = clockwise;
            this.endPoint = endPoint;
            this.arcSize = arcSize;
            this.rotation = rotation;
        }
        bool _sweepClockwise;
        public bool  sweepClockwise {get { return _sweepClockwise;} private set { _sweepClockwise = value; Invalidate();}}
        bool _reflex;
        public bool  reflex {get { return _reflex;} private set { _reflex = value; Invalidate();}}
        Point _endPoint;
        public Point  endPoint {get { return _endPoint;} private set { _endPoint = value; Invalidate();}}
    }
    public class ULine : UGeometryBase
    {
        public ULine(Point endPoint)
        {
            this.endPoint = endPoint;
        }
        public Point _endPoint;
        public Point  endPoint {get { return _endPoint;} private set { _endPoint = value; Invalidate();}}
    }
    public class UBeizer : UGeometryBase
    {
        public UBeizer(Point controlPoint1, Point controlPoint2, Point endPoint)
        {
            this.controlPoint1 = controlPoint1;
            this.controlPoint2 = controlPoint2;
            this.endPoint = endPoint;
        }
        float _resolution = 3f;
        public float resolution { get { return _resolution; } set { _resolution = value; Invalidate(); } }
        Point _endPoint;
        public Point  endPoint {get { return _endPoint;} private set { _endPoint = value; Invalidate();}}
        Point _controlPoint1;
        public Point  controlPoint1 {get { return _controlPoint1;} private set { _controlPoint1 = value; Invalidate();}}
        Point _controlPoint2;
        public Point  controlPoint2 {get { return _controlPoint2;} private set { _controlPoint2 = value; Invalidate();}}
    }

    public enum StrokeCaps { flat, round, triangle };
    public enum StrokeType { solid, custom, dash, dashdot, dashdotdot, dot };
    public enum StrokeJoin { bevel, mitre, round };

    /// <summary>
    /// Keeps a strokestyle, which singletons until properties are changed in which case it regenerates.
    /// </summary>
    public class UStroke : ObjStore
    {
        // Stroke Properties
        float _strokeWidth = 1f;
        StrokeCaps _startCap = StrokeCaps.flat;
        StrokeCaps _endCap = StrokeCaps.flat;
        StrokeCaps _dashCap = StrokeCaps.flat;
        float _offset = 0f;
        StrokeType _dashStyle = StrokeType.solid;
        float[] _custom = new float[] { 1f, 1f };
        StrokeJoin _lineJoin = StrokeJoin.mitre;
        float _mitreLimit = 0f;

        public float strokeWidth { get { return _strokeWidth; } set { _strokeWidth = value; Invalidate(); } }
        public StrokeCaps startCap { get { return _startCap; } set { _startCap = value; Invalidate(); } }
        public StrokeCaps endCap { get { return _endCap; } set { _endCap = value; Invalidate(); } }
        public StrokeCaps dashCap { get { return _dashCap; } set { _dashCap = value; Invalidate(); } }
        public float offset { get { return _offset; } set { _offset = value; Invalidate(); } }
        public StrokeType dashStyle { get { return _dashStyle; } set { _dashStyle = value; Invalidate(); } }
        public float[] custom { get { return _custom; } set { _custom = value; Invalidate(); } }
        public StrokeJoin lineJoin { get { return _lineJoin; } set { _lineJoin = value; Invalidate(); } }
        public float mitreLimit { get { return _mitreLimit; } set { _mitreLimit = value; Invalidate(); } }
    }
    public abstract class UBrush : ObjStore { }
    public class USolidBrush : UBrush
    {
        Color _color = new Color(1); // white... 
        public Color color { get { return _color; } set { _color = value; Invalidate(); } }
    }
    public class ULinearGradientBrush : UBrush
    {
        Color _color1 = new Color(1);
        public Color color1 { get { return _color1; } set { _color1 = value; Invalidate(); } } Color _color2 = new Color(0);
        public Color color2 { get { return _color2; } set { _color2 = value; Invalidate(); } }
        Point _point1 = new Point(0, 0);
        public Point point1 { get { return _point1; } set { _point1 = value; Invalidate(); } } Point _point2 = new Point(0, 0);
        public Point point2 { get { return _point2; } set { _point2 = value; Invalidate(); } }
    }
    /// <summary>
    ///  instance a bitmap!
    /// </summary>
    public class UBitmap : ObjStore
    {
        // FIXME allow changes to bitmap source/data AND the storeInMemory option.  constructor for byte[] too...
        public byte[] bitmapData { get; private set; }
        public String bitmapFile { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="storeInMemory">Only use if this resource will be accessed often _by different renderers_, saving disk access time but using more system memory.</param>
        public UBitmap(string filePath, bool storeInMemory = false)
        {
            if (storeInMemory) bitmapData = System.IO.File.ReadAllBytes(filePath);
            else bitmapFile = filePath;
        }
    }
    public enum UInterp { Linear, Nearest };


}

