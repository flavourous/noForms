using System;
using System.Threading;
using System.Diagnostics;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using NoForms.Renderers;
using SharpDX.Direct2D1;
using SharpDX;
using System.Windows.Forms;
using NoForms.Common;
using NoForms.Windowing;

namespace NoForms.Renderers
{
    public class D2DSwapChain : IRender<IWFWin>, IDraw
    {
        SharpDX.Direct3D10.Device1 device;
        SharpDX.Direct2D1.Factory d2dFactory = new SharpDX.Direct2D1.Factory();
        SharpDX.DXGI.Factory dxgiFactory = new SharpDX.DXGI.Factory();

        SwapChain swapchain;
        Texture2D backBuffer;
        RenderTargetView renderView;
        Surface1 surface;
        RenderTarget d2dRenderTarget;
        IntPtr winHandle;
        Form winForm;

        public void Init(IWFWin iwf, NoForm root)
        {
            // do the form
            noForm = root;
            noForm.renderer = this;
            this.winForm = iwf.form;
            winHandle = iwf.form.Handle;

            SwapChainDescription swapchainDescription = new SwapChainDescription()
            {
                BufferCount = 1,
                Flags = SwapChainFlags.GdiCompatible,
                IsWindowed = true,
                ModeDescription = new ModeDescription((int)noForm.Size.width, (int)noForm.Size.height, new Rational(60, 1), Format.B8G8R8A8_UNorm),
                OutputHandle = winHandle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Sequential,
                Usage = Usage.RenderTargetOutput
            };
            SharpDX.Direct3D10.Device1.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport, swapchainDescription, out device, out swapchain);

            // Initialise d2d things
            backBuffer = Texture2D.FromSwapChain<Texture2D>(swapchain, 0);
            renderView = new RenderTargetView(device, backBuffer);
            surface = backBuffer.QueryInterface<Surface1>();
            d2dRenderTarget = new RenderTarget(d2dFactory, surface, new RenderTargetProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));

            // Init uDraw and assign IRenderElement parts
            _backRenderer = new D2D_RenderElements(d2dRenderTarget);
            _uDraw = new D2DDraw(_backRenderer);

            
        }
        public Thread renderThread = null;
        public void BeginRender()
        {
            renderThread = new Thread(new ThreadStart(() =>
            {
                lock (lo)
                {
                    while (running)
                        RenderPass();

                    // free unmanaged
                    d2dRenderTarget.Dispose();
                    surface.Dispose();
                    renderView.Dispose();
                    backBuffer.Dispose();
                    swapchain.Dispose();
                    device.Dispose();
                }
            }));
            running = true;
            renderThread.Start();
        }
        Object lo = new object();
        bool running = false;
        public void EndRender()
        {
            running = false;
            lock (lo) { }
        }

        Common.Region dirty = new Common.Region();
        public void Dirty(Common.Rectangle rect)
        {
            dirty.Add(rect);
        }

        public NoForm noForm { get; set; }
        void RenderPass()
        {
            lock (noForm)
            {
                // Resize the form and backbuffer to noForm.Size
                Resize();

                // Do Drawing stuff
                DrawingSize rtSize = new DrawingSize((int)d2dRenderTarget.Size.Width, (int)d2dRenderTarget.Size.Height);
                d2dRenderTarget.BeginDraw();
                noForm.DrawBase(this, dirty);
                d2dRenderTarget.EndDraw();

                winForm.BeginInvoke(new System.Windows.Forms.MethodInvoker(() =>
                {
                    winForm.Size = SDGTr.trI(noForm.Size);
                    winForm.Location = SDGTr.trI(noForm.Location);
                }));

                swapchain.Present(0, PresentFlags.None);
            }
        }
        void Resize()
        {
            d2dRenderTarget.Dispose();
            renderView.Dispose();
            surface.Dispose();
            backBuffer.Dispose();

            swapchain.ResizeBuffers(0, (int)noForm.Size.width, (int)noForm.Size.height, Format.B8G8R8A8_UNorm, SwapChainFlags.None);
            backBuffer = Texture2D.FromSwapChain<Texture2D>(swapchain, 0);
            renderView = new RenderTargetView(device, backBuffer);
            surface = backBuffer.QueryInterface<Surface1>();
            d2dRenderTarget = new RenderTarget(d2dFactory, surface, new RenderTargetProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));
            _backRenderer.renderTarget = d2dRenderTarget;
        }

        // IRenderType
        IUnifiedDraw _uDraw;
        public IUnifiedDraw uDraw
        {
            get { return _uDraw; }
        }
        D2D_RenderElements _backRenderer;
        public IRenderElements backRenderer
        {
            get { return _backRenderer; }
        }
        public UnifiedEffects uAdvanced
        {
            get { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
            d2dFactory.Dispose();
            dxgiFactory.Dispose();
        }
    }
}
