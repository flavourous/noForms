using System;
using SharpDX.Direct2D1;
using SysRect = System.Drawing.Rectangle;

namespace NoForms.Controls
{
    public enum ButtonType { Basic, Win8 };
    enum ButtonState { Normal, Hover, Click };
    public enum FontStyle { Normal, Bold, Italic };
    public class Button : Templates.Containable
    {
        // Properties
        public ButtonType type = ButtonType.Basic;
        ButtonState state = ButtonState.Normal;
        public Color buttonColor = new Color(1, 0.6f, 0.6f, 0.6f);
        public String text = "Button";
        public String fontName = "Arial";
        public float fontSize = 12f;
        public FontStyle fontStyle = FontStyle.Normal;
        static SharpDX.DirectWrite.Factory dwfact = new SharpDX.DirectWrite.Factory();
        public Color _textColor = new Color(0);
        public Color textColor
        {
            get { return _textColor; }
            set
            {
                _textColor = value;
                if (brushText != null)
                    (brushText as SolidColorBrush).Color = value;
            }
        }

        // Render methody
        public override void DrawBase(IRenderType renderArg)
        {
            if (renderArg is RenderTarget)
            {
                RenderTarget d2dtarget = renderArg as RenderTarget;
                Util.SetClip<RenderTarget>(d2dtarget, true, DisplayRectangle);
                Draw(d2dtarget);
                Util.SetClip<RenderTarget>(d2dtarget, false,Rectangle.Empty);
            }
            else
            {
                throw new NotImplementedException("Render type " + renderArg.ToString() + " not supported");
            }
        }

        // Direct2D Support
        bool initd2d = false;
        public virtual void Draw(RenderTarget d2dtarget)
        {
            if (!initd2d) Init(d2dtarget);
            SetBrushColors(d2dtarget);

            float lt = 1.0f;
            float bv = lt/2;
            var lr = DisplayRectangle.Inflated(-bv);
            var ir = DisplayRectangle.Inflated(-lt);

            d2dtarget.FillRectangle(ir, brushFill);
            d2dtarget.DrawRectangle(lr, brushLine, lt);
            var tl = new SharpDX.DirectWrite.TextLayout(dwfact, text, new SharpDX.DirectWrite.TextFormat(dwfact, fontName, fontSize), ir.width, ir.height)
            {
                TextAlignment = SharpDX.DirectWrite.TextAlignment.Center,
                ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center,
                WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap
            };
            d2dtarget.DrawTextLayout(ir.Location, tl, brushText);
        }
        Brush brushLine, brushFill, brushText;
        void Init(RenderTarget d2drt)
        {
            brushLine = new SolidColorBrush(d2drt, buttonColor);
            if(type == ButtonType.Basic)
                brushFill = new SolidColorBrush(d2drt, buttonColor);
            if (type == ButtonType.Win8)
                brushFill = CreateLGBY(d2drt, new Color(229f / 255f), new Color(240f / 255f));
            brushText = new SolidColorBrush(d2drt, textColor);
            initd2d = true;
        }

        LinearGradientBrush CreateLGBY(RenderTarget d2drt, Color bottom, Color top)
        {
            GradientStopCollection gsc = new GradientStopCollection(d2drt, new GradientStop[] 
                { 
                    new GradientStop() { Position = 0f, Color = bottom },
                    new GradientStop() { Position = 1f, Color = top }
                });
            LinearGradientBrushProperties lgbp = new LinearGradientBrushProperties()
            {
                StartPoint = new Point(DisplayRectangle.left, DisplayRectangle.bottom),
                EndPoint = new Point(DisplayRectangle.left, DisplayRectangle.top)
            };
            return new LinearGradientBrush(d2drt, lgbp, gsc);
        }
        void SetBrushColors(RenderTarget d2drt)
        {
            if (type == ButtonType.Basic)
            {
                switch (state)
                {
                    case ButtonState.Normal:
                        (brushLine as SolidColorBrush).Color = buttonColor.Scale(focus ? -0.1f : -0.7f);
                        (brushFill as SolidColorBrush).Color = buttonColor;
                        break;
                    case ButtonState.Hover:
                        (brushLine as SolidColorBrush).Color = buttonColor.Scale(focus ? -0.1f : -0.7f);
                        (brushFill as SolidColorBrush).Color = buttonColor.Scale(0.3f);
                        break;
                    case ButtonState.Click:
                        (brushLine as SolidColorBrush).Color = buttonColor.Scale(-0.1f);
                        (brushFill as SolidColorBrush).Color = buttonColor.Scale(0.5f);
                        break;
                }
            }
            if (type == ButtonType.Win8)
            {
                switch (state)
                {
                    case ButtonState.Normal: // Always this grey normally
                        (brushLine as SolidColorBrush).Color = focus ? buttonColor.Add(51f - 96f, 153f - 150f, 255f - 191f, 255) : new Color(172f / 255f);
                        brushFill.Dispose();
                        brushFill = CreateLGBY(d2drt, new Color(229f / 255f), new Color(240f / 255f));
                        break;
                    case ButtonState.Hover: // hover related to theme color
                        (brushLine as SolidColorBrush).Color = buttonColor.Add(126f - 96f, 153f - 180f, 234f - 191f, 255);
                        brushFill.Dispose();
                        brushFill = CreateLGBY(d2drt, buttonColor.Add(220f - 96f, 236f - 150f, 252f - 191f, 255), buttonColor.Add(236f - 96f, 244f - 150f, 252f - 191f, 255));
                        break;
                    case ButtonState.Click:
                        (brushLine as SolidColorBrush).Color = buttonColor.Add(86f - 96f, 157f - 180f, 229f - 191f, 255);
                        brushFill.Dispose();
                        brushFill = CreateLGBY(d2drt, buttonColor.Add(129f - 96f, 224f - 150f, 252f - 191f, 255), buttonColor.Add(218f - 96f, 236f - 150f, 252f - 191f, 255));
                        break;
                }
            }
        }
        SharpDX.DrawingPointF aof(SharpDX.DrawingPointF dp)
        {
            dp.X += DisplayRectangle.left;
            dp.Y += DisplayRectangle.top;
            return dp;
        }

        // GDI Support
        public virtual void Draw(System.Drawing.Graphics graphics)
        {
        }

        // Mousey
        public override void MouseMove(System.Drawing.Point location, bool inComponent, bool amClipped)
        {
            if (inComponent && !md) state = ButtonState.Hover;
            if (!inComponent && !md) state = ButtonState.Normal;
        }
        public delegate void NFAction();
        public event NFAction ButtonClicked;
        bool md = false;
        public override void MouseUpDown(System.Windows.Forms.MouseEventArgs mea, MouseButtonState mbs, bool inComponent, bool amClipped)
        {
            if (mbs == MouseButtonState.DOWN && inComponent)
            {
                md = true;
                focus = true;
                state = ButtonState.Click;
            }
            if (mbs == MouseButtonState.UP && md)
            {
                md = false;
                state = ButtonState.Normal;
                if (ButtonClicked != null && inComponent)
                    foreach (NFAction na in ButtonClicked.GetInvocationList())
                        na();
            }
        }

        public override void KeyDown(System.Windows.Forms.Keys key)
        {

        }
        public override void KeyUp(System.Windows.Forms.Keys key)
        {

        }
        public override void FocusChange(bool focus)
        {
        }
        public override void KeyPress(char c)
        {
        }

    }
}
