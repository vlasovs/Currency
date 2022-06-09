using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
	class Matrix
	{
		static public void Create(out double[][] a, int n, int m)
		{
			a = new double[n][];
			for (int i = 0; i < n; i++)				
				a[i] = new double[m];
		}

		static public void Copy(double[][] a, double[][] b, int n, int m)
		{			
			for (int i = 0; i < n; i++)
				for (int j = 0; j < m; j++)
					b[i][j] = a[i][j];
		}

		static public void Transpose(double[][] a, double[][] b, int n, int m)
		{
			for (int i = 0; i < n; i++)
				for (int j = 0; j < m; j++)
					b[j][i] = a[i][j];
		}

		static public double Norm(double[] a, int n)
		{
			/*
			double sum = 0;
			for (int i = 0; i < n; i++)		
				sum = a[i]* a[i];
			sum /= n;
			return Math.Sqrt(sum);			
			*/

			double max = 0;
			for (int i = 0; i < n; i++)
				if (max < Math.Abs(a[i])) max = Math.Abs(a[i]);
			return max;
		}

		static public void Mul(double[][] a, double[][] b, double[][] c, int k, int m, int n)
		{
			for (int i = 0; i < k; i++)
			{
				for (int j = 0; j < m; j++)
				{
					double sum = 0;
					for (int l = 0; l < n; l++)
					{
						sum += a[i][l] * b[l][j];
					}
					c[i][j] = sum;
				}
			}
		}

		static public void Sum(double[][] a, double[][] b, double[][] c, int n)
		{
			for (int i = 0; i < n; i++)
				for (int j = 0; j < n; j++)
					c[i][j] = a[i][j] + b[i][j];
		}

		static public void Diagonal(double[][] a, int n)
		{
			for (int i = 0; i < n; i++)			
				for (int j = 0; j < n; j++)				
					if (i!=j) a[i][j] = 0;			
		}

		static public bool WithGauss(double[][] M, double[] A, int n)
		{
			int l;
			int bi, r, c, i;
			double[] Temp;
			double a1, b1;
			double el;		
			
			if (n == 0) return false;
			l = n + 1;

			for (i = 0; i < n; i++)
			{
				bi = i;
				for (r = i; r < n; r++)
					if (Math.Abs(M[r][i]) > Math.Abs(M[bi][i])) bi = r;
				Temp = M[bi];
				M[bi] = M[i];
				M[i] = Temp;
				if (M[i][i] == 0.0) return false;
				el = M[i][i];

				M[i][i] = 1;
				for (c = i + 1; c < l; c++)
					if (M[i][c] == 0) M[i][c] = 0;
					else M[i][c] = M[i][c] / el;
				for (r = i + 1; r < n; r++)
				{
					if (M[r][i] == 0) M[r][i] = 0;
					else
					{
						el = M[r][i];
						M[r][i] = 0;
						for (c = i + 1; c < l; c++)
						{
							if (M[r][c] == 0) a1 = 0;
							else a1 = M[r][c];
							if (M[i][c] == 0) b1 = 0;
							else b1 = M[i][c] * el;
							M[r][c] = a1 - b1;
						}
					}
				}
			}

			for (i = n - 1; i >= 0; i--)
			{
				a1 = 0;
				for (c = i + 1; c < n; c++)
					a1 = a1 + M[i][c] * A[c];
				if (a1 == 0) a1 = 0;
				if (M[i][n] == 0) A[i] = -a1;
				else A[i] = M[i][n] - a1;
			}
			
			return true;
		}

		static public bool GetLU(double[][] lu, int[] p, int n, int m)
		{
			int bi, r, c, i, w;
			double[] temp;
			double el, e, a1, b1;
			double z; z = 0;
			for (i = 0; i < n; i++) p[i] = i;
			for (i = 0; i < n - 1; i++)
			{
				bi = i;
				for (r = i; r < n; r++)
				{
					if (Math.Abs(lu[r][i]) > Math.Abs(lu[bi][i])) bi = r;
				}
				w = p[bi]; p[bi] = p[i]; p[i] = w;
				temp = lu[bi]; lu[bi] = lu[i]; lu[i] = temp;
				e = lu[i][i];
				if (e == z) return false;
				for (r = i + 1; r < n; r++)
				{
					if (lu[r][i] == z) lu[r][i] = z;
					else
					{
						el = lu[r][i];
						el /= e;

						lu[r][i] = el;
						for (c = i + 1; c < m; c++)
						{
							if (lu[r][c] == z)
								a1 = z;
							else
								a1 = lu[r][c];
							if (lu[i][c] == z)
								b1 = z;
							else
								b1 = lu[i][c] * el;
							lu[r][c] = a1 - b1;
						}
					}
				}
			}
			return true;
		}

		static public void GetAnswer(double[][] lu, int[] p, double[] b, double[] ans, int n)
		{
			double[] y = new double[n];
			double a1;
			double z; z = 0;
			for (int i = 0; i < n; i++)
			{
				a1 = z;
				for (int j = 0; j < i; j++)
				{
					a1 += (lu[i][j]) * (y[j]);
				}
				if (a1 == z) a1 = z;
				if (b[p[i]] == z)
					y[i] = -a1;
				else
					y[i] = b[p[i]] - (double)a1;
			}
			for (int i = n - 1; i > -1; i--)
			{
				a1 = z;
				for (int j = i + 1; j < n; j++)
					a1 += (lu[i][j] * ans[j]);
				if (a1 == z) a1 = z;
				if (y[i] == z)
					ans[i] = -a1;
				else
					ans[i] = y[i] - a1;
				if (lu[i][i] == z)
					ans[i] = 0;
				else
					ans[i] /= lu[i][i];
			}			
		}
		static public void GetError(double[][] m, double[] b, double[] ans, double[] err, int n)
		{
			double a1, a2, a;
			a = 0;
			for (int i = 0; i < n; i++)
			{
				a1 = a;
				for (int j = 0; j < n; j++)
				{
					a2 = m[i][j];
					a2 *= ans[j];
					a1 += a2;
				}
				err[i] = b[i];
				err[i] -= a1;
			}
			return;
		}
	}
}
