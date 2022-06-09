using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    class Linear
    {        
        private List<double> x;
        private List<double> y;        

        public Linear()
        {
            x = new List<double>();
            y = new List<double>();
        }

        public void Add(double x1, double y1)
        {
            x.Add(x1);
            y.Add(y1);
        }

		public double GetTrend(double x1)
		{
			return 2 * y[y.Count - 1] - y[y.Count - 2];

			/*
			int p = 2;//x.Count;
			int k = 2 * p - 1;
			double[] s = new double[k];
			double[] b = new double[p];
			for (int i = 0; i < k; i++)
			{
				s[i] = 0;
			}
			for (int i = 0; i < p; i++)
			{
				b[i] = 0;
			}
			for (int i = 0; i < p; i++)
			{
				double t = x[i];
				double r = 1;
				for (int j = 0; j < k; j++)
				{
					s[j] += r;
					r *= t;
				}
				r = 1;
				for (int j = 0; j < p; j++)
				{
					b[j] += r * y[i];
					r *= t;
				}
			}
			double[][] m;
			Matrix.Create(out m, p, p);
			for (int i = 0; i < p; i++)
			{
				for (int j = 0; j < p; j++)
				{
					m[i][j] = s[i + j];
				}
			}

			double[] a = new double[p];

			int[] o = new int[p];
			Matrix.GetLU(m, o, p, p);
			Matrix.GetAnswer(m, o, b, a, p);

			double sy = 0;
			double mx = 1;
			for (int i = 0; i < p; i++)
			{
				sy += a[i] * mx;
				mx *= x1;
			}			
			return sy;
			*/
		}
	}
}
