using System;
using System.Windows.Forms;
using SysRect = System.Drawing.Rectangle;

namespace NoForms.Controls.Templates
{
    public abstract class ScrollableContainer : Container
    {
        System.Drawing.Point transform = new System.Drawing.Point(0, 0);
        public override void DrawBase<RenderType>(RenderType renderArgument)
        {
            var trans = transform;
            Util.Set2DTransform(renderArgument, trans);
            base.DrawBase<RenderType>(renderArgument);
            Util.Set2DTransform(renderArgument, System.Drawing.Point.Empty);
        }
    }
}
