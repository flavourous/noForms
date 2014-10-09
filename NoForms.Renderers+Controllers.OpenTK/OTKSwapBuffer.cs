using System;
using System.Threading;
using STimer = System.Threading.Timer;
using System.Diagnostics;
using NoForms.Renderers;
using NoForms.Common;
using NoForms;
using NoForms.Platforms.Win32;
using OpenTK.Platform;
using OpenTK.Graphics;
using lOpenTK = OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace NoForms.Renderers_Controllers.OpenTK
{
    public class OpenTKSwapBuffer : IRender<IW32Win>, IDraw
    {
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
            dobs = new DirtyObserver(noForm, RenderPass, () => noForm.DirtyAnimated, () => noForm.ReqSize);
        }
        void HandleCreatedStuff()
        {
            GraphicsContext glContext;
            var iwi = Utilities.CreateWindowsWindowInfo(w32.handle);
            glContext = new GraphicsContext(GraphicsMode.Default, iwi);
            glContext.MakeCurrent(iwi);
            (glContext as IGraphicsContextInternal).LoadAll();
            glContext.SwapInterval = 1; // vsync

            // Init uDraw and assign IRenderElement parts
            _backRenderer = new OpenTK_RenderElements(glContext);
            _uDraw = new OTKDraw(_backRenderer);

        }
        public void BeginRender()
        {
            // hook move!
            noForm.LocationChanged += noForm_LocationChanged;

            // Start the watcher
            dobs.Dirty(noForm.DisplayRectangle);
            dobs.running = true;
            dobs.StartObserving(HandleCreatedStuff);

            noForm_LocationChanged(noForm.Location);
        }

        void noForm_LocationChanged(Point obj)
        {
            Win32Util.SetWindowLocation(new Win32Util.Point((int)noForm.Location.X, (int)noForm.Location.Y), w32.handle);
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

        public event Action<Size> RenderSizeChanged = delegate { };

        public NoForm noForm { get; set; }
        Stopwatch renderTime = new Stopwatch();
        public float currentFps { get; private set; }
        void RenderPass(Common.Region dc, Common.Size ReqSize)
        {
            renderTime.Start();
            // Resize the form and backbuffer to noForm.Size
            Resize(ReqSize);

            Win32Util.Size w32Size = new Win32Util.Size((int)ReqSize.width, (int)ReqSize.height);
            Win32Util.SetWindowSize(w32Size, w32.handle);

            // Allow noform size to change as requested..like a layout hook (truncating layout passes with the render passes for performance)
            RenderSizeChanged(ReqSize);

            // Do Drawing stuff

            // 1) render to fbo
            throw new NotImplementedException();

             
// Create Color Texture
            uint ColorTexture;
            int FboWidth = (int)ReqSize.width;
            int FboHeight = (int)ReqSize.height;
GL.GenTextures( 1, out ColorTexture );
GL.BindTexture( TextureTarget.Texture2D, ColorTexture );
GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest );
GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest );
GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Clamp );
GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Clamp );
GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, FboWidth, FboHeight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero );

            // create fbo
uint FboHandle;
GL.Ext.GenFramebuffers(1, out FboHandle);
GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, FboHandle);
GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, ColorTexture, 0);
 
            // draw to fbo..?
            noForm.DrawBase(this, dc);
            // unbind fbo
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0); // return to visible framebuffer
            GL.DrawBuffer(DrawBufferMode.Back);

            // 2) go through dirty rects and copy to the window context
            throw new NotImplementedException();
            foreach (var dr in dc.AsRectangles())
            {
                //dostuff
            }

            // 3) call swapbuffers on window context...
            throw new NotImplementedException();

            currentFps = 1f / (float)renderTime.Elapsed.TotalSeconds;
            renderTime.Reset();
        }
       
        void ResizeContext(ref GraphicsContext gc, float w, float h)
        {
            GL.Viewport(0, 0, (int)w, (int)h);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0.0, w, 0.0, h, 0.0, 1.0);
        }
        void Resize(Common.Size ReqSize)
        {

        }

        // IRenderType
        IUnifiedDraw _uDraw;
        public IUnifiedDraw uDraw
        {
            get { return _uDraw; }
        }
        OpenTK_RenderElements _backRenderer;
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
        }
    }
}
