using System;
using NoForms.Renderers;
using NoForms.Common;
using System.Threading;
using STimer = System.Threading.Timer;
using System.Diagnostics;
using NoForms.Platforms.DotNet;
using System.Windows.Forms;
using System.Drawing;

namespace NoForms.Renderers.DotNet
{
    // Base Renderers, exposing some drawing mechanism and options
    public class SDGNormal : IRender<IWFWin>, IDraw
    {
        public float FPSLimit { get; set; }
        public SDGNormal()
        {
            FPSLimit = 60;
        }

        #region IRender - Bulk of class, providing rendering control (specialised to particular IWindow)
        Form winForm;
        Graphics graphics;
        Bitmap buffer;

        DirtyObserver dobs;
        public void Dirty(Common.Rectangle dr) { dobs.Dirty(dr); }

        public void Init(IWFWin wf, NoForm root)
        {
            noForm = root;
            noForm.renderer = this;
                winForm = wf.form;

                // Create buffer
                buffer = new Bitmap(winForm.Width, winForm.Height);
                graphics = Graphics.FromImage(buffer);

                // Init uDraw and assign IRenderElement parts
                _backRenderer = new SDG_RenderElements(graphics);
                _uDraw = new SDGDraw(_backRenderer);

            // Create the observer
            dobs = new DirtyObserver(noForm, RenderPass, () => noForm.DirtyAnimated, () => noForm.ReqSize, () => FPSLimit);    
        }

        public void BeginRender()
        {
            // hook move!
            noForm.LocationChanged += noForm_LocationChanged;

            // Start the watcher
            dobs.Dirty(noForm.DisplayRectangle);
            dobs.running = true;
            dobs.StartObserving();

            noForm_LocationChanged(noForm.Location);
        }

        void noForm_LocationChanged(Common.Point obj)
        {
            winForm.Location = new System.Drawing.Point((int)noForm.Location.X, (int)noForm.Location.Y);
        }

        public void EndRender()
        {
            lock (dobs.lock_render)
            {
                // Free unmanaged stuff
                noForm.LocationChanged -= noForm_LocationChanged;
                dobs.running = false;
            }
        }

        public event Action<Common.Size> RenderSizeChanged = delegate { };

        // object because IRender could be anything, gdi, opengl etc...
        public NoForm noForm { get; set; }
        Stopwatch renderTime = new Stopwatch();
        public float lastFrameRenderDuration { get; private set; }
        SolidBrush blackBack = new SolidBrush(System.Drawing.Color.FromArgb(255, 0, 0, 0));
        void RenderPass(Common.Region dc, Common.Size ReqSize)
        {
            renderTime.Start();
            // Resize the backbuffer to noForm.Size, and fire the noForms sizechanged
            Resize(ReqSize);

            // Allow noform size to change as requested..like a layout hook (truncating layout passes with the render passes for performance)
            RenderSizeChanged(ReqSize);

                foreach (var dr in dc.AsRectangles())
                {
                    var sdr = SDGTr.trF(dr);
                    graphics.FillRectangle(blackBack, sdr);
                }

                // Do Drawing stuff
                noForm.DrawBase(this, dc);

                //res
                winForm.Invoke(new System.Windows.Forms.MethodInvoker(() =>
                {
                    winForm.ClientSize = new System.Drawing.Size((int)ReqSize.width, (int)ReqSize.height);
                    winForm.Location = SDGTr.trI(noForm.Location);
                }));

                // flush buffer to window
                var winGr = winForm.CreateGraphics();
                foreach (var dr in dc.AsRectangles())
                {
                    var sdr = SDGTr.trF(dr);
                    winGr.DrawImage(buffer, sdr, sdr, GraphicsUnit.Pixel);
                }
                winGr.Dispose();
            lastFrameRenderDuration = 1f / (float)renderTime.Elapsed.TotalSeconds;
            renderTime.Reset();
        }
        void Resize(Common.Size ReqSize)
        {

            var obb = buffer;
            graphics.Dispose();


            buffer = new Bitmap((int)ReqSize.width, (int)ReqSize.height);
            graphics = Graphics.FromImage(buffer);
            graphics.DrawImageUnscaledAndClipped(obb, new System.Drawing.Rectangle(0, 0, buffer.Width, buffer.Height));

            _backRenderer.graphics = graphics;

            obb.Dispose();

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
