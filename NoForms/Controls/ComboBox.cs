using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SharpDX.Direct2D1;
using dwfact = SharpDX.DirectWrite.Factory;

namespace NoForms.Controls
{
    public class ComboBox : Templates.Container
    {
        ListBox lb;
        Scribble dropArrowThing = new Scribble();
        dwfact gotdwFactory;
        public ComboBox(dwfact iNeedTextPls)
        {
            gotdwFactory = iNeedTextPls;
            lb = new ListBox(iNeedTextPls);
            lb.selectionChanged += new Action<int>(lb_selectionChanged);
            components.Add(lb);
            lb.visible = false;

            // add a scribble
            dropArrowThing.draw += new Scribble.scribble(dropArrowThing_draw);
            dropArrowThing.Clicked += new Scribble.ClickDelegate(dropArrowThing_Clicked);
            components.Add(dropArrowThing);

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

        void dropArrowThing_draw(RenderTarget rt, SolidColorBrush scb)
        {

            //scb.Color = new Color(0);
            //rt.FillRectangle(dropArrowThing.DisplayRectangle,scb);

            var ddr = dropArrowThing.DisplayRectangle;
            RoundedRectangle rr = new RoundedRectangle()
            {
                Rect = ddr.Inflated(-1f),
                RadiusX = (float)ddr.Size.height / 4f,
                RadiusY = (float)ddr.Size.height / 4f,
            };

            scb.Color = new NoForms.Color(0.6f);
            rt.FillRoundedRectangle(ref rr, scb);
            scb.Color = new NoForms.Color(0.7f);
            rr.Rect = ddr.Inflated(-.5f);
            rt.DrawRoundedRectangle(rr, scb, 1f);
            scb.Color = new NoForms.Color(0);
            var drw = ddr.width;
            var drh = ddr.height;
            var drt = ddr.top;
            var drl = ddr.left;
            var p1 = new SharpDX.DrawingPointF(drl + drw / 4f, drt + drh / 3f);
            var p2 = new SharpDX.DrawingPointF(drl + 2f * drw / 4f, drt + 2f * drh / 3f);
            var p3 = new SharpDX.DrawingPointF(drl + 3f * drw / 4f, drt + drh / 3f);
            rt.DrawLine(p1, p2, scb, 1.5f);
            rt.DrawLine(p2, p3, scb, 1.5f);
        }

        void ComboBox_SizeChanged(Size obj)
        {
            dropArrowThing.Size = new Size(Size.height - 2, Size.height - 2);
            dropArrowThing.Location = new Point(Size.width -1 - Size.height +2, 1);
            Rectangle lbdr = DisplayRectangle;
            float listTextHeight = lb.components.Count > 0 ? lb.components[0].DisplayRectangle.height * lb.components.Count : 0;
            lbdr.height = Math.Min(150, listTextHeight + 2 + lb.components.Count);
            lbdr.Location = new Point(lbdr.left, lbdr.top - lbdr.height + 1);
            lb.DisplayRectangle = lbdr;
        }

        public override void MouseUpDown(System.Windows.Forms.MouseEventArgs mea, MouseButtonState mbs, bool inComponent, bool amClipped)
        {
            if (!Util.CursorInRect(lb.DisplayRectangle, Util.GetTopLevelLocation(lb)))
                lb.visible = false;
            base.MouseUpDown(mea, mbs, inComponent, amClipped);
        }


        bool d2dinit = false;
        public override void DrawBase(IRenderType renderArgument) 
        {
            if (renderArgument is RenderTarget)
            {
                if (!d2dinit) Init(renderArgument as RenderTarget);
                Draw(renderArgument as RenderTarget);
            }
            else throw new Exception("NO THIS OBLY D2d go way now.");
            
            foreach (IComponent c in components)
                if (c.visible)
                    c.DrawBase<RenderType>(renderArgument);
        }
        SolidColorBrush scb;
        void Init(RenderTarget rt)
        {
            scb = new SolidColorBrush(rt, new NoForms.Color(0));
            d2dinit = true;
        }
        void Draw(RenderTarget rt)
        {
            // Draw bg
            scb.Color = new NoForms.Color(0.8f);
            rt.FillRectangle(DisplayRectangle, scb);
            scb.Color = new NoForms.Color(0);
            rt.DrawRectangle(DisplayRectangle.Inflated(-.5f), scb);
            int tpad = 3;
            if(SelectionOptions.Count >0)
                rt.DrawText(SelectionOptions.Count > _selectedOption ? SelectionOptions[_selectedOption] : "", new SharpDX.DirectWrite.TextFormat(gotdwFactory, "Arial", 12f),
                    DisplayRectangle.Inflated(-tpad),
                    scb, DrawTextOptions.Clip);
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
