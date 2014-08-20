using System;
using System.Runtime.InteropServices;

namespace NoForms
{
    public static class Win32Util
    {
        // w32 wrappers
        static public Size GetWindowSize(IntPtr hWnd)
        {
            RECT r = new RECT();
            GetWindowRect(hWnd, out r);
            return new Size(r.right - r.left, r.bottom - r.top);
        }
        static public Point GetWindowLocation(IntPtr hWnd)
        {
            RECT r = new RECT();
            GetWindowRect(hWnd, out r);
            return new Point(r.left, r.top);
        }
        static public void SetWindowSize(Size sz, IntPtr hWnd)
        {
            var loc = GetWindowLocation(hWnd);
            SetWindowPos(hWnd, IntPtr.Zero, loc.x, loc.y, sz.cx, sz.cy, SetWindowPosFlags.SHOWWINDOW);
        }
        static public void SetWindowLocation(Point loc, IntPtr hWnd)  
        {
            var sz = GetWindowSize(hWnd);
            SetWindowPos(hWnd, IntPtr.Zero, loc.x, loc.y, sz.cx, sz.cy, SetWindowPosFlags.SHOWWINDOW);
        }

        enum SetWindowPosFlags
        {
            NOSIZE = 0x0001,
            NOMOVE = 0x0002,
            NOZORDER = 0x0004,
            NOREDRAW = 0x0008,
            NOACTIVATE = 0x0010,
            DRAWFRAME = 0x0020,
            FRAMECHANGED = 0x0020,
            SHOWWINDOW = 0x0040,
            HIDEWINDOW = 0x0080,
            NOCOPYBITS = 0x0100,
            NOOWNERZORDER = 0x0200,
            NOREPOSITION = 0x0200,
            NOSENDCHANGING = 0x0400,
            DEFERERASE = 0x2000,
            ASYNCWINDOWPOS = 0x4000
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hIns, int X, int Y, int cx, int cy, SetWindowPosFlags flags);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        //gets information about the windows
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        //sets bigflags that control the windows styles
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst,
           [In()] ref Point pptDst, [In()] ref Size psize, IntPtr hdcSrc, [In()] ref Point pptSrc, uint crKey,
           [In()] ref BLENDFUNCTION pblend, uint dwFlags);

        [DllImport("user32.dll")]
        public extern static IntPtr GetDC(IntPtr handle);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;

            public BLENDFUNCTION(byte op, byte flags, byte alpha, byte format)
            {
                BlendOp = op;
                BlendFlags = flags;
                SourceConstantAlpha = alpha;
                AlphaFormat = format;
            }
        }

        public struct Point
        {
            public int x;
            public int y;

            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        public struct Size
        {
            public int cx;
            public int cy;

            public Size(int cx, int cy)
            {
                this.cx = cx;
                this.cy = cy;
            }
        }

        public const int GWL_EXSTYLE = (-20);
        public const int WS_EX_LAYERED = 0x80000;
        public const int AC_SRC_OVER = 0x00;
        public const int AC_SRC_ALPHA = 0x01;
        public const uint ULW_ALPHA = 2;
    }
}
