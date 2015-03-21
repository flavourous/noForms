using System;
using System.Collections.Generic;
using System.Text;
using NUnit;
using NUnit.Framework;
using NoForms.Renderers;
using NoForms.Renderers.OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform;
using OpenTK.Graphics.OpenGL;
using NoForms.Common;
using OpenTK;
using NoFormsSDK;

namespace NoForms.Test.OTK
{
    [TestFixture]
    class TextRenderMockTests
    {
        GameWindow gw;
        IUnifiedDraw GetGlDrawObject()
        {
            // get the glinfo
            gw = new GameWindow(50, 50);
            gw.Context.LoadAll();

            // generate buffers
            int FBO_Draw = GL.Ext.GenFramebuffer();
            int FBO_Window = GL.Ext.GenFramebuffer();
            int T2D_Draw = GL.GenTexture();
            int T2D_Window = GL.GenTexture();

            // saveem
            var _backRenderer = new OpenTK_RenderElements(gw.Context, FBO_Draw, T2D_Draw, FBO_Window, T2D_Window);
            return new OTKDraw(_backRenderer);
        }
        IUnifiedDraw udraw;
        [SetUp]
        public void SetUp()
        {
            udraw = GetGlDrawObject();
        }
        

        [Test]
        public void DoesDummyGLContextWork()
        {
            udraw.DrawLine(new Common.Point(0, 1), new Common.Point(1, 1), new Common.USolidBrush(), new Common.UStroke());
        }

        [Test]
        public void DrawSimpleString()
        {
            UText ut = new UText("Hai", UHAlign.Left, UVAlign.Top, false, 50, 50);
            ut.font = new UFont("Arial", 12f, false, false);
            udraw.DrawText(ut, new Point(0, 0), new USolidBrush(), UTextDrawOptions.None, false);
        }

        [Test]
        public void TextFieldTest()
        {
            Textfield tf = new Textfield();
            tf.DisplayRectangle = new Rectangle(0, 0, 50, 50);
            Region rg = new Region();
            rg.Add(tf.DisplayRectangle);
            tf.DrawBase(new MockIDraw(udraw), rg);
        }

    }
    class MockIDraw : IDraw
    {
        IUnifiedDraw ud;
        public MockIDraw(IUnifiedDraw ud)
        {
            this.ud = ud;
        }
        public IUnifiedDraw uDraw {get{return ud;}}
        public IRenderElements backRenderer
        {
            get { throw new NotImplementedException(); }
        }
        public UnifiedEffects uAdvanced
        {
            get { throw new NotImplementedException(); }
        }
    }
}
