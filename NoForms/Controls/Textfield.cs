using System;
using SharpDX.Direct2D1;
using NoForms.Renderers;
using System.Text.RegularExpressions;
using System.Text;

namespace NoForms.Controls
{
    public class Textfield : Abstract.BasicContainer
    {
        public enum LayoutStyle { OneLine, MultiLine, WrappedMultiLine };
        public LayoutStyle _layout = LayoutStyle.OneLine;
        public LayoutStyle layout 
        {
            get { return _layout; }
            set { _layout = value; UpdateTextLayout(); }
        }

        public String text
        {
            get { return data.text; }
            set { data.text = value.Replace("\r\n","\n").Replace("\r","\n"); UpdateTextLayout(); } // internally calls goto data.text
        }
        UText data = new UText("kitty", UHAlign.Left, UVAlign.Middle, false, 0, 0)
        {
            font = new UFont("Arial", 14f, false, false)
        };
        private int _caretPos = 0;
        public int caretPos
        {
            get { return _caretPos; }
            set
            {
                _caretPos = value;
                if (!shift && !mouseSelect)
                {
                    shiftOrigin = value;
                }
                UpdateTextLayout();
            }
        }

        public Thickness padding = new Thickness() { left = 5, right = 5, top = 5, bottom = 5 };
        public Rectangle PaddedRectangle
        {
            get
            {
                return DisplayRectangle.Deflated(padding);
            }
        }

        void tm_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!(caretBrush is USolidBrush)) return;
            var cb = caretBrush as USolidBrush;

            if (cb.color.a == 1f)
            {
                cb.color = new Color(0, 0, 0, 0);
                (sender as System.Timers.Timer).Interval = 300;
            }
            else
            {
                cb.color = new Color(0);
                (sender as System.Timers.Timer).Interval = 800;
            }
        }

        UStyleRange selectRange;
        public Textfield() : base()
        {
            SizeChanged += new Action<Size>(Textfield_SizeChanged);
            LocationChanged += new Action<Point>(Textfield_LocationChanged);

            selectRange = new UStyleRange(0,0, null, selectFG, selectBG);
            data.styleRanges.Add(selectRange);

            Cursor = System.Windows.Forms.Cursors.IBeam;

            UpdateTextLayout();
            System.Timers.Timer tm = new System.Timers.Timer(800) { AutoReset = true };
            tm.Elapsed += new System.Timers.ElapsedEventHandler(tm_Elapsed);
            tm.Start();
        }

        void Textfield_LocationChanged(Point obj)
        {
            UpdateTextLayout();
        }

        void Textfield_SizeChanged(Size obj)
        {
            UpdateTextLayout();
        }

        // Bits
        public UBrush background = new USolidBrush() { color = new Color(.5f, .8f, .8f, .8f) };
        public UBrush borderBrush = new USolidBrush() { color = new Color(0) };
        public UStroke borderStroke = new UStroke() { strokeWidth = 1f };
        public UBrush caretBrush = new USolidBrush() { color = new Color(0) };
        public UStroke caretStroke = new UStroke() { strokeWidth = 1f };
        public UBrush textBrush = new USolidBrush() { color = new Color(0) };
        Point caret1 = new Point(0.5f, 0.5f);
        Point caret2 = new Point(0.5f, 0.5f);

        public UBrush selectBG = new USolidBrush() { color = new Color(.5f, .5f, .5f, 1f) };
        public UBrush selectFG = new USolidBrush() { color = new Color(1f) };

        UBrush defBrush = new USolidBrush() { color = new Color(0) };
        System.Collections.Generic.Queue<Action<IDraw>> runNextRender = new System.Collections.Generic.Queue<Action<IDraw>>();
        UTextInfo lastTextInfo = new UTextInfo();
        public override void Draw(IDraw rt)
        {
            if (textLayoutNeedsUpdate) UpdateTextLayout(rt);
            while (runNextRender.Count > 0)
                runNextRender.Dequeue()(rt);
            
            rt.uDraw.FillRectangle(DisplayRectangle, background);
            rt.uDraw.DrawRectangle(DisplayRectangle.Inflated(new Thickness(-.5f)), borderBrush, borderStroke);
            rt.uDraw.PushAxisAlignedClip(DisplayRectangle,false);

            rt.uDraw.DrawText(data, new Point(PaddedRectangle.left - roX, PaddedRectangle.top - roY), defBrush, UTextDrawOptions.None, true);
            if (FocusManager.FocusGet(this)) rt.uDraw.DrawLine(caret1, caret2, caretBrush, caretStroke);

            rt.uDraw.PopAxisAlignedClip();
        }

        float roX = 0, roY = 0;
        bool textLayoutNeedsUpdate = false;
        private void UpdateTextLayout(IDraw sendMe = null)
        {
            Object passageLocker = new Object();
            lock (passageLocker)
            {
                if (sendMe == null)
                {
                    textLayoutNeedsUpdate = true;
                    return;
                }
                else textLayoutNeedsUpdate = false;

                // sort out some settings
                
                data.wrapped = layout == LayoutStyle.WrappedMultiLine;
                data.width = PaddedRectangle.width;
                data.height = PaddedRectangle.height;
                data.valign = (layout == LayoutStyle.OneLine) ? UVAlign.Middle : UVAlign.Top;

                bool rev = caretPos > shiftOrigin;
                selectRange.start = rev ? shiftOrigin : caretPos;
                selectRange.length = Math.Abs(caretPos-shiftOrigin);

                // get size of the render
                var ti = lastTextInfo = sendMe.uDraw.GetTextInfo(data);
                int lines=ti.numLines;
                Size tms = ti.minSize;
                float lineHeight = tms.height / (float) lines;

                // Get Caret location
                Point cp = sendMe.uDraw.HitText(caretPos, false, data);

                // determine render offset
                roX = cp.X > PaddedRectangle.width ? cp.X - PaddedRectangle.width : 0;
                roY = cp.Y > PaddedRectangle.height - lineHeight ? cp.Y - PaddedRectangle.height + lineHeight : 0;

                float yCenteringOffset = PaddedRectangle.height / 2 - lineHeight / 2;
                if (layout == LayoutStyle.OneLine) roY += yCenteringOffset;

                // set caret line
                caret1 = new Point((float)Math.Round(PaddedRectangle.left + cp.X - roX) + 0.5f, (float)Math.Round(PaddedRectangle.top + cp.Y - roY));
                caret2 = new Point((float)Math.Round(PaddedRectangle.left + cp.X - roX) + 0.5f, (float)Math.Round(PaddedRectangle.top + cp.Y - roY + lineHeight));
            }
        }

        bool ctrl = false, alt = false, win = false, shift = false;
        int shiftOrigin = 0;

        void SetClip(String cs)
        {
            System.Windows.Forms.Clipboard.SetText(cs);
        }
        String GetClip()
        {
            return System.Windows.Forms.Clipboard.GetText();
        }

        void MKeys(System.Windows.Forms.Keys key, bool down)
        {
            if (key == System.Windows.Forms.Keys.Control || key == System.Windows.Forms.Keys.ControlKey)
                ctrl = down;
            // FIXME wtf?
            if (key == System.Windows.Forms.Keys.Alt || key == (System.Windows.Forms.Keys.RButton | System.Windows.Forms.Keys.ShiftKey))
                alt = down;
            if (key == System.Windows.Forms.Keys.LWin)
                win = down;
            if (key == System.Windows.Forms.Keys.ShiftKey)
                shift = down;
        }

        public override void KeyUpDown(System.Windows.Forms.Keys key, ButtonState bs)
        {
            base.KeyUpDown(key, bs);
            if (bs == ButtonState.DOWN) KeyDown(key);
            else KeyUp(key);
        }

        public void KeyDown(System.Windows.Forms.Keys key)
        {
                if (!FocusManager.FocusGet(this)) return;
                MKeys(key, true);
                if (alt) return;

            // We dont like \r\n or \r in our text!
                data.text = text.Replace("\r\n", "\n").Replace("\r", "\n");

                // Copy
                if (ctrl && key == System.Windows.Forms.Keys.C)
                {
                    String cs = BoundSubString(data.text, caretPos, shiftOrigin);
                    if (cs != "") SetClip(cs);
                }

                // Cut 
                if (ctrl && key == System.Windows.Forms.Keys.X)
                {
                    String cs = BoundSubString(data.text, caretPos, shiftOrigin);
                    if (cs != "") SetClip(cs);
                    ReplaceSelectionWithText("");
                }

                // Paste
                if (ctrl && key == System.Windows.Forms.Keys.V)
                    ReplaceSelectionWithText(GetClip());

                // Selcet all
                if (ctrl && key == System.Windows.Forms.Keys.A)
                {
                    caretPos = data.text.Length;
                    shiftOrigin = 0;
                }


                // Backspace
                if (key == System.Windows.Forms.Keys.Back && caretPos > 0)
                {
                    if (caretPos != shiftOrigin)
                        ReplaceSelectionWithText("");
                    else
                    {
                        data.text = data.text.Substring(0, caretPos - 1) + data.text.Substring(caretPos);
                        caretPos--;
                    }
                }

                if (shift && caretPos == shiftOrigin)
                {
                    // cut line
                    if (key == System.Windows.Forms.Keys.Delete)
                    {
                        int start, end;
                        GetLineRange(out start, out end);
                        String cs = BoundSubString(data.text, start, end);
                        if (cs != "") SetClip(cs);
                        data.text = data.text.Substring(0, start) + data.text.Substring(end, data.text.Length - end);
                        caretPos = shiftOrigin = start;
                    }
                    // paste line
                    if (key == System.Windows.Forms.Keys.Insert)
                    {
                        int lineNum, linePos;
                        UTextInfo ti = lastTextInfo;
                        FindMyLine(caretPos, ti.lineLengths, out lineNum, out linePos);
                        _caretPos = shiftOrigin = caretPos - linePos;
                        ReplaceSelectionWithText(GetClip().TrimEnd('\r', '\n') + "\n");
                    }
                }

                // Delete
                else if (key == System.Windows.Forms.Keys.Delete && caretPos < data.text.Length && !ctrl)
                {
                    if (caretPos != shiftOrigin)
                        ReplaceSelectionWithText("");
                    else
                    {
                        data.text = data.text.Substring(0, caretPos) + data.text.Substring(caretPos + 1);
                    }
                }

                if (key == System.Windows.Forms.Keys.Left && caretPos > 0)
                {
                    if (ctrl)
                    {
                        String lText = data.text.Substring(0, caretPos).Replace("\r", "\n").Replace("\n", " ");
                        var reg = new Regex(@"\s+");
                        var mt = reg.Matches(lText);
                        int cp = 0;
                        if (mt.Count > 0)
                            cp = mt[mt.Count - 1].Index + mt[mt.Count - 1].Length;
                        if (cp == caretPos)
                        {
                            if (mt.Count > 1)
                                cp = caretPos = mt[mt.Count - 2].Index + mt[mt.Count - 2].Length;
                            else
                                cp = 0;
                        }
                        caretPos = cp;
                    }
                    else caretPos--;
                }
                if (key == System.Windows.Forms.Keys.Right && caretPos < data.text.Length)
                {
                    if (ctrl)
                    {
                        String rText = data.text.Substring(caretPos, data.text.Length - caretPos).Replace("\r", "\n").Replace("\n", " ");
                        var reg = new Regex(@"\s+");
                        var mt = reg.Matches(rText);
                        int cp = 0;
                        if (mt.Count > 0)
                            cp = mt[0].Index + mt[0].Length;
                        else cp = rText.Length;
                        caretPos += cp;
                    }
                    else caretPos++;
                }
                if (key == System.Windows.Forms.Keys.Up)
                {
                    if (ctrl)
                    {
                        String lText = data.text.Substring(0, caretPos);
                        var reg = new Regex("[^\n]\n\n+[^\n]", RegexOptions.Multiline);
                        var mt = reg.Matches(lText);
                        int cp = 0;
                        if (mt.Count > 0)
                            cp = mt[mt.Count - 1].Index + mt[mt.Count - 1].Length-1;
                        if (cp == caretPos)
                        {
                            if (mt.Count > 1)
                                cp = mt[mt.Count - 2].Index + mt[mt.Count - 2].Length;
                            else
                                cp = 0;
                        }
                        caretPos = cp;
                    }
                    else
                    {
                        int lineNum, linePos;
                        UTextInfo ti = lastTextInfo;
                        FindMyLine(caretPos, ti.lineLengths, out lineNum, out linePos);
                        if (lineNum != 0) // cant go up there!
                        {
                            int prevLine = 0;
                            for (int i = 0; i < lineNum - 1; i++)
                                prevLine += ti.lineLengths[i];
                            int prevLineLen = ti.lineLengths[lineNum - 1] - ti.lineNewLineLength[lineNum - 1];
                            if (linePos > prevLineLen) linePos = prevLineLen;
                            caretPos = prevLine + linePos;
                        }
                    }
                }
                if (key == System.Windows.Forms.Keys.Down)
                {
                    if (ctrl)
                    {
                        String rText = data.text.Substring(caretPos, data.text.Length - caretPos);
                        var reg = new Regex("[^\n]\n\n+[^\n]", RegexOptions.Multiline);
                        var mt = reg.Matches(rText);
                        int cp = 0;
                        if (mt.Count > 0)
                            cp = mt[0].Index + mt[0].Length-1;
                        else cp = rText.Length;
                        caretPos += cp;
                    }
                    else
                    {
                        int lineNum, linePos;
                        UTextInfo ti = lastTextInfo;
                        FindMyLine(caretPos, ti.lineLengths, out lineNum, out linePos);
                        if (lineNum != ti.numLines - 1) // cant go down there!
                        {
                            int nextLine = 0;
                            for (int i = 0; i < lineNum + 1; i++)
                                nextLine += ti.lineLengths[i];
                            int nextLineLen = ti.lineLengths[lineNum + 1] - ti.lineNewLineLength[lineNum + 1];
                            if (linePos > nextLineLen) linePos = nextLineLen;
                            caretPos = nextLine + linePos;
                        }
                    }
                }
                if (key == System.Windows.Forms.Keys.End)
                {
                    if (ctrl) caretPos = data.text.Length;
                    else
                    {

                        int lineNum, linePos;
                        UTextInfo ti = lastTextInfo;
                        FindMyLine(caretPos, ti.lineLengths, out lineNum, out linePos);
                        int cp = caretPos + ti.lineLengths[lineNum] - linePos - 1;

                        char c;
                        while ((c = data.text[cp]) == '\r' || c == '\n')
                            cp--;

                        int el = WrappedEndLine(ti,lineNum) ? 0 : 1;
                        caretPos = cp + el;
                    }
                }
                if (key == System.Windows.Forms.Keys.Home)
                {
                    if (ctrl) caretPos = 0;
                    else
                    {

                        int lineNum, linePos;
                        UTextInfo ti = lastTextInfo;
                        FindMyLine(caretPos, ti.lineLengths, out lineNum, out linePos);
                        caretPos -= linePos;
                    }
                }
        }

        public void KeyUp(System.Windows.Forms.Keys key)
        {
            MKeys(key, false);
        }

        void GetLineRange(out int start, out int end)
        {
            // Get line between \r or and \n
            start = end = caretPos == data.text.Length?caretPos-1:caretPos;
            char c = 'a';
            while (start > 0 && ((c = data.text[start]) == '\r' || c == '\n')) start--;
            while (start > 0 && ((c = data.text[start]) != '\r' && c != '\n')) start--;
            if (c == '\r' || c == '\n') start++;
            while (end < data.text.Length && ((c = data.text[end]) != '\r' && c != '\n')) end++;
            if (end == data.text.Length && start > 0)
            {
                start--;
                while (start > 0 && ((c = data.text[start]) == '\r' || c == '\n')) start--;
                start++;
            }
            // now extend the line by a single "\r" or "\n" or "\r\n"
            if (end < data.text.Length)
            {
                if (c == '\n') end++;
                else if (end + 1 < data.text.Length)
                {
                    if (c == '\r')
                    {
                        if (data.text[end + 1] == '\n')
                            end += 2;
                        else end++;
                    }
                }
                else if (c == '\r') end++;
            }
        }

        bool WrappedEndLine(UTextInfo ti, int lineNum)
        {
            if (ti.lineNewLineLength[lineNum] == 0)
            {
                // could be last line, could be wrapped
                if (ti.numLines == (lineNum + 1)) return false;
                else return true;
            }
            return false;
        }

        void FindMyLine(int myPos, int[] linelens, out int lineNum, out int linePos)
        {
            int myLineStart = 0,myLineNum;
            for (myLineNum = 0; myLineNum < linelens.Length; myLineNum++)
            {
                myLineStart += linelens[myLineNum];
                if (myLineStart > myPos)
                {
                    myLineStart -= linelens[myLineNum];
                    break;
                } 
            }
            if (myLineNum == linelens.Length)
            {
                lineNum = myLineNum - 1;
                linePos = linelens[lineNum];
            }
            else
            {
                lineNum = myLineNum;
                linePos = myPos - myLineStart;
            }
        }


        // case 1
        // 
        // ab|cdd\r
        // cd
        //
        // pos = 2
        // when <=0: -4

        // case 2
        // 
        // 1233\r
        // ab|cdd\r
        // cd
        //
        // pos = 7
        // when <=0: -4

        // case 3
        // 
        // 1233\r
        // abcdd|\r
        // cd
        //
        // pos = 10
        // when <=0: -1

        String BoundSubString(String s, int i1, int i2)
        {
            int start = i1 > i2 ? i2 : i1;
            int length = i1 > i2 ? i1 - i2 : i2 - i1;
            String ss = s.Substring(start, length);
            return ss;
        }

        public override void KeyPress(char c)
        {
                // FIXME unprintable chars
                if (!FocusManager.FocusGet(this)) return;
                if (ctrl || alt || win) return;
                if (c != '\b')
                    if ((c != '\r' && c != '\n') || layout != LayoutStyle.OneLine)
                    {
                        String cc = c == '\r' ? "\n" : "" + c;
                        ReplaceSelectionWithText(cc);
                    }
        }
        void ReplaceSelectionWithText(String text)
        {
            int st, en;

            bool ord = caretPos > shiftOrigin;
            st = ord ? shiftOrigin : caretPos;
            en = ord ? caretPos : shiftOrigin;

            String nled = text.Replace("\r\n", "\n").Replace("\r", "\n");
            String foroneline = nled;
            if (this.layout == LayoutStyle.OneLine && nled.Contains("\n"))
                foroneline = nled.Substring(0, nled.IndexOf("\n"));

            data.text = data.text.Substring(0, st) + foroneline + data.text.Substring(en);

            _caretPos = st + text.Length;
            shiftOrigin = caretPos;
            UpdateTextLayout();
        }
        public override void MouseMove(Point location, bool inComponent, bool amClipped)
        {
            base.MouseMove(location, inComponent, amClipped);
            runNextRender.Enqueue(new Action<IDraw>(rt =>
            {
                if (mouseSelect && inComponent && !amClipped)
                {
                    Point tfPoint = new Point(location.X - Location.X + roX, location.Y - Location.Y + roY);
                    UTextHitInfo htInfo = rt.uDraw.HitPoint(tfPoint, data);
                    int extra = 0;
                    if (htInfo.charPos == data.text.Length - 1 && htInfo.leading) extra++;
                    caretPos = htInfo.charPos + extra;
                }
            }));
        }
        bool mouseSelect = false;
        public override void MouseUpDown(Point location, MouseButton mb, ButtonState mbs, bool inComponent, bool amClipped)
        {
            runNextRender.Enqueue(new Action<IDraw>(rt =>
            {
                if (mbs == ButtonState.DOWN)
                {
                    if (inComponent && !amClipped)
                    {
                        Point tfPoint = new Point(location.X - Location.X + roX, location.Y - Location.Y + roY);
                        var hti = rt.uDraw.HitPoint(tfPoint, data);
                        int extra = 0;
                        if (hti.charPos == data.text.Length - 1 && hti.leading) extra++;
                        caretPos = hti.charPos + extra;
                        FocusManager.FocusSet(this, inComponent);
                        mouseSelect = true;
                    }
                }
                else if (mbs == ButtonState.UP && mouseSelect)
                {
                    mouseSelect = false;
                }
            }));
        }
    }
}
