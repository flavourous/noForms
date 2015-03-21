using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using OpenTK.Graphics.OpenGL;
using NoForms.Renderers.OpenTK;

namespace NoForms.Test.OTK
{
    [TestFixture]
    public class RenderProcessorDirectTests_MockedGL
    {
        RenderData rd;
        MockGLBuffer buf;
        RenderProcessor rp;
        [SetUp]
        public void SetUp()
        {
            rd = new Renderers.OpenTK.RenderData();

            buf = new MockGLBuffer();
            rp = new RenderProcessor(buf);
        }

        [Test] public void RenderQuadsV() { IncrementalIndexTest(PrimitiveType.Quads, true, false); }
        [Test] public void RenderQuadsVC() { IncrementalIndexTest(PrimitiveType.Quads, true, true); }
        [Test] public void RenderLinesV() { IncrementalIndexTest(PrimitiveType.Lines, true, false); }
        [Test] public void RenderLinesVC() { IncrementalIndexTest(PrimitiveType.Lines, true, true); }
        void IncrementalIndexTest(PrimitiveType pt,bool ver, bool col)
        {
            // test one, just put vertex data in only, same stride, same lenghts.  Check the correct data ends up back in.
            int nd = 5 * buf.primsizes[pt] * (2 + 4); // 5 quads
            for (int i = 0; i < 3; i++) // 3 times
            {
                int st = i * nd;
                ArrayData ad = 0 | (ver ? ArrayData.Vertex : 0) | (col ? ArrayData.Color : 0);
                var r = new RenderInfo(st, nd, ad, pt);
                for (int j = st; j < st + nd; j++)
                    rd.sofwareBuffer.Add(j);
                rd.bufferInfo.Add(r);
            }
            rp.ProcessRenderBuffer(rd);

            // check we haaave got ...erm...vertexes
            int chk = 0;
            foreach (var p in buf.renderlist)
            {
                Assert.AreEqual(p.pt, pt);
                foreach (var d in p.data)
                {
                    Assert.IsNull(d.t);
                    if (ver)
                    {
                        Assert.AreEqual(d.v.x, chk++);
                        Assert.AreEqual(d.v.y, chk++);
                    }
                    else Assert.IsNull(d.v);
                    if (col)
                    {
                        Assert.AreEqual(d.c.a, chk++);
                        Assert.AreEqual(d.c.r, chk++);
                        Assert.AreEqual(d.c.g, chk++);
                        Assert.AreEqual(d.c.b, chk++);
                    }
                    else Assert.IsNull(d.c);
                }
            }
            buf.ResetBuffer();
        }

        [Test]
        public void RenderQuadsAndLinesV()
        {
            List<float[]> dt = new List<float[]> { new float[0], new float[0], new float[0], new float[0] };
            List<PrimitiveType> dtp = new List<PrimitiveType> { PrimitiveType.Quads, PrimitiveType.Lines, PrimitiveType.Quads, PrimitiveType.Lines };

            // few quads
            rd.bufferInfo.Add(new RenderInfo(0, 4 * 3 * 2, ArrayData.Vertex, PrimitiveType.Quads));
            rd.sofwareBuffer.AddRange(dt[0] = new float[] 
            {
                1,5, 4,5, 4,8, 1,8,     
                4,5, 7,12, 13,2, 14,19, 
                1,2, 3,4, 1,10, 10,3,   
            });

            // few lines
            rd.bufferInfo.Add(new RenderInfo(rd.sofwareBuffer.Count, 3 * 2 * 2, ArrayData.Vertex, PrimitiveType.Lines));
            rd.sofwareBuffer.AddRange(dt[1] = new float[]
            {
                1,2,  5,6,  
                9,2,  4,7,  
                10,2,  10,2
            });

            // few more quads
            rd.bufferInfo.Add(new RenderInfo(rd.sofwareBuffer.Count, 4 * 3 * 2, ArrayData.Vertex, PrimitiveType.Quads));
            rd.sofwareBuffer.AddRange(dt[2] = new float[]
            {
                6,7, 100,405, 47,124, 123,32,
                100,200, 300,400, 102,506, 1023,9921,
                1,2, 3,4, 6,7, 7,2
            });

            // few more lines
            rd.bufferInfo.Add(new RenderInfo(rd.sofwareBuffer.Count, 3 * 2 * 2, ArrayData.Vertex, PrimitiveType.Lines));
            rd.sofwareBuffer.AddRange(dt[3] = new float[] 
            {
                8,9,  5,6,  
                10,0,  50,12,  
                102,667,  9, 201
            });

            // process buffer!
            rp.ProcessRenderBuffer(rd);

            // assertion timeees
            for (int i = 0; i < 4; i++)
            {
                var p = buf.renderlist[i];
                var pd = dt[i];
                Assert.AreEqual(p.pt, dtp[i]);
                    int pdi = 0;
                    for (int j = 0; j < p.data.Length; j++)
                        foreach (float fl in p.data[j].ToArray())
                            Assert.AreEqual(fl, pd[pdi++]);
            }

            // reset
            buf.ResetBuffer();
        }

        [Test]
        public void RenderQuadsAndLinesVC()
        {
            List<float[]> dt = new List<float[]> { new float[0], new float[0], new float[0], new float[0] };
            List<PrimitiveType> dtp = new List<PrimitiveType> { PrimitiveType.Quads, PrimitiveType.Lines, PrimitiveType.Quads, PrimitiveType.Lines };

            // few quads
            rd.bufferInfo.Add(new RenderInfo(0, 4 * 3 * 6, ArrayData.Vertex | ArrayData.Color, PrimitiveType.Quads));
            rd.sofwareBuffer.AddRange(dt[0] = new float[] 
            {
                93, 57,  0.1f, 0.8f, 0.3f, 0.4f,     21, 36,  0.5f, 0.4f, 0.6f, 0.7f,     95, 54,  0.4f, 0.7f, 0.4f, 0.5f,     70, 35,  0.1f, 0.2f, 0.1f, 0.9f,     
                64, 93,  0.6f, 0.3f, 0.6f, 0.8f,     85, 38,  0.5f, 0.7f, 0.4f, 0.2f,     23, 67,  0.4f, 0.6f, 0.8f, 0.4f,     18, 28,  0.5f, 0.6f, 0.4f, 0.5f,     
                76, 68,  0.5f, 0.5f, 0.1f, 0.7f,     57, 73,  0.4f, 0.3f, 0.1f, 0.3f,     81, 87,  0.7f, 0.8f, 0.5f, 0.2f,     22, 27,  0.1f, 0.1f, 0.5f, 0.7f,     
            });

            // few lines
            rd.bufferInfo.Add(new RenderInfo(rd.sofwareBuffer.Count, 3 * 2 * 6, ArrayData.Vertex | ArrayData.Color, PrimitiveType.Lines));
            rd.sofwareBuffer.AddRange(dt[1] = new float[]
            {
                17, 55,  0.5f, 1.0f, 0.3f, 0.1f,     42, 10,  0.8f, 0.4f, 0.4f, 0.4f,     
                89, 06,  0.3f, 0.6f, 0.9f, 0.0f,     11, 19,  0.6f, 0.8f, 0.4f, 0.7f,     
                26, 05,  0.5f, 0.1f, 0.1f, 0.4f,     33, 06,  0.7f, 0.8f, 0.8f, 0.2f,     
            });

            // few more quads
            rd.bufferInfo.Add(new RenderInfo(rd.sofwareBuffer.Count, 4 * 3 * 6, ArrayData.Vertex | ArrayData.Color, PrimitiveType.Quads));
            rd.sofwareBuffer.AddRange(dt[2] = new float[]
            {
                81, 52,  0.3f, 1.0f, 0.9f, 0.6f,     98, 40,  0.5f, 0.7f, 0.2f, 0.2f,     74, 19,  0.2f, 0.7f, 0.4f, 0.5f,     17, 81,  0.8f, 0.0f, 0.4f, 0.9f,     
                97, 65,  0.4f, 0.3f, 0.2f, 0.2f,     71, 39,  1.0f, 0.4f, 0.5f, 0.2f,     15, 19,  0.6f, 0.7f, 0.5f, 0.4f,     17, 17,  0.9f, 0.2f, 0.9f, 0.5f,     
                28, 97,  0.8f, 0.3f, 0.6f, 0.3f,     18, 79,  0.2f, 0.4f, 1.0f, 0.2f,     15, 67,  0.2f, 0.2f, 1.0f, 0.8f,     21, 36,  0.7f, 1.0f, 0.6f, 0.2f,     
            });

            // few more lines
            rd.bufferInfo.Add(new RenderInfo(rd.sofwareBuffer.Count, 3 * 2 * 6, ArrayData.Vertex | ArrayData.Color, PrimitiveType.Lines));
            rd.sofwareBuffer.AddRange(dt[3] = new float[] 
            {
                73, 21,  0.7f, 0.7f, 0.3f, 0.8f,     95, 25,  0.9f, 0.1f, 0.8f, 0.8f,     
                23, 52,  0.3f, 0.8f, 0.4f, 0.4f,     95, 47,  0.4f, 0.7f, 0.2f, 0.4f,     
                89, 81,  0.7f, 0.5f, 0.7f, 0.5f,     14, 48,  0.1f, 0.3f, 0.6f, 0.6f,     
            });

            // process buffer!
            rp.ProcessRenderBuffer(rd);

            // assertion timeees
            for (int i = 0; i < 4; i++)
            {
                var p = buf.renderlist[i];
                var pd = dt[i];
                Assert.AreEqual(p.pt, dtp[i]);
                int pdi = 0;
                for (int j = 0; j < p.data.Length; j++)
                    foreach (float fl in p.data[j].ToArray())
                        Assert.AreEqual(fl, pd[pdi++]);
            }

            // reset
            buf.ResetBuffer();
        }

        [Test]
        public void RenderQuads_VCMixed()
        {
            List<float[]> dt = new List<float[]> { new float[0], new float[0], new float[0], new float[0] };
            List<PrimitiveType> dtp = new List<PrimitiveType> { PrimitiveType.Quads, PrimitiveType.Quads, PrimitiveType.Quads, PrimitiveType.Quads};

            // few quads
            rd.bufferInfo.Add(new RenderInfo(0, 4 * 3 * 6, ArrayData.Vertex | ArrayData.Color, PrimitiveType.Quads));
            rd.sofwareBuffer.AddRange(dt[0] = new float[] 
            {
                93, 57,  0.1f, 0.8f, 0.3f, 0.4f,     21, 36,  0.5f, 0.4f, 0.6f, 0.7f,     95, 54,  0.4f, 0.7f, 0.4f, 0.5f,     70, 35,  0.1f, 0.2f, 0.1f, 0.9f,     
                64, 93,  0.6f, 0.3f, 0.6f, 0.8f,     85, 38,  0.5f, 0.7f, 0.4f, 0.2f,     23, 67,  0.4f, 0.6f, 0.8f, 0.4f,     18, 28,  0.5f, 0.6f, 0.4f, 0.5f,     
                76, 68,  0.5f, 0.5f, 0.1f, 0.7f,     57, 73,  0.4f, 0.3f, 0.1f, 0.3f,     81, 87,  0.7f, 0.8f, 0.5f, 0.2f,     22, 27,  0.1f, 0.1f, 0.5f, 0.7f,     
            });

            // few quads
            rd.bufferInfo.Add(new RenderInfo(rd.sofwareBuffer.Count, 4 * 3 * 2, ArrayData.Vertex, PrimitiveType.Quads));
            rd.sofwareBuffer.AddRange(dt[1] = new float[] 
            {
                64, 20,      55, 54,      43, 07,      79, 43,      
                54, 72,      71, 30,      28, 41,      00, 02,      
                98, 86,      46, 95,      23, 64,      81, 97,      
            });


            // few more quads
            rd.bufferInfo.Add(new RenderInfo(rd.sofwareBuffer.Count, 4 * 3 * 6, ArrayData.Vertex | ArrayData.Color, PrimitiveType.Quads));
            rd.sofwareBuffer.AddRange(dt[2] = new float[]
            {
                81, 52,  0.3f, 1.0f, 0.9f, 0.6f,     98, 40,  0.5f, 0.7f, 0.2f, 0.2f,     74, 19,  0.2f, 0.7f, 0.4f, 0.5f,     17, 81,  0.8f, 0.0f, 0.4f, 0.9f,     
                97, 65,  0.4f, 0.3f, 0.2f, 0.2f,     71, 39,  1.0f, 0.4f, 0.5f, 0.2f,     15, 19,  0.6f, 0.7f, 0.5f, 0.4f,     17, 17,  0.9f, 0.2f, 0.9f, 0.5f,     
                28, 97,  0.8f, 0.3f, 0.6f, 0.3f,     18, 79,  0.2f, 0.4f, 1.0f, 0.2f,     15, 67,  0.2f, 0.2f, 1.0f, 0.8f,     21, 36,  0.7f, 1.0f, 0.6f, 0.2f,     
            });

            // few quads more
            rd.bufferInfo.Add(new RenderInfo(rd.sofwareBuffer.Count, 4 * 3 * 2, ArrayData.Vertex, PrimitiveType.Quads));
            rd.sofwareBuffer.AddRange(dt[3] = new float[] 
            {
                77, 91,      20, 29,      45, 29,      49, 96,      
                91, 78,      49, 45,      76, 48,      27, 57,      
                87, 51,      22, 43,      93, 61,      51, 02,      
            });

            // process buffer!
            rp.ProcessRenderBuffer(rd);

            // assertion timeees
            for (int i = 0; i < 4; i++)
            {
                var p = buf.renderlist[i];
                var pd = dt[i];
                Assert.AreEqual(p.pt, dtp[i]);
                int pdi = 0;
                for (int j = 0; j < p.data.Length; j++)
                {
                    foreach (float fl in p.data[j].ToArray())
                        Assert.AreEqual(fl, pd[pdi++]);
                }
            }

            // reset
            buf.ResetBuffer();
        }

        [Test]
        public void RenderQuadsAndLines_VCMixed()
        {
            List<float[]> dt = new List<float[]> { new float[0], new float[0], new float[0], new float[0], new float[0], new float[0] };
            List<PrimitiveType> dtp = new List<PrimitiveType> { PrimitiveType.Quads, PrimitiveType.Lines, PrimitiveType.Quads, PrimitiveType.Lines, PrimitiveType.Quads, PrimitiveType.Lines };

            // Quads V
            rd.bufferInfo.Add(new RenderInfo(0, 4 * 3 * 2, ArrayData.Vertex, PrimitiveType.Quads));
            rd.sofwareBuffer.AddRange(dt[0] = new float[] 
            {
                54, 08,      69, 16,      26, 47,      42, 64,      
                35, 72,      51, 24,      61, 61,      69, 18,      
                43, 44,      31, 41,      99, 44,      76, 67,      
            });

            // Lines VC
            rd.bufferInfo.Add(new RenderInfo(rd.sofwareBuffer.Count, 2 * 3 * 6, ArrayData.Vertex | ArrayData.Color, PrimitiveType.Lines));
            rd.sofwareBuffer.AddRange(dt[1] = new float[] 
            {
                25, 78,  0.3f, 0.4f, 0.4f, 0.9f,     67, 86,  0.9f, 0.2f, 0.1f, 0.7f,     
                81, 46,  0.1f, 0.5f, 0.9f, 0.9f,     70, 81,  0.4f, 0.1f, 0.7f, 0.4f,     
                41, 86,  0.2f, 0.4f, 0.2f, 0.1f,     44, 59,  1.0f, 0.1f, 0.9f, 0.6f,     
            });


            // Quads VC
            rd.bufferInfo.Add(new RenderInfo(rd.sofwareBuffer.Count, 4 * 3 * 6, ArrayData.Vertex | ArrayData.Color, PrimitiveType.Quads));
            rd.sofwareBuffer.AddRange(dt[2] = new float[]
            {
                94, 12,  0.8f, 0.9f, 1.0f, 0.3f,     44, 20,  0.9f, 0.0f, 0.2f, 0.7f,     56, 42,  0.6f, 0.1f, 0.1f, 0.6f,     51, 27,  0.9f, 0.8f, 0.2f, 0.6f,     
                89, 73,  0.0f, 0.7f, 0.1f, 0.4f,     32, 62,  0.8f, 0.5f, 0.1f, 0.8f,     55, 54,  0.5f, 0.2f, 0.2f, 0.6f,     63, 25,  0.1f, 0.2f, 0.7f, 0.5f,     
                40, 10,  0.1f, 0.7f, 0.1f, 0.0f,     94, 13,  0.9f, 0.2f, 0.0f, 0.3f,     24, 74,  0.1f, 0.5f, 0.7f, 0.6f,     88, 01,  0.4f, 0.8f, 0.6f, 0.6f,     
            });

            // Lines V
            rd.bufferInfo.Add(new RenderInfo(rd.sofwareBuffer.Count, 2 * 3 * 2, ArrayData.Vertex, PrimitiveType.Lines));
            rd.sofwareBuffer.AddRange(dt[3] = new float[] 
            {
                02, 48,      48, 17,      
                28, 67,      28, 33,      
                65, 27,      58, 99,      
            });
            // Quads VC
            rd.bufferInfo.Add(new RenderInfo(rd.sofwareBuffer.Count, 4 * 3 * 6, ArrayData.Vertex | ArrayData.Color, PrimitiveType.Quads));
            rd.sofwareBuffer.AddRange(dt[4] = new float[]
            {
                54, 56,  0.1f, 0.5f, 0.4f, 0.6f,     19, 64,  0.0f, 0.1f, 0.9f, 0.6f,     37, 91,  0.9f, 1.0f, 0.9f, 0.6f,     28, 34,  0.8f, 0.0f, 0.8f, 0.6f,     
                06, 67,  0.5f, 0.5f, 0.9f, 0.5f,     77, 65,  0.9f, 0.7f, 0.7f, 0.2f,     93, 54,  0.5f, 0.6f, 0.4f, 1.0f,     66, 52,  0.7f, 0.6f, 0.5f, 0.7f,     
                97, 97,  0.6f, 0.5f, 0.9f, 0.2f,     94, 51,  0.8f, 0.5f, 0.5f, 0.7f,     08, 72,  0.7f, 0.5f, 0.3f, 0.2f,     70, 66,  0.2f, 0.7f, 0.0f, 0.4f,     
            });

            // Lines VC
            rd.bufferInfo.Add(new RenderInfo(rd.sofwareBuffer.Count, 2 * 3 * 6, ArrayData.Vertex, PrimitiveType.Lines));
            rd.sofwareBuffer.AddRange(dt[5] = new float[] 
            {
                14, 56,  1.0f, 0.0f, 0.5f, 0.3f,     45, 25,  0.2f, 0.4f, 0.4f, 0.0f,     
                68, 96,  0.8f, 0.7f, 0.7f, 0.9f,     06, 82,  0.8f, 0.6f, 0.8f, 0.6f,     
                85, 99,  0.8f, 0.1f, 0.4f, 0.9f,     07, 99,  0.3f, 0.1f, 0.0f, 0.7f,     
            });

            // process buffer!
            rp.ProcessRenderBuffer(rd);

            // assertion timeees
            for (int i = 0; i < 6; i++)
            {
                var p = buf.renderlist[i];
                var pd = dt[i];
                Assert.AreEqual(p.pt, dtp[i]);
                int pdi = 0;
                for (int j = 0; j < p.data.Length; j++)
                {
                    foreach (float fl in p.data[j].ToArray())
                        Assert.AreEqual(fl, pd[pdi++]);
                }
            }

            // reset
            buf.ResetBuffer();
        }

        
    }

    

}
