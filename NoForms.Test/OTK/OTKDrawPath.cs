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

        [Test]
        public void OneFigureOneLine()
        {
            // Draw path
            UPath path = new UPath();
            path.figures.Add(new UFigure(new Point(0, 0), false, true));
            path.figures[0].geoElements.Add(new ULine(new Point(1, 1)));
            mockedDraw.DrawPath(path, new USolidBrush() { color = new Color(1, 1, 1, 1) }, new UStroke());

            // Process data (will need to when otkdraw puts in vbos...and crashfixy interface mocky...)
            proc.ProcessRenderBuffer(rd);

            // Check data
            Assert.AreEqual(1, buf.renderlist.Count); // should just be 1 line rendered in VC with 2 points
            Assert.AreEqual(buf.renderlist[0].pt, PrimitiveType.Lines);
            Assert.AreEqual(buf.renderlist[0].data.Length, 2);
            float[] exp = new float[] { 0,0, 1,1,1,1,  1,1, 1,1,1,1 };
            var p = buf.renderlist[0];
            int pdi = 0;
            for (int j = 0; j < p.data.Length; j++)
                foreach (float fl in TUtil.lvr(p.data[j]))
                    Assert.AreEqual(fl, exp[pdi++]);
        }
    }
}
