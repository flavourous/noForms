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
            //throw new NotImplementedException();
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
            if (brush is USolidBrush)
            {
                var usb = brush as USolidBrush;
                GL.Color4(usb.color.r, usb.color.g, usb.color.b, usb.color.a);
                GL.Begin(PrimitiveType.Quads);
                GL.Vertex2(rect.left, rect.top);
                GL.Vertex2(rect.right, rect.top);
                GL.Vertex2(rect.right, rect.bottom);
                GL.Vertex2(rect.left, rect.bottom);
                GL.End();
            }
            if (brush is ULinearGradientBrush)
            {
                // 1) establish the angle between the two points
                // 2) determine the 2 crossings of the rectangle of the perpendicuars to the 
                //      line made by the two points at the two points (remember as a distance along the axis of the 2 lines)
                // 3) get the 4 points on the axis where it perpendicular crosses each corner. 2 idential for on axis with rect special case.
                //      by interpolation, find what the color corresponds to for these points (clamp)
                // 4) find on axis points bounded by rectangle crossing, exclude points outside of, draw quadstrips by crossings of each
                //      remaining point by finding rect crossings using discovered color for each pair. 
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
