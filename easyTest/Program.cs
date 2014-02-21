using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NoForms;
using NoForms.Renderers;
using NoForms.Controls;

namespace easyTest
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var rend = new NoForms.Renderers.D2DLayered();
            var nf = new NoForm(rend);
            nf.title = "ytextbox test";
            nf.Size = new Size(400, 400);
            nf.background = new USolidBrush() { color = new Color(0.8f,0.2f,0.2f,0.5f) };
            nf.Location = new Point(300, 100);

            var tb = new Textfield() { Size = new Size(300, 300), Location = new Point(50,50) };
            nf.components.Add(tb);
            tb.background = new ULinearGradientBrush() { color1 = new Color(1, 1, 0, 1), color2 = new Color(1, 0, 1, 0), point1 = new Point(0, 0), point2 = new Point(300, 300) };
            tb.layout = Textfield.LayoutStyle.WrappedMultiLine;

            tb.text = "Paragraph one\nhas some text\n\nnormalparagraph breaher\ni dont spell at 6am\n\n\n\n\nfaraway\n\nENDY";

            nf.Create(true, false);
        }
    }
}
