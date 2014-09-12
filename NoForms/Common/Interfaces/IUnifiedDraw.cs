using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NoForms.Common;

namespace NoForms.Renderers
{
    public interface IUnifiedDraw // This is a combined replacement for 2d2RenderTarget, drawing.graphics etc
    {
        // Render tools
        void PushAxisAlignedClip(Rectangle clipRect, bool ignoreRenderOffset);
        void PopAxisAlignedClip();
        void SetRenderOffset(Point renderOffset);

        // Drawing Methods
        void FillPath(UPath path, UBrush brush);
        void DrawPath(UPath path, UBrush brush, UStroke stroke);
        void DrawBitmap(UBitmap bitmap, float opacity, UInterp interp, Rectangle source, Rectangle destination);
        void FillEllipse(Point center, float radiusX, float radiusY, UBrush brush);
        void DrawEllipse(Point center, float radiusX, float radiusY, UBrush brush, UStroke stroke);
        void DrawLine(Point start, Point end, UBrush brush, UStroke stroke);
        void DrawRectangle(Rectangle rect, UBrush brush, UStroke stroke);
        void FillRectangle(Rectangle rect, UBrush brush);
        void DrawRoundedRectangle(Rectangle rect, float radX, float radY, UBrush brush, UStroke stroke);
        void FillRoundedRectangle(Rectangle rect, float radX, float radY, UBrush brush);
        void DrawText(UText textObject, Point location, UBrush defBrush, UTextDrawOptions opt, bool clientRendering);

        // Info Methods
        UTextHitInfo HitPoint(Point hitPoint, UText text);
        Point HitText(int pos, bool trailing, UText text);
        IEnumerable<Rectangle> HitTextRange(int start, int length, Point offset, UText text);
        UTextInfo GetTextInfo(UText text);
    }

    
    // FIXME using internal setter for the below is hackable, although a useful user guide.

    /// <summary>
    /// Blank interface, the backing to renderes are these only.
    /// </summary>
    public interface IRenderElements { } // Mainly used because graphics/rendertarget can change instance Facout drawing implimentors needing to know. eg resize.
 
}