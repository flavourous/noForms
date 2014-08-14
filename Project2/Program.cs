using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoForms;
using NoForms.Renderers;
using NoForms.Windowing.WinForms;
using NoFormsSDK;
using Common;

namespace Easy
{
    class Program
    {
        static void Main()
        {
            //NoForms.Renderers.D2DSwapChain rsc = new NoForms.Renderers.D2DSwapChain();
            IRender rlw = new D2DLayered();
            IRender sd = new SDGNormal();
            var nf = new mnf(sd, new CreateOptions(true));
            nf.window.Run();
        }
    }

    class mnf : NoForm
    {
        SizeHandle sh;
        MoveHandle mh;
        public mnf(IRender rn, CreateOptions co) : base(rn,co)
        {
            window.Title = "Test App";
            background = new USolidBrush() { color = new Color(.8f,.5f,.5f,.8f) };
            Size = new Size(1150, 600);
            Location = new Point(100, 40);

            mh = new MoveHandle(this);
            mh.Size = new Size(20, 20);
            mh.Location = new Point(5, 5);
            components.Add(mh);

            sh = new SizeHandle(this);
            sh.Size = new Size(20, 20);
            sh.Location = new Point(Size.width - sh.Size.width - 5, Size.height - sh.Size.height - 5);
            sh.ResizeMode = Direction.SOUTH | Direction.EAST;
            components.Add(sh);

            var sc = new Scribble();
            sc.Location = new Point(300, 300);
            sc.Size = new Size(500, 500);

            UPath pth = new UPath();
            UFigure fig;

            for (float i = 0; i < 100; i += 10)
            {
                fig = new UFigure(new Point(sc.Location.X + 0, sc.Location.Y), false, true);
                fig.geoElements.Add(new UArc(sc.Location + new Point(60, 60), new Size(100, 50), true, true, i));
                pth.figures.Add(fig);
                //fig = new UFigure(new Point(sc.Location.X + 0, sc.Location.Y), false, true);
                //fig.geoElements.Add(new UArc(sc.Location + new Point(60, 60), new Size(100, 100), true, true, i));
                //pth.figures.Add(fig);
                //fig = new UFigure(new Point(sc.Location.X + 0, sc.Location.Y), false, true);
                //fig.geoElements.Add(new UArc(sc.Location + new Point(60, 60), new Size(100, 100), false, false, i));
                //pth.figures.Add(fig);
                //fig = new UFigure(new Point(sc.Location.X + 0, sc.Location.Y), false, true);
                //fig.geoElements.Add(new UArc(sc.Location + new Point(60, 60), new Size(100, 100), true, false, i));
                //pth.figures.Add(fig);
            }   
            UText tx = new UText("This is a noremal sentance in arial 12pt\nThis is a new line\nthis is more",
                                 UHAlign.Right, UVAlign.Bottom, true, 500,200);
            tx.font = new UFont("Arial", 12, false, false);

            sc.draw += (r, b, s) =>
            {
                s.strokeWidth = 1;
                b.color = new Color(1f, 1, 0f, 0);
                r.DrawPath(pth,b,s);
                

                //var ti = (r as NoForms.Renderers.SDGDraw).GetSDGTextInfo(tx);
                //float ws = 0;
                //foreach (var gr in ti.glyphRuns)
                //{
                //    if (gr.run.breakingType == SDGDraw.BreakType.line) break;
                //    r.DrawRectangle(new Rectangle(ws+sc.Location.X, sc.Location.Y, gr.run.runSize.width, gr.run.runSize.height), b, s);
                //    ws += gr.run.runSize.width;
                //}

                
                r.FillRectangle(new Rectangle(sc.Location, new Size(tx.width, tx.height)), b);
                b.color = new Color(1f, 0f, 0f, 0f);
                r.DrawText(tx, sc.Location, b, UTextDrawOptions.None, false);
            };
            components.Add(sc);
            
        }
        

        void mnf_SizeChanged(Size sz)
        {
            sh.Location = new Point(Size.width - sh.Size.width - 5, Size.height - sh.Size.height - 5);
        }

    }

}
