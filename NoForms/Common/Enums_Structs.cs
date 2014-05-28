using System; 

namespace NoForms
{
    public delegate void NoFormsAction();
    public class CreateOptions
    {
        public CreateOptions(bool showInTaskbar)
        {
            this.showInTaskbar = showInTaskbar;
        }
        public bool showInTaskbar { get; private set; }
    }

    [Flags]
    public enum Direction { NONE =0, NORTH = 1, SOUTH=2, EAST=4, WEST=8 }; // bitmask

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
                r + (signFactor > 0 ? (1.0f-r)*signFactor : r*signFactor),
                g + (signFactor > 0 ? (1.0f-g)*signFactor : g*signFactor),
                b + (signFactor > 0 ? (1.0f-b)*signFactor : b*signFactor)
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

        public static implicit operator Color(SharpDX.Color4 input)
        {
            return new Color(input.Alpha, input.Red, input.Green, input.Blue);
        }
        public static implicit operator SharpDX.Color4(Color input) 
        {
            return new SharpDX.Color4(input.r, input.g, input.b, input.a);
        }
        public static implicit operator SharpDX.Color(Color input)
        {
            return new SharpDX.Color(input.r, input.g, input.b, input.a);
        }
        public static implicit operator System.Drawing.Color(Color input)
        {
            int a = (int)Math.Round(input.a);
            int r = (int)Math.Round(input.r);
            int g = (int)Math.Round(input.g);
            int b = (int)Math.Round(input.b);
            return System.Drawing.Color.FromArgb(a,r,g,b);
        }
        public static implicit operator Color(System.Drawing.Color input)
        {
            return new Color(input.A, input.R, input.G, input.B);
        }
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

        public static implicit operator System.Drawing.Size(Size me)
        {
            return new System.Drawing.Size((int)(me.width+.5f), (int)(me.height+.5f));
        }
        public static implicit operator Size(System.Drawing.Size you)
        {
            return new Size(you.Width, you.Height);
        }
        public static implicit operator SharpDX.DrawingSizeF(Size me)
        {
            return new SharpDX.DrawingSizeF(me.width, me.height);
        }
        public static implicit operator Size(System.Drawing.SizeF me)
        {
            return new Size(me.Width, me.Height);
        }
        public static implicit operator System.Drawing.SizeF(Size me)
        {
            return new System.Drawing.SizeF(me.width, me.height);
        }
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

        public static implicit operator System.Drawing.PointF(Point me)
        {
            return new System.Drawing.PointF(me.X, me.Y);
        }
        public static implicit operator Point(System.Drawing.PointF you)
        {
            return new Point(you.X, you.Y);
        }

        public static implicit operator Point(System.Drawing.Point you)
        {
            return new Point(you.X, you.Y);
        }
        public static implicit operator SharpDX.DrawingPointF(Point me)
        {
            return new SharpDX.DrawingPointF(me.X, me.Y);
        }
        public static implicit operator System.Drawing.Point(Point me)
        {
            return new System.Drawing.Point((int)(me.X+.5f), (int)(me.Y+.5f));
        }
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

        public Rectangle(Point p1, Point p2)
        {
            float x1 = p1.X;
            float x2 = p2.X;
            float y1 = p1.Y;
            float y2 = p2.Y;

            float x = x1 > x2 ? x2 : x1;
            float y = y1 > y2 ? y2 : y1;
            float w = Math.Abs(x1 - x2);
            float h = Math.Abs(y1 - y2);

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
            set { height = value-top; }
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
            return new Rectangle(left-amount.left, top-amount.top, width+amount.left+amount.right, height+amount.top + amount.bottom);
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
        public static implicit operator SharpDX.RectangleF(Rectangle me)
        {
            return new SharpDX.RectangleF(me.left, me.top, me.right, me.bottom);
        }
        public static implicit operator System.Drawing.RectangleF(Rectangle me)
        {
            return new System.Drawing.RectangleF(me.left, me.top, me.width, me.height);
        }
        
    }

}