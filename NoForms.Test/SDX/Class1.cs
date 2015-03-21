using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NoFormsSDK;
using SharpDX;
using SharpDX.Direct3D10;
using SharpDX.Direct2D1;
using SharpDX.WIC;

namespace NoForms.Test.SDX
{
    [TestFixture]
    class ClientRendering
    {
        [Test]
        public void TextFieldTry()
        {
            Textfield tf = new Textfield();
            var cr=new Common.Region();
            cr.Add(tf.DisplayRectangle);
            tf.Draw(GetSDXContext.GetDrawer(), cr);
        }
    }

    class GetSDXContext : IDraw
    {
        public static IDraw GetDrawer()
        {
            var dv = new SharpDX.Direct3D10.Device1(DriverType.Hardware);
            var df = new SharpDX.Direct2D1.Factory();
            var t2d = new SharpDX.Direct3D10.Texture2D(dv, new Texture2DDescription() { Height = 100, Width = 100, MipLevels = 1, Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm });
            var rt = new RenderTarget(df, t2d.QueryInterface<SharpDX.DXGI.Surface1>(), new RenderTargetProperties(new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));
            return new GetSDXContext(new Renderers.SharpDX.SharpDX_RenderElements(rt));
        }
        private GetSDXContext(Renderers.SharpDX.SharpDX_RenderElements re)
        {
            uDraw = new Renderers.SharpDX.D2DDraw(re);
            backRenderer = re;
        }

        public Renderers.IUnifiedDraw uDraw { get; private set; }
        public Renderers.IRenderElements backRenderer { get; private set; }
        public Renderers.UnifiedEffects uAdvanced { get; private set; }
    }
}
