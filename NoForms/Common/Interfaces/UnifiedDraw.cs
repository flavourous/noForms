using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NoForms.Renderers
{
    public interface IUnifiedDraw // This is a combined replacement for 2d2RenderTarget, drawing.graphics etc
    {
        // Render tools
        void PushAxisAlignedClip(Rectangle clipRect, bool ignoreRenderOffset);
        void PopAxisAlignedClip();
        void SetRenderOffset(Point renderOffset);

        // Drawing Methods
        void Clear(Color color);
        void FillPath(UPath path, UBrush brush);
        void DrawPath(UPath path, UBrush brush, UStroke stroke);
        void DrawBitmap(UBitmap bitmap, float opacity, UInterp interp, Rectangle source, Rectangle destination);
        void FillEllipse(Point center, float radiusX, float radiusY, UBrush brush);
        void DrawEllipse(Point center, float radiusX, float radiusY, UBrush brush, UStroke stroke);
        void DrawLine(Point start, Point end, UBrush brush, UStroke stroke);
        void DrawRectangle(Rectangle rect, UBrush brush, UStroke stroke);
        void FillRectangle(Rectangle rect, UBrush brush);
        void DrawRoundedRectangle(Rectangle rect, float radX, float radY, UBrush brush, UStroke stroke);
        void FillRoundedRectangle(Rectangle rect, float radX, float radY, UBrush brush);
        void DrawText(UText textObject, Point location, UBrush defBrush, UTextDrawOptions opt, bool clientRendering);

        // Info Methods
        UTextHitInfo HitPoint(Point hitPoint, UText text);
        Point HitText(int pos, bool trailing, UText text);
        IEnumerable<Rectangle> HitTextRange(int start, int length, Point offset, UText text);
        UTextInfo GetTextInfo(UText text);
    }

    // Cached base object for drawing objects
    public class DDis : IDisposable { public void Dispose() { } }
    public delegate IDisposable NoCacheDelegate();
    public abstract class ObjStore : IDisposable
    {
        Type storedType = null; // nothing stored to begin with
        bool storedValid = false;
        IDisposable storedObject = new DDis(); // nothing stored...
        Object validationLock = new Object(); // FIXME actually, it's up to framework not to destroy the Retrieved value until it's done with.
        public event NoFormsAction invalidated = delegate { };
        public void Invalidate()
        {
            lock (validationLock)
            {
                storedValid = false;
                invalidated();
            }
        }
        public Object Retreive<RetreiverType>(NoCacheDelegate noCacheAction) where RetreiverType : class, IRenderElements
        {
            lock (validationLock)
            {
                if (!storedValid || storedType != typeof(RetreiverType))
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
        internal IEnumerable<UStyleRange> SafeGetStyleRanges
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
            _styleRanges.collectionChanged += new System.Windows.Forms.MethodInvoker(_styleRanges_collectionChanged);
        }
    }

    public class UStyleRange : IObservable
    {
        int _start;
        public int start { get { return _start; } set { _start = value; collectionChanged(); } }
        int _length;
        public int length { get { return _length; } set { _length = value; collectionChanged(); } }
        UFont? _fontOverride;
        public UFont? fontOverride { get { return _fontOverride; } set { _fontOverride = value; collectionChanged(); } }
        UBrush _fgOverride;
        public UBrush fgOverride { get { return _fgOverride; } set { _fgOverride = value; collectionChanged(); } }
        UBrush _bgOverride;
        public UBrush bgOverride { get { return _bgOverride; } set { _bgOverride = value; collectionChanged(); } }

        public UStyleRange(int start, int length, UFont? font, UBrush foreground, UBrush background)
        {
            _start = start;
            _length = length;
            _fontOverride = font;
            _fgOverride = foreground;
            _bgOverride = background;
        }
        public event System.Windows.Forms.MethodInvoker collectionChanged;
    }
    public class UTextInfo
    {
        public Size minSize = new Size(0,0);
        public int numLines =0;
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
            _figures.collectionChanged += new System.Windows.Forms.MethodInvoker(Invalidate);
        }
    }
    public class UFigure : IObservable
    {
        public event System.Windows.Forms.MethodInvoker collectionChanged = delegate { };
        ObsCollection<UGeometryBase> _geoElements = new ObsCollection<UGeometryBase>();
        public ObsCollection<UGeometryBase> geoElements { get { return _geoElements; } }
        public UFigure(Point start, bool amIFilled, bool amIOpen)
        {
            _startPoint = start;
            _filled = amIFilled;
            _open = amIOpen;
            _geoElements.collectionChanged += new System.Windows.Forms.MethodInvoker(collectionChanged);
        }
        Point _startPoint;
        public Point startPoint { get { return _startPoint; } set { _startPoint = value; collectionChanged(); } }
        bool _filled;
        public bool filled { get { return _filled; } set { _filled = value; collectionChanged(); } }
        bool _open;
        public bool open { get { return _open; } set { _open = value; collectionChanged(); } }
    }
    public abstract class UGeometryBase { }
    public class UArc : UGeometryBase
    {
        public UArc(bool reflex, Point endPoint, Size arcSize)
        {
            this.reflex = reflex;
            this.endPoint = endPoint;
            this.arcSize = arcSize;

            // FIXME advanced features not used
            rotation = 0; 
            sweepClockwise = true;
        }
        public float rotation { get; private set; }
        public bool sweepClockwise { get; private set; }
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

    // FIXME using internal setter for the below is hackable, although a useful user guide.

    /// <summary>
    /// Blank interface, the backing to renderes are these only.
    /// </summary>
    public interface IRenderElements { } // Mainly used because graphics/rendertarget can change instance Facout drawing implimentors needing to know. eg resize.
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