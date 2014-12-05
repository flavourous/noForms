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
        // store vbo for each encountered stride
        Dictionary<int, int> soft_vbo_by_stride = new Dictionary<int, int>();
        // against bufferinfo index, offset in vbo of data.
        Dictionary<int, int> softLocations = new Dictionary<int, int>(); 
        // against vbo, list of chunks to upload indexed in bufferinfo array
        Dictionary<int, List<int>> softUploads = new Dictionary<int, List<int>>(); 
        // against vbo, tracks the tip
        Dictionary<int, int> softUploadHeads = new Dictionary<int, int>(); 
        // against vbo, store current tip for rendering
        Dictionary<int, int> rendervboheads = new Dictionary<int, int>();
        void ProcessRenderBuffer()
        {
            var trlr = _backRenderer.renderData;
            softLocations.Clear();
            softUploadHeads.Clear();
            softUploads.Clear();
            rendervboheads.Clear();

            // First of all, we split into chunks of same strides
            for (int i = 0; i < trlr.bufferInfo.Count; i++)
            {
                var r  = trlr.bufferInfo[i];
                if(r.vbo != -1) continue; // these arent software buffered!

                // Get stride of this render chunk, and create a vbo against that stride if needed
                int s,c,v,t;
                CSTrix(r.dataFormat, out s, out c, out v, out t);
                int vbo = soft_vbo_by_stride.ContainsKey(s) ? soft_vbo_by_stride[s] : soft_vbo_by_stride[s] = GL.GenBuffer();

                // against this vbo we need to get the upload list and upload head
                var ul = softUploads.ContainsKey(vbo) ? softUploads[vbo] : softUploads[vbo] = new List<int>();
                var uh = softUploadHeads.ContainsKey(vbo) ? softUploadHeads[vbo] : softUploadHeads[vbo] = 0;

                // we can now add the upload and update the head
                softUploads[vbo].Add(i);
                softUploadHeads[vbo] += r.count;

                // using the previous head, we can create the softlocation for later
                softLocations[i] = uh;
            }

            // Push sw data to the device, per vbo as calculated
            foreach (var kv in softUploads)
            {
                // bind this vbo
                GL.BindBuffer(BufferTarget.ArrayBuffer, kv.Key);
                int bufLen = softUploadHeads[kv.Key]; // we've remembered the amount of data thats going in
                float[] upload = new float[bufLen];
                int cst = 0, clen = 0, ust=0;
                for (int i = 0; i < kv.Value.Count; i++)
                {
                    RenderInfo r = trlr.bufferInfo[kv.Value[i]];
                    if (clen == 0) cst = r.offset; // starting a new block.
                    if (cst + clen != r.offset)
                    {
                        // We need to flush the buffer
                        trlr.sofwareBuffer.CopyTo(cst, upload, ust, clen);
                        ust += clen;
                        cst = r.offset; clen = r.count;
                    }
                    else clen += r.count; // extend the buffer to copy
                }
                if (clen > 0) trlr.sofwareBuffer.CopyTo(cst, upload, ust, clen); // final flush

                GL.BufferData(
                    BufferTarget.ArrayBuffer,
                    (IntPtr)(upload.Length * sizeof(float)),
                    upload, 
                    BufferUsageHint.StreamDraw
                    );
            }

            // then we render evrythin (which might get asynced by the GL server?)
            ArrayData lastPointaz = 0;// nothing
            PrimitiveType lastPrimitive = 0; // doesnt matter 
            int rlen = 0; int lastvbo = -1; int laststride = 0;
            for (int i = 0; i < trlr.bufferInfo.Count; i++)
            {
                // Get info
                var r = trlr.bufferInfo[i];
                int stride = PointazDiffa(lastPointaz, r.dataFormat);
                int vbo = r.vbo == -1 ? soft_vbo_by_stride[stride] : r.vbo;
                if (r.vbo != -1) rendervboheads[r.vbo] = 0;

                // Have we hit flush condition? (differnt primitive, or arraydata to last time...etc...uuugh)
                if (rendervboheads.ContainsKey(vbo) && (lastPointaz != r.dataFormat || lastPrimitive != r.renderAs || lastvbo !=  vbo))
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, lastvbo);
                    GL.DrawArrays(lastPrimitive, rendervboheads[lastvbo] / laststride, rlen / laststride); // drawy
                    rendervboheads[lastvbo] = rlen;
                    rlen = 0;
                }
                if (!rendervboheads.ContainsKey(vbo)) rendervboheads[vbo] = 0;
                rlen += r.count;

                lastPointaz = r.dataFormat; lastPrimitive = r.renderAs; lastvbo = vbo; laststride = stride;
            }
            if (lastvbo > -1) // render last bufferchunky
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, lastvbo);
                GL.DrawArrays(lastPrimitive, rendervboheads[lastvbo] / laststride, rlen / laststride); // drawy
            }
         
            // clean up
            PointazDiffa(lastPointaz, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            trlr.Clear();
        }

        // TODO use glinterleavedarrays possibly
        int PointazDiffa(ArrayData last, ArrayData now)
        {
            var sf = sizeof(float);

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
                GL.VertexPointer(2, VertexPointerType.Float, stride*sf, ver*sf);
            }
            if (col > -1 && lcol == -1)
            {
                GL.EnableClientState(ArrayCap.ColorArray);
                GL.ColorPointer(4, ColorPointerType.Float, stride*sf, col*sf);
            }
            if (tex > -1 && ltex == -1)
            {
                GL.EnableClientState(ArrayCap.TextureCoordArray);
                GL.TexCoordPointer(2, TexCoordPointerType.Float, stride*sf, tex*sf);
            }

            // Turn em off if dey aint on no moar
            if (ver == -1 && lver > -1) GL.DisableClientState(ArrayCap.VertexArray);
            if (col == -1 && lcol > -1) GL.DisableClientState(ArrayCap.ColorArray);
            if (tex == -1 && ltex > -1) GL.DisableClientState(ArrayCap.TextureCoordArray);

            return stride;
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

        // TODO use glinterleavedarrays possibly
        void CSTrix(ArrayData flags, out int stride, out int ver, out int col, out int tex)
        {
            int idx = 0;
            stride = 0;
            ver = col = tex = -1;
            if ((flags & ArrayData.Vertex) != 0) { ver = idx += stride; stride += 2; }
            if ((flags & ArrayData.Color) != 0) { col = idx += stride; stride += 4; }
            if ((flags & ArrayData.Texture) != 0) { tex = idx += stride; stride += 2; }
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
