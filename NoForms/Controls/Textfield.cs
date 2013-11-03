using System;
using SharpDX.Direct2D1;

namespace NoForms.Controls
{
    
    public class Textfield : Templates.Containable
    {
        static SharpDX.DirectWrite.Factory fact = new SharpDX.DirectWrite.Factory();

        public bool multiline = false;
        public bool wordwrap = false;
        private String _text = "Mega Kitty.";
        public String text
        {
            get { return _text; }
            set
            {
                _text = value;
            }
        }
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

        public Rectangle padding = new Rectangle() { left = 0, right = 0, top = 0, bottom = 0 };
        public Rectangle PaddedRectangle
        {
            get
            {
                return DisplayRectangle.Deflated(padding);
            }
        }

        SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(fact, "", new SharpDX.DirectWrite.TextFormat(fact, "Arial", 14f), 0, 0)
        {
            WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap,
            ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center
        };

        public Textfield() : base()
        {
            SizeChanged += new Action<Size>(Textfield_SizeChanged);
            LocationChanged += new Action<Point>(Textfield_LocationChanged);
        }

        void Textfield_LocationChanged(Point obj)
        {
            UpdateTextLayout();
        }

        void Textfield_SizeChanged(Size obj)
        {
            UpdateTextLayout();
        }

        public override void DrawBase<RenderType>(RenderType renderArgument)
        {
            if (renderArgument is RenderTarget) Draw(renderArgument as RenderTarget);
            else throw new NotImplementedException("Texfield does not support " + renderArgument.ToString());
        }

        bool bInit = false;
        void Draw(RenderTarget d2drt)
        {
            if(!bInit) Init(d2drt);
            if (textLayoutNeedsUpdate) UpdateTextLayout(true);

            d2drt.FillRectangle(DisplayRectangle,new SolidColorBrush(d2drt,new SharpDX.Color4(0.8f, 0.8f, 0.8f, 0.5f)));
            d2drt.DrawRectangle(DisplayRectangle.Inflated(-.5f),new SolidColorBrush(d2drt, SharpDX.Color4.Black), 1f);
            d2drt.PushAxisAlignedClip(DisplayRectangle, AntialiasMode.Aliased);

            d2drt.DrawTextLayout(new SharpDX.DrawingPointF(PaddedRectangle.left-roX,PaddedRectangle.top-roY), textLayout, new SolidColorBrush(d2drt, SharpDX.Color4.Black), DrawTextOptions.NoSnap);
            if(focus) d2drt.DrawLine(caret1, caret2, caretBrush);

            d2drt.PopAxisAlignedClip();
        }
        void Init(RenderTarget d2drt)
        {
            padding = new Rectangle() { top = 5, left = 5, right = 5, bottom = 5 };
            UpdateTextLayout();
            caretBrush = new SolidColorBrush(d2drt, SharpDX.Color4.Black);
            bInit = true;

            System.Timers.Timer tm = new System.Timers.Timer(800) { AutoReset = true };
            tm.Elapsed += new System.Timers.ElapsedEventHandler(tm_Elapsed);
            tm.Start();
        }

        void tm_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (caretBrush.Color == SharpDX.Color.Black)
            {
                caretBrush.Color = SharpDX.Color.Transparent;
                (sender as System.Timers.Timer).Interval = 300;
            }
            else
            {
                caretBrush.Color = SharpDX.Color.Black;
                (sender as System.Timers.Timer).Interval = 800;
            }
        }

        SharpDX.Direct2D1.SolidColorBrush caretBrush;
        SharpDX.DrawingPointF caret1 = new SharpDX.DrawingPointF(0.5f,0.5f), caret2 = new SharpDX.DrawingPointF(0.5f,0.5f);

        float roX = 0, roY = 0;
        float heightLineZero = 0;
        bool textLayoutNeedsUpdate = false;
        private void UpdateTextLayout(bool fromRenderLoop = false)
        {
            if (!fromRenderLoop)
            {
                textLayoutNeedsUpdate = true;
                return;
            }
            else
                textLayoutNeedsUpdate = false;

            //init test layout
            textLayout = new SharpDX.DirectWrite.TextLayout(fact, text, new SharpDX.DirectWrite.TextFormat(fact, "Arial", 14f), PaddedRectangle.width, 0)
            {
                WordWrapping = (multiline && wordwrap) ? SharpDX.DirectWrite.WordWrapping.Wrap : SharpDX.DirectWrite.WordWrapping.NoWrap
            };

            // get size of the render
            float minWidth = textLayout.DetermineMinWidth();
            float minHeight = 0;
            foreach (var tlm in textLayout.GetLineMetrics())
                minHeight += tlm.Height;
            heightLineZero = textLayout.GetLineMetrics()[0].Height;

            // Get Caret location
            float x, y;
            textLayout.HitTestTextPosition(caretPos, false, out x, out y);

            // determine render offset
            roX = x > PaddedRectangle.width ? x - PaddedRectangle.width : 0;
            roY = y > PaddedRectangle.height - heightLineZero ? y - PaddedRectangle.height + heightLineZero: 0;

            float yCenteringOffset = PaddedRectangle.height / 2 - heightLineZero / 2;
            if (!multiline) roY += yCenteringOffset;

            // set caret line
            caret1 = new SharpDX.DrawingPointF((float)Math.Round(PaddedRectangle.left + x - roX)+0.5f, (float)Math.Round(PaddedRectangle.top + y - roY));
            caret2 = new SharpDX.DrawingPointF((float)Math.Round(PaddedRectangle.left + x - roX) + 0.5f, (float)Math.Round(PaddedRectangle.top + y - roY + heightLineZero));
        }

        public override void KeyDown(System.Windows.Forms.Keys key)
        {
            if (key == System.Windows.Forms.Keys.Back && focus && caretPos > 0)
            {
                text = text.Substring(0, caretPos-1) + text.Substring(caretPos);
                caretPos--;
            }
            if (key == System.Windows.Forms.Keys.Left && caretPos >0)
            {
                caretPos--;
            }
            if (key == System.Windows.Forms.Keys.Right && caretPos < text.Length)
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
                if((c!='\r' && c!='\n') || multiline)
                {
                    text = text.Substring(0, caretPos) + c + text.Substring(caretPos);
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
