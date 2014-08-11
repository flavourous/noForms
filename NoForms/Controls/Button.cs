using System;
using NoForms.Renderers;
using NoForms;
using Common;

namespace NoForms.Controls
{
    public enum ButtonType { Basic, Win8 };
    public enum ButtonControlState { Normal, Hover, Click };
    public enum FontStyle { Normal, Bold, Italic };
    public class Button : Abstract.BasicContainer
    {
        // Properties
        ButtonType _type = ButtonType.Basic;
        public ButtonType type
        {
            get { return _type; }
            set { _type = value; Init(); }
        }
        ButtonControlState _state = ButtonControlState.Normal;
        ButtonControlState state
        {
            get { return _state; }
            set { _state = value; SetBrushColors(); }
        }
        public Color buttonColor = new Color(1, 0.6f, 0.6f, 0.6f);
        public UText textData = new UText("Button", UHAlign.Center, UVAlign.Middle, true, 0, 0)
        {
            font = new UFont("Arial", 12f, false, false)
        };
        public Color _textColor = new Color(0);
        public Color textColor
        {
            get { return _textColor; }
            set
            {
                _textColor = value;
                if (brushText != null)
                    (brushText as USolidBrush).color = value;
            }
        }

        public Button()
        {
            Init();
            SizeChanged += new Action<Size>(Button_SizeChanged);
        }

        void Button_SizeChanged(Size obj)
        {
            var ir = DisplayRectangle.Inflated(new Thickness(-edge.strokeWidth));
            textData.width = ir.width;
            textData.height = ir.height;
        }

        // Render methody
        public override void Draw(IDraw rt)
        {
            float lt = edge.strokeWidth;
            float bv = lt / 2;
            var lr = DisplayRectangle.Inflated(new Thickness(-bv));
            var ir = DisplayRectangle.Inflated(new Thickness(-lt));

            rt.uDraw.FillRectangle(ir, brushFill);
            rt.uDraw.DrawRectangle(lr, brushLine, edge);
            rt.uDraw.DrawText(textData, ir.Location, brushText, UTextDrawOptions.Clip,false);
        }
        UStroke edge = new UStroke() { strokeWidth = 1f };
        UBrush brushLine, brushFill, brushText;
        void Init()
        {
            brushLine = new USolidBrush() { color = buttonColor };
            if(type == ButtonType.Basic)
                brushFill = new USolidBrush() { color = buttonColor };
            if (type == ButtonType.Win8)
                brushFill = new ULinearGradientBrush()
                {
                    color1 = new Color(229f / 255f),
                    point1 = new Point(DisplayRectangle.width / 2, DisplayRectangle.bottom),
                    color2 = new Color(240f / 255f),
                    point2 = new Point(DisplayRectangle.width / 2, DisplayRectangle.top)
                };
            brushText = new USolidBrush() { color = textColor };
            SetBrushColors();
        }

        void SetBrushColors()
        {
            bool focus = FocusManager.FocusGet(this);
            if (type == ButtonType.Basic)
            {
                switch (state)
                {
                    case ButtonControlState.Normal:
                        (brushLine as USolidBrush).color = buttonColor.Scale(focus ? -0.1f : -0.7f);
                        (brushFill as USolidBrush).color = buttonColor;
                        break;
                    case ButtonControlState.Hover:
                        (brushLine as USolidBrush).color = buttonColor.Scale(focus ? -0.1f : -0.7f);
                        (brushFill as USolidBrush).color = buttonColor.Scale(0.3f);
                        break;
                    case ButtonControlState.Click:
                        (brushLine as USolidBrush).color = buttonColor.Scale(-0.1f);
                        (brushFill as USolidBrush).color = buttonColor.Scale(0.5f);
                        break;
                }
            }
            if (type == ButtonType.Win8)
            {
                switch (state)
                {
                    case ButtonControlState.Normal: // Always this grey normally
                        (brushLine as USolidBrush).color = focus ? buttonColor.Add(51f - 96f, 153f - 150f, 255f - 191f, 255) : new Color(172f / 255f);
                        (brushFill as ULinearGradientBrush).color1 = new Color(229f / 255f);
                        (brushFill as ULinearGradientBrush).color2 = new Color(240f / 255f);
                        break;
                    case ButtonControlState.Hover: // hover related to theme color
                        (brushLine as USolidBrush).color = buttonColor.Add(126f - 96f, 153f - 180f, 234f - 191f, 255);
                        (brushFill as ULinearGradientBrush).color1 = buttonColor.Add(220f - 96f, 236f - 150f, 252f - 191f, 255);
                        (brushFill as ULinearGradientBrush).color2 = buttonColor.Add(236f - 96f, 244f - 150f, 252f - 191f, 255);
                        break;
                    case ButtonControlState.Click:
                        (brushLine as USolidBrush).color = buttonColor.Add(86f - 96f, 157f - 180f, 229f - 191f, 255);
                        (brushFill as ULinearGradientBrush).color1 = buttonColor.Add(129f - 96f, 224f - 150f, 252f - 191f, 255);
                        (brushFill as ULinearGradientBrush).color2 = buttonColor.Add(218f - 96f, 236f - 150f, 252f - 191f, 255);
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

        // Mousey
        public override void MouseMove(Point location, bool inComponent, bool amClipped)
        {
            base.MouseMove(location, inComponent, amClipped);
            if (inComponent && !md) state = ButtonControlState.Hover;
            if (!inComponent && !md) state = ButtonControlState.Normal;
        }
        public delegate void NFAction();
        public event NFAction ButtonClicked;
        bool md = false;
        public override void MouseUpDown(Point location, MouseButton mb, ButtonState mbs, bool inComponent, bool amClipped)
        {
            base.MouseUpDown(location, mb, mbs, inComponent, amClipped);
            if (mbs == ButtonState.DOWN && inComponent)
            {
                md = true;
                FocusManager.FocusSet(this, true);
                state = ButtonControlState.Click;
            }
            if (mbs == ButtonState.UP && md)
            {
                md = false;
                state = ButtonControlState.Normal;
                if (ButtonClicked != null && inComponent)
                    foreach (NFAction na in ButtonClicked.GetInvocationList())
                        na();
            }
        }

    }
}
