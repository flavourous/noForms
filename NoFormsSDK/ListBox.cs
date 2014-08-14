using System;
using System.Collections.Generic;
using NoForms;
using NoForms.Renderers;
using Common;
using NoForms.ComponentBase;

namespace NoFormsSDK
{
    public class ListBox : BasicContainer
    {
        public override void Draw(IDraw renderArgument)
        {
            PoisitionItems();
            renderArgument.uDraw.FillRectangle(DisplayRectangle, brushy2);
            // so that the box overdraws the childrens
            renderArgument.uDraw.DrawRectangle(DisplayRectangle.Inflated(new Thickness(-.5f)), brushy1, strokey);
        }
        UBrush brushy1 = new USolidBrush() { color = new Color(0) };
        UBrush brushy2 = new USolidBrush() { color = new Color(0.98f) };
        UStroke strokey = new UStroke();

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
            lb.textData.halign = UHAlign.Left;
            lb.textData.valign = UVAlign.Middle;
            lb.autosizeY = true;
            lb.clicked += new VoidAction(() =>
            {
                selected = myIndex;
                if (selectionChanged != null)
                    selectionChanged(selected);
            });
            lb.MouseHover += new Action<bool>(hover => lb.background.color = hover ? new Color(1, .5f, .5f, 1) : new Color(1));
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
