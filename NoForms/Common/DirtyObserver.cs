using System;
using System.Diagnostics;
using System.Collections.Generic;
using NoForms.Common;
using System.Threading;

namespace NoForms.Common
{
    public delegate void RenderHandler(Region dirty, Size ReqSize);
    public delegate IEnumerable<AnimatedRect> AnimatedHandler();
    public delegate Size RequestSizeHandler();
    public delegate float GetFPSLimit();
    public class DirtyObserver
    {
        Object ref_noForm;
        RenderHandler ref_render;
        AnimatedHandler ref_anim;
        RequestSizeHandler ref_rsh;
        GetFPSLimit ref_fps;
        // FIXME just accept some INoForm and IRender
        public DirtyObserver(Object nf, RenderHandler ren, AnimatedHandler ah, RequestSizeHandler rsh, GetFPSLimit fpslim)
        {
            ref_noForm = nf;
            ref_render = ren;
            ref_anim = ah;
            ref_rsh = rsh;
            ref_fps = fpslim;
        }

        public void StartObserving(VoidAction init = null)
        {
            dthread = new Thread(DirtyObs);
            dthread.IsBackground = false;
            dthread.Start(init ?? delegate { });
        }

        Region dirty = new Region();
        public Object lock_dirty = new object(), lock_render = new object();
        public bool running = false;
        public void Dirty(Common.Rectangle rect)
        {
            lock (lock_dirty)
                dirty.Add(rect);
        }
        Thread dthread;
        void DirtyObs(Object o)
        {
            //try
            //{
                (o as VoidAction)();
                while (running)
                    DirtyLook();
            //}
            //catch(Exception e)
            //{
            //    Console.WriteLine("Fatal exception on rendering thread");
            //    Console.Write(e.ToString());
            //    Environment.Exit(-1);
            //}
        }
        Stopwatch ft = new Stopwatch();
        void DirtyLook()
        {
            Region dc = null;
            Size ReqSize;
            lock (ref_noForm) lock (lock_dirty)
                {
                    // dirty animated regions...
                    foreach (var adr in ref_anim()) dirty.Add(adr.area);

                    if (dirty.IsEmpty) return;
                    dc = new Region(dirty);
                    dirty.Reset();

                    ReqSize = ref_rsh();
                }

            lock (lock_render)
            {
                TimeSpan rt;
                if (!running) return;
                if (dc != null)
                {
                    ft.Start();
                    rt = new TimeSpan((long)(TimeSpan.TicksPerSecond / ref_fps())); // FIXME overcalc
                    ref_render(dc, ReqSize);
                    ft.Stop();
                    if (rt > ft.Elapsed) // use this comparison :)
                        Thread.Sleep(rt.Subtract(ft.Elapsed));
                    ft.Reset();
                }
            }
        }
    }
}
