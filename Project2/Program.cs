using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoForms;
using SharpDX.Direct2D1;

namespace Easy
{
    class Program
    {
        static void Main()
        {
            NoForms.Renderers.D2DSwapChain rsc = new NoForms.Renderers.D2DSwapChain();
            var nf = new mnf(rsc);
            nf.Size = new Size(200, 200);
            nf.title = "Test App";
            nf.backColor = SharpDX.Color.Chocolate;
            nf.Create(true, false);
        }
    }

    class mnf : NoForm
    {
        NoForms.Controls.Scribble scrib;
        static SharpDX.DirectWrite.Factory fact = new SharpDX.DirectWrite.Factory();
        public mnf(IRender ir)
            : base(ir)
        {

            

            
            NoForms.Controls.SizeHandle sh = new NoForms.Controls.SizeHandle(this);
            sh.Location = new Point(20, 20);
            sh.Size = new NoForms.Size(20, 20);
            components.Add(sh);
            SizeChanged += new Act( () => sh.Location = new Point(DisplayRectangle.right-30, DisplayRectangle.bottom-30) );

            NoForms.Controls.MoveHandle mh = new NoForms.Controls.MoveHandle(this);
            mh.Location = new Point(10, 10);
            mh.Size = new Size(20, 20);
            components.Add(mh);


            NoForms.Controls.Button bt = new NoForms.Controls.Button();
            bt.Size = new NoForms.Size(100, 20);
            bt.Location = new Point(60, 60);
            bt.text = "MoveTest";
            bt.ButtonClicked += new NoForms.Controls.Button.NFAction(() => { sh.TestResize(100, 0); });
            bt.type = NoForms.Controls.ButtonType.Win8;
            components.Add(bt);

        }



        void scrib_draw(RenderTarget rt, SolidColorBrush scb)
        {
            scb.Color = new Color(0.7f);
            rt.FillRoundedRectangle(new RoundedRectangle() { RadiusX = 20, RadiusY = 20, Rect = scrib.DisplayRectangle }, scb);
            //rt.FillRectangle(scrib.DisplayRectangle, scb);
        }
    }

    class containty : NoForms.Controls.Templates.Container
    {
        SolidColorBrush scb;
        public override void DrawBase<RenderType>(RenderType renderArgument)
        {
            if (renderArgument is RenderTarget)
            {
                var rt = renderArgument as RenderTarget;
                if(scb == null) scb = new SolidColorBrush(rt, new NoForms.Color(.5f));
                rt.FillRectangle(DisplayRectangle, scb);
            }
            base.DrawBase<RenderType>(renderArgument);
        }
    }

}
