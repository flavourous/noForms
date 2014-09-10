using System;
using System.Collections.Generic;
using NoForms.Common;
using System.Threading;

namespace NoForms.Renderers
{
    delegate void RenderHandler(Region dirty, Size ReqSize);
    delegate bool RunningHandler();
    class DirtyObserver
    {
        NoForm ref_noForm;
        RenderHandler ref_render;
        public DirtyObserver(NoForm nf, RenderHandler ren)
        {
            ref_noForm = nf;
            ref_render = ren;
        }

        public void StartObserving()
        {
            dthread = new Thread(DirtyObs);
            dthread.IsBackground = false;
            dthread.Start();
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
            while (running)
            {
                DirtyLook();
                Thread.Sleep(17);
            }
        }
        void DirtyLook()
        {
            Region dc = null;
            Size ReqSize;
            lock (ref_noForm) lock (lock_dirty)
                {
                    // dirty animated regions...
                    foreach (var adr in ref_noForm.DirtyAnimated) dirty.Add(adr.area);

                    if (dirty.IsEmpty) return;
                    dc = new Region(dirty);
                    dirty.Reset();

                    ReqSize = ref_noForm.ReqSize;
                }

            lock (lock_render)
            {
                if (!running) return;
                if (dc != null) ref_render(dc, ReqSize);
            }
        }

    }
}
