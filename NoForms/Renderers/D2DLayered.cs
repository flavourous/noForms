using System;
using System.Threading;
using System.Diagnostics;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using SharpDX.Direct2D1;
using SharpDX;
using System.Windows.Forms;
using NoForms.Common;
using NoForms.Renderers;
using NoForms.Windowing;

namespace NoForms.Renderers
{
    public class D2DLayered : IRender<IW32Win>, IDraw
    {
        #region IRender - Bulk of class, providing rendering control (specialised to particular IWindow)
        SharpDX.Direct3D10.Device1 device;
        SharpDX.Direct2D1.Factory d2dFactory = new SharpDX.Direct2D1.Factory();
        SharpDX.DXGI.Factory dxgiFactory = new SharpDX.DXGI.Factory();

        Texture2D backBuffer;
        RenderTargetView renderView;
        Surface1 surface;
        RenderTarget d2dRenderTarget;
        IntPtr someDC;
        IW32Win w32;

        public D2DLayered()
        {
            device = new SharpDX.Direct3D10.Device1(DriverType.Hardware, DeviceCreationFlags.BgraSupport, SharpDX.Direct3D10.FeatureLevel.Level_10_1);
            someDC = Win32Util.GetDC(IntPtr.Zero); // not sure what this exactly does... root dc perhaps...
        }
        public void Init(IW32Win w32, NoForm root)
        {
            this.w32 = w32;
            noForm = root;
            lock (noForm)
            {
                var sz = root.Size;
                // Initialise d2d things
                backBuffer = new Texture2D(device, new Texture2DDescription()
                {
                    ArraySize = 1,
                    MipLevels = 1,
                    SampleDescription = new SampleDescription(1, 0),
                    OptionFlags = ResourceOptionFlags.GdiCompatible,
                    Width = (int)sz.width,
                    Height = (int)sz.height,
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.RenderTarget,
                    Format = Format.B8G8R8A8_UNorm
                });
                renderView = new RenderTargetView(device, backBuffer);
                surface = backBuffer.QueryInterface<Surface1>();
                d2dRenderTarget = new RenderTarget(d2dFactory, surface, new RenderTargetProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));
                scbTrans = new SolidColorBrush(d2dRenderTarget, new Color4(1f, 0f, 1f, 0f)); // set buffer area to transparent

                // Init uDraw and assign IRenderElement parts
                _backRenderer = new D2D_RenderElements(d2dRenderTarget);
                _uDraw = new D2DDraw(_backRenderer);

            }
        }

        IntPtr hWnd;
        SolidColorBrush scbTrans;
        public Thread renderThread = null;
        public void BeginRender()
        {
            renderThread = new Thread(new ThreadStart(() =>
            {
                while (running)
                    RenderPass();

                // Free unmanaged stuff
                scbTrans.Dispose();
                d2dRenderTarget.Dispose();
                surface.Dispose();
                renderView.Dispose();
                backBuffer.Dispose();
            }));

            // Make sure it gets layered!
            hWnd = w32.handle;
            var wl = Win32Util.GetWindowLong(hWnd, Win32Util.GWL_EXSTYLE);
            var ret = Win32Util.SetWindowLong(hWnd, Win32Util.GWL_EXSTYLE, wl | Win32Util.WS_EX_LAYERED);

            // Begin.
            running = true;
            renderThread.Start();
        }
        Object lo = new object();
        bool running = false;
        public void EndRender()
        {
            running = false;
            renderThread.Join();
        }

        // object because IRender could be anything, gdi, opengl etc...
        public NoForm noForm { get; set; }
        void RenderPass()
        {
            // Resize the form and backbuffer to noForm.Size, and fire the noForms sizechanged
            Resize();
            // make size...
            Win32Util.SetWindowLocation(new Win32Util.Point((int)noForm.Location.X, (int)noForm.Location.Y), hWnd);
            Win32Util.SetWindowSize(new Win32Util.Size((int)noForm.Size.width, (int)noForm.Size.height), hWnd);

            lock (noForm)
            {
                // Do Drawing stuff
                DrawingSize rtSize = new DrawingSize((int)d2dRenderTarget.Size.Width, (int)d2dRenderTarget.Size.Height);
                d2dRenderTarget.BeginDraw();
                d2dRenderTarget.PushAxisAlignedClip(noForm.DisplayRectangle, AntialiasMode.Aliased);
                noForm.DrawBase(this);
                d2dRenderTarget.PopAxisAlignedClip();

                // Fill with transparency the edgeBuffer!
                d2dRenderTarget.FillRectangle(new RectangleF(0, noForm.Size.height, noForm.Size.width + edgeBufferSize, noForm.Size.height + edgeBufferSize), scbTrans);
                d2dRenderTarget.FillRectangle(new RectangleF(noForm.Size.width, 0, noForm.Size.width + edgeBufferSize, noForm.Size.height + edgeBufferSize), scbTrans);
                d2dRenderTarget.EndDraw();

                // Present DC to windows (ugh layered windows sad times)
                IntPtr dxHdc = surface.GetDC(false);
                System.Drawing.Graphics dxdc = System.Drawing.Graphics.FromHdc(dxHdc);
                Win32Util.Point dstPoint = new Win32Util.Point((int)noForm.Location.X, (int)noForm.Location.Y);
                Win32Util.Point srcPoint = new Win32Util.Point(0, 0);
                Win32Util.Size pSize = new Win32Util.Size(rtSize.Width, rtSize.Height);
                Win32Util.BLENDFUNCTION bf = new Win32Util.BLENDFUNCTION() { SourceConstantAlpha = 255, AlphaFormat = Win32Util.AC_SRC_ALPHA, BlendFlags = 0, BlendOp = 0 };

                bool suc = Win32Util.UpdateLayeredWindow(hWnd, someDC, ref dstPoint, ref pSize, dxHdc, ref srcPoint, 1, ref bf, 2);
                surface.ReleaseDC();
                dxdc.Dispose();
            }
        }
        public int edgeBufferSize = 128;
        void Resize()
        {
            d2dRenderTarget.Dispose();
            renderView.Dispose();
            surface.Dispose();
            backBuffer.Dispose();

            // Initialise d2d things
            backBuffer = new Texture2D(device, new Texture2DDescription()
            {
                ArraySize = 1,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                OptionFlags = ResourceOptionFlags.GdiCompatible,
                Width = (int)noForm.Size.width + edgeBufferSize,
                Height = (int)noForm.Size.height + edgeBufferSize,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget,
                Format = Format.B8G8R8A8_UNorm
            });
            renderView = new RenderTargetView(device, backBuffer);
            surface = backBuffer.QueryInterface<Surface1>();
            d2dRenderTarget = new RenderTarget(d2dFactory, surface, new RenderTargetProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));
            _backRenderer.renderTarget = d2dRenderTarget;
        }

        public void Dispose()
        {
            d2dFactory.Dispose();
            dxgiFactory.Dispose();
            device.Dispose();
        }
        #endregion

        #region IDraw - Client facing interface to drawing commands and so on
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
        #endregion

        
      }    
}
