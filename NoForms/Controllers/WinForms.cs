using System;
using System.Collections.Generic;
using System.Text;
using NoForms.Windowing;
using NoForms.Common;
using System.Windows.Forms;

namespace NoForms.Controllers
{
    public class WinformsController : IController<IWFWin>
    {
        Form winForm;
        NoForm nf;
        void IController<IWFWin>.Init(IWFWin initObj, NoForm nf)
        {
            winForm = initObj.form;
            this.nf = nf;
            nf.controller = this;

            // Register!
            winForm.MouseDown += (o, e) => MouseUpDown(SDGTr.tr(e.Location), WFTr.Translate(e.Button), NoForms.Common.ButtonState.DOWN);
            winForm.MouseUp += (o, e) => MouseUpDown(SDGTr.tr(e.Location), WFTr.Translate(e.Button), NoForms.Common.ButtonState.UP);
            winForm.MouseMove += (o, e) => MouseMove(SDGTr.tr(e.Location));
            winForm.KeyDown += (o, e) => KeyUpDown((NoForms.Common.Keys)e.KeyCode, NoForms.Common.ButtonState.DOWN);
            winForm.KeyUp += (o, e) => KeyUpDown((NoForms.Common.Keys)e.KeyCode, NoForms.Common.ButtonState.UP);
            winForm.KeyPress += (o, e) => KeyPress(e.KeyChar);

            // also the noform
            MouseUpDown += (a, b, c) => nf.MouseUpDown(a, b, c, true, false);
            MouseMove += l => nf.MouseMove(l, true, false);
            KeyUpDown += nf.KeyUpDown;
            KeyPress += nf.KeyPress;
        }

        public Point MouseScreenLocation
        {
            get { return SDGTr.tr(System.Windows.Forms.Cursor.Position); }
        }
        public event MouseUpDownHandler MouseUpDown = delegate { };
        public event MouseMoveHandler MouseMove = delegate { };
        public event KeyUpDownHandler KeyUpDown = delegate { };
        public event KeyPressHandler KeyPress = delegate { };
    }

}
