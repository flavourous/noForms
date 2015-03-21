using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using NoForms.Renderers.OpenTK;
using NoForms.Common;

namespace NoForms.Test.OTK
{
    [TestFixture]
    class OTKDrawPath
    {
        RenderProcessor proc;
        MockGLBuffer buf = new MockGLBuffer();
        OTKDraw mockedDraw;
        RenderData rd;
        [SetUp]
        public void SetUp()
        {
            var rel = new OpenTK_RenderElements(null,-1,-1,-1,-1); // THis will crash if any GL is done
            mockedDraw = new OTKDraw(rel);
            rd = rel.renderData;
            proc = new RenderProcessor(buf);
        }

        int[] nlines { get { return new int[] { 1, 2, 5,12 }; } }
        [Test, TestCaseSource("nlines")]
        public void OneFigureNLines(int n)
        {
            // Draw path
            UPath path = new UPath();
            path.figures.Add(new UFigure(new Point(0, 0), false, true));
            List<float> dpts = new List<float>() { 0, 0, 1, 1, 1, 1 };
            for (int i = 0; i < n; i++)
            {
                float x = 3 * .2f * i + .5f * i * i;
                float y = 1 * .6f * i + .2f * i * i;
                path.figures[0].geoElements.Add(new ULine(new Point(x, y)));
                dpts.AddRange(new float[] { x, y, 1, 1, 1, 1 });
                if(i!=n-1) dpts.AddRange(new float[] { x, y, 1, 1, 1, 1 });
            }
            mockedDraw.DrawPath(path, new USolidBrush() { color = new Color(1, 1, 1, 1) }, new UStroke());

            // Process data (will need to when otkdraw puts in vbos...and crashfixy interface mocky...)
            proc.ProcessRenderBuffer(rd);

            // Check data
            Assert.AreEqual(1, buf.renderlist.Count); // should just be 1 line rendered in VC with 2 points
            Assert.AreEqual(buf.renderlist[0].pt, PrimitiveType.Lines);
            AssertEqual(
                buf.renderlist[0],
                dpts.ToArray()
            );

            buf.ResetBuffer();
        }

        [Test]
        public void PlotGeoElementStartpointisend()
        {
            Point sp = new Point(0,0);
            ULine l = new ULine(new Point(1,1));
            USolidBrush sb = new USolidBrush() { color = new Color( 1,1,1,1)};
            mockedDraw.PlotGeoElement(ref sp, l, sb, false);
            Assert.AreEqual(sp.X, 1);
            Assert.AreEqual(sp.Y, 1);
            buf.ResetBuffer();

            var a = new UArc(new Point(5, 4), new Size(3, 3), true, false, 12f);
            mockedDraw.PlotGeoElement(ref sp, a, sb, false);
            Assert.AreEqual(sp.X, 5);
            Assert.AreEqual(sp.Y, 4);
            buf.ResetBuffer();
        }

        void AssertEqual(MockGLBuffer.pol p, float[] exp)
        {
            int pdi = 0;
            for (int j = 0; j < p.data.Length; j++)
                foreach (float fl in p.data[j].ToArray())
                    Assert.AreEqual(fl, exp[pdi++]);
            Assert.AreEqual(pdi, exp.Length);
        }
    }
}
