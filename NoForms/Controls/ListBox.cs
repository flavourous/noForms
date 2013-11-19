using System;
using System.Collections.Generic;
using NoForms.Renderers;

namespace NoForms.Controls
{
    public class ListBox : Templates.Container
    {
        public override void DrawBase(IRenderType renderArgument)
        {
            if (breakForCombo) UnClipAll(renderArgument);
            PoisitionItems();
            renderArgument.uDraw.FillRectangle(DisplayRectangle, brushy2);
            base.DrawBase(renderArgument);
            // so that the box overdraws the childrens
            renderArgument.uDraw.DrawRectangle(DisplayRectangle.Inflated(-.5f), brushy1, strokey);
            if (breakForCombo) ReClipAll(renderArgument);
        }
        UBrush brushy1 = new USolidBrush() { color = new Color(0) };
        UBrush brushy2 = new USolidBrush() { color = new Color(0.98f) };
        UStroke strokey = new UStroke();

        internal bool breakForCombo = false;
        public ListBox() : base()
        {
            doClip = false;
        }

        void PoisitionItems()
        {
            float yt = 1;
            foreach (TextLabel tl in components)
            {
                if (tl == null) continue;
                tl.Size = new Size(Size.width - 2, tl.Size.height);
                tl.Location = new Point(1, yt);
                yt += tl.Size.height + 1;
            }
        }

        // Model
        List<String> SelectionOptions = new List<string>();
        public event Action<int> selectionChanged;
        public void AddItem(String s)
        {
            int myIndex = SelectionOptions.Count;
            SelectionOptions.Add(s);
            var lb = new TextLabel();
            lb.textData.text = s;
            lb.textData.wrapped = false;
            lb.textData.halign = UHAlign_Enum.Left;
            lb.textData.valign = UVAlign_Enum.Middle;
            lb.autosizeY = true;
            lb.clicked += new System.Windows.Forms.MethodInvoker(() =>
            {
                selected = myIndex;
                if (selectionChanged != null)
                    selectionChanged(selected);
            });
            lb.MouseHover += new Action<bool>(hover => lb.background.color = hover ? new NoForms.Color(1, .5f, .5f, 1) : new NoForms.Color(1));
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
