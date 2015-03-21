using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;
using NoForms.Renderers.OpenTK;

namespace NoForms.Test.OTK
{
    class MockGLBuffer : IGLBuffer
    {
        public void ResetBuffer()
        {
            buffers = new Dictionary<int, float[]> { { 0, new float[0] } };
            renderlist.Clear();
            enables.Clear();
            pointers.Clear();
        }

        Dictionary<int, float[]> buffers = new Dictionary<int, float[]> { { 0, new float[0] } };
        public int GenBuffer()
        {
            int i = 1;
            while (buffers.ContainsKey(i)) i++;
            buffers[i] = new float[0];
            return i;
        }
        public void DeleteBuffer(int buf)
        {
            buffers.Remove(buf);
        }

        int activeBuffer = 0;
        public void BufferData(OpenTK.Graphics.OpenGL.BufferTarget targ, IntPtr length, float[] data, OpenTK.Graphics.OpenGL.BufferUsageHint hint)
        {
            if (!buffers.ContainsKey(activeBuffer)) throw new ArgumentException();
            buffers[activeBuffer] = data;
        }

        public void BindBuffer(OpenTK.Graphics.OpenGL.BufferTarget targ, int buf)
        {
            if (!buffers.ContainsKey(activeBuffer)) throw new ArgumentException();
            activeBuffer = buf;
        }

        public Dictionary<PrimitiveType, int> primsizes = new Dictionary<PrimitiveType, int> 
        { 
            { PrimitiveType.Quads, 4 }, 
            { PrimitiveType.Lines, 2 } 
        };

        public class ver { public float x, y;}
        public class col { public float r, g, b, a;}
        public class tex { public float u, v;}
        public class rend
        {
            public ver v; public col c; public tex t;
            public override string ToString()
            {
                return
                    (v == null ? "" : ("v:" + v.x + "," + v.y + " ")) +
                    (c == null ? "" : ("c:" + c.a + "," + c.r + "," + c.g + "," + c.b + " ")) +
                    (t == null ? "" : ("t:" + t.u + "," + t.v + " "));
            }
            public float[] ToArray()
            {
                int nd = (v==null ? 0 : 2)+(c==null ? 0 : 4)+(t==null ? 0 : 2);
                float[] ret = new float[nd];
                int rc = 0;
                if (v != null)
                {
                    ret[rc++] = v.x;
                    ret[rc++] = v.y;
                }
                if (c != null)
                {
                    ret[rc++] = c.a;
                    ret[rc++] = c.r;
                    ret[rc++] = c.g;
                    ret[rc++] = c.b;
                }
                if (t != null)
                {
                    ret[rc++] = t.u;
                    ret[rc++] = t.v;
                }
                return ret;
            }
        }
        public class pol { public rend[] data; public PrimitiveType pt;}
        public List<pol> renderlist = new List<pol>();
        public void DrawArrays(OpenTK.Graphics.OpenGL.PrimitiveType pt, int st, int len)
        {
            int sf = sizeof(float);
            float[] ab = buffers[activeBuffer];

            bool ev = enables.ContainsKey(ArrayCap.VertexArray);
            bool ec = enables.ContainsKey(ArrayCap.ColorArray);
            bool et = enables.ContainsKey(ArrayCap.TextureCoordArray);
            int vi = ev ? pointers[ArrayCap.VertexArray].offset / sf : 0;
            int ci = ec ? pointers[ArrayCap.ColorArray].offset / sf : 0;
            int ti = et ? pointers[ArrayCap.TextureCoordArray].offset / sf : 0;
            int vs = ev ? pointers[ArrayCap.VertexArray].stride / sf : 0;
            int cs = ec ? pointers[ArrayCap.ColorArray].stride / sf : 0;
            int ts = et ? pointers[ArrayCap.TextureCoordArray].stride / sf : 0;
            vi += vs * st; ci += cs * st; ti += ts * st;
            List<rend> data = new List<rend>();
            for (int i = 0; i < len; i++) // index only for counting loop.
            {
                ver v = null; col c = null; tex t = null;
                if (ev)
                {
                    v = new ver() { x = ab[vi], y = ab[vi + 1] };
                    vi += vs;
                }
                if (ec)
                {
                    c = new col() { a = ab[ci], r = ab[ci + 1], g = ab[ci + 2], b = ab[ci + 3] };
                    ci += cs;
                }
                if (et)
                {
                    t = new tex() { u = ab[ti], v = ab[ti + 1] };
                    ti += ts;
                }
                data.Add(new rend() { v = v, c = c, t = t });
            }
            renderlist.Add(new pol() { pt = pt, data = data.ToArray() });
        }

        Dictionary<ArrayCap, Object> enables = new Dictionary<ArrayCap, Object>();
        public void ArrayEnabled(OpenTK.Graphics.OpenGL.ArrayCap type, bool enabled)
        {
            if (!enabled) enables.Remove(type);
            else if (enables.ContainsKey(type)) throw new ArgumentException();
            else enables.Add(type, new object());
        }

        public struct vpoint { public int size, offset, stride; }
        Dictionary<ArrayCap, vpoint> pointers = new Dictionary<ArrayCap, vpoint>();
        public void SetPointer(OpenTK.Graphics.OpenGL.ArrayCap type, int nel, int stride, int offset)
        {
            pointers[type] = new vpoint() { size = nel, stride = stride, offset = offset };
        }

        Dictionary<TextureTarget, List<int>> texturesbound = new Dictionary<TextureTarget, List<int>>();
        public void BindTexture(TextureTarget tt, int tex)
        {
            texturesbound[tt].Add(tex);
            GL.BindTexture(tt, tex);
        }
    }
}
