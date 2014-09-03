using System;
using System.Threading;
using STimer = System.Threading.Timer;
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
    public class D2DSwapChain : IRender<IW32Win>, IDraw
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
        IW32Win w32;

        public void Init(IW32Win w32, NoForm root)
        {
            // do the form
            noForm = root;
            noForm.renderer = this;
            this.w32 = w32;
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
        public void BeginRender()
        {
            // Make sure it gets layered!
            winHandle = w32.handle;

            // dirty it
            Dirty(noForm.DisplayRectangle);

            // hook move!
            noForm.LocationChanged += noForm_LocationChanged;

            // sutff
            HandleCreatedStuff();

            // Start the watcher
            dtm = new STimer(o => DirtyLook(), null, 0, 17);
            running = true;
        }

        void noForm_LocationChanged(Point obj)
        {
            Win32Util.SetWindowLocation(new Win32Util.Point((int)noForm.Location.X, (int)noForm.Location.Y), winHandle);
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
                d2dRenderTarget.Dispose();
                surface.Dispose();
                renderView.Dispose();
                backBuffer.Dispose();
            }
        }
        Region dirty = new Region();
        public void Dirty(Common.Rectangle rect)
        {
            lock (lock_dirty)
                dirty.Add(rect);
        }
        STimer dtm;
        void DirtyLook()
        {
            Region dc = null;
            lock (lock_dirty)
            {
                if (dirty.IsEmpty) return;
                dc = new Region(dirty);
                dirty.Reset();
            }

            lock (lock_render)
            {
                if (!running) return;
                if (dc != null) RenderPass(dc);
            }
        }


        public NoForm noForm { get; set; }
        void RenderPass(Region dc)
        {
            // Resize the form and backbuffer to noForm.Size
            Resize();

            Win32Util.Size w32Size = new Win32Util.Size((int)d2dRenderTarget.Size.Width, (int)d2dRenderTarget.Size.Height);
            Win32Util.SetWindowSize(w32Size, winHandle);


            lock (noForm)
            {
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
            }
        }
        void Resize()
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

            swapchain.ResizeBuffers(0, (int)noForm.Size.width, (int)noForm.Size.height, Format.B8G8R8A8_UNorm, SwapChainFlags.None);
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
