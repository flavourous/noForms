using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;


namespace NoForms.Renderers.OpenTK
{
    public enum VertexType { Vertex, Color, Texcoord };
    public interface IGLBuffer
    {
        int GenBuffer();
        void DeleteBuffer(int buf);
        void BufferData(BufferTarget targ, IntPtr length, float[] data, BufferUsageHint hint);
        void BindBuffer(BufferTarget targ, int buf);
        void BindTexture(TextureTarget tt, int tex);
        void DrawArrays(PrimitiveType pt, int st, int len);
        void ArrayEnabled(ArrayCap type, bool enabled);
        void SetPointer(ArrayCap type, int nel, int stride, int offset);
    }
    public class RenderProcessor
    {
        IGLBuffer buffer;
        public RenderProcessor(IGLBuffer buf) { buffer = buf; }

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
        public void ProcessRenderBuffer(RenderData trlr)
        {
            softLocations.Clear();
            softUploadHeads.Clear();
            softUploads.Clear();
            rendervboheads.Clear();

            // First of all, we split into chunks of same strides
            for (int i = 0; i < trlr.bufferInfo.Count; i++)
            {
                var r = trlr.bufferInfo[i];
                if (r.vbo != -1) continue; // these arent software buffered!

                // Get stride of this render chunk, and create a vbo against that stride if needed
                int s, c, v, t;
                CSTrix(r.dataFormat, out s, out c, out v, out t);
                int vbo = soft_vbo_by_stride.ContainsKey(s) ? soft_vbo_by_stride[s] : soft_vbo_by_stride[s] = buffer.GenBuffer();

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
                buffer.BindBuffer(BufferTarget.ArrayBuffer, kv.Key);
                int bufLen = softUploadHeads[kv.Key]; // we've remembered the amount of data thats going in
                float[] upload = new float[bufLen];
                int cst = 0, clen = 0, ust = 0;
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
                buffer.BufferData(
                    BufferTarget.ArrayBuffer,
                    (IntPtr)(upload.Length * sizeof(float)),
                    upload,
                    BufferUsageHint.StreamDraw
                    );
            }

            // then we render evrythin (which might get asynced by the GL server?)
            ArrayData lastPointaz = 0;// nothing
            ArrayData lastUsedPointaz = 0;
            PrimitiveType lastPrimitive = 0; // doesnt matter 
            int lastTexta = 0;
            int rlen = 0; int lastvbo = -1; int laststride = 0;
            for (int i = 0; i < trlr.bufferInfo.Count; i++)
            {
                // Get info
                var r = trlr.bufferInfo[i];

                // Want dese pointaz bozz
                int ver, col, tex, stride;
                CSTrix(r.dataFormat, out stride, out ver, out col, out tex);

                // get this vbo, init render head
                int vbo = r.vbo == -1 ? soft_vbo_by_stride[stride] : r.vbo;
                if (!rendervboheads.ContainsKey(vbo)) rendervboheads[vbo] = 0;

                // Have we hit flush condition? for previous one (lookbehind) (differnt primitive, or arraydata to last time...etc...uuugh)
                if (i > 0 && (lastPointaz != r.dataFormat || lastPrimitive != r.renderAs || lastvbo != vbo || lastTexta != r.texture))
                {
                    buffer.BindBuffer(BufferTarget.ArrayBuffer, lastvbo);
                    PointazDiffa(lastUsedPointaz, lastPointaz);
                    lastUsedPointaz = lastPointaz;
                    buffer.BindTexture(TextureTarget.Texture2D, lastTexta);
                    buffer.DrawArrays(lastPrimitive, rendervboheads[lastvbo] / laststride, rlen / laststride); // drawy
                    rendervboheads[lastvbo] += rlen;
                    rlen = 0;
                }
                rlen += r.count;

                lastPointaz = r.dataFormat; lastPrimitive = r.renderAs; lastvbo = vbo; laststride = stride; lastTexta = r.texture;
            }
            if (lastvbo > -1) // render last bufferchunky
            {
                buffer.BindBuffer(BufferTarget.ArrayBuffer, lastvbo);
                PointazDiffa(lastUsedPointaz, lastPointaz);
                buffer.BindTexture(TextureTarget.Texture2D, lastTexta);
                buffer.DrawArrays(lastPrimitive, rendervboheads[lastvbo] / laststride, rlen / laststride); // drawy
            }
            buffer.BindTexture(TextureTarget.Texture2D, 0);

            // clean up
            PointazDiffa(lastPointaz, 0);
            buffer.BindBuffer(BufferTarget.ArrayBuffer, 0);
            trlr.Clear();
        }

        // TODO use glinterleavedarrays possibly
        void PointazDiffa(ArrayData last, ArrayData now)
        {
            var sf = sizeof(float);

            // Last pointaz
            int lver, lcol, ltex, lstride;
            CSTrix(last, out lstride, out lver, out lcol, out ltex);

            // Want dese pointaz bozz
            int ver, col, tex, stride;
            CSTrix(now, out stride, out ver, out col, out tex);

            // Enabel dem if day waznt ooon befar
            if (ver > -1 && lver == -1) buffer.ArrayEnabled(ArrayCap.VertexArray, true);
            if (col > -1 && lcol == -1) buffer.ArrayEnabled(ArrayCap.ColorArray, true);
            if (tex > -1 && ltex == -1) buffer.ArrayEnabled(ArrayCap.TextureCoordArray, true);

            // Configah sum pointerz if dey on (caz strdez couldah changes)
            if (ver > -1) buffer.SetPointer(ArrayCap.VertexArray, 2, stride * sf, ver * sf);
            if (col > -1) buffer.SetPointer(ArrayCap.ColorArray, 4, stride * sf, col * sf);
            if (tex > -1) buffer.SetPointer(ArrayCap.TextureCoordArray, 2, stride * sf, tex * sf);

            // Turn em off if dey aint on no moar
            if (ver == -1 && lver > -1) buffer.ArrayEnabled(ArrayCap.VertexArray, false);
            if (col == -1 && lcol > -1) buffer.ArrayEnabled(ArrayCap.ColorArray, false);
            if (tex == -1 && ltex > -1) buffer.ArrayEnabled(ArrayCap.TextureCoordArray, false);
        }

        // TODO use glinterleavedarrays possibly
        void CSTrix(ArrayData flags, out int stride, out int ver, out int col, out int tex)
        {
            stride = 0;
            ver = col = tex = -1;
            if ((flags & ArrayData.Vertex) != 0) {ver = stride; stride +=2;}
            if ((flags & ArrayData.Color) != 0) {col  = stride; stride+=4; }
            if ((flags & ArrayData.Texture) != 0) {tex = stride; stride+=2; }
        }
    }
}
