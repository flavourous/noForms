using System;
using System.Threading;
using STimer = System.Threading.Timer;
using System.Diagnostics;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using NoForms.Renderers;
using SharpDX.Direct2D1;
using SharpDX;
using NoForms.Common;
using NoForms.Platforms.Win32;
using SharpDXLib = SharpDX;

namespace NoForms.Renderers.SharpDX
{
    public class D2DSwapChain : IRender<IW32Win>, IDraw
    {
        public float FPSLimit { get; set; }
        public D2DSwapChain()
        {
            FPSLimit = 60;
        }

        SharpDXLib.Direct3D10.Device1 device;
        SharpDXLib.Direct2D1.Factory d2dFactory = new SharpDXLib.Direct2D1.Factory();
        SharpDXLib.DXGI.Factory dxgiFactory = new SharpDXLib.DXGI.Factory();

        SwapChain swapchain;
        Texture2D backBuffer;
        RenderTargetView renderView;
        Surface1 surface;
        RenderTarget d2dRenderTarget;
        IntPtr winHandle;
        IW32Win w32;

        DirtyObserver dobs;
        public void Dirty(Common.Rectangle dr) { dobs.Dirty(dr); }

        public void Init(IW32Win w32, NoForm root)
        {
            // do the form
            noForm = root;
            noForm.renderer = this;
            this.w32 = w32;

            // Create the observer
            dobs = new DirtyObserver(noForm, RenderPass, () => noForm.DirtyAnimated, () => noForm.ReqSize, () => FPSLimit);    
        }
        void HandleCreatedStuff()
        {
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
            SharpDXLib.Direct3D10.Device1.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport, swapchainDescription, out device, out swapchain);

            // Initialise d2d things
            backBuffer = Texture2D.FromSwapChain<Texture2D>(swapchain, 0);
            renderView = new RenderTargetView(device, backBuffer);
            surface = backBuffer.QueryInterface<Surface1>();
            d2dRenderTarget = new RenderTarget(d2dFactory, surface, new RenderTargetProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));

            // Init uDraw and assign IRenderElement parts
            _backRenderer = new SharpDX_RenderElements(d2dRenderTarget);
            _uDraw = new D2DDraw(_backRenderer);

        }
        public void BeginRender()
        {
            winHandle = w32.handle;

            // hook move!
            noForm.LocationChanged += noForm_LocationChanged;

            // sutff
            HandleCreatedStuff();

            // Start the watcher
            dobs.Dirty(noForm.DisplayRectangle);
            dobs.running = true;
            dobs.StartObserving();

            noForm_LocationChanged(noForm.Location);
        }

        void noForm_LocationChanged(Point obj)
        {
            Win32Util.SetWindowLocation(new Win32Util.Point((int)noForm.Location.X, (int)noForm.Location.Y), winHandle);
        }
        public void EndRender()
        {
            lock (dobs.lock_render)
            {
                // Free unmanaged stuff
                noForm.LocationChanged -= noForm_LocationChanged;
                dobs.running = false;
                d2dRenderTarget.Dispose();
                surface.Dispose();
                renderView.Dispose();
                backBuffer.Dispose();
            }
        }

        public event Action<Size> RenderSizeChanged = delegate { };

        public NoForm noForm { get; set; }
        Stopwatch renderTime = new Stopwatch();
        public float lastFrameRenderDuration { get; private set; }
        void RenderPass(Common.Region dc, Common.Size ReqSize)
        {
            renderTime.Start();
            // Resize the form and backbuffer to noForm.Size
            Resize(ReqSize);

            Win32Util.Size w32Size = new Win32Util.Size((int)ReqSize.width, (int)ReqSize.height);
            Win32Util.SetWindowSize(w32Size, winHandle); // FIXME blocks when closing->endrender event is locked...

            // Allow noform size to change as requested..like a layout hook (truncating layout passes with the render passes for performance)
            RenderSizeChanged(ReqSize);

            // Do Drawing stuff
            DrawingSize rtSize = new DrawingSize((int)d2dRenderTarget.Size.Width, (int)d2dRenderTarget.Size.Height);
            using (Texture2D t2d = new Texture2D(backBuffer.Device, backBuffer.Description))
            {
                using (Surface1 srf = t2d.QueryInterface<Surface1>())
                {
                    using (RenderTarget trt = new RenderTarget(d2dFactory, srf, new RenderTargetProperties(d2dRenderTarget.PixelFormat)))
                    {
                        _backRenderer.renderTarget = trt;
                        trt.BeginDraw();
                        noForm.DrawBase(this, dc);
                        trt.EndDraw();

                        foreach (var rc in dc.AsRectangles())
                            t2d.Device.CopySubresourceRegion(t2d, 0,
                                new ResourceRegion() { Left = (int)rc.left, Right = (int)rc.right, Top = (int)rc.top, Bottom = (int)rc.bottom, Back = 1, Front = 0 }, backBuffer, 0,
                                (int)rc.left, (int)rc.top, 0);
                    }
                }
            }
            swapchain.Present(0, PresentFlags.None);

            //System.Threading.Thread.Sleep(1000);
            lastFrameRenderDuration = 1f / (float)renderTime.Elapsed.TotalSeconds;
            renderTime.Reset();
        }
        void Resize(Common.Size ReqSize)
        {
            var w = (int)d2dRenderTarget.Size.Width;
            var h = (int)d2dRenderTarget.Size.Height;
            var nbb = new Texture2D(device, backBuffer.Description);
            ResourceRegion rrgn = new ResourceRegion() { Front = 0, Back = 1, Top = 0, Left = 0, Right = w, Bottom = h };
            device.CopySubresourceRegion(backBuffer, 0, rrgn, nbb, 0, 0, 0, 0);

            d2dRenderTarget.Dispose();
            renderView.Dispose();
            surface.Dispose();
            backBuffer.Dispose();

            swapchain.ResizeBuffers(0, (int)ReqSize.width, (int)ReqSize.height, Format.B8G8R8A8_UNorm, SwapChainFlags.None);
            backBuffer = Texture2D.FromSwapChain<Texture2D>(swapchain, 0);
            renderView = new RenderTargetView(device, backBuffer);
            surface = backBuffer.QueryInterface<Surface1>();
            d2dRenderTarget = new RenderTarget(d2dFactory, surface, new RenderTargetProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));

            device.CopySubresourceRegion(nbb, 0, rrgn, backBuffer, 0, 0, 0, 0);
            nbb.Dispose();
        }

        // IRenderType
        IUnifiedDraw _uDraw;
        public IUnifiedDraw uDraw
        {
            get { return _uDraw; }
        }
        SharpDX_RenderElements _backRenderer;
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
