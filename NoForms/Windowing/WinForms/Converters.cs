﻿using System;
using System.Collections.Generic;
using System.Text;
using c = System.Windows.Forms.Cursors;

namespace NoForms.Windowing.WinForms
{
    class Converters
    {
        public static System.Windows.Forms.Cursor Translate(NoForms.Common.Cursors cur)
        {
            switch (cur)
            {
                case NoForms.Common.Cursors.AppStarting: return c.AppStarting;
                case NoForms.Common.Cursors.Arrow: return c.Arrow;
                case NoForms.Common.Cursors.Cross: return c.Cross;
                default:
                case NoForms.Common.Cursors.Default: return c.Default;
                case NoForms.Common.Cursors.Hand: return c.Hand;
                case NoForms.Common.Cursors.Help: return c.Help;
                case NoForms.Common.Cursors.HSplit: return c.HSplit;
                case NoForms.Common.Cursors.IBeam: return c.IBeam;
                case NoForms.Common.Cursors.No: return c.No;
                case NoForms.Common.Cursors.NoMove2D: return c.NoMove2D;
                case NoForms.Common.Cursors.NoMoveHoriz: return c.NoMoveHoriz;
                case NoForms.Common.Cursors.NoMoveVert: return c.NoMoveVert;
                case NoForms.Common.Cursors.PanEast: return c.PanEast;
                case NoForms.Common.Cursors.PanNE: return c.PanNE;
                case NoForms.Common.Cursors.PanNorth: return c.PanNorth;
                case NoForms.Common.Cursors.PanNW: return c.PanNW;
                case NoForms.Common.Cursors.PanSE: return c.PanSE;
                case NoForms.Common.Cursors.PanSouth: return c.PanSouth;
                case NoForms.Common.Cursors.PanSW: return c.PanSW;
                case NoForms.Common.Cursors.PanWest: return c.PanWest;
                case NoForms.Common.Cursors.SizeAll: return c.SizeAll;
                case NoForms.Common.Cursors.SizeNESW: return c.SizeNESW;
                case NoForms.Common.Cursors.SizeNS: return c.SizeNS;
                case NoForms.Common.Cursors.SizeNWSE: return c.SizeNWSE;
                case NoForms.Common.Cursors.SizeWE: return c.SizeWE;
                case NoForms.Common.Cursors.UpArrow: return c.UpArrow;
                case NoForms.Common.Cursors.VSplit: return c.VSplit;
                case NoForms.Common.Cursors.WaitCursor: return c.WaitCursor;
            }
        }
        public static NoForms.Common.Cursors Translate(System.Windows.Forms.Cursor cur)
        {
            if (cur == c.AppStarting) return NoForms.Common.Cursors.AppStarting;
            if (cur == c.Arrow) return NoForms.Common.Cursors.Arrow;
            if (cur == c.Cross) return NoForms.Common.Cursors.Cross;
            if (cur == c.Default) return NoForms.Common.Cursors.Default;
            if (cur == c.Hand) return NoForms.Common.Cursors.Hand;
            if (cur == c.Help) return NoForms.Common.Cursors.Help;
            if (cur == c.HSplit) return NoForms.Common.Cursors.HSplit;
            if (cur == c.IBeam) return NoForms.Common.Cursors.IBeam;
            if (cur == c.No) return NoForms.Common.Cursors.No;
            if (cur == c.NoMove2D) return NoForms.Common.Cursors.NoMove2D;
            if (cur == c.NoMoveHoriz) return NoForms.Common.Cursors.NoMoveHoriz;
            if (cur == c.NoMoveVert) return NoForms.Common.Cursors.NoMoveVert;
            if (cur == c.PanEast) return NoForms.Common.Cursors.PanEast;
            if (cur == c.PanNE) return NoForms.Common.Cursors.PanNE;
            if (cur == c.PanNorth) return NoForms.Common.Cursors.PanNorth;
            if (cur == c.PanNW) return NoForms.Common.Cursors.PanNW;
            if (cur == c.PanSE) return NoForms.Common.Cursors.PanSE;
            if (cur == c.PanSouth) return NoForms.Common.Cursors.PanSouth;
            if (cur == c.PanSW) return NoForms.Common.Cursors.PanSW;
            if (cur == c.PanWest) return NoForms.Common.Cursors.PanWest;
            if (cur == c.SizeAll) return NoForms.Common.Cursors.SizeAll;
            if (cur == c.SizeNESW) return NoForms.Common.Cursors.SizeNESW;
            if (cur == c.SizeNS) return NoForms.Common.Cursors.SizeNS;
            if (cur == c.SizeNWSE) return NoForms.Common.Cursors.SizeNWSE;
            if (cur == c.SizeWE) return NoForms.Common.Cursors.SizeWE;
            if (cur == c.UpArrow) return NoForms.Common.Cursors.UpArrow;
            if (cur == c.VSplit) return NoForms.Common.Cursors.VSplit;
            if (cur == c.WaitCursor) return NoForms.Common.Cursors.WaitCursor;
            return NoForms.Common.Cursors.Default;
        }
    }
}
