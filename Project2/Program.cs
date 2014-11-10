﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NoForms;
using NoForms.Renderers;
using NoFormsSDK;
using NoForms.Common;

// Implimentations
using NoForms.Platforms.Win32;
using NoForms.Platforms.DotNet;
using NoForms.Renderers.DotNet;
using NoForms.Renderers.SharpDX;
using NoForms.Controllers.DotNet;
using NoForms.Renderers.OpenTK;

namespace Easy
{
    class Program
    {
        public static IRender rdr;
        static void Main()
        {
            D2DLayered r1 = new D2DLayered();
            D2DSwapChain r2 = new D2DSwapChain();
            SDGNormal r3 = new SDGNormal();
            OTKNormal r4 = new OTKNormal(); 
            WinformsController c1 = new WinformsController();
            var wco = new WindowCreateOptions(true, WindowBorderStyle.NoBorder);
            IPlatform plt1 = new Win32(r1, c1, wco);
            IPlatform plt2 = new Win32(r2, c1, wco);
            IPlatform plt3 = new WinForms(r3, c1, wco);
            IPlatform plt4 = new Win32(r4, c1, wco);
            var nf = new mnf(plt4);
            rdr = r4;
            nf.window.Run();
        }
    }

    class mnf : NoForm
    {
        SizeHandle sh;
        MoveHandle mh;
        Scribble sc;

        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        void procq(Object o)
        {
            var cdr = sc.DisplayRectangle;
            cdr.top -= 50;
            Dirty(cdr);
        }

        public mnf(IPlatform rn) : base(rn)
        {
            window.Title = "Test App";
            Size = new Size(1150, 600);
            Location = new Point(100, 40);

            //mh = new MoveHandle(this);
            //mh.Size = new Size(20, 20);
            //mh.Location = new Point(5, 5);
            //components.Add(mh);

            //sh = new SizeHandle(this);
            //sh.Size = new Size(20, 20);
            //sh.Location = new Point(Size.width - sh.Size.width - 5, Size.height - sh.Size.height - 5);
            //sh.ResizeMode = Direction.SOUTH | Direction.EAST;
            //components.Add(sh);

            sc = new Scribble();
            sc.Location = new Point(100, 100);
            sc.Size = new Size(500, 300);

            UPath pth = new UPath(), easypth = new UPath();
            UFigure fig;

            for (float i = 0; i < 100; i += 100)
            {
                var p1 = sc.Location + new Point(200, 200);
                fig = new UFigure(sc.Location, false, true);
                //fig.geoElements.Add(new UEasyArc(-160,160f, new Size(100, 50), true, true, i));
                fig.geoElements.Add(new UBeizer(sc.Location + new Point(20, -20), sc.Location + new Point(60, 30), sc.Location + new Point(100, 0)));
                easypth.figures.Add(fig);
                fig = new UFigure(p1, true, true);
                fig.geoElements.Add(new UArc(p1 += new Point(0, 50), new Size(200, 50), true, true, i));
                fig.geoElements.Add(new UArc(p1 += new Point(0, 50), new Size(150, 50), true, false, i));
                fig.geoElements.Add(new UArc(p1 += new Point(0, 50), new Size(100, 50), true, true, i));
                fig.geoElements.Add(new UArc(p1 += new Point(0, 50), new Size(50, 50), true, false, i));
                pth.figures.Add(fig);
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

            var dr = new AnimatedRect() { area = sc.DisplayRectangle };
            var oleft = dr.area.left;
            var id = BeginDirty(dr);
            Rectangle[] ors = new Rectangle[100];
            int ri = 0;


            UPath pp = new UPath();
            var ff = new UFigure(new Point(10, 10), false, false);
            ff.geoElements.Add(new ULine(new Point(30, 10)));
            ff.geoElements.Add(new ULine(new Point(60, 20)));
            ff.geoElements.Add(new ULine(new Point(20, 50)));
            pp.figures.Add(ff);

            var sw = System.Diagnostics.Stopwatch.StartNew();

            // n-1 * x = n?
            // x=(n-1)/n;

            // ((x1 + x2)/2 + x3/2) * n-1/n = (x1+x2+x3)/3


            sc.draw += (r, b, s) =>
            {
                //r.DrawPath(pp, b, s);
                s.strokeWidth = 1;
                b.color = new Color(.2f, 1, 0, 1);
                r.FillRectangle(sc.DisplayRectangle, b);
                return;

                //foreach (var f in pth.figures)
                //    foreach (UArc a in f.geoElements)
                //        a.resolution = 0.01f + (float)ssw * 0.1f;

                double ssw = (Math.Sin(sw.ElapsedMilliseconds / 1000.0) + 1.0) / 2.0;
                //b.color = new Color(1f, (float)ssw, 0f, 0f);
                r.DrawPath(easypth, b, s);
                r.FillPath(pth, lg2);

                if (dr != null)
                {
                    dr.area.left = oleft + (float)ssw * 50f;
                    if (sw.Elapsed.TotalSeconds > 50)
                    {
                        EndDirty(id);
                        dr = null;
                    }
                }

                //var ti = (r as NoForms.Renderers.SDGDraw).GetSDGTextInfo(tx);
                //float ws = 0;
                //foreach (var gr in ti.glyphRuns)
                //{
                //    if (gr.run.breakingType == SDGDraw.BreakType.line) break;
                //    r.DrawRectangle(new Rectangle(ws+sc.Location.X, sc.Location.Y, gr.run.runSize.width, gr.run.runSize.height), b, s);
                //    ws += gr.run.runSize.width;
                //}

                //r.FillRectangle(new Rectangle(sc.Location, new Size(tx.width, tx.height)), b);
                b.color = new Color(.1f, 0f, 0f, 0f);
                r.FillRectangle(sc.DisplayRectangle, b);


                float h = ((Program.rdr.currentFps)/300f)*tx.height;
                float x = (float)ssw * (tx.width-5) + sc.Location.X;
                ors[ri] = new Rectangle(x, sc.Location.Y +  tx.height - h, 5, h);
                for(int ii=100;ii>=0;ii--)
                {
                    int at = (ri+ii) % 100;
                    b.color = new Color((float)ii/100f, 0f, (float)(ssw + ii/100f)/2f, 0f);
                    r.FillRectangle(ors[at], b);
                }
                ri = (ri+1)%100;
                //if (ors.Count > 100)
                //{
                //    var ns = new Stack<Rectangle>();
                //    t = 0;
                //    foreach (var rct in ors)
                //    {
                //        ns.Push(rct);
                //        if (t++ > 100) break;
                //    }
                //    ors = ns;
                //}

                //r.DrawText(tx, sc.Location, b, UTextDrawOptions.None, false);
            };
            sc.Clicked += p => Size = new Size(Size.width + 10, Size.height+10);
            components.Add(sc);
            SizeChanged += mnf_SizeChanged;
           
        }
        ULinearGradientBrush lg2 = new ULinearGradientBrush()
        {
            point1 = new Point(200, 300),
            point2 = new Point(100, 600),
            color1 = new Color(1, 1, 1, 0),
            color2 = new Color(1, 0, 1, 1)
        };
        ULinearGradientBrush lg = new ULinearGradientBrush()
        {
            point1 = new Point(20, 20),
            point2 = new Point(300, 700),
            color1 = new Color(1, 1, 0, 0),
            color2 = new Color(1, 0, 1, 0)
        };
        UBrush bgb = new USolidBrush() { color = new Color(.5f, 0, 0, 1) };
        public override void Draw(IDraw rt, Region dirty)
        {
            base.Draw(rt, dirty);
            rt.uDraw.FillRectangle(DisplayRectangle, lg);
        }

        void mnf_SizeChanged(Size sz)
        {
            
            //sh.Location = new Point(Size.width - sh.Size.width - 5, Size.height - sh.Size.height - 5);
        }

    }

}
