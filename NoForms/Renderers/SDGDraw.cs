using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace NoForms.Renderers
{
    /// <summary>
    /// SDG stands for system.drawing.graphics
    /// </summary>
    class SDGDraw : IUnifiedDraw
    {
        SDG_RenderElements realRenderer;
        public SDGDraw(SDG_RenderElements els)
        {
            realRenderer = els;
        }
        Stack<System.Drawing.Region> PushedClips = new Stack<System.Drawing.Region>();
        public void PushAxisAlignedClip(Rectangle clipRect, bool ignoreRenderOffset)
        {
            // This probabbly pushes an empty clip...
            if (PushedClips.Count == 0) PushedClips.Push(realRenderer.graphics.Clip);

            var cr = clipRect;
            if (ignoreRenderOffset) cr -= new Point(realRenderer.graphics.Transform.OffsetX, realRenderer.graphics.Transform.OffsetY);
            var cr2 = new System.Drawing.Region(clipRect);
            realRenderer.graphics.Clip = cr2;
            PushedClips.Push(cr2);
        }
        public void PopAxisAlignedClip()
        {
            PushedClips.Pop();
            realRenderer.graphics.Clip = PushedClips.Peek();
        }
        public void SetRenderOffset(Point offset)
        {
            var ox = realRenderer.graphics.Transform.OffsetX;
            var oy = realRenderer.graphics.Transform.OffsetY;
            realRenderer.graphics.Transform.Translate(offset.X - ox, offset.Y - oy);
        }
        public void Clear(Color color)
        {
            realRenderer.graphics.Clear(color);
        }
        public void FillPath(UPath path, UBrush brush)
        {
            realRenderer.graphics.FillPath(CreateBrush(brush), CreatePath(path));
        }
        public void DrawPath(UPath path, UBrush brush, UStroke stroke)
        {
            realRenderer.graphics.DrawPath(CreatePen(brush,stroke), CreatePath(path));
        }
        public void DrawBitmap(UBitmap bitmap, float opacity, UInterp interp, Rectangle source, Rectangle destination)
        {
            realRenderer.graphics.DrawImage(CreateBitmap(bitmap), destination, opacity, Translate(interp), source);
        }
        public void FillEllipse(Point center, float radiusX, float radiusY, UBrush brush)
        {
            realRenderer.graphics.FillEllipse(CreateBrush(brush), center.X - radiusX, center.Y - radiusY, radiusX * 2, radiusY * 2);
        }
        public void DrawEllipse(Point center, float radiusX, float radiusY, UBrush brush, UStroke stroke)
        {
            realRenderer.graphics.DrawEllipse(CreatePen(brush, stroke), center.X - radiusX, center.Y - radiusY, radiusX * 2, radiusY * 2);
        }
        public void DrawLine(Point start, Point end, UBrush brush, UStroke stroke)
        {
            realRenderer.graphics.DrawLine(CreatePen(brush,stroke),start, end);
        }
        public void DrawRectangle(Rectangle rect, UBrush brush, UStroke stroke)
        {
            realRenderer.graphics.DrawRectangle(CreatePen(brush, stroke), rect.left, rect.top, rect.width, rect.height);
        }
        public void FillRectangle(Rectangle rect, UBrush brush)
        {
            realRenderer.graphics.FillRectangle(CreateBrush(brush), rect.left, rect.top, rect.width, rect.height);
        }
        public void DrawRoundedRectangle(Rectangle rect, float radX, float radY, UBrush brush, UStroke stroke)
        {
            var rr = ShapeHelpers.RoundedRectangle(rect, radX, radY);
            realRenderer.graphics.DrawPath(CreatePen(brush, stroke), rr);
        }
        public void FillRoundedRectangle(Rectangle rect, float radX, float radY, UBrush brush)
        {
            var rr = ShapeHelpers.RoundedRectangle(rect, radX, radY);
            realRenderer.graphics.FillPath(CreateBrush(brush), rr);
        }
        public void DrawText(UText textObject, Point location, UBrush defBrush, UTextDrawOptions opt, bool clientRendering)
        {
            var tl = CreateTextElements(textObject);

            foreach (var ord in textObject.SafeGetStyleRanges)
            {
                if (ord.bgOverride != null)
                {
                    foreach (var r in HitTextRange(ord.start, ord.length, location, textObject))
                        FillRectangle(r, ord.bgOverride);
                }
            }

            if (clientRendering)
            {
                // set default foreground brush
                tl.textRenderer.defaultEffect.fgBrush = CreateBrush(defBrush);

                // Draw the text (foreground & background in the client renderer)
                tl.textLayout.Draw(new Object[] { location, textObject }, tl.textRenderer, location.X, location.Y);
            }
            else
            {
                // Use D2D implimentation of text layout rendering
                realRenderer.renderTarget.DrawTextLayout(location, tl.textLayout, CreateBrush(defBrush));
            }

            

        }

        

        // Text Measuring
        public UTextHitInfo HitPoint(Point hitPoint, UText text)
        {
            var textLayout = CreateTextElements(text).textLayout;
            SharpDX.Bool trailing, inside;
            var htm = textLayout.HitTestPoint(hitPoint.X, hitPoint.Y, out trailing, out inside);
            return new UTextHitInfo()
            {
                charPos = htm.TextPosition,
                leading = hitPoint.X > htm.Left + htm.Width / 2,
                isText = htm.IsText
            };
        }

        public Point HitText(int pos, bool trailing, UText text)
        {
            var textLayout = CreateTextElements(text).textLayout;
            float hx, hy;
            var htm = textLayout.HitTestTextPosition(pos, trailing, out hx, out hy);
            return new Point(hx, hy);
        }

        public IEnumerable<Rectangle> HitTextRange(int start, int length, Point offset, UText text)
        {
            var textLayout = CreateTextElements(text).textLayout;
            foreach (var htm in textLayout.HitTestTextRange(start, length, offset.X, offset.Y))
                yield return new Rectangle(htm.Left, htm.Top, htm.Width, htm.Height);
        }

        // FIXME StyleRanges!!!
        IEnumerable<SizeF> MeasureCharacters(UText text, int start, int length)
        {
            // For MesaureString FIXME is substring or new String(char) faster?
            Font defFont = Translate(text.font), useFont;
            for (int i = 0; i < text.text.Length; i++)
            {
                // FIXME write helper to transform the styleranges into sections, iterate those not the text.
                useFont = defFont;
                foreach (var sr in text.SafeGetStyleRanges)
                    if (sr.fontOverride != null && sr.start <= i && sr.length > 0)
                        useFont = Translate((UFont)sr.fontOverride);
                yield return realRenderer.graphics.MeasureString(text.text.Substring(i, 1), useFont);
            }
        }

        enum BreakType { none = 0, word = 1, line = 2, font = 4 }
        class TR { public BreakType type; public int location; public String content; public UStyleRange[] styley = new UStyleRange[2]; }
        static String[] wordBreak = new String[] { " " };
        static String[] lineBreak = new String[] { "\r\n", "\n" }; //order is important
        IEnumerable<SDGGlyphRun> GetGlyphRuns(UText text)
        {
            // concecutive wordbreaks are one glyphrun, while concecutive linebreaks are individual glpyhruns
            List<TR> breaks = new List<TR>();
            foreach (String lb in lineBreak)
                foreach (int idx in AllIndexes(text.text, lb))
                    breaks.Add(new TR() { type = BreakType.line, content = lb, location = idx });
            int lastIdx = -2;
            foreach (String wb in wordBreak)
                foreach (int idx in AllIndexes(text.text, wb))
                {
                    if (idx == lastIdx + 1) // inflate last break
                        breaks[breaks.Count - 1].content += wb;
                    else breaks.Add(new TR() { type = BreakType.word, content = wb, location = idx });
                }

            int cpos = 0;
            UStyleRange cstyle = null;
            bool flatch = true;

            // splitting glyphs also by changes in UStyleRange
            var srs = new List<UStyleRange>(text.SafeGetStyleRanges);
            foreach (var sr in NormaliseStyleRanges(srs, text.text.Length))
            {
                var lsi = sr.leftStlyeIdx;
                var rsi = sr.rightStyleIdx;
                var tr = new TR()
                {
                    type = BreakType.font,
                    content = "",
                    location = sr.splitPoint,
                    styley = new UStyleRange[]  
                    {
                        lsi > -1 ? srs[lsi] : null,
                        rsi > -1 ? srs[rsi] : null
                    }
                };
                breaks.Add(tr);
                if (flatch)
                {
                    cstyle = tr.styley[0];
                    flatch = false;
                }
            }

            // Sort those breaks... FIXME sorting is slow
            breaks.Sort((a, b) => a.location.CompareTo(b.location));

            // build glyphruns from the breaks...
            foreach (var tr in breaks)
            {
                // two glyphruns in this, first is before the break (could be zero length)
                var gr = new SDGGlyphRun()
                {
                    startPosition = cpos,
                    runLength = tr.location - cpos,
                    breakingType = BreakType.none,
                    drawStyle = cstyle,
                    charSizes = new List<Size>()
                };
                float h = 0, w = 0;
                foreach (var cs in MeasureCharacters(text,gr.startPosition, gr.runLength))
                {
                    gr.charSizes.Add(new Size(cs.Width,cs.Height));
                    w += cs.Width;
                    h = Math.Max(cs.Height, h);
                }
                gr.runSize = new Size(w, h);
                yield return gr;

                // next is the break itself, dont add font breaks (geting dirty here)
                if (tr.type == BreakType.font)
                {
                    cstyle = tr.styley[1];
                }
                else
                {
                    // but add other types
                    gr = new SDGGlyphRun()
                    {
                        startPosition = tr.location,
                        runLength = tr.content.Length,
                        breakingType = tr.type,
                        charSizes = new List<Size>()
                    };
                    h = w = 0;
                    foreach (var cs in MeasureCharacters(text, gr.startPosition, gr.runLength))
                    { // might as well measure these glyphs...should have zero size though...
                        gr.charSizes.Add(new Size(cs.Width, cs.Height));
                        w += cs.Width;
                        h = Math.Max(cs.Height, h);
                    }
                    gr.runSize = new Size(w, h);
                    yield return gr;
                }

                // cpos set to after the break
                cpos = tr.location + tr.content.Length;
            }
            // possible last glyphrun
            if (cpos < text.text.Length)
            {
                SDGGlyphRun gr = new SDGGlyphRun()
                {
                    startPosition = cpos,
                    runLength = text.text.Length - cpos,
                    breakingType = BreakType.none, // it's impossible for this to be a breaking glyphrun
                    charSizes = new List<Size>(),
                    drawStyle = cstyle
                };
                float h = 0, w = 0;
                foreach (var cs in MeasureCharacters(text, gr.startPosition, gr.runLength))
                { // might as well measure these glyphs...should have zero size though...
                    gr.charSizes.Add(new Size(cs.Width, cs.Height));
                    w += cs.Width;
                    h = Math.Max(cs.Height, h);
                }
                gr.runSize = new Size(w, h);
                yield return gr;
            }
        }

        IEnumerable<int> AllIndexes(String s, String c)
        {
            int res = 0;
            do
            {
                res = s.IndexOf(c, res + 1);
                if (res > 0) yield return res;
            } while (res > 0 && (res + 1) < s.Length);
        }

        // FIXME bit object spammy? probabbly doesnt matter in grand scheme...
        struct BV { public int leftStlyeIdx; public int rightStyleIdx; public int splitPoint;}
        IEnumerable<BV> NormaliseStyleRanges(IList<UStyleRange> messy, int textLen)
        {
            if (textLen == 0) yield break;

            // Create a mask to determine which indexes count...
            int[] styleMask = new int[textLen];
            for (int i = 0; i < messy.Count; i++) styleMask[i] = -1;
            for (var sri = 0; sri < messy.Count; sri++)
            {
                var sr = messy[sri];
                for (int i = 0; i < sr.length; i++)
                    styleMask[i + sr.start] = sri;
            }
            int cval = styleMask[0];
            for (int i = 0; i < textLen; i++)
                if (cval != styleMask[i])
                    yield return new BV()
                    {
                        leftStlyeIdx = cval,
                        rightStyleIdx = cval = styleMask[i],
                        splitPoint = i
                    };

        }

        class SDGTextInfo
        {
            public List<SDGGlyphRunLayoutInfo> glyphRuns;
            public List<int> lineLengths;
            public List<int> newLineLengths;
        }

        class SDGGlyphRunLayoutInfo
        {
            public SDGGlyphRun run;
            public Point locationLowerRight;
            public int lineNumber;
        }

        class SDGGlyphRun
        {
            public int startPosition;
            public int runLength;
            public UStyleRange drawStyle; // from original, not necessarily same indices as the glyphrun and may be shared
            public Size runSize;
            public List<Size> charSizes;
            public BreakType breakingType;
        }

        SDGTextInfo GetSDGTextInfo(UText text)
        {
            // gotta do a line or end before we can decide the char tops (baseline aligned, presumably...)
            float currX = 0, currY =0, maxGlyphHeight =0;
            int lglst=0; // last glyphrun line start
            int lastWordBreak = -1; // last wordbreaking glyph on the current line
            int currLine = 0;
            SDGTextInfo ret = new SDGTextInfo();

            // begin on assumption we're top left align..then correct after
            foreach(var gr in GetGlyphRuns(text))
            {
                // tracking max height for the line baselineing
                maxGlyphHeight = Math.Max(gr.runSize.height, maxGlyphHeight);
                // whichever way, this line is this many chars longer

                // Potential runs include words, spaces and linebreaks, and font breaks
                if (gr.breakingType == BreakType.line)
                {//LineBreaking
                    // add the glyphrun, resolve info for the line, and begin on new line!
                    ret.glyphRuns.Add(new SDGGlyphRunLayoutInfo()
                    {
                        lineNumber = currLine,
                        locationLowerRight = new Point(currX, 0), // dunno about y position yet
                        run = gr
                    });

                    currY += maxGlyphHeight;
                    maxGlyphHeight = 0;
                    lastWordBreak = -1;
                    currX = 0;

                    // resolving the glyphruns of this line
                    while (lglst++ < ret.glyphRuns.Count)
                    {
                        var igr = ret.glyphRuns[lglst];
                        igr.locationLowerRight = new Point(igr.locationLowerRight.X, currY);
                    }

                    // begin a new line
                    currLine++;
                }
                else if (lastWordBreak > -1 && currX + gr.runSize.width > text.width && text.wrapped)
                {// WordWrapping
                    // Must define the concept of glyph groups here.  Those between line and/or word breaks.
                    // The whole of such a group must be broken.  We need to define the breaking glyph.
                    currY += maxGlyphHeight;
                    maxGlyphHeight = 0;
                    currX = 0;
                    // #1 Is there a word break previous to this glyphrun on this line?  
                    //    then put all glyphs following that on the next line.
                    for (int i = lastWordBreak+1; i < ret.glyphRuns.Count; i++)
                    {
                        ret.glyphRuns[i].lineNumber++;
                        ret.glyphRuns[i].locationLowerRight = new Point(currX,0);
                    }
                    lastWordBreak = -1;
                    currLine++;

                    // Is this glyphrun a wordbreak? who cares, next iteration will take care via #1. 
                    // Not wordbreak? no prev worbreak? who cares, carry on.
                    ret.glyphRuns.Add(new SDGGlyphRunLayoutInfo()
                    {
                        lineNumber = currLine,
                        locationLowerRight = new Point(currX, 0), // dunno about y position yet
                        run = gr
                    });

                    // resolving the glyphruns of the last line
                    while (lglst++ <= lastWordBreak)
                    {
                        var igr = ret.glyphRuns[lglst];
                        igr.locationLowerRight = new Point(igr.locationLowerRight.X, currY);
                    }
                }
                else
                {// Buisness as Normal
                    // add glyphrun, increment currX
                    ret.glyphRuns.Add(new SDGGlyphRunLayoutInfo()
                    {
                        lineNumber = currLine,
                        locationLowerRight = new Point(currX, 0), // dunno about y position yet
                        run = gr
                    });
                    if (gr.breakingType == BreakType.word)
                        lastWordBreak = ret.glyphRuns.Count - 1;
                    currX += gr.runSize.width;
                }
            }
            // apply h and v align offsets after main calc...
            int cl = 0; int cc = 0;
            float cx, cy;
            cx = cy = 0;
            for(int i=0;i<ret.glyphRuns.Count;i++)
            {
                var gri = ret.glyphRuns[i];
                if (gri.lineNumber > cl)
                {
                    cl = gri.lineNumber;
                    ret.lineLengths.Add(cc);
                    ret.newLineLengths.Add(cl);
                    cc = cl = 0;
                }
                cc += gri.run.runLength;
                if (gri.run.breakingType == BreakType.line) 
                    cl += gri.run.runLength;
            }
        }

        public UTextInfo GetTextInfo(UText text)
        {
            
        }

        Font Translate(UFont font)
        {
            return new Font(font.name, font.size, (font.italic ? FontStyle.Italic : 0) | (font.bold ? FontStyle.Bold : 0));
        }

        // Element Creators
        // FIXME how to avoid switching on type, and allowing mixing of UBrush etc between renderes at runtime?  Factory method on specific class wouldnt work...
        Brush CreateBrush(UBrush b)
        {
            return b.Retreive<SDG_RenderElements>(new NoCacheDelegate(() => CreateNewBrush(b))) as Brush;
        }
        Brush CreateNewBrush(UBrush b)
        {
            // FIXME can use PathGradientBrush, d2d just has gradientbrush...
            Brush ret;
            if (b is USolidBrush)
            {
                // FIXME support brushProperties
                USolidBrush sb = b as USolidBrush;
                ret = new SolidBrush(sb.color);
            }
            else if (b is ULinearGradientBrush)
            {
                var lb = b as ULinearGradientBrush;
                ret = new LinearGradientBrush(lb.point1, lb.point2, lb.color1, lb.color2);
            }
            else throw new NotImplementedException();
            return ret;
        }

        Bitmap CreateBitmap(UBitmap b)
        {
            return b.Retreive<D2D_RenderElements>(new NoCacheDelegate(() => CreateNewBitmap(b))) as Bitmap;
        }
        Bitmap CreateNewBitmap(UBitmap b)
        {
            System.Drawing.Bitmap bm;
            if (b.bitmapData != null)
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream(b.bitmapData);
                bm = new System.Drawing.Bitmap(ms);
            }
            else bm = new System.Drawing.Bitmap(b.bitmapFile);
            return bm;
        }

        Pen CreatePen(UBrush b, UStroke s)
        {
            return s.Retreive<SDG_RenderElements>(new NoCacheDelegate(() => CreateNewPen(b, s))) as Pen;
        }
        Pen CreateNewPen(UBrush b, UStroke s)
        {
            // Link to invalidation of brush, once.
            //  the cache of the pen is stored in the stroke.
            NoFormsAction inval = delegate { };
            inval = () =>
            {
                s.Invalidate();
                b.invalidated -= inval;
            };
            b.invalidated += inval;

            // FIXME advanced features not used!
            Pen ret;
            ret = new Pen(CreateBrush(b));
            ret.DashCap = Translate1(s.dashCap);
            ret.EndCap = Translate2(s.endCap);
            ret.StartCap = Translate2(s.startCap);
            ret.LineJoin = Translate(s.lineJoin);
            ret.DashStyle = Translate(s.dashStyle);
            ret.MiterLimit = s.mitreLimit;
            ret.DashOffset = s.offset;

            // FIXME maybe not right for compatibility with other renderes?
            ret.Alignment = PenAlignment.Center;

            if (s.dashStyle == StrokeType.custom)
                ret.DashPattern = s.custom;

            return ret;
        }

        DashCap Translate1(StrokeCaps sc)
        {
            switch (sc)
            {
                case (StrokeCaps.flat): return DashCap.Flat;
                case (StrokeCaps.round): return DashCap.Round;
                case (StrokeCaps.triangle): return DashCap.Triangle;
                default: return DashCap.Flat;
            }
        }
        LineCap Translate2(StrokeCaps sc)
        {
            switch (sc)
            {
                case (StrokeCaps.flat): return LineCap.Flat;
                case (StrokeCaps.round): return LineCap.Round;
                case (StrokeCaps.triangle): return LineCap.Triangle;
                default: return LineCap.Flat;
            }
        }
        LineJoin Translate(StrokeJoin sj)
        {
            switch (sj)
            {
                case (StrokeJoin.bevel): return LineJoin.Bevel;
                case (StrokeJoin.mitre): return LineJoin.Miter;
                case (StrokeJoin.round): return LineJoin.Round;
                default: return LineJoin.Bevel;
            }
        }
        DashStyle Translate(StrokeType st)
        {
            switch (st)
            {
                case (StrokeType.solid): return DashStyle.Solid;
                case (StrokeType.custom): return DashStyle.Custom;
                case (StrokeType.dash): return DashStyle.Dash;
                case (StrokeType.dashdot): return DashStyle.DashDot;
                case (StrokeType.dashdotdot): return DashStyle.DashDotDot;
                case (StrokeType.dot): return DashStyle.Dot;
                default: return DashStyle.Solid;
            }
        }


        GraphicsPath CreatePath(UPath p)
        {
            return p.Retreive<SDG_RenderElements>(new NoCacheDelegate(() => CreateNewPath(p))) as GraphicsPath;
        }
        GraphicsPath CreateNewPath(UPath p)
        {
            var gp = new GraphicsPath();
            foreach (var f in p.figures)
            {
                // FIXME hollow/filled not used here.
                gp.StartFigure();
                Point current = f.startPoint;
                foreach (var gb in f.geoElements) AppendGeometry(gp, gb, current);
                if (!f.open) gp.CloseFigure();
            }
            return gp;
        }

        // FIXME another example of type switching.  Cant put d2d code on other side of bridge in eg UGeometryBase abstraction
        //       and also, using a factory to create the UGeometryBase will make d2d specific versions.
        // IDEA  This is possible:  Use factory (DI?) and check the Uxxx instances when passed to this type of class, and recreate
        //       a portion of the Uxxx if it doesnt match D2D?  Factory could be configured for d2d/ogl etc.  This links in with cacheing
        //       code too? Not sure if this will static compile :/ thats kinda the point...  we'd need the Uxxx.ICreateStuff to be a specific
        //       D2D interface...could subclass... would check if(ICreateStuff is D2DCreator) as d2dcreator else Icreatestuff=new d2dcreator...
        void AppendGeometry(GraphicsPath path, UGeometryBase geo, Point start)
        {
            
            if (geo is UArc)
            {
                UArc arc = geo as UArc;
                Rectangle boundingRectangle = new Rectangle(start,arc.endPoint);
                // D2D's crazy prescription probabbly it seems, will increase the arc radii if the two points form a rectangle
                //  bounding a elipse with greater radii than specified in arcsize.
                Size useArcSize = new Size(Math.Max(arc.arcSize.width, boundingRectangle.width),
                                           Math.Max(arc.arcSize.height, boundingRectangle.height));
                // FIXME solve this transform when you have pen and paper.
                path.AddArc();
            }
            else if (geo is ULine)
            {
                ULine line = geo as ULine;
                path.AddLine(start, line.endPoint);
            }
            else if (geo is UBeizer)
            {
                UBeizer beizer = geo as UBeizer;
                path.AddBezier(start, beizer.controlPoint1, beizer.controlPoint2, beizer.endPoint);
            }
            else throw new NotImplementedException();
        }

        // Drawing Helpers
        static class ShapeHelpers
        {
            public static GraphicsPath RoundedRectangle(Rectangle r, float rx, float ry)
            {
                // FIXME surely this is slow? we need some special caching for GDI?
                var gp = new GraphicsPath();
                gp.StartFigure();
                float x = r.left;
                float y = r.top;
                float w = r.width;
                float h = r.height;
                gp.AddLine(x + rx, y, x + w - rx, y); // top line
                gp.AddArc(x + w - 2 * rx, y, 2 * rx, 2 * ry, 270, 90); // top-right corner
                gp.AddLine(x + w, y + ry, x + w, y + h - ry); // right line
                gp.AddArc(x + w - 2 * rx, y + h - 2 * ry, 2 * rx, 2 * ry, 0, 90);// bottom-right corner
                gp.AddLine(x + w - rx, y + h, x + rx, y + h); // bottom line
                gp.AddArc(x, y + h - 2 * ry, 2 * rx, 2 * ry, 90, 90); // bottom-left corner
                gp.AddLine(x, y + h - ry, x, y + ry); // left line
                gp.AddArc(x, y, 2 * rx, 2 * ry, 180, 90); // top-left corner
                gp.CloseFigure();
                return gp;
            }
        }
    }

}
