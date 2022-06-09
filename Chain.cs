using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WindowsFormsApp1
{
    class Chain
    {
        private byte[] my_alpha; //number of the formula
        private int[] order;
        private List<double> x;
        private List<double> y;
        private double[] mem1;
        private double[] mem2;
        private double[] chain;

        public Chain()
        { 
            x = new List<double>();
            y = new List<double>();            
        }

        public void Add(double x1, double y1) {
            x.Add(x1);
            y.Add(y1);
        }

        public byte ReadAlpha(int i) {
            return my_alpha[i];
        }
        
        public void Save(String path) {
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine(n().ToString());
                for (int i = 0; i < n(); i++)
                {
                    sw.WriteLine(chain[i].ToString());
                }
            }
        }

        public double Newton(double arg) {
            if (n() == 0) return 0;
            
            int i0 = 2;
            int i1 = 1;
            int i2 = 0;

            double d1 = (y[i1] - y[i0]) / (x[i1] - x[i0]);
            d1 = 1 / d1;
            double d2 = (y[i2] - y[i0]) / (x[i2] - x[i0]);
            d2 = 1 / d2;
            double d3 = (d2 - d1) / (x[i2] - x[i1]);
            d3 = 1 / d3;

            double v1 = (arg - x[i0]);
            double v2 = (arg - x[i1]);

            return (y[i0] + v1 / (d1 + v2 / d3));
            
            /*
            double f, u, v, w;
            int index = n() - 1;
            f = Direct(my_alpha[index], chain[index]);
            for (int i = n() - 2; i >= 0; i--)
            {
                u = chain[i];
                v = arg - x[order[i]];
                w = u + v * f;
                f = Direct(my_alpha[i], w);
            }
            return f;
            */
        }

        private int n() {
            return x.Count;
        }
        private int m() {
            return x.Count+2;
        }

        private double core(int i, int j) {            
            return mem1[order[i]];
        }
        private void core(int i, int j, double val) {
            mem2[order[i]] = val;
        }
        private bool IsZero(double x) {
            const double eps = 1E-14;
            return x <= eps;
        }

        private byte AlphaChoice(int i) {
            double s = 0;
            for (int j = i; j < n(); j++)
                s += core(j, i + 2);
            double w0 = s / (n() - i);
            double w1 = 0;
            double tmp;

            bool b = true;
            //b=i;
            for (int j = i; j < n(); j++)
            {
                if (IsZero(core(j, i + 2)))
                {
                    return 0;
                }
            }           

            s = 0;
            for (int j = i; j < n(); j++)
            {
                tmp = core(j, i + 2);
                s += 1 / tmp;
            }
            w1 = s / (n() - i);

            s = 0;
            for (int j = i; j < n(); j++)
            {
                tmp = core(j, i + 2) - w0;
                s += tmp * tmp;
            }
            double d0 = Math.Sqrt(s / (n() - i));
            s = 0;

            for (int j = i; j < n(); j++)
            {
                tmp = 1 / core(j, i + 2) - w1;
                s += tmp * tmp;
            }

            double d1 = Math.Sqrt(s / (n() - i));

            if (i <= 0) b = false;
            else if (Math.Abs(w0 * w1) < Math.Abs(d0 * d1)) b = true;
            else if (Math.Abs(w0) < Math.Abs(w1) * 1E15) b = true;
            else if (Math.Abs(d0) < Math.Abs(d1)) b = true;
            else b = false;

            int k;
            if (b)
            {
                k = i;
                for (int j = i + 1; j < n(); j++)
                    if ((1 / core(k, i + 2) - w1) >
                        (1 / core(j, i + 2) - w1))
                        k = j;
            }
            else
            {
                k = i;
                for (int j = i + 1; j < n(); j++)
                    if ((core(k, i + 2) - w0) >
                        (core(j, i + 2) - w0))
                        k = j;
            }

            int p = order[i]; order[i] = order[k]; order[k] = p;

            if (b) return 1;
            else return 0;
        }
        private double Direct(byte alpha, double Value) {
            if (alpha == 0) return Value;
            else if (IsZero(Value))
                return 1.7E300;
            else return 1.0 / Value;
        }
        private double Reverse(byte alpha, double Value) {
            if (alpha == 0) return Value;
            else if (IsZero(Value))
                return 1.7E300;
            else return 1.0 / Value;
        }
        public void CallProcessing()
        {
            chain=new double[n()];
            mem1 = new double[n()];
            mem2 = new double[n()];
            if (n() == 0) return;
            order = new int[n()];            
            double dx, dy, tmp;            
            my_alpha = new byte[n()];
            for (int i = 0; i < n(); i++)
            {
                order[i] = i;
                //core(i,0,x[i]);
                //core(i,1,y[i]);
                core(i, 2, y[i]);
            }
            double[] t = mem1; mem1 = mem2; mem2 = t;
            my_alpha[0] = AlphaChoice(0);            

            for (int i = 0; i < n(); i++)
            {
                tmp = core(i, 2);
                tmp = Reverse(my_alpha[0], tmp);
                core(i, 2, tmp);
            }
            t = mem1; mem1 = mem2; mem2 = t;
            chain[0] = core(0, 2);
            double x0, y0, x1, y1;
            for (int j = 0; j < n() - 1; j++)
            {
                y0 = core(j, j + 2);
                x0 = x[order[j]];
                for (int i = j + 1; i < n(); i++)
                {
                    y1 = core(i, j + 2);
                    x1 = x[order[i]];
                    dy = y1 - y0;
                    dx = x1 - x0;
                    tmp = dy / dx;
                    core(i, j + 3, tmp);
                }
                t = mem1; mem1 = mem2; mem2 = t;
                my_alpha[j + 1] = AlphaChoice(j + 1);
                for (int i = j + 1; i < n(); i++)
                {
                    tmp = core(i, j + 3);
                    tmp = Reverse(my_alpha[j + 1], tmp);
                    core(i, j + 3, tmp);
                }
                t = mem1; mem1 = mem2; mem2 = t;
                chain[j + 1] = core(j + 1, j + 3);
            }
        }
    }
}
