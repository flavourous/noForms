using System;
using SysRect = System.Drawing.Rectangle;
using System.Windows.Forms;

namespace NoForms
{
    static class FocusManager
    {
        static Object focusLock = new object();
        static IComponent focused;
        public static void FocusSet(IComponent setFocus, bool focus)
        {
            lock (focusLock)
            {
                if (focused == setFocus && !focus)
                    focused = null;
                else if(focus)
                    focused = setFocus;
            }
        }
        public static bool FocusGet(IComponent getFocus)
        {
            lock(focusLock)
                return object.ReferenceEquals(getFocus, focused);
        }
    }
}
