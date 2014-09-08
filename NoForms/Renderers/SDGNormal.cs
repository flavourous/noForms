using System;
using NoForms.Renderers;
using NoForms.Common;
using System.Threading;
using STimer = System.Threading.Timer;
using System.Diagnostics;
using NoForms.Windowing;
using System.Windows.Forms;
using System.Drawing;

namespace NoForms.Renderers
{
    // Base Renderers, exposing some drawing mechanism and options
    public class SDGNormal : IRender<IWFWin>, IDraw
    {
        #region IRender - Bulk of class, providing rendering control (specialised to particular IWindow)
        Form winForm;
        Graphics graphics;
        Bitmap buffer;

        public void Init(IWFWin wf, NoForm root)
        {
            noForm = root;
            noForm.renderer = this;
            lock (noForm)
            {
                winForm = wf.form;

                // Create buffer
                buffer = new Bitmap(winForm.Width, winForm.Height);
                graphics = Graphics.FromImage(buffer);

                // Init uDraw and assign IRenderElement parts
                _backRenderer = new SDG_RenderElements(graphics);
                _uDraw = new SDGDraw(_backRenderer);

            }

        }

        public void BeginRender()
        {
            // dirty it
            Dirty(noForm.DisplayRectangle);

            // hook move!
            noForm.LocationChanged += noForm_LocationChanged;

            // Start the watcher
            running = true;
            DirtyObserver = new Thread(DirtyObs);
            DirtyObserver.IsBackground = false;
            DirtyObserver.Start();
            noForm_LocationChanged(noForm.Location);
        }

        void noForm_LocationChanged(Common.Point obj)
        {
            winForm.Location = new System.Drawing.Point((int)noForm.Location.X, (int)noForm.Location.Y);
        }

        public Object lock_dirty = new object(), lock_render = new object();
        bool running = false;
        public void EndRender()
        {
            lock (lock_render)
            {
                // Free unmanaged stuff
                noForm.LocationChanged -= noForm_LocationChanged;
                running = false;
            }
        }
        Common.Region dirty = new Common.Region();
        public void Dirty(Common.Rectangle rect)
        {
            lock (lock_dirty)
                dirty.Add(rect);
        }
        Thread DirtyObserver;
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
            Common.Region dc = null;
            lock (lock_dirty)
            {
                // dirty animated regions...
                foreach (var adr in noForm.DirtyAnimated) dirty.Add(adr.area);

                if (dirty.IsEmpty) return;
                dc = new Common.Region(dirty);
                dirty.Reset();
            }

            lock (lock_render)
            {
                if (!running) return;
                if (dc != null) RenderPass(dc);
            }
        }

        // object because IRender could be anything, gdi, opengl etc...
        public NoForm noForm { get; set; }
        Stopwatch renderTime = new Stopwatch();
        public float currentFps { get; private set; }
        void RenderPass(Common.Region dc)
        {
            renderTime.Start();
            // Resize the form and backbuffer to noForm.Size, and fire the noForms sizechanged
            Resize();

            lock (noForm)
            {
                // Do Drawing stuff
                noForm.DrawBase(this, dc);

                // flush buffer to window
                var winGr = winForm.CreateGraphics();
                foreach (var dr in dc.AsRectangles())
                {
                    var sdr = SDGTr.trF(dr);
                    winGr.DrawImage(buffer, sdr, sdr, GraphicsUnit.Pixel);
                }
                winGr.Dispose();
            }
            currentFps = 1f / (float)renderTime.Elapsed.TotalSeconds;
            renderTime.Reset();
        }
        void Resize()
        {
            winForm.Invoke(new System.Windows.Forms.MethodInvoker(() =>
            {
                var obb = buffer;
                graphics.Dispose();
                
                winForm.ClientSize = new System.Drawing.Size((int)noForm.Size.width , (int)noForm.Size.height );
                winForm.Location = SDGTr.trI(noForm.Location);
                buffer = new Bitmap(winForm.Width, winForm.Height);
                graphics = Graphics.FromImage(buffer);
                graphics.DrawImageUnscaledAndClipped(obb, new System.Drawing.Rectangle(0, 0, buffer.Width, buffer.Height));

                _backRenderer.graphics = graphics;

                obb.Dispose();

            }));
        }

        #endregion

        #region IDraw - Client facing interface to drawing commands and so on
        IUnifiedDraw _uDraw;
        public IUnifiedDraw uDraw
        {
            get { return _uDraw; }
        }
        SDG_RenderElements _backRenderer;
        public IRenderElements backRenderer
        {
            get { return _backRenderer; }
        }
        public UnifiedEffects uAdvanced
        {
            get { throw new NotImplementedException(); }
        }
        #endregion

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }    
}
