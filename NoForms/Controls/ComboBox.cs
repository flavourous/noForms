using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NoForms.Renderers;

namespace NoForms.Controls
{
    public class ComboBox : Templates.Component
    {
        
        ListBox lb;
        Scribble dropArrowThing = new Scribble();

        public float dropLength { get { return lb.Size.height; } set { lb.Size = new Size(Size.width,value); } }

        public ComboBox()
        {
            lb = new ListBox();
            dropLength = 50;
            lb.selectionChanged += new Action<int>(lb_selectionChanged);
            lb.breakForCombo = true;
            components.Add(lb);
            lb.visible = false;

            // add a scribble
            dropArrowThing.draw += new Scribble.scribble(dropArrowThing_draw);
            dropArrowThing.Clicked += new Scribble.ClickDelegate(dropArrowThing_Clicked);
            components.Add(dropArrowThing);

            LocationChanged += new Action<Point>(pt => ComboBox_SizeChanged(Size));
            SizeChanged += new Action<Size>(ComboBox_SizeChanged);
            ComboBox_SizeChanged(Size);
        }

        public event Action<int> selectionChanged;
        void lb_selectionChanged(int obj)
        {
            lb.visible = false;
            _selectedOption = obj;
            if (selectionChanged != null)
                selectionChanged(obj);
        }

        void dropArrowThing_Clicked(Point loc)
        {
            ComboBox_SizeChanged(Size);
            lb.visible = true;
        }

        void dropArrowThing_draw(IUnifiedDraw ud, USolidBrush brsh, UStroke strk)
        {
            var ddr = dropArrowThing.DisplayRectangle;
            Rectangle rr = ddr.Inflated(-1f);
            float rad = (float)ddr.Size.height / 4f;

            brsh.color = new NoForms.Color(0.6f);
            ud.FillRoundedRectangle(rr, rad, rad, brsh);
            brsh.color = new NoForms.Color(0.7f);
            rr = ddr.Inflated(-.5f);
            strk.strokeWidth = 1f;
            ud.DrawRoundedRectangle(rr, rad, rad, brsh, strk);
            brsh.color = new NoForms.Color(0);
            var drw = ddr.width;
            var drh = ddr.height;
            var drt = ddr.top;
            var drl = ddr.left;
            var p1 = new Point(drl + drw / 4f, drt + drh / 3f);
            var p2 = new Point(drl + 2f * drw / 4f, drt + 2f * drh / 3f);
            var p3 = new Point(drl + 3f * drw / 4f, drt + drh / 3f);
            strk.strokeWidth = 1.5f;
            ud.DrawLine(p1, p2, brsh, strk);
            ud.DrawLine(p2, p3, brsh, strk);
        }


        int textPad = 3;
        void ComboBox_SizeChanged(Size obj)
        {
            dropArrowThing.Size = new Size(Size.height - 2, Size.height - 2);
            dropArrowThing.Location = new Point(Size.width -1 - Size.height +2, 1);
            lb.Location = new Point(0, 1 - lb.Size.height);
            lb.Size = new Size(Size.width, lb.Size.height);
            selectyTexty.height = DisplayRectangle.height - textPad*2;
            selectyTexty.width = DisplayRectangle.width - Size.height - textPad * 2;
        }

        public override void MouseUpDown(System.Windows.Forms.MouseEventArgs mea, MouseButtonState mbs, bool inComponent, bool amClipped)
        {
            if (!Util.CursorInRect(lb.DisplayRectangle, Util.GetTopLevelLocation(lb)))
                lb.visible = false;
            base.MouseUpDown(mea, mbs, inComponent, amClipped);
        }

        USolidBrush back = new USolidBrush() { color = new Color(0.8f) };
        USolidBrush edge = new USolidBrush() { color = new Color(0f) };
        UStroke edgeStroke = new UStroke();
        UText selectyTexty = new UText("", UHAlign.Left, UVAlign.Middle, false, 0, 0)
        {
            font = new UFont("Arial", 12f, false, false)
        };
        public override void Draw(IRenderType ra) 
        {
            // Draw bg
            ra.uDraw.FillRectangle(DisplayRectangle, back);
            ra.uDraw.DrawRectangle(DisplayRectangle.Inflated(-.5f), edge, edgeStroke);
            if (SelectionOptions.Count > 0) 
            {
                Point tp = new Point(DisplayRectangle.left + textPad, DisplayRectangle.top + textPad);
                selectyTexty.text = SelectionOptions.Count > _selectedOption ? SelectionOptions[_selectedOption] : "";
                ra.uDraw.DrawText(selectyTexty, tp, edge, UTextDrawOptions.Clip,false);
            }
            
            foreach (IComponent c in components)
                if (c.visible) c.DrawBase(ra);
        }
        
        // Model
        List<String> SelectionOptions = new List<string>();
        public void AddItem(String s)
        {
            SelectionOptions.Add(s);
            lb.AddItem(s);
        }
        public void RemoveItem(int i)
        {
            SelectionOptions.RemoveAt(i);
            lb.RemoveItem(i);
            if (i <= selectedOption) selectedOption--;
        }
        public String this[int i]
        {
            get { return SelectionOptions[i]; }
        }
        public int Count
        {
            get { return SelectionOptions.Count; }
        }
        public String selectedText
        {
            get { return SelectionOptions[_selectedOption]; }
        }
        public bool Contains(String s)
        {
            return SelectionOptions.Contains(s);
        }
        int _selectedOption = 0;
        public int selectedOption
        {
            get { return _selectedOption; }
            set { _selectedOption = value > SelectionOptions.Count ? _selectedOption : value; }
        }
        public void SelectOption(String opt)
        {
            int idx = _selectedOption;
            if ((idx = SelectionOptions.IndexOf(opt)) > -1)
                _selectedOption = idx;
        }
    }
}
