using System;
using System.Collections.Generic;
using System.Text;
using Common;

namespace NoForms
{
    // FIXME Migrating from sys.win.forms.keys
    // ---------------------------------------
    //  I aint touching that right now. Fucking hard, with
    //  the massive enum and not to memtion the localisations
    //  from the typeconverter! Minefield!  
    //
    //  Possible Solution:  just send the int keycode. good luck with ll8n! (create ll8n helper? lol)



    public delegate void MouseUpDownHandler(Point location, MouseButton mb, ButtonState bs);
    public delegate void MouseMoveHandler(Point location);
    public delegate void KeyUpDownHandler(Common.Keys key, ButtonState bs);
    public delegate void KeyPressHandler(char c);
    public interface IController
    {
        Point MouseScreenLocation { get; }
        event MouseUpDownHandler MouseUpDown;
        event MouseMoveHandler MouseMove;
        event KeyUpDownHandler KeyUpDown;
        event KeyPressHandler KeyPress;
    }
}
