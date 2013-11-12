using System;
using System.Windows.Forms;
using SysRect = System.Drawing.Rectangle;

namespace NoForms.Controls.Templates
{
    public abstract class ScrollableContainer : Container
    {
        System.Drawing.Point transform = new System.Drawing.Point(0, 0);
        public override void DrawBase(IRenderType renderArgument)
        {
            Util.Set2DTransform(renderArgument, transform);
            base.DrawBase(renderArgument);
            Util.Set2DTransform(renderArgument, System.Drawing.Point.Empty);
        }
    }
}
