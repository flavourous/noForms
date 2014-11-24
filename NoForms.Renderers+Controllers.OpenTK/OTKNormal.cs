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

        public float FPSLimit { get; set; }
        public OTKNormal()
        {
            FPSLimit = 60;
        }

        public void Init(IW32Win initObj, NoForm nf)
        {
            // do the form
            noForm = nf;
            noForm.renderer = this;
            this.w32 = initObj;

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
            dobs.StartObserving(() =>
            {
                // get the glinfo
                winfo = Utilities.CreateWindowsWindowInfo(w32.handle);

                // get context running...
                glContext = new GraphicsContext(GraphicsMode.Default, winfo);
                (glContext as IGraphicsContextInternal).LoadAll(); // makes current i think. or constructor does.
                glContext.SwapInterval = 0; // vsync off, manage self.

                // generate buffers
                int FBO_Draw = GL.Ext.GenFramebuffer();
                int FBO_Window = GL.Ext.GenFramebuffer();
                int T2D_Draw = GL.GenTexture();
                int T2D_Window = GL.GenTexture();

                // saveem
                _backRenderer = new OpenTK_RenderElements(glContext, FBO_Draw, T2D_Draw, FBO_Window, T2D_Window);
                _uDraw = new OTKDraw(_backRenderer);
                vboS = GL.GenBuffer();
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

            foreach (var tex in new int[] { _backRenderer.T2D_Draw, _backRenderer.T2D_Window })
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
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, _backRenderer.FBO_Draw);
            GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, _backRenderer.T2D_Draw, 0);

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
            noForm.DrawBase(this, dc);
            ProcessRenderBuffer();

            // Intermediate render to window buffer...
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, _backRenderer.FBO_Window);
            GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, _backRenderer.T2D_Window, 0);

            // Alpha...
            GL.Disable(EnableCap.Blend); // we just want to emplace.

            // set up ortho
            GL.Viewport(0, 0, (int)w, (int)h);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0.0, w, 0.0, h, 0.0, 1.0);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            // Texture mapping...
            GL.Enable(EnableCap.Texture2D);

            // Draw the Color Texture - only parts of it!!
            GL.BindTexture(TextureTarget.Texture2D, _backRenderer.T2D_Draw);
            GL.Color4(1f, 1f, 1f, 1f);
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

            GL.BindTexture(TextureTarget.Texture2D, _backRenderer.T2D_Window);
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

            lastFrameRenderDuration = 1f / (float)renderTime.Elapsed.TotalSeconds;
            renderTime.Reset();
        }

        int vboS;
        void ProcessRenderBuffer()
        {
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(20, 20, 100, 100);
            var trlr = _backRenderer.renderData;

            // Push sw data to the device
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboS);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                (IntPtr)(trlr.sofwareBuffer.Count * sizeof(float)),
                trlr.sofwareBuffer.ToArray(), // FIXME one day I will avoid this copy in safe mode :/ y no expose as the internal array?? :/
                BufferUsageHint.StreamDraw
                );

            // then we render evrythin (which might get asynced by the GL server?)
            ArrayData lastPointaz = 0; // nothin
            int lastvbo = -1, lasttex = -1; //an impossible one to start
            for (int i = 0; i < trlr.bufferInfo.Count; i++)
            {
                var r = trlr.bufferInfo[i];
                if(lastvbo != r.vbo)
                    GL.BindBuffer(BufferTarget.ArrayBuffer, r.vbo == -1 ? vboS : r.vbo);
                PointazDiffa(lastPointaz, r.dataFormat);
                lastPointaz = r.dataFormat;
                //if(lasttex!=r.texture)
                //    GL.BindTexture(TextureTarget.Texture2D, r.texture);
                GL.DrawArrays(r.renderAs, r.offset, r.count * sizeof(float)); // drawy
                lastvbo = r.vbo; lasttex = r.texture;
            }
         
            // clean up
            PointazDiffa(lastPointaz, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            //GL.BindTexture(TextureTarget.Texture2D, 0);
            trlr.Clear();
        }

        void PointazDiffa(ArrayData last, ArrayData now)
        {
            // Last pointaz
            int lver, lcol, ltex, lstride;
            CSTrix(last, out lstride, out lver, out lcol, out ltex);

            // Want dese pointaz bozz
            int ver, col, tex, stride;
            CSTrix(now, out stride, out ver, out col, out tex);

            // Enabel dem if day waznt ooon befar
            if (ver > -1 && lver == -1)
            {
                GL.EnableClientState(ArrayCap.VertexArray);
                GL.VertexPointer(2, VertexPointerType.Float, stride, ver);
            }
            if (col > -1 && lcol == -1)
            {
                GL.EnableClientState(ArrayCap.ColorArray);
                GL.ColorPointer(4, ColorPointerType.Float, stride, col);
            }
            if (tex > -1 && ltex == -1)
            {
                GL.EnableClientState(ArrayCap.TextureCoordArray);
                GL.TexCoordPointer(2, TexCoordPointerType.Float, stride, tex);
            }

            // Turn em off if dey aint on no moar
            if (ver == -1 && lver > -1) GL.DisableClientState(ArrayCap.VertexArray);
            if (col == -1 && lcol > -1) GL.DisableClientState(ArrayCap.ColorArray);
            if (tex == -1 && ltex > -1) GL.DisableClientState(ArrayCap.TextureCoordArray);
        }

        //void Pointaz(ArrayData ad, bool ena)
        //{
        //    int ver, col, tex, stride;
        //    CSTrix(ad, out stride, out ver, out col, out tex);
        //    if (ena)
        //    {
        //        // Point interleaved data
        //        if (ver > -1)
        //        {
        //            GL.EnableClientState(ArrayCap.VertexArray);
        //            GL.VertexPointer(2, VertexPointerType.Float, stride, ver);
        //        }
        //        if (col > -1)
        //        {
        //            GL.EnableClientState(ArrayCap.ColorArray);
        //            GL.ColorPointer(4, ColorPointerType.Float, stride, col);
        //        }
        //        if (tex > -1)
        //        {
        //            GL.EnableClientState(ArrayCap.TextureCoordArray);
        //            GL.TexCoordPointer(2, TexCoordPointerType.Float, stride, tex);
        //        }
        //    }
        //    else
        //    {
        //        if (ver > -1) GL.DisableClientState(ArrayCap.VertexArray);
        //        if (col > -1) GL.DisableClientState(ArrayCap.ColorArray);
        //        if (tex > -1) GL.DisableClientState(ArrayCap.TextureCoordArray);
        //    }
        //}

        void CSTrix(ArrayData flags, out int stride, out int ver, out int col, out int tex)
        {
            int idx = 0;
            stride = 0;
            ver = col = tex = -1;
            if ((flags & ArrayData.Vertex) != 0) { ver = idx += stride; stride += sizeof(float) * 2; }
            if ((flags & ArrayData.Color) != 0) { col = idx += stride; stride += sizeof(float) * 4; }
            if ((flags & ArrayData.Texture) != 0) { tex = idx += stride; stride += sizeof(float) * 2; }
        }

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
        public float lastFrameRenderDuration { get; private set; }

        public event Action<Size> RenderSizeChanged;

        public void Dispose()
        {
            GL.DeleteBuffer(vboS);
            GL.DeleteFramebuffer(_backRenderer.FBO_Draw);
            GL.DeleteFramebuffer(_backRenderer.FBO_Window);
            GL.DeleteTexture(_backRenderer.T2D_Draw);
            GL.DeleteTexture(_backRenderer.T2D_Window);
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
