using System;
using System.Collections.Generic;
using System.Text;
using c = System.Windows.Forms.Cursors;

namespace NoForms.Windowing.WinForms
{
    class Converters
    {
        public static System.Windows.Forms.Cursor Translate(Common.Cursors cur)
        {
            switch (cur)
            {
                case Common.Cursors.AppStarting: return c.AppStarting;
                case Common.Cursors.Arrow: return c.Arrow;
                case Common.Cursors.Cross: return c.Cross;
                default:
                case Common.Cursors.Default: return c.Default;
                case Common.Cursors.Hand: return c.Hand;
                case Common.Cursors.Help: return c.Help;
                case Common.Cursors.HSplit: return c.HSplit;
                case Common.Cursors.IBeam: return c.IBeam;
                case Common.Cursors.No: return c.No;
                case Common.Cursors.NoMove2D: return c.NoMove2D;
                case Common.Cursors.NoMoveHoriz: return c.NoMoveHoriz;
                case Common.Cursors.NoMoveVert: return c.NoMoveVert;
                case Common.Cursors.PanEast: return c.PanEast;
                case Common.Cursors.PanNE: return c.PanNE;
                case Common.Cursors.PanNorth: return c.PanNorth;
                case Common.Cursors.PanNW: return c.PanNW;
                case Common.Cursors.PanSE: return c.PanSE;
                case Common.Cursors.PanSouth: return c.PanSouth;
                case Common.Cursors.PanSW: return c.PanSW;
                case Common.Cursors.PanWest: return c.PanWest;
                case Common.Cursors.SizeAll: return c.SizeAll;
                case Common.Cursors.SizeNESW: return c.SizeNESW;
                case Common.Cursors.SizeNS: return c.SizeNS;
                case Common.Cursors.SizeNWSE: return c.SizeNWSE;
                case Common.Cursors.SizeWE: return c.SizeWE;
                case Common.Cursors.UpArrow: return c.UpArrow;
                case Common.Cursors.VSplit: return c.VSplit;
                case Common.Cursors.WaitCursor: return c.WaitCursor;
            }
        }
        public static Common.Cursors Translate(System.Windows.Forms.Cursor cur)
        {
            if (cur == c.AppStarting) return Common.Cursors.AppStarting;
            if (cur == c.Arrow) return Common.Cursors.Arrow;
            if (cur == c.Cross) return Common.Cursors.Cross;
            if (cur == c.Default) return Common.Cursors.Default;
            if (cur == c.Hand) return Common.Cursors.Hand;
            if (cur == c.Help) return Common.Cursors.Help;
            if (cur == c.HSplit) return Common.Cursors.HSplit;
            if (cur == c.IBeam) return Common.Cursors.IBeam;
            if (cur == c.No) return Common.Cursors.No;
            if (cur == c.NoMove2D) return Common.Cursors.NoMove2D;
            if (cur == c.NoMoveHoriz) return Common.Cursors.NoMoveHoriz;
            if (cur == c.NoMoveVert) return Common.Cursors.NoMoveVert;
            if (cur == c.PanEast) return Common.Cursors.PanEast;
            if (cur == c.PanNE) return Common.Cursors.PanNE;
            if (cur == c.PanNorth) return Common.Cursors.PanNorth;
            if (cur == c.PanNW) return Common.Cursors.PanNW;
            if (cur == c.PanSE) return Common.Cursors.PanSE;
            if (cur == c.PanSouth) return Common.Cursors.PanSouth;
            if (cur == c.PanSW) return Common.Cursors.PanSW;
            if (cur == c.PanWest) return Common.Cursors.PanWest;
            if (cur == c.SizeAll) return Common.Cursors.SizeAll;
            if (cur == c.SizeNESW) return Common.Cursors.SizeNESW;
            if (cur == c.SizeNS) return Common.Cursors.SizeNS;
            if (cur == c.SizeNWSE) return Common.Cursors.SizeNWSE;
            if (cur == c.SizeWE) return Common.Cursors.SizeWE;
            if (cur == c.UpArrow) return Common.Cursors.UpArrow;
            if (cur == c.VSplit) return Common.Cursors.VSplit;
            if (cur == c.WaitCursor) return Common.Cursors.WaitCursor;
            return Common.Cursors.Default;
        }
    }
}
