using System;
using System.Collections.Generic;
using System.Text;
using spf = System.Drawing.PointF;

namespace NoForms.Common
{
    public static class Bezier
    {
        struct spft{public spf p; public double t;}
        public static spf[] Get2D(spf p1, spf p2, spf p3, spf p4, double maxStep)
        {
            // init
            LinkedList<spft> pts = new LinkedList<spft>();
            pts.AddLast(new spft() { p = p1, t=0});
            pts.AddLast(new spft() { p = p4, t=1});

            // Bisect points on curve.
            //  begin at first, look at next, add after head if step is too short.
            //  if added, remain at head, else, goto head.next. 
            //  finish when head is last (no next!)
            var head = pts.First;
            while (!Object.ReferenceEquals(head, pts.Last))
            {
                if (dst(head.Value.p, head.Next.Value.p) > maxStep)
                {
                    double tmid = (head.Value.t + head.Next.Value.t)/2.0;
                    var tvs = gtv(tmid);
                    float bx = (float)bez(tvs, p1.X, p2.X, p3.X, p4.X);
                    float by = (float)bez(tvs, p1.Y, p2.Y, p3.Y, p4.Y);
                    pts.AddAfter(head, new spft() { t = tmid, p = new spf(bx, by) });
                }
                else head = head.Next;
            }

            // output
            var pc = pts.Count;
            var ret = new spf[pc];
            head = pts.First;
            for(int i=0;i<pc;i++) 
            {
                ret[i] = head.Value.p;
                head=head.Next;
            }
            return ret;
        }

        struct tvals { public double m1t_3, m1t_2, t3, t3_2, m1t, t_3; };
        static tvals gtv(double t)
        {
            return new tvals()
            {
                m1t_3 = Math.Pow(1-t,3),
                m1t_2 = Math.Pow(1 - t, 2),
                t3 = 3.0*t,
                t3_2 = 3.0*t*t,
                m1t = 1-t,
                t_3 = Math.Pow(t, 3),
            };
        }
        // parametric bezier in 2d, t : [0,1]:
        // Px = P1x*(1-t)^3 + P2x*3t*(1-t)^2 + P3x*3t^2*(1-t) + P4x*t^3
        // Py = P1y*(1-t)^3 + P2y*3t*(1-t)^2 + P3y*3t^2*(1-t) + P4y*t^3
        static double bez(tvals t, double b1, double b2, double b3, double b4)
        {
            return b1 * t.m1t_3 + b2 * t.t3 * t.m1t_2 + b3 * t.t3_2 * t.m1t + b4 * t.t_3;
        }
        static double dst(spf pt1, spf pt2)
        {
            double dx = pt2.X - pt1.X;
            double dy = pt2.Y - pt1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
