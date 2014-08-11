using System;
using System.Collections.Generic;
using System.Text;
using Common;

namespace GlyphRunLib
{
    public class GlyphRun<FontClass>
    {
        public delegate Size MeasureFnc(String txt, FontClass font);
        public delegate FontClass FntTranslate(UFont font);
        MeasureFnc MeasureString;
        FntTranslate Translate;
        public GlyphRun(MeasureFnc measurer, FntTranslate fontTranslator)
        {
            MeasureString = measurer;
            Translate = fontTranslator;
        }



        public enum BreakType { none = 0, word = 1, line = 2, font = 4 }
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
                yield return BuildGlyphRun(cpos, tr.location - cpos, cstyle, text, BreakType.none);

                // next is the break itself, dont add font breaks (geting dirty here)
                if (tr.type == BreakType.font) cstyle = tr.styley[1];
                else yield return BuildGlyphRun(tr.location, tr.content.Length, cstyle, text, tr.type);

                // cpos set to after the break
                cpos = tr.location + tr.content.Length;
            }
            // possible last glyphrun
            if (cpos < text.text.Length)
                yield return BuildGlyphRun(cpos, text.text.Length - cpos, cstyle, text, BreakType.none);
        }

        SDGGlyphRun BuildGlyphRun(int start, int length, UStyleRange currentStyle, UText text, BreakType bt)
        {
            FontClass useFont = Translate(currentStyle == null ? text.font : currentStyle.fontOverride ?? text.font);

            var gr = new SDGGlyphRun()
            {
                startPosition = start,
                runLength = length,
                breakingType = bt,
                drawStyle = currentStyle,
                charSizes = new Size[length]
            };

            float tw = 0;
            gr.content = gr.runLength > 0 ? text.text.Substring(gr.startPosition, gr.runLength) : "";
            gr.runSize = MeasureString(gr.content, useFont);
            if (bt == BreakType.word)
            {
                // handle space spacing ;)
                String ltx = gr.startPosition > 0 ? text.text.Substring(gr.startPosition - 1, 1) : "";
                String rtx = gr.startPosition + length + 1 < text.text.Length ? text.text.Substring(length, 1) : "";
                var s1 = MeasureString(ltx + gr.content + rtx, useFont);
                var s2 = MeasureString(ltx + rtx, useFont);
                gr.runSize.width = s1.width - s2.width;
            }
            for (int i = 0; i < gr.content.Length; i++)
            {
                gr.charSizes[i] = MeasureString(gr.content.Substring(i, 1), useFont);
                tw += gr.charSizes[i].width;
            }
            float corr = gr.runSize.width / tw;
            for (int i = 0; i < gr.content.Length; i++)
                gr.charSizes[i].width *= corr;

            return gr;
        }

        float Max(float max1, params float[] maxes)
        {
            var ret = max1;
            foreach (var fl in maxes)
                if (ret < fl) ret = fl;
            return ret;
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

        public class SDGTextInfo
        {
            public Size minSize;
            public List<SDGGlyphRunLayoutInfo> glyphRuns = new List<SDGGlyphRunLayoutInfo>();
            public List<int> lineLengths = new List<int>();
            public List<int> newLineLengths = new List<int>();
            public List<Size> lineSizes = new List<Size>();
        }

        public class SDGGlyphRunLayoutInfo
        {
            public SDGGlyphRun run;
            public Point location;
            public int lineNumber;
        }

        public class SDGGlyphRun
        {
            public int startPosition;
            public int runLength;
            public UStyleRange drawStyle; // from original, not necessarily same indices (on the stylerange) as the glyphrun and may be shared with other glyphruns
            public Size runSize;
            public Size[] charSizes;
            public String content;
            public BreakType breakingType;
        }

        // FIXME Cache!! (most importantly)
        public SDGTextInfo GetSDGTextInfo(UText text)
        {
            // gotta do a line or end before we can decide the char tops (baseline aligned, presumably...)
            float currX = 0, currY = 0, maxGlyphHeight = 0;
            int lglst = 0; // last glyphrun line start
            int lastWordBreak = -1; // last wordbreaking glyph on the current line
            int currLine = 0;
            SDGTextInfo ret = new SDGTextInfo();

            // begin on assumption we're top left align..then correct after
            foreach (var gr in GetGlyphRuns(text))
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
                    do
                    {
                        var igr = ret.glyphRuns[lglst];
                        igr.location = new Point(igr.location.X, currY - igr.run.runSize.height);
                    } while (++lglst < ret.glyphRuns.Count);

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
                    for (int i = lastWordBreak + 1; i < ret.glyphRuns.Count; i++)
                    {
                        ret.glyphRuns[i].lineNumber++;
                        ret.glyphRuns[i].location = new Point(currX, 0);
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
                    do
                    {
                        var igr = ret.glyphRuns[lglst];
                        igr.location = new Point(igr.location.X, currY - igr.run.runSize.height);
                    } while (++lglst <= lastWordBreak);
                }
                else
                {// Buisness as Normal
                    // add glyphrun, increment currX
                    ret.glyphRuns.Add(new SDGGlyphRunLayoutInfo()
                    {
                        lineNumber = currLine,
                        location = new Point(currX, 0),
                        run = gr
                    });
                    if (gr.breakingType == BreakType.word)
                        lastWordBreak = ret.glyphRuns.Count - 1;
                    currX += gr.runSize.width;
                }
            }
            currY += maxGlyphHeight;
            maxGlyphHeight = 0;
            lastWordBreak = -1;
            currX = 0;
            // resolving the glyphruns of the final line
            do
            {
                var igr = ret.glyphRuns[lglst];
                igr.location = new Point(igr.location.X, currY - igr.run.runSize.height);
            } while (++lglst < ret.glyphRuns.Count);


            // assign the linelengths to textinfo
            int cl = 0; int cc = 0; int cnl = 0;
            float cx = 0, cy = 0, maxy = 0, maxx = 0;
            for (int i = 0; i < ret.glyphRuns.Count; i++)
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
                maxy = Math.Max(gri.run.runSize.height, maxy);
            }
            ret.minSize = new Size(maxx, cy);
            return ret;

        }

        public UTextHitInfo HitPoint(SDGTextInfo ti, Point hitPoint)
        {
            int charPos = 0;

            // points
            float hx = hitPoint.X, cx;
            float hy = hitPoint.Y, cy;

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
            int sgr = -1, egr = -1;
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
            for (hitGlyphChar = 0; hitGlyphChar < hgr.charSizes.Length; hitGlyphChar++)
                if (hx >= cx && hx < (cx += hgr.charSizes[hitGlyphChar].width))
                    break;
            charPos += hitGlyphChar;
            cx -= hgr.charSizes[hitGlyphChar].width; // reset to start of char.

            // determine if we've hit text, done simply by checking if we are inside the hit glyph
            Common.Point charlocation = ti.glyphRuns[hitGlyph].location + new Common.Point(cx, hgr.runSize.height - hgr.charSizes[hitGlyphChar].height);
            Common.Rectangle charRect = new Common.Rectangle(charlocation, hgr.charSizes[hitGlyphChar]);
            bool isText = charRect.Contains(hitPoint);

            // Determine trailing or leading hit
            bool leading = hitPoint.X > cx + charRect.width / 2;

                return new UTextHitInfo(charPos, leading, isText);
        }
        public Common.Point HitText(SDGTextInfo ti, int pos, bool trailing)
        {
            // Find the hit glyphrun
            int g, cc = 0;
            for (g = 0; g < ti.glyphRuns.Count; g++)
                if ((cc += ti.glyphRuns[g].run.runLength) > pos)
                    break;
            cc -= ti.glyphRuns[g].run.runLength;

            // Get x-location of hit char in glyph
            float cx = 0;
            int i;
            for (i = 0; i < pos - cc; i++)
                cx += ti.glyphRuns[g].run.charSizes[i].width;
            if (trailing)
                cx -= ti.glyphRuns[g].run.charSizes[i].width;

            // Get any y-offset of hit char in the glyph and return
            return ti.glyphRuns[g].location + new Common.Point(cx, ti.glyphRuns[g].run.runSize.height - ti.glyphRuns[g].run.charSizes[i].height);
        }
        public IEnumerable<Common.Rectangle> HitTextRange(SDGTextInfo ti, int start, int length, Common.Point offset)
        {
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
                    yield return new Common.Rectangle(glyph.location.X + cx,
                                               glyph.location.Y + (glyph.run.runSize.height - gchar.height),
                                               gchar.width,
                                               gchar.height);

                    // inc counters for following glyphs
                    cx += gchar.width;
                }
            }
        }
    }
}