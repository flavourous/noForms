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
    public class D2D_ClientTextEffect : SharpDX.ComObject
    {
        public SharpDX.Direct2D1.Brush brsh;
    }
    public class D2D_ClientTextRenderer : SharpDX.DirectWrite.TextRendererBase
    {
        internal D2D_ClientTextEffect defaultEffect;
        SharpDX.Direct2D1.RenderTarget rt;
        public D2D_ClientTextRenderer(SharpDX.Direct2D1.RenderTarget rt, D2D_ClientTextEffect defaultEffect)
        {
            this.rt = rt;
            this.defaultEffect = defaultEffect;
        }

        public override SharpDX.Result DrawGlyphRun(object clientDrawingContext, float baselineOriginX, float baselineOriginY, SharpDX.Direct2D1.MeasuringMode measuringMode, SharpDX.DirectWrite.GlyphRun glyphRun, SharpDX.DirectWrite.GlyphRunDescription glyphRunDescription, SharpDX.ComObject clientDrawingEffect)
        {
            var cce = (D2D_ClientTextEffect)clientDrawingEffect;
            if(cce==null) cce= defaultEffect;

            rt.DrawGlyphRun(new Point(baselineOriginX, baselineOriginY), glyphRun, cce.brsh, SharpDX.Direct2D1.MeasuringMode.Natural);

            bool fuckingIdiotMode = false;
            if (fuckingIdiotMode)
            {
                var pg = new SharpDX.Direct2D1.PathGeometry(cce.brsh.Factory);
                var sink = pg.Open();
                if (glyphRun.Indices.Length > 0)
                    glyphRun.FontFace.GetGlyphRunOutline(glyphRun.FontSize, glyphRun.Indices, glyphRun.Advances, glyphRun.Offsets, glyphRun.IsSideways, glyphRun.BidiLevel % 2 != 0, sink);
                sink.Close();
                var rtt = rt.Transform;
                rt.Transform = new SharpDX.Matrix3x2(1, 0, 0, 1, baselineOriginX, baselineOriginY);
                rt.FillGeometry(pg, cce.brsh);
                rt.Transform = rtt;
            }

            return SharpDX.Result.Ok;
        }
    }
}
