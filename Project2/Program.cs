using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoForms.Controllers;
using NoForms;
using NoForms.Renderers;
using NoFormsSDK;
using NoForms.Common;
using NoForms.Windowing;

namespace Easy
{
    class Program
    {
        static void Main()
        {
            //NoForms.Renderers.D2DSwapChain rsc = new NoForms.Renderers.D2DSwapChain();
            IPlatform plt = new Win32(new D2DLayered(), new WinformsController());
            IPlatform plt2 = new WinForms(new D2DSwapChain(), new WinformsController());
            var nf = new mnf(plt, new CreateOptions(true,false));
            nf.window.Run();
        }
    }

    class mnf : NoForm
    {
        SizeHandle sh;
        MoveHandle mh;
        Scribble sc;

        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        System.Threading.Timer st;
        void procq(Object o)
        {
            var cdr = sc.DisplayRectangle;
            cdr.top -= 50;
            Dirty(cdr);
        }

        public mnf(IPlatform rn, CreateOptions co) : base(rn,co)
        {
            window.Title = "Test App";
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

            sc = new Scribble();
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
                double ssw = Math.Sin(sw.ElapsedMilliseconds / 1000.0);
                b.color = new Color(1f, (float)(ssw + 1.0 / 2.0), 0f, 0f);
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
            sc.Clicked += p => Size = new Size(Size.width - 10, Size.height-10);
            components.Add(sc);
            SizeChanged += mnf_SizeChanged;

            st = new System.Threading.Timer(procq, null, 0, 10);
        }

        UBrush bgb = new USolidBrush() { color = new Color(1, 0, 0, 1) };
        public override void Draw(IDraw rt, Region dirty)
        {
            base.Draw(rt, dirty);
            rt.uDraw.FillRectangle(DisplayRectangle, bgb);
        }

        void mnf_SizeChanged(Size sz)
        {
            sh.Location = new Point(Size.width - sh.Size.width - 5, Size.height - sh.Size.height - 5);
        }

    }

}
