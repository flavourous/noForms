using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoForms;
using NoForms.Common;

namespace NoForms.Example
{
    class Program
    {
        static void Main()
        {
            RunForm<TextFieldDemoSlashTest>();
        }
        static void RunForm<T>() where T : NoForm
        {
            var rd = new Renderers.OpenTK.OTKNormal();
            var wc = new Controllers.DotNet.WinformsController();
            var wfp = new Platforms.Win32.Win32(rd, wc, new WindowCreateOptions(true, WindowBorderStyle.Fixed));

            T f = (T)Activator.CreateInstance(typeof(T), wfp);
            wfp.Run();
        }
    }

    
}
