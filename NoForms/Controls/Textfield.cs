using System;
using SharpDX.Direct2D1;
using NoForms.Renderers;


namespace NoForms.Controls
{
    public class Textfield : Templates.Containable
    {

        public enum LayoutStyle { OneLine, MultiLine, WrappedMultiLine };
        public LayoutStyle _layout = LayoutStyle.OneLine;
        public LayoutStyle layout 
        {
            get { return _layout; }
            set { _layout = value; UpdateTextLayout(); }
        }

        public String text
        {
            get { return data.text; }
            set { data.text = value; UpdateTextLayout(); } // internally calls goto data.text
        }
        UText data = new UText("kitty", UHAlign_Enum.Center, UVAlign_Enum.Middle, false, 0, 0)
        {
            font = new UFont("Arial", 14f, false, false)
        };
        private int _caretPos = 0;
        public int caretPos
        {
            get { return _caretPos; }
            set
            {
                _caretPos = value;
                UpdateTextLayout();
            }
        }

        public Rectangle padding = new Rectangle() { left = 5, right = 5, top = 5, bottom = 5 };
        public Rectangle PaddedRectangle
        {
            get
            {
                return DisplayRectangle.Deflated(padding);
            }
        }

       

        void tm_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!(caretBrush is USolidBrush)) return;
            var cb = caretBrush as USolidBrush;

            if (cb.color == new Color(0))
            {
                cb.color = new Color(0, 0, 0, 0);
                (sender as System.Timers.Timer).Interval = 300;
            }
            else
            {
                cb.color = new Color(0);
                (sender as System.Timers.Timer).Interval = 800;
            }
        }

        public Textfield() : base()
        {
            SizeChanged += new Action<Size>(Textfield_SizeChanged);
            LocationChanged += new Action<Point>(Textfield_LocationChanged);

            UpdateTextLayout();
            System.Timers.Timer tm = new System.Timers.Timer(800) { AutoReset = true };
            tm.Elapsed += new System.Timers.ElapsedEventHandler(tm_Elapsed);
            tm.Start();
        }

        void Textfield_LocationChanged(Point obj)
        {
            UpdateTextLayout();
        }

        void Textfield_SizeChanged(Size obj)
        {
            UpdateTextLayout();
        }

        // Bits
        public UBrush background = new USolidBrush() { color = new Color(.5f, .8f, .8f, .8f) };
        public UBrush borderBrush = new USolidBrush() { color = new Color(0) };
        public UStroke borderStroke = new UStroke() { strokeWidth = 1f };
        public UBrush caretBrush = new USolidBrush() { color = new Color(0) };
        public UStroke caretStroke = new UStroke() { strokeWidth = 1f };
        public UBrush textBrush = new USolidBrush() { color = new Color(0) };
        Point caret1 = new Point(0.5f, 0.5f);
        Point caret2 = new Point(0.5f, 0.5f);

        public override void DrawBase(IRenderType rt)
        {
            if (textLayoutNeedsUpdate) UpdateTextLayout(rt);
            
            rt.uDraw.FillRectangle(DisplayRectangle, background);
            rt.uDraw.DrawRectangle(DisplayRectangle.Inflated(-.5f), borderBrush, borderStroke);
            rt.uDraw.PushAxisAlignedClip(DisplayRectangle);

            rt.uDraw.DrawText(data, new Point(PaddedRectangle.left - roX, PaddedRectangle.top - roY), textBrush, UTextDrawOptions_Enum.None);
            if (focus) rt.uDraw.DrawLine(caret1, caret2, caretBrush, caretStroke);

            rt.uDraw.PopAxisAlignedClip();
        }

        float roX = 0, roY = 0;
        bool textLayoutNeedsUpdate = false;
        private void UpdateTextLayout(IRenderType sendMe = null)
        {
            Object passageLocker = new Object();
            lock (passageLocker)
            {
                if (sendMe == null)
                {
                    textLayoutNeedsUpdate = true;
                    return;
                }
                else textLayoutNeedsUpdate = false;

                // sort out some settings
                data.wrapped = layout == LayoutStyle.WrappedMultiLine;
                data.width = PaddedRectangle.width;
                data.height = PaddedRectangle.height;
                data.valign = (layout == LayoutStyle.OneLine) ? UVAlign_Enum.Middle : UVAlign_Enum.Top;

                // update UText to make measurments available
                sendMe.uDraw.MeasureText(data);

                // get size of the render
                int lines;
                Size tms = data.TextMinSize(out lines);
                float lineHeight = tms.height / (float) lines;

                // Get Caret location
                Point cp = data.HitText(caretPos, false);

                // determine render offset
                roX = cp.X > PaddedRectangle.width ? cp.X - PaddedRectangle.width : 0;
                roY = cp.Y > PaddedRectangle.height - lineHeight ? cp.Y - PaddedRectangle.height + lineHeight : 0;

                float yCenteringOffset = PaddedRectangle.height / 2 - lineHeight / 2;
                if (layout == LayoutStyle.OneLine) roY += yCenteringOffset;

                // set caret line
                caret1 = new Point((float)Math.Round(PaddedRectangle.left + cp.X - roX) + 0.5f, (float)Math.Round(PaddedRectangle.top + cp.Y - roY));
                caret2 = new Point((float)Math.Round(PaddedRectangle.left + cp.X - roX) + 0.5f, (float)Math.Round(PaddedRectangle.top + cp.Y - roY + lineHeight));
            }
        }

        public override void KeyDown(System.Windows.Forms.Keys key)
        {
            if (key == System.Windows.Forms.Keys.Back && focus && caretPos > 0)
            {
                data.text = data.text.Substring(0, caretPos - 1) + data.text.Substring(caretPos);
                caretPos--;
            }
            if (key == System.Windows.Forms.Keys.Left && caretPos >0)
            {
                caretPos--;
            }
            if (key == System.Windows.Forms.Keys.Right && caretPos < data.text.Length)
            {
                caretPos++;
            }
        }
        public override void KeyUp(System.Windows.Forms.Keys key)
        {
        }
        public override void KeyPress(char c)
        {
            if (c != '\b' && focus)
                if((c!='\r' && c!='\n') || layout != LayoutStyle.OneLine)
                {
                    data.text = data.text.Substring(0, caretPos) + c + data.text.Substring(caretPos);
                    caretPos++;
                }
        }
        public override void MouseMove(System.Drawing.Point location, bool inComponent, bool amClipped)
        {
        }
        public override void MouseUpDown(System.Windows.Forms.MouseEventArgs mea, MouseButtonState mbs, bool inComponent, bool amClipped)
        {
            if (mbs == MouseButtonState.DOWN)
                focus = inComponent;
        }
        public override void FocusChange(bool focus)
        {
        }
    }
}
