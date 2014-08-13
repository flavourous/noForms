using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NoForms.Renderers;
using Common;

namespace NoForms.Controls
{
    public enum ComboBoxDirection { None, Above, Below, MostSpace, LeastSpace };
    public class ComboBox : Abstract.BasicContainer
    {
        ListBox lb;
        Scribble dropArrowThing = new Scribble();

        public ComboBoxDirection dropDirection = ComboBoxDirection.MostSpace;
        public float dropLength { get { return lb.Size.height; } set { lb.Size = new Size(Size.width, value); OnLocationChanged(); } }

        IRender dropRenderer = null;
        NoForm ddf;
        void recreateddf()
        {
            ddf = new NoForm(dropRenderer, new CreateOptions(false)) { background = new USolidBrush() { color = new Color(1, 1, 0, 0) } };
            ddf.components.Add(lb);
        }
        public ComboBox(IRender dropRenderer = null)
        {
            if (dropRenderer != null) this.dropRenderer = dropRenderer;
            lb = new ListBox();
            dropLength = 50;
            lb.SizeChanged += s => { if (ddf != null) ddf.Size = s; };

            // add a scribble
            dropArrowThing.draw += new Scribble.scribble(dropArrowThing_draw);
            dropArrowThing.Clicked += new Scribble.ClickDelegate(dropArrowThing_Clicked);
            components.Add(dropArrowThing);

            lb.selectionChanged += new Action<int>(lb_selectionChanged);
            LocationChanged += new Action<Point>(pt => ComboBox_SizeChanged(Size));
            SizeChanged += new Action<Size>(ComboBox_SizeChanged);
            ComboBox_SizeChanged(Size);
        }

        // Hide the dropbox
        void hideLb()
        {
            if (shown)
            {
                shown = false;
                if (ddf == null)
                    lb.Parent.components.Remove(lb);
                else ddf.window.Close();
            }
        }

        public event Action<int> selectionChanged;
        void lb_selectionChanged(int obj)
        {
            hideLb();
            _selectedOption = obj;
            if (selectionChanged != null)
                selectionChanged(obj);
        }
        
        // Show the dropbox
        bool shown = false;
        IComponent tlc;
        void dropArrowThing_Clicked(Point loc)
        {
            tlc = Util.GetTopLevelComponent(this);
            if (dropRenderer != null) recreateddf();

            ComboBox_SizeChanged(Size);
            if (ddf == null)
                tlc.components.Add(lb);
            else
            {
                recreateddf();
                ddf.window.Show();
            }
            shown = true;
            OnLocationChanged();
        }

        protected override void OnLocationChanged()
        {
            base.OnLocationChanged();

            tlc = Util.GetTopLevelComponent(this);
            float abo = DisplayRectangle.top - lb.Size.height + 1f;
            float bel = DisplayRectangle.bottom - 1f;
            float abo_spc = DisplayRectangle.top;
            float bel_spc = tlc.Size.height - DisplayRectangle.bottom;

            float toppy = 0;
            switch (dropDirection)
            {
                case ComboBoxDirection.Above:
                    toppy = abo;
                    break;
                case ComboBoxDirection.Below:
                    toppy = bel;
                    break;
                case ComboBoxDirection.MostSpace:
                    toppy = abo_spc > bel_spc ? abo : bel;
                    break;
                case ComboBoxDirection.LeastSpace:
                    toppy = abo_spc < bel_spc ? abo : bel;
                    break;
            }

            if (dropRenderer == null) 
            {
                lb.Location = new Point(DisplayRectangle.left, toppy);
            }
            if( ddf != null)
            {
                var ofp = tlc.Location + Location;
                ddf.Location = new Point(ofp.X, tlc.Location.Y + toppy);
            }
        }

        void dropArrowThing_draw(IUnifiedDraw ud, USolidBrush brsh, UStroke strk)
        {
            var ddr = dropArrowThing.DisplayRectangle;
            Rectangle rr = ddr.Inflated(new Thickness(-1f));
            float rad = (float)ddr.Size.height / 4f;

            brsh.color = new Color(0.6f);
            ud.FillRoundedRectangle(rr, rad, rad, brsh);
            brsh.color = new Color(0.7f);
            rr = ddr.Inflated(new Thickness(-.5f));
            strk.strokeWidth = 1f;
            ud.DrawRoundedRectangle(rr, rad, rad, brsh, strk);
            brsh.color = new Color(0);
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
            lb.Size = new Size(Size.width, lb.Size.height);
            selectyTexty.height = DisplayRectangle.height - textPad*2;
            selectyTexty.width = DisplayRectangle.width - Size.height - textPad * 2;
        }



        public override void MouseUpDown(Point location, MouseButton mb, ButtonState bs, bool inComponent, bool amClipped)
        {
            if ((!lb.DisplayRectangle.Contains(location) && ddf == null) || (ddf != null))
                hideLb();
            base.MouseUpDown(location,mb, bs, inComponent, amClipped);
        }

        USolidBrush back = new USolidBrush() { color = new Color(0.8f) };
        USolidBrush edge = new USolidBrush() { color = new Color(0f) };
        UStroke edgeStroke = new UStroke();
        UText selectyTexty = new UText("", UHAlign.Left, UVAlign.Middle, false, 0, 0)
        {
            font = new UFont("Arial", 12f, false, false)
        };
        public override void Draw(IDraw ra)
        {
            // Draw bg
            ra.uDraw.FillRectangle(DisplayRectangle, back);
            ra.uDraw.DrawRectangle(DisplayRectangle.Inflated(new Thickness(-.5f)), edge, edgeStroke);
            if (SelectionOptions.Count > 0)
            {
                Point tp = new Point(DisplayRectangle.left + textPad, DisplayRectangle.top + textPad);
                selectyTexty.text = SelectionOptions.Count > _selectedOption ? SelectionOptions[_selectedOption] : "";
                ra.uDraw.DrawText(selectyTexty, tp, edge, UTextDrawOptions.Clip, false);
            }
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
