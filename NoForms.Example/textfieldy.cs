using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoForms.Common;
using NoFormsSDK;

namespace NoForms.Example
{
    class Highlightytesttesty : NoForm
    {
        public Highlightytesttesty(IPlatform plt)
            : base(plt)
        {
            Size = new Size(300, 300);
            t.styleRanges.Add(new   UStyleRange(0,5, null, null, usb1));
            t.styleRanges.Add(new   UStyleRange(8,2, null, null, usb1));
            AnimatedRect ar = new AnimatedRect();
            ar.area = new Rectangle(0, 0, 300, 300);
            BeginDirty(ar);
        }
        static ULinearGradientBrush usb1 = new ULinearGradientBrush()
        {
            point2 = new Point(0, 0),
            point1 = new Point(300, 300),
            color1 = new Color(.5f),
            color2 = new Color(1f, 1f, .5f, 1f)
        };
        ULinearGradientBrush usb2 = new ULinearGradientBrush()
        {
            point1 = new Point(0, 0),
            point2 = new Point(300, 300),
            color1 = new Color(.5f),
            color2 = new Color(1f, 1f, .5f, 1f)
        };
        USolidBrush ub = new USolidBrush() { color = new Color(0f) };
        UText t = new UText("Hglyo I am Tesxt", UHAlign.Left, UVAlign.Top, false, 0, 0)
        {
            font = new UFont("Arial", 22f, false, false),
        };
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        public override void Draw(IDraw rt, Common.Region dirty)
        {
            float dt = (float)Math.Sin(sw.ElapsedMilliseconds / 1000f) + 1f;
            base.Draw(rt, dirty);
            rt.uDraw.FillRectangle(DisplayRectangle, usb2);
            rt.uDraw.DrawText(t, new Point(dt * 30, dt * 40), ub, UTextDrawOptions.None, false);
        }
    }
    class TextFieldDemoSlashTest : NoForm
    {
        public TextFieldDemoSlashTest(IPlatform plt)
            : base(plt)
        {
            Size = new Size(300, 300);
            TFc tc = new TFc() { Size = new Size(400, 200) };
            Textfield tf = new Textfield() { Size = new Size(300, 50), layout = Textfield.LayoutStyle.OneLine};
            tc.components.Add(tf);
            components.Add(tc);
            
        }
        ULinearGradientBrush usb2 = new ULinearGradientBrush()
        {
            point1 = new Point(0, 0),
            point2 = new Point(300, 300),
            color1 = new Color(.5f),
            color2 = new Color(1f, 1f, .5f, 1f)
        };
        public override void Draw(IDraw rt, Common.Region dirty)
        {
            base.Draw(rt, dirty);
            rt.uDraw.FillRectangle(DisplayRectangle, usb2);
        }
    }
    class TFc : ComponentBase.Container
    {
        ULinearGradientBrush usb2 = new ULinearGradientBrush()
        {
            point1 = new Point(0, 0),
            point2 = new Point(300, 300),
            color1 = new Color(1f, 1f, .5f, 0f),
            color2 = new Color(.5f)
        };
        public override void Draw(IDraw renderArgument, Region dirty)
        {
            renderArgument.uDraw.FillRectangle(DisplayRectangle, usb2);
        }
    }
}
