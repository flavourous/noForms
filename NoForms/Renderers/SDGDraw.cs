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
            var tl = GetSDGTextInfo(textObject);

            foreach (var glyphrun in tl.glyphRuns)
            {
                var style = glyphrun.run.drawStyle;
                UFont font = style.fontOverride ?? textObject.font;
                FontStyle fs = (font.bold ? FontStyle.Bold: 0) | (font.italic ? FontStyle.Italic: 0);
                var sdgFont = new Font(font.name, font.size, fs);
                UBrush brsh = style.fgOverride ?? defBrush;
                if (style.bgOverride != null)
                    FillRectangle(new Rectangle(glyphrun.location, glyphrun.run.runSize), style.bgOverride);
                realRenderer.graphics.DrawString(glyphrun.run.content, sdgFont, CreateBrush(brsh), location);
            }
        }

        // WARNING this all assumes that char rects and lines are "tightly packed", except  differing font sizes
        //         on same line, which get "baselined".  Is this what really happens?  What about line,word and char spacing adjustments?
        //         dirty way would be to adjust char rects, but does SDG do that? (does it even support that?)
        // Text Measuring - isText tells you if you actually hit a part of the string...
        public UTextHitInfo HitPoint(Point hitPoint, UText text)
        {
            // points
            float hx = hitPoint.X, cx;
            float hy = hitPoint.Y, cy;

            // Grab a ref to the sdgtextinfo
            var ti = GetSDGTextInfo(text);
            int charPos = 0;

            // Find the line we're hitting on
            int hitLine = 0;
            cy = 0;
            for (hitLine = 0; hitLine < ti.lineSizes.Count; hitLine++)
            {
                if (hy >= cy && hy < (cy += ti.lineSizes[hitLine].height))
                    break;
                charPos += ti.lineLengths[hitLine];
            }
            
            // Get glyphruns on this line
            int sgr =-1, egr = -1;
            for (int gr = 0; gr < ti.glyphRuns.Count; gr++)
            {
                if (ti.glyphRuns[gr].lineNumber == hitLine)
                    if (sgr == -1) sgr = gr;
                else if (sgr != -1) egr = gr; // WARNING this is first on next line (will be loopy on i<egr, so is ok)
            }

            // Find the glyphrun we're hitting on (in x direction)
            int hitGlyph = sgr;
            cx = 0;
            for (hitGlyph = sgr; hitGlyph < egr; hitGlyph++)
            {
                if (hx >= cx && hx < (cx += ti.glyphRuns[hitGlyph].run.runSize.width))
                    break;
                charPos += ti.glyphRuns[hitGlyph].run.content.Length;
            }
            cx -= ti.glyphRuns[hitGlyph].run.runSize.width; // reset to start of glyphrun hit

            // find the intersecting char rect (x direction)
            int hitGlyphChar;
            var hgr = ti.glyphRuns[hitGlyph].run;
            for (hitGlyphChar = 0; hitGlyphChar < hgr.charSizes.Count; hitGlyphChar++)
                if (hx >= cx && hx < (cx += hgr.charSizes[hitGlyphChar].width))
                    break;
            charPos += hitGlyphChar;
            cx -= hgr.charSizes[hitGlyphChar].width; // reset to start of char.

            // determine if we've hit text, done simply by checking if we are inside the hit glyph
            Point charlocation = ti.glyphRuns[hitGlyph].location + new Point(cx, hgr.runSize.height - hgr.charSizes[hitGlyphChar].height);
            Rectangle charRect = new Rectangle(charlocation, hgr.charSizes[hitGlyphChar]);
            bool isText = Util.PointInRect(hitPoint, charRect);

            // Determine trailing or leading hit
            bool leading = hitPoint.X > cx + charRect.width / 2;

            return new UTextHitInfo(charPos, leading, isText);
        }

        public Point HitText(int pos, bool trailing, UText text)
        {
            // Grab a ref to the sdgtextinfo
            var ti = GetSDGTextInfo(text);

            // Find the hit glyphrun
            int g, cc =0;
            for (g = 0; g < ti.glyphRuns.Count; g++)
                if ((cc += ti.glyphRuns[g].run.runLength) > pos)
                    break;
            cc -= ti.glyphRuns[g].run.runLength;

            // Get x-location of hit char in glyph
            float cx = 0;
            int i;
            for (i = 0; i < pos - cc; i++)
                cx += ti.glyphRuns[g].run.charSizes[i].width;
            if(trailing)
                cx -= ti.glyphRuns[g].run.charSizes[i].width;

            // Get any y-offset of hit char in the glyph and return
            return ti.glyphRuns[g].location + new Point(cx, ti.glyphRuns[g].run.runSize.height - ti.glyphRuns[g].run.charSizes[i].height);
        }

        public IEnumerable<Rectangle> HitTextRange(int start, int length, Point offset, UText text)
        {
            // Grab a ref to the sdgtextinfo
            var ti = GetSDGTextInfo(text);

            int cc = 0;
            foreach (var glyph in ti.glyphRuns)
            {
                // stride through glyphruns that are not involved.
                if (cc + glyph.run.runLength <= start)
                {
                    cc += glyph.run.runLength;
                    continue;
                }
                float cx = 0;
                foreach (var gchar in glyph.run.charSizes)
                {
                    // end iteration when we fall past this bound
                    if (cc >= start + length) yield break;

                    // calculate and yield the current char rect
                    yield return new Rectangle(glyph.location.X + cx,
                                               glyph.location.Y + (glyph.run.runSize.height - gchar.height),
                                               gchar.width,
                                               gchar.height);

                    // inc counters for following glyphs
                    cx += gchar.width;
                }
            }
        }

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
        SizeF MeasureRun(SDGGlyphRun run, UFont defFont)
        {
            if (run.content == "") return new SizeF(0, 0);
            // For MesaureString FIXME is substring or new String(char) faster?
            Font useFont = Translate(run.drawStyle.fontOverride ?? defFont);
            return realRenderer.graphics.MeasureString(run.content, useFont);
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
                gr.content = gr.runLength > 0 ? text.text.Substring(gr.startPosition, gr.runLength) : "";
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
                    gr.content = gr.runLength > 0 ? text.text.Substring(gr.startPosition, gr.runLength) : "";
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
                foreach (var cs in MeasureCharacters(text, gr.startPosition, gr.runLength))
                    gr.charSizes.Add(new Size(cs.Width, cs.Height));
                gr.content = text.text.Substring(gr.startPosition, gr.runLength);
                gr.runSize = MeasureRun(gr, text.font);
                
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
            public Size minSize;
            public List<SDGGlyphRunLayoutInfo> glyphRuns = new List<SDGGlyphRunLayoutInfo>();
            public List<int> lineLengths = new List<int>();
            public List<int> newLineLengths = new List<int>();
            public List<Size> lineSizes = new List<Size>();
        }

        class SDGGlyphRunLayoutInfo
        {
            public SDGGlyphRun run;
            public Point location;
            public int lineNumber;
        }

        class SDGGlyphRun
        {
            public int startPosition;
            public int runLength;
            public UStyleRange drawStyle; // from original, not necessarily same indices (on the stylerange) as the glyphrun and may be shared with other glyphruns
            public Size runSize;
            public List<Size> charSizes;
            public String content;
            public BreakType breakingType;
        }

        // FIXME Cache!! (most importantly)
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
                        location = new Point(currX, 0), // dunno about y position yet
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
                        igr.location = new Point(igr.location.X, currY - igr.run.runSize.height);
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
                        ret.glyphRuns[i].location = new Point(currX,0);
                    }
                    lastWordBreak = -1;
                    currLine++;

                    // Is this glyphrun a wordbreak? who cares, next iteration will take care via #1. 
                    // Not wordbreak? no prev worbreak? who cares, carry on.
                    ret.glyphRuns.Add(new SDGGlyphRunLayoutInfo()
                    {
                        lineNumber = currLine,
                        location = new Point(currX, 0), // dunno about y position yet
                        run = gr
                    });

                    // resolving the glyphruns of the last line
                    while (lglst++ <= lastWordBreak)
                    {
                        var igr = ret.glyphRuns[lglst];
                        igr.location = new Point(igr.location.X, currY - igr.run.runSize.height);
                    }
                }
                else
                {// Buisness as Normal
                    // add glyphrun, increment currX
                    ret.glyphRuns.Add(new SDGGlyphRunLayoutInfo()
                    {
                        lineNumber = currLine,
                        location = new Point(currX, 0), // dunno about y position yet
                        run = gr
                    });
                    if (gr.breakingType == BreakType.word)
                        lastWordBreak = ret.glyphRuns.Count - 1;
                    currX += gr.runSize.width;
                }
            }
            // resolving the glyphruns of the final line
            while (lglst++ < ret.glyphRuns.Count)
            {
                var igr = ret.glyphRuns[lglst];
                igr.location = new Point(igr.location.X, currY - igr.run.runSize.height);
            }

            // assign the linelengths to textinfo
            int cl = 0; int cc = 0; int cnl = 0;
            float cx=0, cy = 0, maxy=0, maxx =0;
            for(int i=0;i<ret.glyphRuns.Count;i++)
            {
                var gri = ret.glyphRuns[i];
                if (gri.lineNumber > cl)
                {
                    // add lineinfo (and reset counters)
                    cl = gri.lineNumber;
                    ret.lineLengths.Add(cc);
                    ret.newLineLengths.Add(cl);
                    ret.lineSizes.Add(new Size(cx, maxy));
                    cc = cnl = 0;
                    cy += maxy;
                    maxx = Math.Max(maxx, cx);
                    maxy = cx = 0;
                }

                // Continue adding lineinfo data
                cc += gri.run.runLength;
                cx += gri.run.runSize.width;
                if (gri.run.breakingType == BreakType.line) 
                    cnl += gri.run.runLength;
                maxy = Math.Max(gri.run.runSize.height,maxy);
            }
            ret.minSize = new Size(maxx, cy);
            return ret;
        
        }
        

        // FIXME Cache!
        public UTextInfo GetTextInfo(UText text)
        {
            var sti = GetSDGTextInfo(text);
            return new UTextInfo()
            {
                minSize = sti.minSize,
                lineLengths = sti.lineLengths.ToArray(),
                lineNewLineLength = sti.newLineLengths.ToArray(),
                numLines = sti.lineLengths.Count
            };
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
