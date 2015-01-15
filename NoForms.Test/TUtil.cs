using System;
using System.Collections.Generic;
using System.Text;

namespace NoForms.Test.OTK
{
    delegate IEnumerable<float> getdel(MockGLBuffer.rend rd);
    class TUtil
    {
        static public IEnumerable<float> lvr(MockGLBuffer.rend rd)
        {
            return new float[] { rd.v.x, rd.v.y };
        }
        static public IEnumerable<float> lvcr(MockGLBuffer.rend rd)
        {
            return new float[] { rd.v.x, rd.v.y, rd.c.a, rd.c.r, rd.c.g, rd.c.b };
        }
    }
}
