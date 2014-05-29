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
            //NoForms.Renderers.D2DSwapChain rsc = new NoForms.Renderers.D2DSwapChain();
            NoForms.Renderers.D2DLayered rlw = new D2DLayered();
            NoForms.Renderers.SDGNormal sd = new SDGNormal();
            var nf = new mnf(sd, new CreateOptions(true));
            nf.window.Run();
        }
    }

    class mnf : NoForm
    {
        NoForms.Controls.SizeHandle sh;
        NoForms.Controls.MoveHandle mh;
        public mnf(IRender rn, CreateOptions co) : base(rn,co)
        {
            window.Title = "Test App";
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
            sh.ResizeMode = Direction.SOUTH | Direction.EAST;
            components.Add(sh);


            var sc = new Scribble();
            sc.draw += (r, b, s) =>
            {
                UFigure fig = new UFigure(sc.Location, false, true);
                fig.geoElements.Add(new UArc(false, sc.Location+new Point(500,500), new Size(5,5)));
                UPath pth = new UPath();
                pth.figures.Add(fig);
                r.DrawPath(pth,b,s);
            };
            components.Add(sc);
            sc.Location = new Point(300, 300);
            sc.Size = new Size(500, 500);
        }
        public override void Draw(IDraw rt)
        {
            rt.uDraw.Clear(new Color(.5f));
            //base.Draw(rt);
        }

        void mnf_SizeChanged(Size sz)
        {
            sh.Location = new Point(Size.width - sh.Size.width - 5, Size.height - sh.Size.height - 5);
        }

    }

}
