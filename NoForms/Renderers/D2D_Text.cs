using System;
using System.Collections.Generic;
using System.Text;

namespace NoForms.Renderers
{
    public class D2D_TextElements : IDisposable
    {
        public SharpDX.DirectWrite.TextLayout textLayout;
        public D2D_ClientTextRenderer textRenderer;

        public D2D_TextElements(SharpDX.DirectWrite.TextLayout textLayout, D2D_ClientTextRenderer textRenderer)
        {
            this.textLayout = textLayout;
            this.textRenderer = textRenderer;
        }

        public void Dispose()
        {
            textRenderer.Dispose();
            textLayout.Dispose();
        }
    }
    public class ClientTextEffect : SharpDX.ComObject
    {
        public UBrush fgBrush;
        public UBrush bgBrush;
    }
    public class D2D_ClientTextRenderer : SharpDX.DirectWrite.TextRendererBase
    {
        internal ClientTextEffect defaultEffect;
        SharpDX.Direct2D1.RenderTarget rt;
        public D2D_ClientTextRenderer(SharpDX.Direct2D1.RenderTarget rt, ClientTextEffect defaultEffect)
        {
            this.rt = rt;
            this.defaultEffect = defaultEffect;
        }

        public override SharpDX.Result DrawGlyphRun(object clientDrawingContext, float baselineOriginX, float baselineOriginY, SharpDX.Direct2D1.MeasuringMode measuringMode, SharpDX.DirectWrite.GlyphRun glyphRun, SharpDX.DirectWrite.GlyphRunDescription glyphRunDescription, SharpDX.ComObject clientDrawingEffect)
        {
            var cce = (ClientTextEffect)clientDrawingEffect;
            var args = (Object[])clientDrawingContext;
            var ofs = (Point)args[0];
            var ut = (UText)args[1];

            Point origin = new Point(baselineOriginX, baselineOriginY);

            var fgb = cce == null ? defaultEffect.fgBrush : cce.fgBrush;
            var brsh = fgb.GetD2D(rt);
            rt.DrawGlyphRun(origin, glyphRun, brsh, SharpDX.Direct2D1.MeasuringMode.Natural);            

            return SharpDX.Result.Ok;
        }
    }
}
