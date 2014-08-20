using System;
using NoForms.Renderers;
using NoForms.Common;
using System.Threading;
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

        public Thread renderThread = null;
        public void BeginRender()
        {
            renderThread = new Thread(new ThreadStart(() =>
            {
                while (running)
                    RenderPass();
                stopped();
            }));
            running = true;
            renderThread.Start();
        }
        public bool running { get; private set; }
        public event VoidAction stopped = delegate { };
        public void EndRender()
        {
            running = false;
        }

        // object because IRender could be anything, gdi, opengl etc...
        public NoForm noForm { get; set; }
        void RenderPass()
        {
            // Resize the form and backbuffer to noForm.Size, and fire the noForms sizechanged
            Resize();

            lock (noForm)
            {
                // Do Drawing stuff
                noForm.DrawBase(this);

                // flush buffer to window
                var winGr = winForm.CreateGraphics();
                winGr.DrawImageUnscaled(buffer, new System.Drawing.Point(0, 0));
                winGr.Dispose();
            }
        }
        void Resize()
        {
            winForm.Invoke(new System.Windows.Forms.MethodInvoker(() =>
            {
                graphics.Dispose();
                buffer.Dispose();
                winForm.ClientSize = new System.Drawing.Size((int)noForm.Size.width , (int)noForm.Size.height );
                winForm.Location = SDGTr.trI(noForm.Location);
                buffer = new Bitmap(winForm.Width, winForm.Height);
                graphics = Graphics.FromImage(buffer);
                _backRenderer.graphics = graphics;
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
