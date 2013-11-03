using System;
using System.Runtime.InteropServices;

namespace NoForms
{
    public static class Win32
    {
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

        [DllImportAttribute("user32.dll")]
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
