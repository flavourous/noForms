using System;
using System.Collections.Generic;
using System.Text;
using NoForms.Common;
using OTK = OpenTK;
using OpenTK.Graphics.OpenGL;

namespace NoForms.Renderers.OpenTK
{
    public class OpenTK_RenderElements : IRenderElements
    {
        public OpenTK_RenderElements(OTK.Graphics.IGraphicsContext gc)
        {
            graphicsContext = gc;
        }
        public OTK.Graphics.IGraphicsContext graphicsContext { get; internal set; }
    }
    public class OTKDraw : IUnifiedDraw
    {
        public void PushAxisAlignedClip(Rectangle clipRect, bool ignoreRenderOffset)
        {
            //throw new NotImplementedException();
        }

        public void PopAxisAlignedClip()
        {
            //throw new NotImplementedException();
        }

        public void SetRenderOffset(Point renderOffset)
        {
            throw new NotImplementedException();
        }

        public void FillPath(UPath path, UBrush brush)
        {
            throw new NotImplementedException();
        }

        public void DrawPath(UPath path, UBrush brush, UStroke stroke)
        {
            throw new NotImplementedException();
        }

        public void DrawBitmap(UBitmap bitmap, float opacity, UInterp interp, Rectangle source, Rectangle destination)
        {
            throw new NotImplementedException();
        }

        public void FillEllipse(Point center, float radiusX, float radiusY, UBrush brush)
        {
            throw new NotImplementedException();
        }

        public void DrawEllipse(Point center, float radiusX, float radiusY, UBrush brush, UStroke stroke)
        {
            throw new NotImplementedException();
        }

        public void DrawLine(Point start, Point end, UBrush brush, UStroke stroke)
        {
            //throw new NotImplementedException();
        }

        public void DrawRectangle(Rectangle rect, UBrush brush, UStroke stroke)
        {
            throw new NotImplementedException();
        }

        public void FillRectangle(Rectangle rect, UBrush brush)
        {
            SetBrush(brush);
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex2(rect.left, rect.top);
            GL.Vertex2(rect.right, rect.top);
            GL.Vertex2(rect.right, rect.bottom);
            GL.Vertex2(rect.left, rect.bottom);
            GL.End();
        }
        void SetBrush(UBrush brush)
        {
            if (brush is USolidBrush)
            {
                var usb = brush as USolidBrush;
                GL.Color4(usb.color.r, usb.color.g, usb.color.b, usb.color.a);
            }
        }

        public void DrawRoundedRectangle(Rectangle rect, float radX, float radY, UBrush brush, UStroke stroke)
        {
            throw new NotImplementedException();
        }

        public void FillRoundedRectangle(Rectangle rect, float radX, float radY, UBrush brush)
        {
            throw new NotImplementedException();
        }

        public void DrawText(UText textObject, Point location, UBrush defBrush, UTextDrawOptions opt, bool clientRendering)
        {
            throw new NotImplementedException();
        }

        public UTextHitInfo HitPoint(Point hitPoint, UText text)
        {
            throw new NotImplementedException();
        }

        public Point HitText(int pos, bool trailing, UText text)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Rectangle> HitTextRange(int start, int length, Point offset, UText text)
        {
            throw new NotImplementedException();
        }

        public UTextInfo GetTextInfo(UText text)
        {
            throw new NotImplementedException();
        }
    }
}
