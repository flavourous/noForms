using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NoForms;
using System.Diagnostics;
using System.Xml;
using System.IO;
using NoForms.Renderers;
using NoForms.Controls;

namespace testapp
{
    class Program
    {
        public static Collection<Story> Stories = new Collection<Story>();
        public static Collection<Project> Projects = new Collection<Project>();
        public static Project selectedProject;
        public static NoForm rootForm;

        [STAThread]
        public static void Main()
        {
            Load();

            NoForms.Renderers.D2DLayered d2dlayered = new NoForms.Renderers.D2DLayered();
            NoForms.Renderers.D2DSwapChain d2dswapchain = new NoForms.Renderers.D2DSwapChain();
            NoForm nf = rootForm = new MyNoForm(d2dlayered);
            nf.title = "Test App";
            nf.Size = new System.Drawing.Size(700, 500);
            nf.MinSize = new System.Drawing.Size(700, 300);

            nf.Create(true,false);
            Save();
        }
        static void Save()
        {
            XmlDocument xd = new XmlDocument();
            var xdr = xd.AppendChild(xd.CreateElement("Kanban"));
            var xn = xdr.AppendChild(xd.CreateElement("Stories"));
            foreach (var st in Stories)
            {
                if (st.state == StoryState.none) st.state = st.os;
                var el = xn.AppendChild(xd.CreateElement("Story"));
                var state = el.Attributes.Append(xd.CreateAttribute("state"));
                state.Value = ((int)st.state).ToString();
                var text = el.Attributes.Append(xd.CreateAttribute("text"));
                text.Value = st.storyText;
                var title = el.Attributes.Append(xd.CreateAttribute("title"));
                title.Value = st.storyTitle;
                var project = el.Attributes.Append(xd.CreateAttribute("project"));
                project.Value = st.projectName;
            }
            xn = xdr.AppendChild(xd.CreateElement("Projects"));
            foreach (var pr in Projects)
            {
                var el = xn.AppendChild(xd.CreateElement("Project"));
                var state = el.Attributes.Append(xd.CreateAttribute("name"));
                state.Value = pr.name;
            }
            XmlWriter xw = XmlWriter.Create("data", new XmlWriterSettings() { Indent = true, NewLineHandling = NewLineHandling.Entitize });
            xd.WriteTo(xw);
            xw.Close();
        }
        static void Load()
        {
            if(!File.Exists("data")) return;
            var xdd = new XmlDocument();
            xdd.LoadXml(File.ReadAllText("data"));
            var xd = xdd.DocumentElement;
            if (xd["Stories"] != null)
                foreach (XmlNode stx in xd["Stories"].ChildNodes)
                {
                    String title = stx.Attributes["title"] == null ? "" : stx.Attributes["title"].Value;
                    String proj = stx.Attributes["project"] == null ? "" : stx.Attributes["project"].Value;
                    Story ns = new Story(
                        title,
                        stx.Attributes["text"].Value, 
                        (StoryState)int.Parse(stx.Attributes["state"].Value),
                        proj
                        );
                    Stories.Add(ns);
                }
            if(xd["Projects"] != null)
                foreach (XmlNode stx in xd["Projects"].ChildNodes)
                {
                    Projects.Add(new Project() { name = stx.Attributes["name"].Value });
                }
            if (Projects.Count > 0)
                selectedProject = Projects[0];
            else
                Projects.Add(selectedProject = new Project() { name = "Default" });
        }
        public static String BreakSentance(String strin, int maxlen)
        {
            if (strin.Length < maxlen)
                return strin;
            return strin.Substring(0, maxlen).Trim() + "...";
        }
    }

    
    class MyNoForm : NoForm 
    {
        MoveHandle mh;
        SizeHandle sh;
        MainContainer mc;
        ComboBox cbProject;
        Scribble editProject, delProject, newProject;


        public MyNoForm(IRender ir) : base(ir) 
        {
            background = new USolidBrush() { color = new Color(0.5f, 0, 0, 0) };

            // move and resize
            mh = new MoveHandle(this);
            sh = new SizeHandle(this);
            components.Add(mh); components.Add(sh);

            mc = new MainContainer();
            components.Add(mc);

            SizeChanged += new Action<Size>(MyNoForm_OnSizeChanged);

            cbProject = new ComboBox();
            cbProject.selectionChanged += new Action<int>(cbProject_selectionChanged);
            components.Add(cbProject);

            editProject = new Scribble();
            editProject.Clicked += new Scribble.ClickDelegate(editProject_Clicked);
            editProject.draw += new Scribble.scribble(editProject_draw);
            components.Add(editProject);

            delProject = new Scribble();
            delProject.Clicked += new Scribble.ClickDelegate(delProject_Clicked);
            delProject.draw += new Scribble.scribble(delProject_draw);
            components.Add(delProject);

            newProject = new Scribble();
            newProject.Clicked += new Scribble.ClickDelegate(newProject_Clicked);
            newProject.draw += new Scribble.scribble(newProject_draw);
            components.Add(newProject);

            var bordercolor = themeColor;
            bordercolor.a = 0.9f;
            scb = new USolidBrush() { color = bordercolor };
            var mainColor = new Color(1f);
            scb1 = new USolidBrush() { color = mainColor };
            var barColor = bordercolor.Scale(0.2f);
            brushBars = new USolidBrush() { color = barColor };

            MyNoForm_OnSizeChanged(Size);
        }
        USolidBrush scb, scb1, brushBars;
        void cbProject_selectionChanged(int obj)
        {
            foreach(var p in Program.Projects)
                if(cbProject[obj] == p.name)
                    Program.selectedProject = p;
        }

        UText n = new UText("n", UHAlign.Center, UVAlign.Middle, false, 0, 0) { font = new UFont("Arial", 12f, false, false) };
        void newProject_draw(IUnifiedDraw ud, USolidBrush scb, UStroke stroke)
        {
            scb.color = new NoForms.Color(.5f, 1, 1, 1);
            var dr = newProject.DisplayRectangle;
            var ir = dr.Inflated(new Thickness(-3.5f));
            var or = dr.Inflated(new Thickness(-.5f));
            ud.FillRectangle(or, scb);
            scb.color = new NoForms.Color(1, 0, 0, 1);
            ud.DrawRectangle(or, scb, stroke);
            ud.DrawRectangle(ir, scb, stroke);
            ir.left += 3; ir.top -= 1;
            ud.DrawText(n,newProject.DisplayRectangle.Location, scb, UTextDrawOptions.Clip,false);
        }

        void newProject_Clicked(Point loc)
        {
            var renderer = new NoForms.Renderers.D2DLayered();

            Project pref = new Project();
            var editDlg = new ProjectEditDialog(renderer, pref);
            
            editDlg.MinSize = editDlg.Size = new System.Drawing.Size(400, 300);
            var pl = Program.rootForm.Location;
            var ps = Program.rootForm.Size;
            editDlg.Location = new Point(pl.X - 200 + ps.width / 2, pl.Y - 150 + ps.height / 2);
            editDlg.Create(false, true);
            Program.Projects.Add(pref);
        }

        UText d = new UText("d", UHAlign.Center, UVAlign.Middle, false, 0, 0) { font = new UFont("Arial", 12f, false, false) };
        void delProject_draw(IUnifiedDraw ud, USolidBrush scb, UStroke stroke)
        {
            scb.color = new NoForms.Color(.5f, 1, 1, 1);
            var dr = delProject.DisplayRectangle;
            var ir = dr.Inflated(new Thickness(-3.5f));
            var or = dr.Inflated(new Thickness(-.5f));
            ud.FillRectangle(or, scb);
            scb.color = new NoForms.Color(1, 1, 0, 0);
            ud.DrawRectangle(or, scb, stroke);
            ud.DrawRectangle(ir, scb, stroke);
            ir.left += 3; ir.top -= 1;

            ud.DrawText(d,delProject.DisplayRectangle.Location, scb, UTextDrawOptions.Clip,false);
        }

        bool del = false;
        void delProject_Clicked(Point loc)
        {
            if (del) return;
            del = true;
            Program.selectedProject.RemoveThisOne();
            del = false;
        }


        void UpdateProjectComboBox()
        {
            // Add new ones
            foreach (var p in Program.Projects)
            {
                if (!cbProject.Contains(p.name))
                {
                    cbProject.AddItem(p.name);
                    if (p == Program.selectedProject)
                        cbProject.SelectOption(p.name);
                }
            }

            // remove old ones
            int i=0;
            while (i < cbProject.Count)
            {
                bool dropit = true;
                foreach (var p in Program.Projects)
                    if (p.name == cbProject[i])
                    {
                        i++;
                        dropit = false;
                        break;
                    }
                if (dropit)
                {
                    cbProject.RemoveItem(i);
                    foreach (var p in Program.Projects)
                        if (p.name == cbProject.selectedText)
                            Program.selectedProject = p;
                }
            }
        }

        UText e = new UText("e", UHAlign.Center, UVAlign.Middle, false, 0, 0) { font = new UFont("Arial", 12f, false, false) };
        void editProject_draw(IUnifiedDraw ud, USolidBrush scb, UStroke stroke)
        {
            scb.color = new NoForms.Color(.5f, 1, 1, 1);
            var ir = editProject.DisplayRectangle.Inflated(new Thickness(-3.5f));
            var or = editProject.DisplayRectangle.Inflated(new Thickness(-.5f));
            ud.FillRectangle(or, scb);
            scb.color = new NoForms.Color(1, 0, 1, 0);
            ud.DrawRectangle(or, scb, stroke);
            ud.DrawRectangle(ir, scb, stroke);
            ir.left += 3; ir.top -= 1;
            ud.DrawText(e,editProject.DisplayRectangle.Location, scb, UTextDrawOptions.Clip,false);
        }

        void editProject_Clicked(Point loc)
        {
            Project pref = null;
            var renderer = new NoForms.Renderers.D2DLayered();

            foreach(var pr in Program.Projects) 
                if(pr.name == cbProject.selectedText)
                    pref = pr;

            var editDlg = new ProjectEditDialog(renderer, pref);
            editDlg.MinSize = editDlg.Size = new System.Drawing.Size(400, 300);
            var pl = Program.rootForm.Location;
            var ps = Program.rootForm.Size;
            editDlg.Location = new Point(pl.X - 200 + ps.width / 2, pl.Y - 150 + ps.height / 2);
            editDlg.Create(false, true);
        }

        void MyNoForm_OnSizeChanged(Size sz)
        {
            mh.DisplayRectangle = new Rectangle(10, 10, 20, 20);
            sh.DisplayRectangle = new Rectangle(Size.width - 30, Size.height - 30, 20, 20);
            mc.DisplayRectangle = new Rectangle(gap, gap + barwid, Size.width - gap * 2, Size.height - gap * 2 - barwid * 2);

            int cbwid = 150;
            cbProject.DisplayRectangle = new Rectangle(Size.width - 10 - cbwid - 50, Size.height - 30, cbwid, 20);
            editProject.DisplayRectangle = new Rectangle(Size.width - 10 - cbwid - 50 - 25, Size.height - 30, 20, 20);
            e.width = editProject.DisplayRectangle.width; e.height = editProject.DisplayRectangle.height;
            delProject.DisplayRectangle = new Rectangle(Size.width - 10 - cbwid - 50 - 50, Size.height - 30, 20, 20);
            d.width = delProject.DisplayRectangle.width; d.height = delProject.DisplayRectangle.height;
            newProject.DisplayRectangle = new Rectangle(Size.width - 10 - cbwid - 50 - 75, Size.height - 30, 20, 20);
            n.width = newProject.DisplayRectangle.width; n.height = newProject.DisplayRectangle.height;
        }

        float gap = 5;
        float barwid = 30;
        NoForms.Color themeColor = new NoForms.Color(1, 95f/255f,150f/255f,190f/255f);
        public override void Draw(IRenderType rt) 
        {
            rt.uDraw.FillRectangle(new Rectangle(0, 0, Size.width, gap), scb);
            rt.uDraw.FillRectangle(new Rectangle(0, Size.height - gap, Size.width, Size.height), scb);
            rt.uDraw.FillRectangle(new Rectangle(0, 0, gap, Size.height), scb);
            rt.uDraw.FillRectangle(new Rectangle(Size.width - gap, 0, Size.width, Size.height), scb);

            var innerRect = new Rectangle(gap, gap+barwid, Size.width - gap*2, Size.height - gap*2-barwid*2);
            rt.uDraw.FillRectangle(innerRect, scb1);

            var topbarrect = new Rectangle(gap, gap, Size.width - gap*2, barwid);
            var botbarrect = new Rectangle(gap, Size.height-gap-barwid, Size.width - gap*2, barwid);
            rt.uDraw.FillRectangle(topbarrect, brushBars);
            rt.uDraw.FillRectangle(botbarrect, brushBars);

            UpdateProjectComboBox();
            foreach (var s in Program.Stories)
                s.visible = s.projectName == Program.selectedProject.name;

            foreach (var s in Program.Stories)
                if (s.state == StoryState.none)
                    if (s.Parent != this)
                    {
                        s.IsDisplayRectangleCalculated = false;
                        var sp = (s.Parent as StoryListContainer);
                        lock (sp)
                        {
                            iAmDropped_oldParent = sp.state;
                            components.Add(s);
                            iAmDropped = s;
                        }
                    }
            Story ias = (iAmDropped as Story);
            if (ias != null && ias.state != StoryState.none)
            { // we have a pickup...
                components.Remove(iAmDropped);
                ias.IsDisplayRectangleCalculated = true;
                iAmDropped = null;
            }
            if(ias != null && ias.state == StoryState.none)
                if ((iAmDropped as Story).dragtime == false)
                {
                    components.Remove(iAmDropped);
                    ias.state = iAmDropped_oldParent;
                    ias.IsDisplayRectangleCalculated = true;
                    iAmDropped = null;
                }
        }
        public static NoForms.IComponent iAmDropped = null;
        public static StoryState iAmDropped_oldParent;

        
    }

    class MainContainer : NoForms.Controls.Abstract.BasicContainer
    {
        public StoryListContainer backlog, planned, inprogress,moreinfo, testing, deploy, done;
        StoryListContainer[] slcs;
        TextLabel[] slcts;
        public MainContainer()
            : base()
        {
            backlog = new StoryListContainer("Backlog", StoryState.backlog);
            planned = new StoryListContainer("Planned",StoryState.planned);
            inprogress = new StoryListContainer("In Progress",StoryState.inprogress);
            moreinfo = new StoryListContainer("More Info",StoryState.moreinfo);
            testing = new StoryListContainer("Testing",StoryState.testing);
            deploy = new StoryListContainer("To Deploy",StoryState.deploy);
            done = new StoryListContainer("Done",StoryState.done);
            slcs = new StoryListContainer[] { backlog, planned, inprogress, moreinfo, testing, deploy, done };
            slcts = new TextLabel[slcs.Length];
            for (int i=0;i<slcs.Length;i++)
            {
                components.Add(slcs[i]);
                slcts[i] = new TextLabel();
                slcts[i].textData.text = slcs[i].name;
                components.Add(slcts[i]);
            }
            SizeChanged += new Action<Size>(MainContainer_SizeChanged);
            MainContainer_SizeChanged(Size);
        }

        float titleHeight = 25;
        float pad = 5;
        void MainContainer_SizeChanged(Size obj)
        {
            DoLayout();
        }

        void DoLayout()
        {
            float slcWidth = (Size.width - pad * (slcs.Length + 1)) / (float)slcs.Length;
            float slcHeight = Size.height - pad - titleHeight;
            float remain = (float)Math.Round(Size.width - (int)slcWidth * slcs.Length - pad * (slcs.Length + 1));
            int rc = 0;
            for (int i = 0; i < slcs.Length; i++)
            {
                int rem = (int)(remain-- > 0 ? 1 : 0);

                slcs[i].Size = new System.Drawing.Size((int)slcWidth + rem, (int)slcHeight);
                slcs[i].Location = new System.Drawing.Point((int)(pad + slcWidth) * i + (int)pad + rc, (int)titleHeight);

                slcts[i].Size = new System.Drawing.Size((int)slcWidth + rem, (int)titleHeight);
                slcts[i].Location = new System.Drawing.Point((int)(pad + slcWidth) * i + (int)pad + rc, 0);

                rc += rem;
            }
        }

        public override void Draw(IRenderType renderArgument)
        {
        }
    }

    class StoryListContainer : NoForms.Controls.Abstract.ScrollContainer
    {
        public String name;
        public StoryState state;
        Scribble botPad = new Scribble();
        public StoryListContainer(String name, StoryState state)
            : base()
        {
            this.name = name;
            this.state = state;
            add = new Scribble() { Scrollable = false };
            add.Cursor = System.Windows.Forms.Cursors.Hand;
            add.ZIndex = 1;
            components.Add(add);
            components.Add(botPad);
            add.draw += new Scribble.scribble(add_draw);
            add.Clicked += new Scribble.ClickDelegate(add_Clicked);
            botPad.draw += botPad_draw;
            cycle = 20;
            step = 5;
            GrabStories();
        }

        void botPad_draw(IUnifiedDraw uDraw, USolidBrush tehBrush, UStroke strk)
        {
        }

        void add_Clicked(Point loc)
        {
            if (!Util.AmITopZOrder(add, loc)) return;
            Story ns = new Story("","", state, Program.selectedProject.name);
            Program.Stories.Add(ns);

            var renderer = new NoForms.Renderers.D2DLayered();
            var editDlg = new StoryEditDialog(renderer, ns);
            editDlg.MinSize = editDlg.Size = new System.Drawing.Size(400, 300);
            var pl = Program.rootForm.Location;
            var ps = Program.rootForm.Size;
            editDlg.Location = new Point(pl.X - 200 + ps.width / 2, pl.Y - 150 + ps.height / 2);
            editDlg.Create(false, true);
        }

        void add_draw(IUnifiedDraw ud, USolidBrush scb, UStroke stroke)
        {
            var crt = add.DisplayRectangle;
            stroke.endCap = stroke.startCap = StrokeCaps.round;
            stroke.strokeWidth = 3f;
            scb.color = new Color(1, 0, 1, 0);
            ud.DrawLine(new Point(crt.left + crt.width / 2, crt.top), new Point(crt.left + crt.width / 2, crt.bottom), scb, stroke);
            ud.DrawLine(new Point(crt.left, crt.top + crt.height / 2), new Point(crt.right, crt.top + crt.height / 2), scb, stroke);
        }

        protected override void OnSizeChanged()
        {
            LayoutStories();
            base.OnSizeChanged();
            add.Size = new Size(10, 10);
            add.Location = new Point(5, Size.height - 15);
        }

        public void LayoutStories()
        {
            float padStories = 5;
            float startY = padStories; // very top padding

            foreach (var s in components)
            {
                if (s is Story && s.visible)
                {
                    s.Location = new Point(padStories, startY);
                    s.Size = new NoForms.Size(Size.width - (VerticalScrollbarVisible ? VerticalScrollbarWidth : 0) - 2*padStories, s.Size.height);
                    startY += s.DisplayRectangle.height + padStories;
                }
            }

            botPad.Size = new Size(0, 0);
            botPad.Location = new Point(0, startY);
        }

        Scribble add;
        public override void Draw(IRenderType ra)
        {
            lock (this)
            {
                GrabStories();
                CheckLayout();

                var dr = DisplayRectangle;
                var ir = DisplayRectangle.Deflated(new Thickness(.5f, .5f, .5f, .5f)); // float positioning...
                ra.uDraw.FillRectangle(ir, fillBrush);
                ra.uDraw.DrawRectangle(ir, borderBrush, edge);
            }
        }
        void GrabStories()
        {
            foreach (var s in Program.Stories)
                if (s.state == state && s.projectName == Program.selectedProject.name)
                    if (s.Parent == null)
                        components.Add(s); // was dropped in, or grabbed on init...
        }
        int lastStories = 0;
        void CheckLayout()
        {
            int nowStories=0;
            foreach(var c in components)
                if(c is Story) nowStories++;
            if (lastStories != nowStories)
            {
                LayoutStories();
                lastStories = nowStories;
            }
        }
        UBrush borderBrush = new USolidBrush() { color = new NoForms.Color(0.7f, 0, 0, 0) };
        UBrush fillBrush = new USolidBrush() { color = new NoForms.Color(0.3f, .7f, .7f, .7f) };
        UStroke edge = new UStroke();

        public override void MouseUpDown(System.Windows.Forms.MouseEventArgs mea, MouseButtonState mbs, bool inComponent, bool amClipped)
        {
                if (inComponent && mea.Button == System.Windows.Forms.MouseButtons.Left && mbs == MouseButtonState.UP)
                    if (MyNoForm.iAmDropped != null && MyNoForm.iAmDropped is Story)
                        (MyNoForm.iAmDropped as Story).state = state;
            base.MouseUpDown(mea, mbs, inComponent,amClipped);
        }
        
    }

    enum StoryState { none, backlog, planned, inprogress,moreinfo, testing, deploy, done, undefined };

    class Story : NoForms.Controls.Abstract.BasicContainer
    {
        NoForms.Controls.Scribble cx;
        public Story(String title, String txt, StoryState state, String project)
        {
            projectName = project;
            storyTitle = title;
            storyText = txt;
            this.state = state;

            Size = new NoForms.Size(30, 50);

            cx = new Scribble();
            components.Add(cx);
            SizeChanged += new Action<Size>(Story_SizeChanged);

            cx.Cursor = System.Windows.Forms.Cursors.Hand;

            bsr = new UStyleRange(0, storyTitle.Length, new UFont("Arial", 12f, true, false), red, null);
            textyTime = new UText("", UHAlign.Left, UVAlign.Top, true, inRect2.width, 0) { font = new UFont("Arial", 10f, false, false) };
            textyTime.styleRanges.Add(bsr);

            cx.draw += new Scribble.scribble(cx_draw);
            cx.Clicked += new Scribble.ClickDelegate(cx_Clicked);
            Story_SizeChanged(Size);
        }

        void cx_Clicked(Point loc)
        {
            if (dragtime || !Util.AmITopZOrder(cx,loc) ) return;
            state = StoryState.undefined;
            Parent.components.Remove(this);
            Program.Stories.Remove(this);
        }

        void cx_draw(IUnifiedDraw ud, USolidBrush scb, UStroke stroke)
        {
            stroke.strokeWidth = 2f;
            stroke.startCap = stroke.endCap = StrokeCaps.round;
            scb.color = new Color(1, 1, 0, 0);
            var dr = cx.DisplayRectangle;
            ud.DrawLine(new Point(dr.left, dr.top), new Point(dr.right, dr.bottom), scb, stroke);
            ud.DrawLine(new Point(dr.right, dr.top), new Point(dr.left, dr.bottom), scb, stroke);
        }

        void Story_SizeChanged(Size obj)
        {
            cx.Size = new System.Drawing.Size(7, 7);
            cx.Location = new Point(Size.width - 10, Size.height - 10);
        }
        public String projectName;
        public String storyTitle;
        public String storyText;
        public StoryState state;
        float boxHeight = 100;

        UBrush red = new USolidBrush() { color = new Color(1, 1, 0, 0) };
        UBrush white = new USolidBrush() { color = new Color(1) };

        //public override void DrawBase(IRenderType renderArgument)
        //{
        //    var cr = Parent.DisplayRectangle.Deflated(new Thickness(5));
        //    renderArgument.uDraw.PushAxisAlignedClip(cr, true);
        //    base.DrawBase(renderArgument);
        //    renderArgument.uDraw.PopAxisAlignedClip();
        //}

        // Drawybit
        public override void Draw(IRenderType ra)
        {
            DetermineTextAndSize(ra.uDraw);
            ra.uDraw.DrawText(textyTime, inRect2.Location, scb_text, UTextDrawOptions.None,false);
        }

        Rectangle inRect2;
        UText textyTime;
        UStyleRange bsr;

        public void DetermineTextAndSize(IUnifiedDraw ud) 
        {
            // do bottom padding on the slc
            Rectangle clr = DisplayRectangle;

            Rectangle inRect = DisplayRectangle; // fill displayrect
            ud.FillRectangle(inRect, scb_back);
            inRect2 = inRect.Deflated(new  Thickness(5)); // text padding

            textyTime.width = inRect2.width;
            textyTime.text = storyTitle + "\r\n" + Program.BreakSentance(storyText, 50);
            bsr.length = storyTitle.Length;

            var ti = ud.GetTextInfo(textyTime);
            int nlines = ti.numLines;
            float ct = ti.minSize.height;
            ct += 5 +5; // top and bottom padding for text
            float obh = boxHeight;
            boxHeight = (int)Math.Round(ct);
            Size = new Size(Size.width, boxHeight);
            if (Parent is StoryListContainer && obh != boxHeight)
                (Parent as StoryListContainer).LayoutStories(); // FIXME should be able to LayoutStory(this), worth it?
        }
        
        USolidBrush scb_back = new USolidBrush() { color = new Color(.4f, .2f, .2f, .2f) };
        USolidBrush scb_text = new USolidBrush() { color = new Color(1f, .1f, .1f, .1f) };
        UStroke edge = new UStroke();

        int sdx, sdy;
        public override void MouseMove(System.Drawing.Point location, bool inComponent, bool amClipped)
        {
            if (maybeDrag || dragtime)
            {
                System.Drawing.Point nloc = System.Windows.Forms.Cursor.Position;
                int dx = nloc.X - lloc.X;
                int dy = nloc.Y - lloc.Y;
                sdx += dx; sdy += dy;
                lloc = nloc;
                if (!dragtime && Math.Sqrt(sdx * sdx + sdy * sdy) > 15)
                {
                    dragtime = true;
                    maybeDrag = false;
                    os = state;
                    state = StoryState.none;
                    dx += sdx; dy += sdy;
                    sdx = sdy = 0;
                }

                if (dragtime)
                {
                    DisplayRectangle = new Rectangle(DisplayRectangle.left + dx, DisplayRectangle.top + dy, DisplayRectangle.width, DisplayRectangle.height);
                }
            }
            base.MouseMove(location, inComponent,amClipped);
        }
        public bool dragtime = false;
        System.Drawing.Point lloc;
        DateTime dtLastClick = DateTime.Now.AddDays(-1);
        public StoryState os;
        public override void MouseUpDown(System.Windows.Forms.MouseEventArgs mea, MouseButtonState mbs, bool inComponent, bool amClipped)
        {
            bool tzo = Util.AmITopZOrder(this, mea.Location);
            if (mea.Button == System.Windows.Forms.MouseButtons.Left && mbs == MouseButtonState.UP && inComponent && !amClipped)
            {
                var dt = DateTime.Now.Subtract(dtLastClick);
                if (dt.TotalMilliseconds < 300)
                {  // Double clicked...
                    dragtime = maybeDrag = false;
                    if(state == StoryState.none)
                        state = os;
                    var renderer = new NoForms.Renderers.D2DLayered();
                    var editDlg = new StoryEditDialog(renderer, this);
                    editDlg.MinSize=editDlg.Size = new System.Drawing.Size(400, 300);
                    var pl = Program.rootForm.Location;
                    var ps = Program.rootForm.Size;
                    editDlg.Location = new Point(pl.X - 200 + ps.width/2, pl.Y - 150 + ps.height/2);
                    editDlg.Create(false, true);
                }
                dtLastClick = DateTime.Now;
            }
            if (dragtime && mea.Button == System.Windows.Forms.MouseButtons.Left && mbs == MouseButtonState.UP)
            {
                dragtime = false;
            }
            if (maybeDrag && mea.Button == System.Windows.Forms.MouseButtons.Left && mbs == MouseButtonState.UP)
            {
                maybeDrag = false;
            }
            if (tzo && inComponent && mbs == MouseButtonState.DOWN && mea.Button == System.Windows.Forms.MouseButtons.Left && !amClipped)
            {
                lloc = System.Windows.Forms.Cursor.Position;
                maybeDrag = true;
                sdx = sdy = 0;
            }
            base.MouseUpDown(mea,mbs,inComponent,amClipped);
        }
        bool maybeDrag = false;
    }

    class StoryEditDialog : NoForm
    {
        MoveHandle mh;
        SizeHandle sh;
        Textfield tf,tft;
        Button bt;
        ComboBox pcb;

        Story refStory;

        public StoryEditDialog(IRender ir, Story sedthis)
            : base(ir) 
        {
            // hold ref to stroy
            refStory = sedthis;

            // move and resize
            mh = new MoveHandle(this);
            sh = new SizeHandle(this);
            components.Add(mh); components.Add(sh);

            tft = new Textfield();
            tft.text = refStory.storyTitle;
            tft.layout = Textfield.LayoutStyle.OneLine;
            components.Add(tft);

            tf = new Textfield();
            tf.text = refStory.storyText;
            tf.layout = Textfield.LayoutStyle.WrappedMultiLine;
            components.Add(tf);

            pcb = new ComboBox();
            foreach (var p in Program.Projects)
                pcb.AddItem(p.name);
            pcb.SelectOption(refStory.projectName);
            components.Add(pcb);

            bt = new Button();
            bt.textData.text = "Save & Close";
            bt.buttonColor = themeColor;
            bt.textColor = new NoForms.Color(0);
            bt.ButtonClicked += new Button.NFAction( () => 
            {
                refStory.storyTitle = tft.text; 
                refStory.storyText = tf.text;
                refStory.projectName = pcb.selectedText;
                this.Close();
            });
            components.Add(bt);

            SizeChanged += new Action<Size>(SED_OnSizeChanged);
            SED_OnSizeChanged(Size);

            var bordercolor = themeColor;
            bordercolor.a = 0.9f;
            scb = new USolidBrush() { color = bordercolor };
            var mainColor = new Color(1f);
            scb1 = new USolidBrush() { color = mainColor };
            var barColor = bordercolor.Scale(0.2f);
            brushBars = new USolidBrush() { color = barColor };
        }

        void SED_OnSizeChanged(Size sz)
        {
            mh.DisplayRectangle = new Rectangle(10, 10, 20, 20);
            sh.DisplayRectangle = new Rectangle(Size.width - 30, Size.height - barwid, 20, 20);
            bt.DisplayRectangle = new Rectangle(Size.width - 135, Size.height - (barwid+gap - 5/2), 100, 25);
            var ir = new Rectangle(gap, gap + barwid, Size.width - gap*2, Size.height - gap*2 - barwid*2);
            int tbh = 25;
            tf.DisplayRectangle = new Rectangle(ir.left+5, ir.top+5+tbh+5, ir.width-10, ir.height-10-tbh-5);
            tft.DisplayRectangle = new Rectangle(ir.left + 5, ir.top + 5, ir.width - 10, tbh);
            pcb.DisplayRectangle = new Rectangle(20, Size.height - (barwid + gap - 5), 150, 20);
        }

        float gap = 5;
        float barwid = 30;
        NoForms.Color themeColor = new NoForms.Color(1, 0.7f, 0.7f, 0.75f);
        public override void Draw(IRenderType rt)
        {
            rt.uDraw.FillRectangle(new Rectangle(0, 0, Size.width, gap), scb);
            rt.uDraw.FillRectangle(new Rectangle(0, Size.height - gap, Size.width, gap), scb);
            rt.uDraw.FillRectangle(new Rectangle(0, 0, gap, Size.height), scb);
            rt.uDraw.FillRectangle(new Rectangle(Size.width - gap, 0, gap, Size.height), scb);

            var innerRect = new Rectangle(gap, gap + barwid, Size.width - gap * 2, Size.height - gap * 2 - barwid * 2);
            rt.uDraw.FillRectangle(innerRect, scb1);

            var topbarrect = new Rectangle(gap, gap, Size.width - gap * 2, barwid);
            var botbarrect = new Rectangle(gap, Size.height - gap - barwid, Size.width - gap * 2, barwid);
            rt.uDraw.FillRectangle(topbarrect, brushBars);
            rt.uDraw.FillRectangle(botbarrect, brushBars);

            var titRect = topbarrect;
            titRect.left += 30;
            titRect.right -= 30;

            titleText.width = titRect.width;
            titleText.height = titRect.height;

            rt.uDraw.DrawText(titleText, titRect.Location, scb1, UTextDrawOptions.Clip,false);
        }
        USolidBrush scb, scb1, brushBars;
        UText titleText = new UText("Editing Story", UHAlign.Center, UVAlign.Middle, false, 0, 0)
        {
            font = new UFont("Arial", 12f, false, false)
        };
        
    }

    public class Project
    {
        public void RemoveThisOne()
        {
            int ps = 0;
            foreach (var st in Program.Stories)
            {
                if (st.projectName == name)
                    ps++;
            }
            if (ps > 0 || Program.Projects.Count == 1) ChangeNameWithStories("Default");
            else // ok, remove teh project
            {
                int myIdx = Program.Projects.IndexOf(this);
                int selIdx = (myIdx - 1) > 0 ? myIdx - 1 : myIdx + 1;
                Program.selectedProject = Program.Projects[selIdx];
                Program.Projects.Remove(this);
            }
        }
        /// <summary>
        /// Changes the name of  astory, and merges if another exists with the same name...
        /// </summary>
        /// <param name="to"></param>
        public void ChangeNameWithStories(String to)
        {
            foreach (var st in Program.Stories)
            {
                if (st.projectName == name)
                    st.projectName = to;
            }
            bool discard = false;
            foreach (var p in Program.Projects)
                if (p.name == to && p!=this)
                    discard = true;
            if (discard)
                Program.Projects.Remove(this);
            else
                name = to;
        }
        public String name = "";
        public String details = "";
    }

    public class ProjectEditDialog :NoForm
    {
        MoveHandle mh;
        SizeHandle sh;
        Textfield tf,tft;
        Button bt;

        Project refProject;

        public ProjectEditDialog(IRender ir, Project pedthis)
            : base(ir) 
        {
            // hold ref to stroy
            refProject = pedthis;

            // move and resize
            mh = new MoveHandle(this);
            sh = new SizeHandle(this);
            components.Add(mh); components.Add(sh);

            tft = new Textfield();
            tft.text = refProject.name;
            tft.layout = Textfield.LayoutStyle.OneLine;
            components.Add(tft);

            tf = new Textfield();
            tf.text = refProject.details;
            tf.layout = Textfield.LayoutStyle.WrappedMultiLine;
            components.Add(tf);

            bt = new Button();
            bt.textData.text = "Save & Close";
            bt.buttonColor = themeColor;
            bt.textColor = new NoForms.Color(0);
            bt.ButtonClicked += new Button.NFAction( () => 
            {
                if (refProject.name != tft.text)
                {
                    bool hasalready = false;
                    foreach (var p in Program.Projects)
                        if (tft.text == p.name)
                            hasalready = true;
                    if (!hasalready)
                        refProject.ChangeNameWithStories(tft.text);
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("No, that project already Exists");
                        return;
                    }
                }
                refProject.details = tf.text;
                this.Close();
            });
            components.Add(bt);

            SizeChanged += new Action<Size>(SED_OnSizeChanged);
            SED_OnSizeChanged(Size);

            var bordercolor = themeColor;
            bordercolor.a = 0.9f;
            scb = new USolidBrush() { color = bordercolor };
            var mainColor = new Color(1f);
            scb1 = new USolidBrush() { color = mainColor };
            var barColor = bordercolor.Scale(0.2f);
            brushBars = new USolidBrush() { color = barColor };
        }

        void SED_OnSizeChanged(Size sz)
        {
            mh.DisplayRectangle = new Rectangle(10, 10, 20, 20);
            sh.DisplayRectangle = new Rectangle(Size.width - 30, Size.height - barwid, 20, 20);
            bt.DisplayRectangle = new Rectangle(Size.width - 135, Size.height - (barwid+gap - 5/2), 100, 25);
            var ir = new Rectangle(gap, gap + barwid, Size.width - gap*2, Size.height - gap*2 - barwid*2);
            int tbh = 25;
            tf.DisplayRectangle = new Rectangle(ir.left+5, ir.top+5+tbh+5, ir.width-10, ir.height-10-tbh-5);
            tft.DisplayRectangle = new Rectangle(ir.left + 5, ir.top + 5, ir.width - 10, tbh);
        }

        float gap = 5;
        float barwid = 30;
        NoForms.Color themeColor = new NoForms.Color(1, 0.7f, 0.7f, 0.75f);
        public override void Draw(IRenderType rt)
        {
            rt.uDraw.FillRectangle(new Rectangle(0, 0, Size.width, gap), scb);
            rt.uDraw.FillRectangle(new Rectangle(0, Size.height - gap, Size.width, gap), scb);
            rt.uDraw.FillRectangle(new Rectangle(0, 0, gap, Size.height), scb);
            rt.uDraw.FillRectangle(new Rectangle(Size.width - gap, 0, gap, Size.height), scb);

            var innerRect = new Rectangle(gap, gap + barwid, Size.width - gap*2, Size.height - gap*2 - barwid*2);
            rt.uDraw.FillRectangle(innerRect, scb1);

            var topbarrect = new Rectangle(gap, gap, Size.width - gap*2, barwid);
            var botbarrect = new Rectangle(gap, Size.height - gap - barwid, Size.width - gap*2, barwid);
            rt.uDraw.FillRectangle(topbarrect, brushBars);
            rt.uDraw.FillRectangle(botbarrect, brushBars);

            var titRect = topbarrect;
            titRect.left += 30; 
            titRect.right -= 30;

            titleText.width = titRect.width;
            titleText.height = titRect.height;

            rt.uDraw.DrawText(titleText, titRect.Location, scb1, UTextDrawOptions.Clip,false);
        }
        USolidBrush scb, scb1, brushBars;
        UText titleText = new UText("Editing Project", UHAlign.Center, UVAlign.Middle, false, 0, 0)
        {
            font = new UFont("Arial", 12f, false, false)
        };
        
    }

}