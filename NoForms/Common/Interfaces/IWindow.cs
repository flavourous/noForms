using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace NoForms
{
    // FIXME different windowing systems are going to have different capabilities, some will be wildly different eg xbox/android.  How to cope?
    //       probable dirty solution: unify with approximate or null implimentations...NotImplimentedException may be a bit harsh...compiler warnings would be better.
    //                                for example "On xbox, showdialog is the same as show.  window is always maximised.  Only one window may be created, others will be ignored"
    //                                could use IRender->IWindow->IController interplay to fake a new window on the same rendering target, possibly...I digress. For laters.
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
        NoForms.Common.Cursors Cursor { get; set; }
        bool CaptureMouse { get; set; }
        
        void Close();
        void Run();
        void Show();
        void ShowDialog();
        void Hide();

        void SetClipboard(String s);
        void GetClipboard(out String s);
    }
}
