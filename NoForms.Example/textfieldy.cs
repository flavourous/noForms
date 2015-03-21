using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoForms.Common;
using NoFormsSDK;

namespace NoForms.Example
{
    class TextFieldDemoSlashTest : NoForm
    {
        public TextFieldDemoSlashTest(IPlatform plt)
            : base(plt)
        {
            Size = new Size(300, 300);
            Textfield tf = new Textfield() { Size = new Size(300, 50), layout = Textfield.LayoutStyle.MultiLine };
            components.Add(tf);
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
}
