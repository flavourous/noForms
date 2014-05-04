using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace NoForms
{
    // FIXME make more generic, no system.drawing or winforms
    public interface IWindow
    {
        bool Minimise();
        bool Maximise();
        bool Restore();
        String Title { get; set; }
        bool showIcon { get; set; }
        Icon Icon { get; set; }
        bool BringToFront();
        Cursor Cursor { get; set; }

        public void Close();
        public void Run();
        public void Show();
        public void ShowDialog();
        public void Hide();
    }
}
