using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoForms;
using NoForms.Renderers;
using NoForms.Controls;

namespace Easy
{
    class Program
    {
        static void Main()
        {
            NoForms.Renderers.D2DSwapChain rsc = new NoForms.Renderers.D2DSwapChain();
            NoForms.Renderers.D2DLayered rlw = new D2DLayered();
            var nf = new mnf(rlw);
            nf.Create(true, false);
        }
    }

    class mnf : NoForm
    {
        NoForms.Controls.SizeHandle sh;
        NoForms.Controls.MoveHandle mh;
        con cont;
        public mnf(IRender ir)
            : base(ir)
        {
            title = "Test App";
            background = new USolidBrush() { color = new Color(.8f,.5f,.5f,.8f) };
            Size = new Size(1150, 600);
            Location = new Point(100, 40);

            mh = new NoForms.Controls.MoveHandle(this);
            mh.Size = new Size(20, 20);
            mh.Location = new Point(5, 5);
            components.Add(mh);

            sh = new NoForms.Controls.SizeHandle(this);
            sh.Size = new Size(20, 20);
            sh.Location = new Point(Size.width - sh.Size.width - 5, Size.height - sh.Size.height - 5);
            components.Add(sh);

            cont = new con();
            cont.Location = new Point(30, 90);
            cont.Size = new Size(Size.width - 60, Size.height - 60);
            components.Add(cont);

            NoForms.Controls.Button bt = new NoForms.Controls.Button();
            bt.textData.text = "Hellos!";
            bt.Location = new Point(5, 5);
            bt.Size = new NoForms.Size(70, 20);
            cont.components.Add(bt);

            ComboBox cb = new ComboBox();
            cb.AddItem("Kitty");
            cb.AddItem("Nyan");
            cb.Size = new NoForms.Size(100, 20);
            cb.Location = new Point(90, 5);
            cont.components.Add(cb);

            ListBox lb = new ListBox();
            lb.AddItem("Kitty");
            lb.AddItem("Nyan");
            lb.Size = new NoForms.Size(100, 100);
            lb.Location = new Point(200, 5);
            cont.components.Add(lb);

            bool fc = false;
            Scribble scrib = new Scribble();
            scrib.draw += new Scribble.scribble((ud, scb, str) =>
            {
                if(fc)
                    ud.FillRectangle(scrib.DisplayRectangle.Inflated(-2f), scb);
                else 
                    ud.DrawRectangle(scrib.DisplayRectangle.Inflated(-2.5f), scb, str);
            });
            scrib.Clicked += new Scribble.ClickDelegate(pt => { fc = !fc; });
            scrib.Size = new NoForms.Size(50, 50);
            scrib.Location = new Point(5, 30);
            cont.components.Add(scrib);

            TextLabel tl = new TextLabel()
            {
                textData = new UText("?hell", UHAlign_Enum.Center, UVAlign_Enum.Middle, false, 100, 50)
                {
                    font = new UFont("Arial Black", 15f, true, true)
                }
            };
            tl.Size = new NoForms.Size(100, 50);
            tl.Location = new Point(60, 30);
            tl.background = new USolidBrush() { color = new Color(.5f, 0, 0, 0) };
            tl.foreground = new USolidBrush() { color = new Color(.5f, 1, 1, 1) };
            cont.components.Add(tl);

            Textfield tf = new Textfield();
            tf.layout = Textfield.LayoutStyle.MultiLine;
            tf.Size = new Size(100, 100);
            tf.Location = new Point(5, 150);
            cont.components.Add(tf);

            Textfield tf2 = new Textfield();
            tf2.layout = Textfield.LayoutStyle.OneLine;
            tf2.Size = new Size(100, 25);
            tf2.Location = new Point(5, 255);
            cont.components.Add(tf2);

            SizeChanged += new Act(mnf_SizeChanged);
            mnf_SizeChanged();
        }

       

        void mnf_SizeChanged()
        {
            sh.Location = new Point(Size.width - sh.Size.width - 5, Size.height - sh.Size.height - 5);
            cont.Size = new Size(Size.width - 60, Size.height - 120);
        }
    }

    class con : NoForms.Controls.Templates.Container
    {
        public override void DrawBase(IRenderType renderArgument)
        {
            lgb.point1 = DisplayRectangle.Location;
            lgb.point2 = new Point(DisplayRectangle.right, DisplayRectangle.bottom);
            renderArgument.uDraw.FillRectangle(DisplayRectangle, lgb);
            base.DrawBase(renderArgument);
        }
        ULinearGradientBrush lgb = new ULinearGradientBrush()
        {
            color1 = new Color(.8f,0,.3f,0), color2 = new Color(.8f,0,.5f,0)
        };
    }
}
