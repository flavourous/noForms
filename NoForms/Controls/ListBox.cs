using System;
using System.Collections.Generic;
using SharpDX.Direct2D1;
using dwfact = SharpDX.DirectWrite.Factory;

namespace NoForms.Controls
{
    public class ListBox : Templates.Container
    {
        public dwfact gotdwFactory;
        public ListBox(dwfact iNeedTextPls)
        {
            gotdwFactory = iNeedTextPls;
        }

        bool d2dinit = false;
        public override void DrawBase<RenderType>(RenderType renderArgument)
        {
            if (renderArgument is RenderTarget)
            {
                if (!d2dinit) Init(renderArgument as RenderTarget);
                Draw(renderArgument as RenderTarget);
            }
            else throw new Exception("NO THIS OBLY D2d go way now.");
            base.DrawBase<RenderType>(renderArgument);
            if (renderArgument is RenderTarget)
            {
                scb.Color = new NoForms.Color(0);
                (renderArgument as RenderTarget).DrawRectangle(DisplayRectangle.Inflated(-.5f), scb);
            }
        }
        SolidColorBrush scb;
        void Init(RenderTarget rt)
        {
            scb = new SolidColorBrush(rt, new NoForms.Color(0));
            d2dinit = true;
        }

        internal bool breakClip = false;
        void Draw(RenderTarget rt)
        {
            // Draw bg
            scb.Color = new NoForms.Color(0.98f);
            rt.FillRectangle(DisplayRectangle, scb);
            

            float yt = 1;
            foreach (TextLabel tl in components)
            {
                if (tl == null) continue;
                tl.Size = new Size(Size.width-2, tl.Size.height);
                tl.Location = new Point(1, yt);
                yt += tl.Size.height+1;
            }
        }
        // Model
        List<String> SelectionOptions = new List<string>();
        public event Action<int> selectionChanged;
        public void AddItem(String s)
        {
            int myIndex = SelectionOptions.Count;
            SelectionOptions.Add(s);
            var lb = new TextLabel(gotdwFactory);
            lb.text = s;
            lb.textAlign = new Align() { horizontal = HAlign.left, vertical = VAlign.middle };
            lb.Size = new Size(Size.width, lb.getLineHeight());
            lb.clicked += new System.Windows.Forms.MethodInvoker(() =>
            {
                selected = myIndex;
                if (selectionChanged != null)
                    selectionChanged(selected);
            });
            lb.MouseHover += new Action<bool>(hover => lb.backColor = hover ? new NoForms.Color(1, .5f, .5f, 1) : new NoForms.Color(1));
            components.Add(lb);
        }

        
        public void RemoveItem(int i)
        {
            SelectionOptions.RemoveAt(i);
            components.RemoveAt(i);
        }
        public String this[int i]
        {
            get { return SelectionOptions[i]; }
        }
        public int Count
        {
            get { return SelectionOptions.Count; }
        }
        public int selected = 0;

        
    }
}
