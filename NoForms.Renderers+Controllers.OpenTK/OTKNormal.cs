using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using NoForms.Common;
using NoForms.Platforms.Win32;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform;
using OpenTK.Graphics.OpenGL;

namespace NoForms.Renderers.OpenTK
{
    public class OTKNormal : IRender<IW32Win>, IDraw
    {
        IW32Win w32;
        DirtyObserver dobs;
        IGraphicsContext glContext;
        IWindowInfo winfo;

        public void Init(IW32Win initObj, NoForm nf)
        {
            // do the form
            noForm = nf;
            noForm.renderer = this;
            this.w32 = initObj;

            // Create the observer
            dobs = new DirtyObserver(noForm, RenderPass, () => noForm.DirtyAnimated, () => noForm.ReqSize);    
        }

        public void BeginRender()
        {
            // hook move!
            noForm.LocationChanged += noForm_LocationChanged;

            // Start the watcher
            dobs.Dirty(noForm.DisplayRectangle);
            dobs.running = true;
            dobs.StartObserving(() =>
            {
                // get the glinfo
                winfo = Utilities.CreateWindowsWindowInfo(w32.handle);

                // get context running...
                glContext = new GraphicsContext(GraphicsMode.Default, winfo);
                (glContext as IGraphicsContextInternal).LoadAll(); // makes current i think. or constructor does.
                glContext.SwapInterval = 0; // vsync off, manage self.

                // saveem
                _backRenderer = new OpenTK_RenderElements(glContext);
                _uDraw = new OTKDraw();

                // generate buffers
                FBO_Draw = GL.Ext.GenFramebuffer();
                FBO_Window = GL.Ext.GenFramebuffer();
                T2D_Draw = GL.GenTexture();
                T2D_Window = GL.GenTexture();
            });

            noForm_LocationChanged(noForm.Location);
        }
        void noForm_LocationChanged(Point obj)
        {
            Win32Util.SetWindowLocation(new Win32Util.Point((int)noForm.Location.X, (int)noForm.Location.Y), w32.handle);
        }

        Stopwatch sw = Stopwatch.StartNew();
        void RenderPass(Region dc, Size ReqSize)
        {
            renderTime.Start();
            double w = ReqSize.width, h = ReqSize.height;

            // Resize the window
            Win32Util.Size w32Size = new Win32Util.Size((int)ReqSize.width, (int)ReqSize.height);
            Win32Util.SetWindowSize(w32Size, w32.handle);

            // Allow noform size to change as requested..like a layout hook (truncating layout passes with the render passes for performance)
            RenderSizeChanged(ReqSize);

            foreach (var tex in new int[] { T2D_Draw, T2D_Window })
            {
                GL.BindTexture(TextureTarget.Texture2D, tex); // bind to texture, set things,
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, (int)w, (int)h, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
                GL.BindTexture(TextureTarget.Texture2D, 0);// unbind from texture
            }

            // bind to framebuffer, and bind its output to the texture
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, FBO_Draw);
            GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, T2D_Draw, 0);

            // Alpha...
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // set up ortho
            GL.Viewport(0, 0, (int)w, (int)h);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0.0, w, 0.0, h, 0.0, 1.0);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            // draw on fbo
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0, 0, 0, 0 }); // fixme does this do anything? :/
            noForm.DrawBase(this, dc);
            
            // Intermediate render to window buffer...
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, FBO_Window);
            GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, T2D_Window, 0);

            // Alpha...
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // set up ortho
            GL.Viewport(0, 0, (int)w, (int)h);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0.0, w, 0.0, h, 0.0, 1.0);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            
            
            // Texture mapping...
            GL.Enable(EnableCap.Texture2D);

            // set up ortho
            GL.Viewport(0, 0, (int)w, (int)h);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0.0, w, 0.0, h, 0.0, 1.0);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            //GL.Clear(ClearBufferMask.ColorBufferBit);

            // Draw the Color Texture - only parts of it!!
            GL.BindTexture(TextureTarget.Texture2D, T2D_Draw);
            GL.Color4(1f, 1f, 1f, 1f);
            GL.Disable(EnableCap.Blend); // we just want to emplace.
            GL.Begin(PrimitiveType.Quads);

            foreach (var d in dc.AsRectangles())
            {
                double fl = d.left / w;
                double fr = d.right / w;
                double ft = d.top / h;
                double fb = d.bottom / h;

                double l = d.left; double r = d.right;
                double t = d.top; double b = d.bottom;

                GL.TexCoord2(fl, ft);
                GL.Vertex2(l, t);
                GL.TexCoord2(fr, ft);
                GL.Vertex2(r, t);
                GL.TexCoord2(fr, fb);
                GL.Vertex2(r, b);
                GL.TexCoord2(fl, fb);
                GL.Vertex2(l, b);
            }
            GL.End();
            GL.BindTexture(TextureTarget.Texture2D, 0);

            // bind to window (FBO Zero!)
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);

            // Texture mapping...
            GL.Enable(EnableCap.Texture2D);

            // set up ortho
            GL.Viewport(0, 0, (int)w, (int)h);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0.0, w, 0.0, h, 0.0, 1.0);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.BindTexture(TextureTarget.Texture2D, T2D_Window);
            GL.Color4(1f, 1f, 1f, 1f);
            GL.Disable(EnableCap.Blend); // we just want to emplace.

            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0f, 1f);
            GL.Vertex2(0.0, 0.0);
            GL.TexCoord2(1f, 1f);
            GL.Vertex2(w, 0.0);
            GL.TexCoord2(1f, 0f);
            GL.Vertex2(w, h);
            GL.TexCoord2(0f, 0f);
            GL.Vertex2(0.0, h);
            GL.End();
            
            GL.BindTexture(TextureTarget.Texture2D, 0);

            // Swap buffers on window FBO
            glContext.SwapBuffers();

            currentFps = 1f / (float)renderTime.Elapsed.TotalSeconds;
            renderTime.Reset();
        }

        int FBO_Draw, T2D_Draw, FBO_Window, T2D_Window;
    
        public void EndRender()
        {
            lock (dobs.lock_render)
            {
                noForm.LocationChanged -= noForm_LocationChanged;
                dobs.running = false;
            }
        }

        public NoForm noForm { get; set; }

        public void Dirty(Rectangle rect)
        {
            dobs.Dirty(rect);
        }

        Stopwatch renderTime = new Stopwatch();
        public float currentFps { get; private set; }

        public event Action<Size> RenderSizeChanged;

        public void Dispose()
        {
        }

        OTKDraw _uDraw;
        public Renderers.IUnifiedDraw uDraw
        {
            get { return _uDraw; }
        }
        OpenTK_RenderElements _backRenderer;
        public Renderers.IRenderElements backRenderer
        {
            get { return _backRenderer; }
        }

        public Renderers.UnifiedEffects uAdvanced
        {
            get { throw new NotImplementedException(); }
        }
    }
}
