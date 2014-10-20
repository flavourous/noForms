using System;
using System.Collections.Generic;

namespace EllipseLib
{
    // Because fuck that slow shit.
    public static class EasyEllipse
    {
        public class EECOut : IDisposable
        {
            public float x, y; public bool clockwise, reflex;
            public void Dispose() { }
        }
        public struct EasyEllipseInput { public float t1, t2, rotation, rx, ry, start_x, start_y, resolution; }
        public static IEnumerable<System.Drawing.PointF> Generate(EasyEllipseInput eei)
        {
            float rt1 = (eei.t1/180f)*(float)Math.PI;
            float rt2 = (eei.t2/180f)*(float)Math.PI;
            float rot = (float)Math.PI*(eei.rotation/180f);
            float cr = (float)Math.Cos(rot);
            float sr = (float)Math.Sin(rot);

            // init point get offset to start
            float x, y;
            getpoint(rt1, cr, sr, eei.rx, eei.ry, out x, out y);
            float of_x = eei.start_x - x; 
            float of_y = eei.start_y - y; 

            foreach(double sd in EllipseUtil.GetThetas(rt1,rt2,eei.resolution, eei.rx, eei.ry))
            {
                float fsd = (float)sd;
                getpoint(rt1, cr, sr, eei.rx, eei.ry, out x, out y);
                yield return new System.Drawing.PointF(x + of_x, y + of_y);
            }
        }
        static void getpoint(float rt1, float crot, float srot, float rx, float ry, out float x, out float y)
        {
            float ct = (float)Math.Cos(rt1);
            float st = (float)Math.Sin(rt1);
            
            float r1 = rx * ct + ry * st; // get radiaus on nonrotated ellipse at rt1
            float x1 = r1 * ct; float y1 = r1 * st; // get x,y of that
            rotate(x1, y1, crot, srot, out x, out y);
        }
        static void rotate(float inx, float iny, float ct, float st, out float x, out float y)
        {
            x = inx * ct - iny * st;
            y = inx * st + iny * ct;
        }

        public static EECOut ConvertEndPoint(EasyEllipseInput eei)
        {
            float rt1 = (eei.t1 / 180f) * (float)Math.PI;
            float rt2 = (eei.t2 / 180f) * (float)Math.PI;
            float rot = (float)Math.PI * (eei.rotation / 180f);
            float cr = (float)Math.Cos(rot);
            float sr = (float)Math.Sin(rot);

            //get point 1
            float x1, y1;
            getpoint(rt1, cr, sr, eei.rx, eei.ry, out x1, out y1);

            // get point 2
            float x2, y2;
            getpoint(rt2, cr, sr, eei.rx, eei.ry, out x2, out y2);

            // the end point is desired start, minus our start, plus our end.
            return new EECOut()
            {
                x = eei.start_x - x1 + x2,
                y = eei.start_y - y1 + y2,
                reflex = Math.Abs(eei.t2 - eei.t1) > 180f,
                clockwise = true
            };
        }
    }

    static class EllipseUtil
    {
        public static double[] GetThetas(double t1, double dt, double minarcdrop, double rx, double ry)
        {
            // FIXME reimpliment using LinkedList<>, because the inserts are order N here....
            List<double> tts = new List<double>();
            tts.Add(t1);
            tts.Add(t1+dt);
            double it0, it1;
            for (int i = 0; i < tts.Count - 1; i++)
            {
                it0 = tts[i]; it1 = tts[i + 1];
                //do we need one inbetween i and i+1?
                double r1 = Math.Sqrt(Math.Pow(rx*Math.Cos(it0), 2) + Math.Pow(ry*Math.Sin(it0), 2));
                double r2 = Math.Sqrt(Math.Pow(rx*Math.Cos(it1), 2) + Math.Pow(ry*Math.Sin(it1), 2));
                double dr = r1 - r2 * Math.Cos(it1-it0);
                if (Math.Abs(dr) > minarcdrop) // FIXME shouldnt need abs here....?
                {
                    tts.Insert(i+1, (tts[i] + tts[i + 1]) / 2);
                    i--; // back one, we can done single pass here.
                }
            }

            return tts.ToArray();
        }

        

    }

    /// <summary>
    /// For solving the offset of the elipse centre, given
    /// the elipse rectangle Size and rotation, and two points on it.
    /// </summary>
    public struct Ellipse_Input
    {
        public Ellipse_Input(double x1, double y1, double x2, double y2, double rx, double ry, double theta)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
            this.rx = rx;
            this.ry = ry;
            this.theta = theta;
        }
        /// <summary>
        /// in degrees
        /// </summary>
        public double theta;
        public double x1, x2;
        public double y1, y2;
        public double rx, ry;
    }
    public struct Ellipse_Output
    {
        public Ellipse_Output(double X, double Y, double T1, double T2, double rx, double ry, bool err)
        {
            this.X = X;
            this.Y = Y;
            this.T1 = T1;
            this.T2 = T2;
            this.rx = rx;
            this.ry = ry;
            this.errored = err;
        }
        public double X, Y;
        public double T1, T2;
        public double rx, ry;
        public bool errored;
    }

    public static class Ellipse
    {
        //static Random rd = new Random((int)DateTime.Now.Ticks);
        //public static double Get_x(double y, double X0, double Y0, double rx, double ry, double th, bool solution)
        //{
        //    double real, imag;

        //    double s = Math.Sin(Math.PI * th / 180);
        //    double c = Math.Cos(Math.PI * th / 180);

        //    double Cx = ry * ry * c * c + rx * rx * s * s;
        //    double Cy = ry * ry * s * s + rx * rx * c * c;
        //    double Cxy = s * c * (rx * rx - ry * ry);

        //    if (Cx < tol)
        //    { // will fail, we have no Y extent to this ellipse...fair enuf...
        //        double xExtent = Math.Sqrt(Cy);
        //        // pick one of the degenerate solutions here...
        //        return X0 + (rd.NextDouble() * 2 - 1);
        //    }

        //    double left = (Cx * X0 - Cxy * (y - Y0))/Cx;
        //    double dis = Cx - (y - Y0) * (y - Y0);
        //    double right = (solution ? 1.0 : -1.0) * rx * ry * Math.Sqrt(Math.Abs(dis)) / Cx;

        //    if(dis < 0)
        //    {
        //        real = left;
        //        imag = right;
        //    }
        //    else 
        //    {
        //        real = left + right;
        //        imag = 0;
        //    }

        //    if (imag != 0 || double.IsNaN(real)) throw new NotImplementedException();

        //    return real;
        //}

        static double tol = 1e-4; // that will do for subpixel stuff! FIXME there will be scale transform problems?
        public static IEnumerable<Ellipse_Output> Get_X0Y0(Ellipse_Input input)
        {
            if (input.rx < 0) throw new ArgumentException("Ellipse_Input x-radius cannot be less than zero");
            if (input.ry < 0) throw new ArgumentException("Ellipse_Input y-radius cannot be less than zero");

            double x1 = input.x1;
            double y1 = input.y1;
            double x2 = input.x2;
            double y2 = input.y2;

            double rx = input.rx;
            double ry = input.ry;

            double s = Math.Sin(Math.PI * input.theta / 180);
            double c = Math.Cos(Math.PI * input.theta / 180);

            double dx = x1 - x2;
            double dy = y1 - y2;
            double dxy = x1 * y1 - x2 * y2;
            double dx2 = x1 * x1 - x2 * x2;
            double dy2 = y1 * y1 - y2 * y2;

            // correct theta
            input.theta = AddAngle(input.theta % 360, 0);

            // Correct rx and ry, because they may be too small to fit on the points
            double ms = Math.Abs(s), mc = Math.Abs(c), mdx = Math.Abs(dx), mdy = Math.Abs(dy);
            double wid_x = mdx * mc + mdy * ms;
            double wid_y = mdy * mc + mdx * ms;
            if (rx < wid_x) rx = wid_x;
            if (ry < wid_y) ry = wid_y;

            double Cx = ry * ry * c * c + rx * rx * s * s;
            double Cy = ry * ry * s * s + rx * rx * c * c;
            double Cxy = s * c * (rx * rx - ry * ry);

            double Dx = 2.0 * Cx * dx + 2.0 * Cxy * dy;
            double Dy = 2.0 * Cy * dy + 2.0 * Cxy * dx;
            double Dxy = Cx * dx2 + Cy * dy2 + 2.0 * Cxy * dxy;

            // Alright, we get into trouble when dx=dy=0, unsolvable. there is also no arc to draw ;).
            // WE run into a parametric problem when cos(theta)*sin(theta) = 0, and either dy=0 or dy=0, and must
            // solve in a different manner, due to divide by-zero errors!.

            // Simplified solutions needed
            //  - dx and dy are zero, so we need to just pick two angles on the ellipse and offset that are there.
            if (Math.Abs(dx) < tol && Math.Abs(dy) < tol) 
            {
                // Just pick a couple stupid solutions with no delta theta FIXME input angle...
                // so, whats the angles for left and right of centre?

                double rcentre = AddAngle(0, input.theta);
                double lcentre = rcentre + 180;
                double dcentre = rx * Math.Abs(Math.Cos(Math.PI * rcentre / 180)) + ry * Math.Abs(Math.Sin(Math.PI * rcentre / 180));

                yield return new Ellipse_Output(input.x1 - dcentre, input.y1, rcentre, rcentre, rx, ry, false);
                yield return new Ellipse_Output(input.x1 + dcentre, input.y1, lcentre, lcentre, rx, ry, false);
                yield break;
            }
            //  - This means we have a line, and again we need to just pick a couple solutions for that line
            //  - note, this means our code mayy have streched rx or ry by the tolerance...so it will be slightly arcy..
            if (Math.Abs(rx) < tol || Math.Abs(ry) < tol)
            {
                // it's not necessary for dx or dy to be zero, but rx or ry will be zero, and a (respectively) horizontal or vertical line
                // rotated clockwise by the theta of length ry or rx respectly should join the two points.
                // infact we should have sqrt(dx^2+dy^2) = 2*(rx +ry).

                // so we'll shrink if need be, assume its got the right angle and put the centre
                var ln = Math.Sqrt(dx * dx + dy * dy);
                if (rx > ry) rx += (ln - 2 * rx) / 2;
                if (ry > rx) ry += (ln - 2 * ry) / 2;
                // of the ellipse at the midpoint of the two points
                double Xmid = (x1 + x2) / 2;
                double Ymid = (y1 + y2) / 2;
                // figuring out which is the starting and ending theta to use
                double t1u=0, t2u=0;

                if (rx > ry)
                {
                    t1u = Get_Angle(x1 - Xmid, y1 - Ymid) + input.theta;
                    t2u = Get_Angle(x2 - Xmid, y2 - Ymid) + input.theta;
                    // Just return it twice!
                    yield return new Ellipse_Output(Xmid, Ymid, t1u, t2u, rx, ry, false);
                    yield return new Ellipse_Output(Xmid, Ymid, t1u, t2u, rx, ry, false);
                }
                else if (ry > rx)
                {
                    t1u = Get_Angle(x1 - Xmid, y1 - Ymid) + input.theta;
                    t2u = Get_Angle(x2 - Xmid, y2 - Ymid) + input.theta;
                    // Just return it twice!
                    yield return new Ellipse_Output(Xmid, Ymid, t1u, t2u, rx, ry, false);
                    yield return new Ellipse_Output(Xmid, Ymid, t1u, t2u, rx, ry, false);
                }
                yield break;
            }

            // There's two ways to get solutions...we'll keep small numbers out of the bottom of fractions (i.e zeros)
            bool err = false;
            if (Math.Abs(Dx) < Math.Abs(Dy))
            {
                double T1 = Cx * x1 - Cy * (Dx / Dy) * (y1 - (Dxy / Dy)) + Cxy * (y1 - (Dxy / Dy) - x1 * (Dx / Dy));
                double dis = Cx + Cy * ((Dx * Dx) / (Dy * Dy)) - 2.0 * Cxy * (Dx / Dy) - Math.Pow(y1 - (Dxy / Dy) + x1 * (Dx / Dy), 2.0);
                //if (Math.Abs(dis) < tol) dis = 0;
                if (dis < 0) { err = true; dis = 0; }
                double T2 = rx * ry * Math.Sqrt(dis);
                double T3 = Cx + Cy * ((Dx * Dx) / (Dy * Dy)) - 2.0 * Cxy * (Dx / Dy);

                double Xs1 = (T1 + T2) / T3;
                double Xs2 = (T1 - T2) / T3;
                double Ys1 = (Dxy - Xs1 * Dx) / Dy;
                double Ys2 = (Dxy - Xs2 * Dx) / Dy;
                var th = input.theta;

                yield return new Ellipse_Output(Xs1, Ys1, Get_Angle(x1 - Xs1, y1 - Ys1), Get_Angle(x2 - Xs1, y2 - Ys1), rx, ry,err);
                yield return new Ellipse_Output(Xs2, Ys2, Get_Angle(x1 - Xs2, y1 - Ys2), Get_Angle(x2 - Xs2, y2 - Ys2), rx, ry, err);
            }
            else
            {
                double T1 = Cy * y1 - Cx * (Dy / Dx) * (x1 - (Dxy / Dx)) + Cxy * (x1 - (Dxy / Dx) - y1 * (Dy / Dx));
                double dis = Cy + Cx * ((Dy * Dy) / (Dx * Dx)) - 2.0 * Cxy * (Dy / Dx) - Math.Pow(x1 - (Dxy / Dx) + y1 * (Dy / Dx), 2.0);
                //if (Math.Abs(dis) < tol) dis = 0; // allow a bit of negative
                if (dis < 0) { err = true; dis = 0; }
                double T2 = rx * ry * Math.Sqrt(dis);
                double T3 = Cy + Cx * ((Dy * Dy) / (Dx * Dx)) - 2.0 * Cxy * (Dy / Dx);

                double Ys1 = (T1 + T2) / T3;
                double Ys2 = (T1 - T2) / T3;
                double Xs1 = (Dxy - Ys1 * Dy) / Dx;
                double Xs2 = (Dxy - Ys2 * Dy) / Dx;
                var th = input.theta;

                yield return new Ellipse_Output(Xs1, Ys1, Get_Angle(x1 - Xs1, y1 - Ys1), Get_Angle(x2 - Xs1, y2 - Ys1), rx, ry, err);
                yield return new Ellipse_Output(Xs2, Ys2, Get_Angle(x1 - Xs2, y1 - Ys2), Get_Angle(x2 - Xs2, y2 - Ys2), rx, ry, err);
            }

            //Console.WriteLine("dx={0}", dx);
            //Console.WriteLine("dy={0}", dy);
            //Console.WriteLine("dxy={0}", dxy);
            //Console.WriteLine("dx2={0}", dx2);
            //Console.WriteLine("dy2={0}\n", dy2);

            //Console.WriteLine("Dx={0}", Dx);
            //Console.WriteLine("Dy={0}", Dy);
            //Console.WriteLine("Dxy={0}\n", Dxy);
            
        }
        /// <summary>
        /// gets the angle from x-axis of the line drawn by two points
        /// </summary>
        static double Get_Angle(double x, double y)
        {
            var r = Math.Sqrt(x * x + y * y);
            double ret = 0;
            // Quadrants. Easytimes.
            if(x==0 && y==0) return 0;
            var ang = 180.0*Math.Asin(Math.Abs(y) / r)/Math.PI;
            if (x >= 0 && y >= 0)
                ret =  ang;
            else if (x < 0 && y >= 0)
                ret =  180.0 - ang;
            else if (x < 0 && y < 0)
                ret =  ang - 180.0;
            else
                ret =  -ang;

            if (ret > 180) ret -= 360;
            if (ret < -180) ret += 360;

            return ret;
        }
        static void Test(float[] t1s, float[] t2s, bool cw, bool lrg)
        {
            String debug = "";
            int fnd = 0;
            bool gotIt = false;
            for (int i = 0; i < 2; i++)
            {
                float t1 = t1s[i];
                float t2 = t2s[i];
                foreach (var dt in GetAngles(t1, t2))
                {
                    debug += String.Format("t1={0:f1}, t2={1:f1}, dt={2:f1} ", t1, t2, dt);
                    bool? cw1, lr1;
                    if (ArcTry(t1, t2, dt, out cw1, out lr1))
                    {
                        fnd++;
                        debug += " TRUE " + (cw1.HasValue ? cw1.Value ? "cw" : "acw" : "cw&acw") + ", " + (lr1.HasValue ? lr1.Value ? "l" : "s" : "l&s");
                        if (!gotIt && (!cw1.HasValue || cw1.Value == cw1) && (!lr1.HasValue || lr1.Value == lrg))
                        {
                            gotIt = true;
                            debug += " FOUND";
                        }
                    }
                    debug += "\n";
                }
            }
            if (!gotIt || fnd < 4)
                throw new NotImplementedException();
        }
        public static void FindArc(Ellipse_Input ein, Ellipse_Output[] eoa, bool largeArc, bool clockwise, out int useSol, out double angleStart, out double angleSpan, out String debug)
        {
            // "Tests"
            String testString = String.Format("Test(new float[] {0}, new float[] {1}, false, true);", "{" + eoa[0].T1 + "f," + eoa[1].T1 + "f}", "{" + eoa[0].T2 + "f," + eoa[1].T2 + "f}");
            //Test( new float[] { 119.9997276704309f, 0.00027269609041701644f }, new float[] { -179.99990015473779f, -60.000099478751331f }, false, true);
            //Test(new float[] { 3.43008202371647E-09f, 3.43008202371647E-09f }, new float[] { 179.99999999657f, 179.99999999657f }, false, true);

            // need vars... 
            useSol = -1; // has t1 and t2;
            angleSpan = float.NaN; // go from eoa[use].T1 by useDx
            angleStart = float.NaN;

            debug = "";
            bool gotIt = false;
            int fnd = 0;
            for (int i = 0; i < 2; i++)
            {
                float t1 = (float)eoa[i].T1;
                float t2 = (float)eoa[i].T2;
                foreach (var dt in GetAngles(t1, t2))
                {
                    debug += String.Format("t1={0:f1}, t2={1:f1}, dt={2:f1} ", t1, t2, dt);
                    bool? cw, lr;
                    if (ArcTry(t1, t2, dt, out cw, out lr))
                    {
                        fnd++;
                        debug += " TRUE " + (cw.HasValue ? cw.Value ? "cw" : "acw" : "cw&acw") + ", " + (lr.HasValue ? lr.Value ? "l" : "s" : "l&s");
                        if (!gotIt && (!cw.HasValue || cw.Value == clockwise) && (!lr.HasValue || lr.Value == largeArc))
                        {
                            useSol = i;
                            angleStart = t1 + ein.theta;
                            angleSpan = dt;
                            gotIt = true;
                            debug += " FOUND";
                        }
                    }
                    debug += "\n";
                }
            }

            if (!gotIt || fnd < 4)
                throw new NotImplementedException();
        }

        public static void SampleArc(Ellipse_Input ei, Ellipse_Output[] eo, bool big, bool clockwise, double resolution, out System.Drawing.PointF[] pointys)
        {
            List<System.Drawing.PointF> pts = new List<System.Drawing.PointF>();
            SampleArc(ei, eo, big, clockwise,resolution, (x, y) => pts.Add(new System.Drawing.PointF((float)x, (float)y)));
            pointys = pts.ToArray();
        }

        delegate void Assignor(double x, double y);
        static void SampleArc(Ellipse_Input ei, Ellipse_Output[] eo, bool big, bool clockwise, double resolution, Assignor ass)
        {
            int usl; double t1, dt; String dummy;
            EllipseLib.Ellipse.FindArc(ei, eo, big, clockwise, out usl, out t1, out dt, out dummy);
            var us = eo[usl];

            double s_rot = Math.Sin(-(ei.theta * Math.PI) / 180.0);
            double c_rot = Math.Cos(-(ei.theta * Math.PI) / 180.0);
            double rt1 = t1 * Math.PI / 180.0;
            double rdt = dt * Math.PI / 180.0;

            foreach(double theta_now in EllipseUtil.GetThetas(rt1,rdt,resolution, ei.rx,ei.ry))
            {
                double s = Math.Sin(theta_now );
                double c = Math.Cos(theta_now );
                double x_now = c * us.rx;
                double y_now = s * us.ry;
                ass(x_now * c_rot - y_now * s_rot + us.X, x_now * s_rot + y_now * c_rot + us.Y);
            }
        }

        static bool ArcTry(double t1, double t2, double angle, out bool? clockwise, out bool? large)
        {
            //special cases:  angle == 0, either clock or anti.  angle == +=180, large or small
            clockwise = angle > 0;
            large = Math.Abs(angle) > 180.0;

            if (Math.Abs(angle) == 180.0) large = null;
            if (angle == 0) clockwise = null;

            var mang = AddAngle(t1, angle);
            return CompareAngles(mang,t2);
        }
        static IEnumerable<float> GetAngles(float t1, float t2)
        {
            yield return Math.Abs(t1 - t2);
            yield return 360 - Math.Abs(t1 - t2);
            yield return -Math.Abs(t1 - t2);
            yield return -360 + Math.Abs(t1 - t2);
        }
        public static double AddAngle(double angle, double add)
        {
            var rr = angle + add;
            if (rr > 180.0f) rr -= 360.0f;
            if (rr < -180.0f) rr += 360.0f;
            return rr;
        }
        public static bool CompareAngles(double a1, double a2)
        {
            var a1n = AddAngle(a1, 0);
            var a2n = AddAngle(a2, 0);
            var rem = Math.Abs(a1n - a2n) % 360.0f;
            return  rem < tol || Math.Abs(rem-360f) < tol || Math.Abs(rem + 360f) < tol;
        }
    }
}
