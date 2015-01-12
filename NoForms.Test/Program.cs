using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;


namespace NoForms.Test
{
    class Program
    {
        public static void Main()
        {
            //OTKRenderTests t = new OTKRenderTests();
            //t.SetUp();
            //t.RenderQuadsAndLinesV();
            //return;
            
            String nunit = "C:\\Program Files (x86)\\NUnit 2.6.2\\bin\\nunit.exe";
            var asm = Assembly.GetExecutingAssembly();
            String myself = asm.Location;
            System.Diagnostics.Process.Start( nunit, "/run " + myself );
        }
    }
}
