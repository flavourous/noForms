using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoForms.Common;

namespace NoForms.Example
{
    class textAlighyTest : NoForm
    {
        UBitmap ubit = new UBitmap("opentk.png", true);
        public textAlighyTest(IPlatform plt)
            : base(plt)
        {
            Size = new Size(300, 300);
        }
        UText tt = new UText("Kitten", UHAlign.Center, UVAlign.Middle, true, 100, 100)
        {
            font = new UFont("Arial", 12f, false, false)
        };
        USolidBrush usb = new USolidBrush() { color = new Color(1f, 1f, 0f, 1f) };
        USolidBrush usb2 = new USolidBrush() { color = new Color(1f, 0f, 1f, 0f) };
        public override void Draw(IDraw rt, Common.Region dirty)
        {
            base.Draw(rt, dirty);
            rt.uDraw.FillRectangle(DisplayRectangle, usb2);
            dtt(0, 0, UHAlign.Left, UVAlign.Top, new Color(1, 1, 0, 0), rt);
            dtt(100, 0, UHAlign.Center, UVAlign.Top, new Color(1, 1, 1, 0), rt);
            dtt(200, 0, UHAlign.Right, UVAlign.Top, new Color(1, 1, 0, 1), rt);
            dtt(0, 100, UHAlign.Left, UVAlign.Middle, new Color(1, 0, 1, 1), rt);
            dtt(100, 100, UHAlign.Center, UVAlign.Middle, new Color(1, 0, 1, 0), rt);
            dtt(200, 100, UHAlign.Right, UVAlign.Middle, new Color(1, 1, 1, 0), rt);
            dtt(0, 200, UHAlign.Left, UVAlign.Bottom, new Color(1, 1, 0, 1), rt);
            dtt(100, 200, UHAlign.Center, UVAlign.Bottom, new Color(1, 1, 1, 0), rt);
            dtt(200, 200, UHAlign.Right, UVAlign.Bottom, new Color(1, 0, 0, 1), rt);
        }
        void dtt(float x, float y, UHAlign h, UVAlign v, Color cl, IDraw rt)
        {
            tt.halign = h; tt.valign = v;
            //rt.uDraw.FillRectangle(new Rectangle(x, y, 100, 100), new USolidBrush() { color = cl });
            rt.uDraw.DrawText(tt, new Point(x, y), usb, UTextDrawOptions.None, false);
        }
    }
}
