using System;
using System.Collections.Generic;
using c = System.Windows.Forms.Cursors;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace NoForms.Common
{
    public class WFTr
    {
        static int[] iconSizes = { 16, 32, 48, 64, 128, 256, 512 };
        public static System.Drawing.Icon Translate(UBitmap[] frames)
        {
            System.Drawing.Bitmap[] sizedIcons = new System.Drawing.Bitmap[iconSizes.Length];
            List<System.Drawing.Bitmap> loaded = new List<System.Drawing.Bitmap>();
            foreach (var fr in frames) loaded.Add(Getit(fr));

            // Find correct sized ones
            for (int i = 0; i < iconSizes.Length; i++)
            {
                int isz = iconSizes[i];
                int idx = loaded.FindIndex(bm => bm.Width == isz && bm.Width == isz);
                if (idx > -1) sizedIcons[i] = loaded[idx];
            }

            //Bosh into a stream? :/
            MemoryStream ms = new MemoryStream();
            Bitmap firstbm = null;

            Encoder encoder = Encoder.SaveFlag;
            ImageCodecInfo encoderInfo = ImageCodecInfo.GetImageEncoders()[0]; // find correct one
            EncoderParameters encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(encoder, (long)EncoderValue.MultiFrame);

            foreach (var bm in loaded)
            {
                if (bm == null) continue;
                if (ms.Length == 0)
                {
                    firstbm = bm;
                    firstbm.Save(ms, encoderInfo, encoderParameters);
                    encoderParameters.Param[0] = new EncoderParameter(encoder, (long)EncoderValue.FrameDimensionPage);
                }
                else firstbm.SaveAdd(bm, encoderParameters);
            }
            encoderParameters.Param[0] = new EncoderParameter(encoder, (long)EncoderValue.Flush);
            if (ms.Length > 0) firstbm.SaveAdd(encoderParameters);

            return new Icon(ms);
        }
            

        static System.Drawing.Bitmap Getit(UBitmap bm)
        {
            using (var ms = new MemoryStream(bm.bitmapData ?? File.ReadAllBytes(bm.bitmapFile)))
                return new System.Drawing.Bitmap(ms);
        }
        public static System.Windows.Forms.Cursor Translate(Cursors cur)
        {
            switch (cur)
            {
                case Cursors.AppStarting: return c.AppStarting;
                case Cursors.Arrow: return c.Arrow;
                case Cursors.Cross: return c.Cross;
                default:
                case Cursors.Default: return c.Default;
                case Cursors.Hand: return c.Hand;
                case Cursors.Help: return c.Help;
                case Cursors.HSplit: return c.HSplit;
                case Cursors.IBeam: return c.IBeam;
                case Cursors.No: return c.No;
                case Cursors.NoMove2D: return c.NoMove2D;
                case Cursors.NoMoveHoriz: return c.NoMoveHoriz;
                case Cursors.NoMoveVert: return c.NoMoveVert;
                case Cursors.PanEast: return c.PanEast;
                case Cursors.PanNE: return c.PanNE;
                case Cursors.PanNorth: return c.PanNorth;
                case Cursors.PanNW: return c.PanNW;
                case Cursors.PanSE: return c.PanSE;
                case Cursors.PanSouth: return c.PanSouth;
                case Cursors.PanSW: return c.PanSW;
                case Cursors.PanWest: return c.PanWest;
                case Cursors.SizeAll: return c.SizeAll;
                case Cursors.SizeNESW: return c.SizeNESW;
                case Cursors.SizeNS: return c.SizeNS;
                case Cursors.SizeNWSE: return c.SizeNWSE;
                case Cursors.SizeWE: return c.SizeWE;
                case Cursors.UpArrow: return c.UpArrow;
                case Cursors.VSplit: return c.VSplit;
                case Cursors.WaitCursor: return c.WaitCursor;
            }
        }
        public static Cursors Translate(System.Windows.Forms.Cursor cur)
        {
            if (cur == c.AppStarting) return Cursors.AppStarting;
            if (cur == c.Arrow) return Cursors.Arrow;
            if (cur == c.Cross) return Cursors.Cross;
            if (cur == c.Default) return Cursors.Default;
            if (cur == c.Hand) return Cursors.Hand;
            if (cur == c.Help) return Cursors.Help;
            if (cur == c.HSplit) return Cursors.HSplit;
            if (cur == c.IBeam) return Cursors.IBeam;
            if (cur == c.No) return Cursors.No;
            if (cur == c.NoMove2D) return Cursors.NoMove2D;
            if (cur == c.NoMoveHoriz) return Cursors.NoMoveHoriz;
            if (cur == c.NoMoveVert) return Cursors.NoMoveVert;
            if (cur == c.PanEast) return Cursors.PanEast;
            if (cur == c.PanNE) return Cursors.PanNE;
            if (cur == c.PanNorth) return Cursors.PanNorth;
            if (cur == c.PanNW) return Cursors.PanNW;
            if (cur == c.PanSE) return Cursors.PanSE;
            if (cur == c.PanSouth) return Cursors.PanSouth;
            if (cur == c.PanSW) return Cursors.PanSW;
            if (cur == c.PanWest) return Cursors.PanWest;
            if (cur == c.SizeAll) return Cursors.SizeAll;
            if (cur == c.SizeNESW) return Cursors.SizeNESW;
            if (cur == c.SizeNS) return Cursors.SizeNS;
            if (cur == c.SizeNWSE) return Cursors.SizeNWSE;
            if (cur == c.SizeWE) return Cursors.SizeWE;
            if (cur == c.UpArrow) return Cursors.UpArrow;
            if (cur == c.VSplit) return Cursors.VSplit;
            if (cur == c.WaitCursor) return Cursors.WaitCursor;
            return Cursors.Default;
        }
        public static MouseButton Translate(System.Windows.Forms.MouseButtons mb)
        {
            // FIXME where da buttons?
            switch (mb)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    return MouseButton.LEFT;
                case System.Windows.Forms.MouseButtons.Middle:
                    break;
                case System.Windows.Forms.MouseButtons.None:
                    break;
                case System.Windows.Forms.MouseButtons.Right:
                    return MouseButton.RIGHT;
                case System.Windows.Forms.MouseButtons.XButton1:
                    break;
                case System.Windows.Forms.MouseButtons.XButton2:
                    break;
                default:
                    return MouseButton.NONE;
            }
            return MouseButton.NONE;
        }
    }
}
