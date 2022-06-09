using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace WindowsFormsApp1
{
    class help_func
    {
        public static double SigmaFunc(double Sigma)
        {
            if (Sigma > Math.Log(1.0E300))
            {
                return 1;
            }
            if (Sigma < -Math.Log(1.0E300))
            {
                return 0;
            }
            double res = 1.0 / (1.0 + Math.Exp(-Sigma));
            return res;
        }

        public static double TanhFunc(double Sigma)
        {
            if (Sigma > Math.Log(1.0E300))
            {
                return 1;
            }
            if (Sigma < -Math.Log(1.0E300))
            {
                return -1;
            }
            double e1 = Math.Exp(Sigma);
            double e2 = Math.Exp(-Sigma);
            double res = (e1 - e2) / (e1 + e2);
            return res;
        }
        public static String ReadWord(ref String s)
        {
            int k;
            k = 0;
            s = s.Trim() + " ";
            while (k + 1 < s.Length && !((s[k] == ' ') && (s[k + 1] != ' '))) k++;
            String Result = s.Substring(0, k);
            s = s.Substring(k + 1, s.Length - k - 1);
            return Result;
        }

        public static string SwapComma(ref string s)
        {

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == ',')
                    sb.Append('.');
                else
                    sb.Append(s[i]);
            }
            return sb.ToString();
        }
        public static string SwapDot(ref string s)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '.')
                    sb.Append(',');
                else
                    sb.Append(s[i]);
            }
            return sb.ToString();
        }
    }

    class Layer
    {

        protected Random r;
        protected int EnterCount;
        protected int NeuronCount;
        protected int ExitCount;
        protected int Amount;
        protected double[] w;
        protected double[] DeltaW;
        protected double[] DeltaSigma;
        protected double[] Enter;
        protected double[] Out;
        protected Layer next;
        protected Layer prev;
        protected string name;

        public Layer()
        {
            r = new Random();
            w = null;
            DeltaW = null;
            DeltaSigma = null;
            Enter = null;
            Out = null;
            name = "Layer";
            next = null;
            prev = null;
        }
        public string GetName()
        {
            return name;
        }
        public virtual void SetCoeff(double val, int i)
        {
            this.w[i] = val;
        }

        public virtual double GetCoeff(int i)
        {
            return w[i];
        }

        public virtual void SetDelta(double val, int count)
        {
            DeltaW[count] = val;
        }

        public virtual double GetDelta(int count)
        {
            return DeltaW[count];
        }
        public void SetNeighbors(Layer next, Layer prev)
        {
            this.next = next;
            this.prev = prev;
        }

        public void SetEnter(double[] enter)
        {
            for (int k = 0; k < EnterCount; k++)
            {
                Enter[k] = enter[k];
            }
        }

        public void SetEnter(double[] enter, int count)
        {
            EnterCount = count;
            Enter = new double[count];
            for (int k = 0; k < count; k++)
            {
                Enter[k] = enter[k];
            }
        }
        public virtual int GetCoeffAmount()
        {
            return Amount;
        }

        public int GetEnterCount()
        {
            return EnterCount;
        }

        public int GetNeuronCount()
        {
            return NeuronCount;
        }

        public int GetExitCount()
        {
            return ExitCount;
        }

        public void Fill(int flag, double Amplitude = 1.0)
        {
            for (int k = 0; k < Amount; k++)
            {
                double f = 0;
                if (flag < 0)
                    f = (r.NextDouble() * 2 - 1.0) * Amplitude;
                else if (flag == 0)
                    f = 0;
                else if (flag == 1)
                    f = 1;
                else if (flag == 2)
                    f = 0.5;
                SetCoeff(f, k);
            }
        }
        public double[] GetAnswer()
        {
            return Out;
        }

        public double[] GetDeltaSigma()
        {
            return DeltaSigma;
        }
        public double[] GetDelta()
        {
            return DeltaW;
        }
        public virtual void FreeDelta()
        {
            if (DeltaW != null)
            {
                for (int k = 0; k < Amount; k++)
                {
                    DeltaW[k] = 0;
                }
            }
            if (DeltaSigma != null)
            {
                for (int k = 0; k < NeuronCount; k++)
                {
                    DeltaSigma[k] = 0;
                }
            }
        }

        public void SetDifference(double[] Delta)
        {
            for (int k = 0; k < Amount; k++)
            {
                SetDelta(Delta[k], k);
            }
        }

        public void AplyError(double h)
        {
            for (int k = 0; k < Amount; k++)
            {
                double kof = GetCoeff(k);
                double d = GetDelta(k);
                kof = kof + h * d;
                SetCoeff(kof, k);
            }
        }

        public virtual void GetLayerInfo(ref int n, ref int m, ref double[] array)
        {
            array = w;
            n = ExitCount;
            m = Amount / n;
        }

        public virtual void SetEnterCount(int EnterCount, int NeuronCount, int ExitCount) { }
        public virtual void Execute(double[] Enter) { }
        public virtual void CalcDeltaSigma(double[] ds, int stage, int exit = -1) { }
        public virtual void GetGradient(double[] Gradient, int offset) { }
        public virtual void CalcDelta(double Alpha) { }
        public virtual void Clear() { }
        public virtual void ClearDeltaSigma() { }
    }
    class Dense : Layer
    {

        public Dense() : base()
        {
            this.name = "Dense";
        }

        public override void SetEnterCount(int EnterCount, int NeuronCount, int ExitCount)
        {
            this.EnterCount = EnterCount;
            this.NeuronCount = NeuronCount;
            this.ExitCount = ExitCount;

            Amount = NeuronCount * (EnterCount + 1);
            int a = Amount;
            Enter = new double[EnterCount];
            w = new double[a];
            DeltaW = new double[a];
            Out = new double[ExitCount];
            DeltaSigma = new double[ExitCount];
        }

        public override void Execute(double[] Enter)
        {
            int count = Enter.Count();
            SetEnter(Enter);
            int count1 = 0;
            for (int i = 0; i < NeuronCount; i++)
            {
                double sum = 0;
                for (int e = 0; e < EnterCount; e++)
                {
                    sum += this.Enter[e] * w[count1++];
                }
                sum += w[count1++];
                Out[i] = sum;
            }
            if (next != null)
            {
                next.Execute(Out);
            }
        }

        public override void CalcDeltaSigma(double[] ds, int stage, int exit)
        {

            for (int e = 0; e < NeuronCount; e++)
            {
                double sum = 0;
                double dfo = 1;
                if (ds != null)
                {
                    dfo *= ds[e];
                }
                sum = dfo;//NeuronCount
                if (exit == -1)
                {
                    DeltaSigma[e] += sum;
                }
                else
                {
                    DeltaSigma[e] += e == exit ? sum : 0;
                }
            }

            if (prev != null)
            {
                double[] ds2 = new double[EnterCount];
                for (int k = 0; k < EnterCount; k++)
                {
                    ds2[k] = 0;
                    for (int n = 0; n < NeuronCount; n++)
                    {
                        double fs = DeltaSigma[n];
                        double s = fs * w[n * (EnterCount + 1) + k];
                        ds2[k] += s;
                    }
                }
                prev.CalcDeltaSigma(ds2, 0);
            }
        }

        public override void GetGradient(double[] Gradient, int offset)
        {
            for (int i = 0; i < Amount; i++)
            {
                Gradient[offset + i] = 0;
            }
            for (int i = 0; i < NeuronCount; i++)
            {
                double dfo = 0;
                dfo = DeltaSigma[i];
                for (int l = 0; l < EnterCount; l++)
                {
                    Gradient[offset + i * (EnterCount + 1) + l] = Enter[l] * dfo;
                }
                Gradient[offset + i * (EnterCount + 1) + EnterCount] = dfo;
            }
        }

        public override void CalcDelta(double Alpha)
        {
            int count = 0;
            for (int i = 0; i < NeuronCount; i++)
            {
                double dfo = DeltaSigma[i];
                for (int l = 0; l < EnterCount; l++)
                {
                    DeltaW[count] = Alpha * (DeltaW[count]) + Enter[l] * dfo;
                    count++;
                }
                DeltaW[count] = Alpha * (DeltaW[count]) + dfo;
                count++;
            }
        }

        public override void Clear()
        {
            //Nothing
        }
        public override void ClearDeltaSigma()
        {
            for (int i = 0; i < NeuronCount; i++)
            {
                DeltaSigma[i] = 0;
            }
        }
    }

    class DenseSoftmax : Dense
    {
        public DenseSoftmax() : base()
        {
            name = "DenseSoftmax";
        }
        public override void Execute(double[] Enter)
        {
            int count = Enter.Count();
            SetEnter(Enter);
            int count1 = 0;
            double sum1 = 0;
            for (int i = 0; i < NeuronCount; i++)
            {
                //count1=0;
                double sum2 = 0;
                for (int e = 0; e < EnterCount; e++)
                {
                    sum2 += this.Enter[e] * w[count1++];
                }
                sum2 += w[count1++];
                Out[i] = Math.Exp(sum2);
                sum1 += Out[i];
            }
            for (int i = 0; i < NeuronCount; i++)
            {
                Out[i] /= sum1;
            }
            if (next != null)
            {
                next.Execute(Out);
            }
        }

        public override void CalcDeltaSigma(double[] ds, int stage, int exit = -1)
        {
            for (int e = 0; e < NeuronCount; e++)
            {
                double sum = 0;
                double dfo;
                if (ds != null && exit == -1)
                {
                    for (int q = 0; q < NeuronCount; q++)
                    {
                        double r = (e == q ? 1 : 0);
                        dfo = Out[q] * (r - Out[e]);
                        dfo *= ds[q];
                        sum += dfo;//NeuronCount;
                    }
                }
                else
                {
                    double r = e == exit ? 1 : 0;
                    dfo = Out[exit] * (r - Out[e]);
                    sum += dfo;
                }
                DeltaSigma[e] += sum;
            }
            if (prev != null)
            {
                double[] ds2 = new double[EnterCount];
                for (int k = 0; k < EnterCount; k++)
                {
                    ds2[k] = 0;
                    for (int n = 0; n < NeuronCount; n++)
                    {
                        double fs = DeltaSigma[n];
                        double s = fs * w[n * (EnterCount + 1) + k];
                        ds2[k] += s;
                    }
                }
                prev.CalcDeltaSigma(ds2, 0);
            }
        }
    }

    /********************************************************************/

    class DenseSigmoid : Dense
    {
        public DenseSigmoid() : base()
        {
            name = "DenseSigmoid";
        }
        public override void Execute(double[] Enter)
        {
            int count = Enter.Count();
            SetEnter(Enter);
            int count1 = 0;
            for (int i = 0; i < NeuronCount; i++)
            {
                double sum2 = 0;
                for (int e = 0; e < EnterCount; e++)
                {
                    sum2 += this.Enter[e] * w[count1++];
                }
                sum2 += w[count1++];
                Out[i] = help_func.SigmaFunc(sum2);
            }
            if (next != null)
            {
                next.Execute(Out);
            }
        }

        public override void CalcDeltaSigma(double[] ds, int stage, int exit)
        {
            for (int e = 0; e < NeuronCount; e++)
            {
                double dfo = Out[e] * (1 - Out[e]);
                if (ds != null)
                {
                    dfo *= ds[e];
                }
                if (exit == -1 || exit == e)
                {
                    DeltaSigma[e] = dfo;
                }
                else
                {
                    DeltaSigma[e] = 0;
                }
            }
            if (prev != null)
            {
                double[] ds2 = new double[EnterCount];
                for (int k = 0; k < EnterCount; k++)
                {
                    ds2[k] = 0;
                    for (int n = 0; n < NeuronCount; n++)
                    {
                        double fs = DeltaSigma[n];
                        double s = fs * w[n * (EnterCount + 1) + k];
                        ds2[k] += s;
                    }
                }
                prev.CalcDeltaSigma(ds2, 0);
            }
        }
    }

    /********************************************************************/

    class LSTM : Layer
    {
        protected int CoeffCount;

        protected List<double[]> Stack;
        protected List<double[]>[] Sigma;
        protected List<double[]>[] DeltaSigmas;
        protected List<double>[] Pipe;
        protected double[] Exit;
        protected double[] C;

        public LSTM() : base()
        {
            name = "LSTM";
            Stack = new List<double[]>();
            Exit = null;
            C = null;
        }
        public override void Clear()
        {
            while (Stack.Count > 0)
            {
                for (int i = 0; i < ExitCount; i++)
                {
                    Sigma[i].RemoveAt(Sigma[i].Count - 1);
                    DeltaSigmas[i].RemoveAt(DeltaSigmas[i].Count - 1);
                    Pipe[i].RemoveAt(Pipe[i].Count - 1);
                }
                Stack.RemoveAt(Stack.Count - 1);
            }
            for (int i = 0; i < ExitCount; i++)
            {
                Exit[i] = 0;
                C[i] = 0;
            }
        }
        public override void SetEnterCount(int EnterC, int NeuronCount, int ExitCount)
        {
            this.ExitCount = ExitCount;
            EnterCount = EnterC;
            CoeffCount = (ExitCount / NeuronCount + EnterC + 1);
            this.NeuronCount = NeuronCount;
            Amount = 4 * CoeffCount * ExitCount;
            int a = Amount;
            w = new double[a];
            DeltaW = new double[a];
            DeltaSigma = new double[ExitCount];
            Sigma = new List<double[]>[ExitCount];
            DeltaSigmas = new List<double[]>[ExitCount];
            Pipe = new List<double>[ExitCount];
            for (int i = 0; i < ExitCount; i++)
            {
                Sigma[i] = new List<double[]>();
                DeltaSigmas[i] = new List<double[]>();
                Pipe[i] = new List<double>();
            }
            Exit = new double[ExitCount];
            C = new double[ExitCount];
            Enter = new double[EnterCount];
            Out = new double[ExitCount];
        }
        public void NewStage()
        {
            Stack.Add(new double[ExitCount + EnterCount]);
            for (int j = 0; j < ExitCount; j++)
            {
                Stack.Last()[j] = Exit[j];
            }
            for (int i = 0; i < ExitCount; i++)
            {
                Sigma[i].Add(new double[4]);
                DeltaSigmas[i].Add(new double[4]);
                Pipe[i].Add(C[i]);
            }
        }
        public override void Execute(double[] Enter)
        {
            int count = Enter.Count();
            NewStage();
            int count1 = 0;
            int cur = Stack.Count - 1;
            for (int k = 0; k < EnterCount; k++)
            {
                Stack[cur][k + ExitCount] = Enter[k];
            }
            int tcount = ExitCount / NeuronCount;
            for (int i = 0; i < ExitCount; i++)
            {
                int group = i / tcount;
                for (int k = 0; k < 4; k++)
                {
                    double sum = 0;
                    for (int j = 0; j < tcount; j++)
                    {
                        int p = group * tcount + j;
                        double e = this.Stack[cur][p];
                        double ww = w[count1++];
                        sum += ww * e;
                    }
                    for (int j = 0; j < EnterCount; j++)
                    {
                        double e = this.Stack[cur][ExitCount + j];
                        double ww = w[count1++];
                        sum += ww * e;
                    }
                    sum += w[count1++];
                    Sigma[i][cur][k] = sum;
                }
                double[] ss = Sigma[i][cur];
                double s;
                s = ss[0];
                double s1 = help_func.SigmaFunc(s);
                s = ss[1];
                double s2 = help_func.SigmaFunc(s);
                s = ss[2];
                double t1 = help_func.TanhFunc(s);
                s = ss[3];
                double s3 = help_func.SigmaFunc(s);
                double c1 = s1 * C[i] + s2 * t1;
                double t2 = help_func.TanhFunc(c1);
                double e1 = s3 * t2;
                Exit[i] = e1;
                C[i] = c1;
            }
            for (int i = 0; i < ExitCount; i++)
            {
                Out[i] = Exit[i];
            }
            if (next != null)
            {
                next.Execute(Out);
            }
        }
        public override void CalcDeltaSigma(double[] ds, int stage, int exit)
        {
            int count = ExitCount / NeuronCount;
            double[] c1 = new double[ExitCount];
            double[] de = new double[ExitCount];
            double[] dc = new double[ExitCount];
            double[] prevf = new double[ExitCount];
            for (int l = 0; l < ExitCount; l++)
            {
                c1[l] = C[l];
                if (ds != null)
                {
                    de[l] = ds[l];
                }
                else
                {
                    de[l] = 1;
                }
                dc[l] = 0;
                prevf[l] = 0;
            }
            for (int i = Stack.Count - 1; i >= 0; i--)
            {
                for (int l = 0; l < ExitCount; l++)
                {
                    double tc = help_func.TanhFunc(c1[l]);
                    double _tc = 1 - tc * tc;
                    double[] ss = Sigma[l][i];
                    double[] dss = DeltaSigmas[l][i];
                    double f = help_func.SigmaFunc(ss[0]); //f
                    double _f = f * (1 - f);
                    double q = help_func.SigmaFunc(ss[1]); //i
                    double _q = q * (1 - q);
                    double g = help_func.TanhFunc(ss[2]);  //g
                    double _g = 1 - g * g;
                    double o = help_func.SigmaFunc(ss[3]); //o
                    double _o = o * (1 - o);
                    dc[l] = de[l] * o * _tc + dc[l] * prevf[l];
                    c1[l] = Pipe[l][i];
                    double res1 = dc[l] * _f * c1[l];
                    dss[0] += res1;
                    double res2 = dc[l] * _q * g;
                    dss[1] += res2;
                    double res3 = dc[l] * _g * q;
                    dss[2] += res3;
                    double res4 = de[l] * _o * tc;
                    dss[3] += res4;
                    prevf[l] = f;
                }
                for (int p = 0; p < ExitCount; p++)
                {
                    de[p] = 0;
                    int number = p % count;
                    int group = p / count;
                    for (int l = 0; l < count; l++)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            int r = group * count + l;
                            de[p] += DeltaSigmas[r][i][k] * w[(4 * r + k) * CoeffCount + number];
                        }
                    }
                }
            }
            if (prev != null)
            {
                double[] ds2 = new double[EnterCount];
                int c = Stack.Count - 1;
                for (int i = c; i >= 0; i--)
                {
                    for (int e = 0; e < EnterCount; e++)
                    {
                        ds2[e] = 0;
                        for (int l = 0; l < ExitCount; l++)
                        {
                            for (int k = 0; k < 4; k++)
                            {
                                double dss = DeltaSigmas[l][i][k];
                                ds2[e] += dss * w[(4 * l + k) * CoeffCount + e + count];
                            }
                        }
                    }
                    prev.CalcDeltaSigma(ds2, i);
                }
            }
        }
        public override void GetGradient(double[] Gradient, int offset)
        {
            for (int i = 0; i < Amount; i++)
            {
                Gradient[offset + i] = 0;
            }
            int tcount = ExitCount / NeuronCount;
            for (int l = 0; l < ExitCount; l++)
            {
                double ds1, e;
                int group = l / tcount;
                for (int n = 0; n < Stack.Count; n++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        ds1 = DeltaSigmas[l][n][k];
                        double ed;
                        for (int j = 0; j < tcount; j++)
                        {
                            int p = group * tcount + j;
                            e = Stack[n][p];
                            ed = e * ds1;
                            Gradient[offset + (4 * l + k) * CoeffCount + j] += ed;
                        }
                        for (int i = 0; i < EnterCount; i++)
                        {
                            e = Stack[n][ExitCount + i];
                            ed = e * ds1;
                            Gradient[offset + (4 * l + k) * CoeffCount + tcount + i] += ed;
                        }
                        Gradient[offset + (4 * l + k + 1) * CoeffCount - 1] += ds1;
                    }
                }
            }
        }
        public override void CalcDelta(double Alpha)
        {
            int index;
            double[] Gradient = new double[Amount];
            for (int i = 0; i < Amount; i++)
            {
                Gradient[i] = 0;
            }
            GetGradient(Gradient, 0);
            for (int k = 0; k < Amount; k++)
            {
                DeltaW[k] = Alpha * DeltaW[k] + Gradient[k];
            }
        }
        public override void ClearDeltaSigma()
        {
            for (int l = 0; l < ExitCount; l++)
            {
                for (int i = 0; i < Stack.Count; i++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        DeltaSigmas[l][i][k] = 0;
                    }
                }
            }
        }
    }

    class Embedding : Layer
    {
        public Embedding() : base()
        {
            name = "Embedding";
        }
        public override void SetEnterCount(int EnterC, int NeuronC, int ExitC)
        {
            EnterCount = EnterC;
            NeuronCount = NeuronC;
            ExitCount = ExitC;
            Enter = new double[1];
            int a = Amount = NeuronCount * ExitCount;
            w = new double[a];
            DeltaW = new double[a];
            DeltaSigma = new double[a];
            Out = new double[ExitCount];
        }
        public override void Execute(double[] Enter)
        {
            int count = Enter.Count();
            SetEnter(Enter, count);
            for (int e = 0; e < EnterCount; e++)
            {
                for (int i = 0; i < ExitCount; i++)
                {
                    Out[i] = w[(int)Enter[e] * ExitCount + i];
                }
                if (next != null)
                {
                    next.Execute(Out);
                }
            }
        }
        public override void CalcDeltaSigma(double[] ds, int stage, int exit)
        {
            for (int i = 0; i < ExitCount; i++)
            {
                DeltaSigma[(int)Enter[stage] * ExitCount + i] += ds[i];
            }
        }
        public override void GetGradient(double[] Gradient, int offset)
        {
            for (int i = 0; i < Amount; i++)
            {
                double dfo = DeltaSigma[i];
                Gradient[offset + i] = dfo;
            }
        }
        public override void CalcDelta(double Alpha)
        {
            for (int i = 0; i < Amount; i++)
            {
                double dfo = DeltaSigma[i];
                DeltaW[i] = Alpha * (DeltaW[i]) + dfo;
            }
        }
        public override void ClearDeltaSigma()
        {
            for (int i = 0; i < Amount; i++)
            {
                DeltaSigma[i] = 0;
            }
        }
        public override void GetLayerInfo(ref int n, ref int m, ref double[] array)
        {
            array = w;
            n = NeuronCount;
            m = Amount / n;
        }
    }

    // NN - Neural_Net

    class NN : Layer
    {
        double[] g;
        double[] v;
        double[] m;
        List<Layer> layers;
        public NN() : base()
        {
            name = "NN";
            layers = new List<Layer>();
        }

        private int FindIndex(ref int count)
        {
            int a = layers[0].GetCoeffAmount();
            int i = 0;
            while (count >= a)
            {
                count -= a;
                i++;
                a = layers[i].GetCoeffAmount();
            }
            return i;
        }
        public override void SetCoeff(double val, int count)
        {
            int i = FindIndex(ref count);
            layers[i].SetCoeff(val, count);
        }
        public override double GetCoeff(int count)
        {
            int i = FindIndex(ref count);
            return layers[i].GetCoeff(count);
        }
        public override void SetDelta(double val, int count)
        {
            int i = FindIndex(ref count);
            layers[i].SetDelta(val, count);
        }

        public override double GetDelta(int count)
        {
            int i = FindIndex(ref count);
            return layers[i].GetDelta(count);
        }
        public void Load(string FileName)
        {
            int ec1 = 0, nc1 = 0, xc1 = 0;
            bool first = true;
            if (File.Exists(FileName))
            {
                layers.Clear();

                string[] alltext = File.ReadAllLines(FileName);

                int line = 0;

                while (line < alltext.Length)
                {
                    string text = alltext[line++];
                    Layer l;
                    if (text == "Embedding")
                    {
                        l = new Embedding();
                    }
                    else if (text == "LSTM")
                    {
                        l = new LSTM();
                    }
                    else if (text == "DenseSoftmax")
                    {
                        l = new DenseSoftmax();
                    }
                    else if (text == "DenseSigmoid")
                    {
                        l = new DenseSigmoid();
                    }
                    else if (text == "Dense")
                    {
                        l = new Dense();
                    }
                    else
                    {
                        break;
                    }

                    int ec, nc, xc;
                    ec = int.Parse(alltext[line++]);
                    nc = int.Parse(alltext[line++]);
                    xc = int.Parse(alltext[line++]);
                    l.SetEnterCount(ec, nc, xc);
                    if (first)
                    {
                        ec1 = ec;
                        nc1 = nc;
                        first = false;
                    }
                    xc1 = xc;
                    string s = alltext[line++];
                    help_func.SwapComma(ref s);
                    for (int r = 0; r < l.GetCoeffAmount(); r++)
                    {
                        string word = help_func.ReadWord(ref s);
                        double we = double.Parse(word);
                        l.SetCoeff(we, r);
                    }
                    layers.Add(l);
                }
            }
            SetEnterCount(ec1, nc1, xc1);
            int ls = layers.Count;
            for (int i = 0; i < ls; i++)
            {
                Layer next = null;
                Layer prev = null;
                if (i > 0)
                {
                    prev = layers[i - 1];
                }
                if (i < layers.Count - 1)
                {
                    next = layers[i + 1];
                }
                layers[i].SetNeighbors(next, prev);
            }
        }

        public void Save(string FileName)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < layers.Count; i++)
            {
                sb.AppendLine(layers[i].GetName());
                sb.AppendLine(layers[i].GetEnterCount().ToString());
                sb.AppendLine(layers[i].GetNeuronCount().ToString());
                sb.AppendLine(layers[i].GetExitCount().ToString());

                int Amount = layers[i].GetCoeffAmount();

                for (int k = 0; k < Amount; k++)
                {
                    sb.Append(layers[i].GetCoeff(k).ToString());
                    sb.Append(" ");
                }
                sb.Append("\n");
            }
            File.WriteAllText(FileName, sb.ToString());
        }

        public override int GetCoeffAmount()
        {
            return Amount;
        }

        public override void Clear()
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].Clear();
            }
        }
        public override void SetEnterCount(int EnterCount, int NeuronCount, int ExitCount)
        {
            this.ExitCount = ExitCount;
            this.EnterCount = EnterCount;
            this.NeuronCount = NeuronCount;
            w = new double[1];
            DeltaW = null;
            DeltaSigma = new double[NeuronCount];
            Enter = new double[EnterCount];
            Out = new double[ExitCount];
            int a = 0;
            for (int i = 0; i < layers.Count; i++)
            {
                a += layers[i].GetCoeffAmount();
            }
            Amount = a;
            g = new double[a];
            v = new double[a];
            m = new double[a];
        }

        public override void Execute(double[] Enter)
        {

            layers[0].Execute(Enter);
            int last = layers.Count - 1;
            int s = layers[last].GetNeuronCount();
            for (int i = 0; i < s; i++)
            {
                double ex = layers[last].GetAnswer()[i];
                Out[i] = ex;
            }
        }

        public override void CalcDeltaSigma(double[] ds, int stage, int exit)
        {
            ClearDeltaSigma();
            int s = layers.Count - 1;
            layers[s].CalcDeltaSigma(ds, 0, exit);
        }

        public override void GetGradient(double[] Gradient, int offset)
        {
            int count = 0;
            int s = layers.Count;
            for (int i = 0; i < s; i++)
            {
                layers[i].GetGradient(Gradient, offset);
                offset += layers[i].GetCoeffAmount();
            }
        }
        public override void CalcDelta(double Alpha)
        {
            int s = layers.Count;
            for (int i = 0; i < s; i++)
            {
                layers[i].CalcDelta(Alpha);
            }
        }
        public override void FreeDelta()
        {
            int s = layers.Count;
            for (int i = 0; i < s; i++)
            {
                layers[i].FreeDelta();
            }
            for (int i = 0; i < Amount; i++)
            {
                g[i] = 0;
                v[i] = 0;
                m[i] = 0;
            }
        }

        public override void ClearDeltaSigma()
        {
            int s = layers.Count;
            for (int i = 0; i < s; i++)
            {
                layers[i].ClearDeltaSigma();
            }
        }

        public void AdaMax(int n, double beta1, double beta2)
        {
            int s = layers.Count;
            for (int i = 0; i < s; i++)
            {
                layers[i].CalcDelta(0);
            }
            int fc = Amount;

            //double beta1=0.9l;
            //double beta2=0.999l;
            double epsilon = 1E-8;

            double gmax = 0;
            for (int k = 0; k < fc; k++)
            {
                g[k] = GetDelta(k);
                if (Math.Abs(g[k]) > gmax) gmax = Math.Abs(g[k]);
            }

            // AdaMax
            for (int k = 0; k < fc; k++)
            {
                m[k] = beta1 * m[k] + (1 - beta1) * g[k];
                if (beta2 * v[k] > gmax)
                    v[k] = beta2 * v[k];
                else
                    v[k] = gmax;
                double delta = (1.0 / (1.0 - Math.Pow(beta1, n + 1))) * m[k] / v[k];
                SetDelta(delta, k);
            }
        }

        public void Adam(int n, double beta1, double beta2)
        {
            int s = layers.Count;
            for (int i = 0; i < s; i++)
            {
                layers[i].CalcDelta(0);
            }
            int fc = Amount;

            //double beta1=0.9l;
            //double beta2=0.999l;
            double epsilon = 1E-8;

            double gmax = 0;
            for (int k = 0; k < fc; k++)
            {
                g[k] = GetDelta(k);
                if (Math.Abs(g[k]) > gmax) gmax = Math.Abs(g[k]);
            }
            // Adam
            for (int k = 0; k < fc; k++)
            {

                m[k] = beta1 * m[k] + (1 - beta1) * g[k];
                v[k] = beta2 * v[k] + (1 - beta2) * g[k] * g[k];
                double m_;
                double v_;
                m_ = m[k] / (1 - Math.Pow(beta1, n + 1));
                v_ = v[k] / (1 - Math.Pow(beta2, n + 1));
                double delta = m_ / (Math.Sqrt(v_) + epsilon);
                SetDelta(delta, k);
            }
        }

        public void GetInfo(int layer, ref int n, ref int m, ref double[] array)
        {
            layers[layer].GetLayerInfo(ref n, ref m, ref array);
        }
    }

    class TNeuroNet
    {
        private String ReadWord(ref String s)
        {
            int k;
            k = 0;
            s = s.Trim() + " ";
            while (k + 1 < s.Length && !((s[k] == ' ') && (s[k + 1] != ' '))) k++;
            String Result = s.Substring(0, k);
            s = s.Substring(k + 1, s.Length - k - 1);
            return Result;
        }

        public TNeuroNet(String FileName)
        {

            if (File.Exists(FileName))
            {
                // Read entire text file content in one string    
                string[] text = File.ReadAllLines(FileName);
                int n, k, ec;
                String s, word;
                int line = 0;
                ec = int.Parse(text[line]); line++;
                SetEnterCount(ec);
                k = int.Parse(text[line]); line++;
                SetLayerCount(k);
                s = text[line]; line++;
                for (int i = 0; i < k; i++)
                {
                    word = ReadWord(ref s);
                    n = int.Parse(word);
                    SetNeuronCount(i, n);
                }

                for (int r = 0; r < LayerCount; r++)
                {
                    line++;
                    int nc = NeuronCount[r];
                    for (int i = 0; i < nc; i++)
                    {
                        s = text[line]; line++;
                        int no;
                        if (r == 0)
                            no = EnterCount + 1;
                        else
                            no = NeuronCount[r - 1] + 1;

                        for (int j = 0; j < no; j++)
                        {
                            word = ReadWord(ref s);
                            double we = double.Parse(word);
                            w[r][i][j] = we;
                        }
                    }
                }
            }
        }

        public void Save(String FileName)
        {
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(FileName, false))
            {
                String s, word;
                file.WriteLine(EnterCount.ToString());
                file.WriteLine(LayerCount.ToString());
                s = "";
                for (int i = 0; i < LayerCount; i++)
                {
                    s += NeuronCount[i].ToString() + " ";
                }
                file.WriteLine(s);

                for (int k = 0; k < LayerCount; k++)
                {
                    s = k.ToString() + " layer";
                    file.WriteLine(s);
                    for (int i = 0; i < w[k].Length; i++)
                    {
                        s = "";
                        for (int j = 0; j < w[k][i].Length; j++)
                        {
                            word = w[k][i][j].ToString();
                            s = s + word + " ";
                        }
                        file.WriteLine(s);
                    }
                }
            }
        }
        public double[][] GetLayer(int k)
        {
            return w[k];
        }

        public double[] GetNeuron(int k, int i)
        {
            return w[k][i];
        }

        public void SetEnterCount(int ECount)
        {
            Enter = new double[ECount];
            EnterCount = ECount;
            Amount = 0;
        }

        public void SetLayerCount(int nk)
        {
            w = new double[nk][][];
            OutMatrix = new double[nk][];
            NeuronCount = new int[nk];
            LayerCount = nk;
        }

        public void SetNeuronCount(int k, int nn)
        {
            w[k] = new double[nn][];
            OutMatrix[k] = new double[nn];
            NeuronCount[k] = nn;
            int c;
            if (k == 0) c = EnterCount + 1;
            else c = NeuronCount[k - 1] + 1;

            Amount += c * nn;
            for (int i = 0; i < nn; i++)
            {
                w[k][i] = new double[c];
            }
            if (k == LayerCount - 1)
            {
                DeltaW = new double[LayerCount][][];
                DeltaMatrix = new double[LayerCount][];

                for (int p = 0; p < LayerCount; p++)
                {
                    int nc = NeuronCount[p];

                    int no = EnterCount + 1;
                    if (p > 0) no = NeuronCount[p - 1] + 1;

                    DeltaMatrix[p] = new double[nc];
                    DeltaW[p] = new double[nc][];

                    for (int i = 0; i < nc; i++)
                    {
                        DeltaW[p][i] = new double[no];
                    }
                }
            }
        }

        public int GetCoeffAmount()
        {
            return Amount;
        }

        public double SigmaFunc(double Sigma)
        {
            return Sigma;
            //return 1.0 / (1.0 + Math.Exp(-Sigma));
        }
        public double DiffSigma(int k, int i)
        {
            return 1.0;
            //OutMatrix[k][i];
            //return OutMatrix[k][i] * (1 - OutMatrix[k][i]);
        }

        public void Fill(int flag)
        {
            var rand = new Random();
            for (int k = 0; k < w.Length; k++)
                for (int i = 0; i < w[k].Length; i++)
                    for (int j = 0; j < w[k][i].Length; j++)
                    {
                        if (flag < 0)
                            w[k][i][j] = (rand.NextDouble() - 0.5);
                        else if (flag == 0)
                            w[k][i][j] = 0;
                        else if (flag == 1)
                            w[k][i][j] = 1;
                        else if (flag == 2)
                            w[k][i][j] = 0.5;
                    }
        }

        public void Execute(double[] Enter)
        {
            for (int i = 0; i < EnterCount; i++)
                this.Enter[i] = Enter[i];

            for (int i = 0; i < NeuronCount[0]; i++)
            {
                double sum = 0;
                for (int j = 0; j < EnterCount; j++)
                    sum += this.Enter[j] * w[0][i][j];
                sum += w[0][i][EnterCount];
                OutMatrix[0][i] = SigmaFunc(sum);
            }
            for (int k = 1; k < LayerCount; k++)
            {
                for (int i = 0; i < w[k].Length; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < w[k][i].Length - 1; j++)
                        sum += OutMatrix[k - 1][j] * w[k][i][j];
                    sum += w[k][i][w[k][i].Length - 1];
                    OutMatrix[k][i] = SigmaFunc(sum);
                }
            }
        }

        public double[] GetAnswer()
        {
            return OutMatrix[LayerCount - 1];
        }

        public void FreeDelta()
        {
            for (int k = 0; k < LayerCount; k++)
            {
                int nc = NeuronCount[k];
                int no = EnterCount;
                if (k > 0) no = NeuronCount[k - 1];

                for (int i = 0; i < nc; i++)
                    for (int j = 0; j < no; j++)
                        DeltaW[k][i][j] = 0;
            }
        }
        public void GetGradient(double[] Gradient)
        {
            double sum, d;
            int LastLayer = NeuronCount[LayerCount - 1];
            for (int i = 0; i < LastLayer; i++)
            {
                d = DiffSigma(w.Length - 1, i);
                DeltaMatrix[w.Length - 1][i] = d;
            }
            for (int k = w.Length - 2; k >= 0; k--)
            {
                for (int i = 0; i < w[k].Length; i++)
                {
                    sum = 0;
                    for (int j = 0; j < w[k + 1].Length; j++)
                    {
                        sum += DeltaMatrix[k + 1][j] * w[k + 1][j][i];
                    }
                    DeltaMatrix[k][i] = sum * DiffSigma(k, i);
                }
            }

            for (int i = 0; i < w[0].Length; i++)
            {
                for (int j = 0; j < w[0][i].Length - 1; j++)
                {
                    DeltaW[0][i][j] = DeltaMatrix[0][i] * Enter[j];
                }
                DeltaW[0][i][w[0][i].Length - 1] = DeltaMatrix[0][i];
            }
            for (int k = 1; k < w.Length; k++)
                for (int i = 0; i < w[k].Length; i++)
                {
                    for (int j = 0; j < w[k][i].Length - 1; j++)
                    {
                        DeltaW[k][i][j] = DeltaMatrix[k][i] * OutMatrix[k - 1][j];
                    }
                    DeltaW[k][i][w[k][i].Length - 1] = DeltaMatrix[k][i];
                }
            int count = 0;
            for (int k = 0; k < w.Length; k++)
                for (int i = 0; i < w[k].Length; i++)
                    for (int j = 0; j < w[k][i].Length; j++)
                    {
                        Gradient[count] = DeltaW[k][i][j];
                        count++;
                    }
        }
        public void SetDifference(double[] Delta)
        {
            int count = 0;
            for (int k = 0; k < w.Length; k++)
                for (int i = 0; i < w[k].Length; i++)
                    for (int j = 0; j < w[k][i].Length; j++)
                    {
                        DeltaW[k][i][j] = Delta[count];
                        count++;
                    }
        }

        public void CalcDelta(double[] CorrectAnswer, double Alpha)
        {
            double sum, d;
            int LastLayerSize = NeuronCount[LayerCount - 1];
            for (int i = 0; i < LastLayerSize; i++)
            {
                d = (CorrectAnswer[i] - GetAnswer()[i]) * DiffSigma(w.Length - 1, i);
                DeltaMatrix[w.Length - 1][i] = d;
            }
            for (int k = w.Length - 2; k >= 0; k--)
            {
                for (int i = 0; i < w[k].Length; i++)
                {
                    sum = 0;
                    for (int j = 0; j < w[k + 1].Length; j++)
                    {
                        sum += DeltaMatrix[k + 1][j] * w[k + 1][j][i];
                    }
                    DeltaMatrix[k][i] = sum * DiffSigma(k, i);
                }
            }

            for (int i = 0; i < w[0].Length; i++)
            {
                for (int j = 0; j < w[0][i].Length - 1; j++)
                {
                    DeltaW[0][i][j] = DeltaW[0][i][j] * Alpha + DeltaMatrix[0][i] * Enter[j];
                }
                DeltaW[0][i][w[0][i].Length - 1] = DeltaW[0][i][w[0][i].Length - 1] * Alpha + DeltaMatrix[0][i];
            }
            for (int k = 1; k < w.Length; k++)
                for (int i = 0; i < w[k].Length; i++)
                {
                    for (int j = 0; j < w[k][i].Length - 1; j++)
                    {
                        DeltaW[k][i][j] = DeltaW[k][i][j] * Alpha + DeltaMatrix[k][i] * OutMatrix[k - 1][j];
                    }
                    DeltaW[k][i][w[k][i].Length - 1] = DeltaW[k][i][w[k][i].Length - 1] * Alpha + DeltaMatrix[k][i];
                }
        }

        private void RandomDirection()
        {
            var rand = new Random();
            for (int k = 0; k < DeltaW.Length; k++)
                for (int i = 0; i < DeltaW[k].Length; i++)
                    for (int j = 0; j < DeltaW[k][i].Length; j++)
                    {
                        DeltaW[k][i][j] = rand.NextDouble() * 2 - 2;
                    }
        }

        public void AplyError(double h)
        {
            for (int k = 0; k < w.Length; k++)
                for (int i = 0; i < w[k].Length; i++)
                    for (int j = 0; j < w[k][i].Length; j++)
                        w[k][i][j] = w[k][i][j] + h * DeltaW[k][i][j];
        }

        private double Func1(double t, double[] CorrectAnswer)
        {
            AplyError(t);
            double[] v1 = CorrectAnswer;
            double[] v2 = GetAnswer();
            double Result = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                Result += (v1[i] - v2[i]) * (v1[i] - v2[i]);
            }
            AplyError(-t);
            return Math.Sqrt(Result / v1.Length);
        }

        private void GoldenSearch(double[] CorrectAnswer, double l, double r, double EpsArg, bool FindMax,
                                 int MaxIteration, ref double Arg, ref double Ans)
        {
            double Sech;
            double shag, vect;
            double t, d;
            double f, fn;
            double a, b, Eps;
            int IterationCount;
            IterationCount = 0;
            Sech = (Math.Sqrt(5.0) - 1.0) / 2.0;
            a = l; b = r;
            Eps = EpsArg;
            shag = (b - a) * (1 - Sech);
            t = a + shag;
            vect = 1;
            f = Func1(t, CorrectAnswer);

            while ((shag > Eps / 2) && (IterationCount < MaxIteration))
            {
                shag = shag * (Sech);
                t = t + shag * vect;
                fn = Func1(t, CorrectAnswer);
                if (fn > f == FindMax)
                    f = fn;
                else
                {
                    vect = -vect;
                    t += shag * vect;
                }
                IterationCount++;
            }
            d = Func1(t, CorrectAnswer);
            Ans = d;
            Arg = t;
        }

        public void CalcStep(double[] CorrectAnswer, double h)
        {
            double f, f1, f2, tou, tou1, tou2, bound;
            bound = 16;
            f = Func1(0, CorrectAnswer);
            f1 = Func1(0, CorrectAnswer);
            tou = 0;
            tou1 = tou;
            tou = 1.0 / 1024.0;
            f2 = Func1(tou, CorrectAnswer);
            while ((f1 >= f2) && (tou < bound))
            {
                tou = tou * 2;
                f2 = Func1(tou, CorrectAnswer);
            }
            if (tou >= bound) tou = bound;
            else
            {
                tou2 = tou;
                GoldenSearch(CorrectAnswer, tou1, tou2, 1E-16, false, 1000, ref tou, ref f1);
            }
            h = tou;
        }

        private int EnterCount;
        private int LayerCount;
        private int[] NeuronCount;
        private int Amount;

        private double[][][] w;
        private double[][] OutMatrix;
        private double[][] DeltaMatrix;
        private double[][][] DeltaW;
        private double[] Enter;

    };
    class LSTM1
    {
        public int EnterCount;
        public int CoeffCount;
        public int Amount;

        private String ReadWord(ref String s)
        {
            int k;
            k = 0;
            s = s.Trim() + " ";
            while (k + 1 < s.Length && !((s[k] == ' ') && (s[k + 1] != ' '))) k++;
            String Result = s.Substring(0, k);
            s = s.Substring(k + 1, s.Length - k - 1);
            return Result;
        }

        public LSTM1(String FileName)
        {
            Enter = new List<double[]>();
            Sigma = new List<double[]>();
            DeltaSigma = new List<double[]>();
            Pipe = new List<double>();
            DeltaPipe = new List<double>();

            if (File.Exists(FileName))
            {
                // Read entire text file content in one string    
                string[] text = File.ReadAllLines(FileName);
                int ec;
                String s, word;
                int line = 0;
                ec = int.Parse(text[line]); line++;

                SetEnterCount(ec);
                for (int k = 0; k < 4; k++)
                {
                    s = text[line]; line++;
                    for (int r = 0; r < CoeffCount; r++)
                    {
                        word = ReadWord(ref s);
                        double we = double.Parse(word);
                        int i = k * CoeffCount + r;
                        w[i] = we;
                    }
                }
            }
        }

        public void Save(String FileName)
        {
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(FileName, false))
            {
                String s;
                file.WriteLine(EnterCount.ToString());
                for (int k = 0; k < 4; k++)
                {
                    s = "";
                    for (int i = 0; i < CoeffCount; i++)
                    {
                        s += w[k * CoeffCount + i].ToString() + " ";
                    }
                    file.WriteLine(s);
                }
            }
        }

        private void SetEnterCount(int EnterC)
        {
            EnterCount = EnterC;
            CoeffCount = (EnterC + 2);
            Amount = 4 * CoeffCount;

            w = new double[Amount];
            DeltaW = new double[Amount];
        }
        private double SigmaFunc(double Sigma)
        {
            if (Sigma > Math.Log(1.0E300))
            {
                return 1;
            }
            if (Sigma < -Math.Log(1.0E300))
            {
                return 0;
            }
            double res = 1.0 / (1.0 + Math.Exp(-Sigma));
            return res;
        }

        private double TanhFunc(double Sigma)
        {
            if (Sigma > Math.Log(1.0E300))
            {
                return 1;
            }
            if (Sigma < -Math.Log(1.0E300))
            {
                return -1;
            }
            double e1 = Math.Exp(Sigma);
            double e2 = Math.Exp(-Sigma);
            double res = (e1 - e2) / (e1 + e2);
            return res;
        }
        private double DTanhFunc(double Sigma)
        {
            if (Sigma > Math.Log(1.0E300))
            {
                return 1;
            }
            if (Sigma < -Math.Log(1.0E300))
            {
                return -1;
            }
            double e1 = Math.Exp(Sigma);
            double e2 = Math.Exp(-Sigma);
            double res = 2 / (e1 + e2);
            return res * res;
        }
        public void Fill(int flag)
        {
            Random rnd = new Random();
            for (int k = 0; k < Amount; k++)
            {
                if (flag < 0)
                    w[k] = rnd.NextDouble() * 2 - 1.0;
                else if (flag == 0)
                    w[k] = 0;
                else if (flag == 1)
                    w[k] = 1;
                else if (flag == 2)
                    w[k] = 0.5;
            }
        }

        public void Clear()
        {
            Enter.Clear();
            DeltaSigma.Clear();
            Enter.Clear();
            Sigma.Clear();
            DeltaSigma.Clear();
            DeltaPipe.Clear();
            Exit = 0;
            C = 1;
        }

        private void NewStage()
        {
            Enter.Add(new double[EnterCount + 1]);
            Sigma.Add(new double[4]);
            DeltaSigma.Add(new double[4]);
            Enter[Enter.Count - 1][0] = Exit;
            Pipe.Add(C);
        }

        public void Execute(double[] Enter)
        {
            NewStage();
            int cur = this.Enter.Count - 1;
            for (int i = 0; i < EnterCount; i++)
                this.Enter[cur][i + 1] = Enter[i];
            int p = CoeffCount;
            int count = 0;
            for (int k = 0; k < 4; k++)
            {
                Sigma[cur][k] = 0;
                for (int j = 0; j < EnterCount + 1; j++)
                {
                    Sigma[cur][k] += w[count++] * (this.Enter[cur][j]);
                }
                Sigma[cur][k] += w[count++];
            }
            int c = 0;
            double s1 = SigmaFunc(Sigma[cur][0]);
            double s2 = SigmaFunc(Sigma[cur][1]);
            double t1 = TanhFunc(Sigma[cur][2]);
            double s3 = SigmaFunc(Sigma[cur][3]);
            double c1 = s1 * C + s2 * t1;
            double t2 = TanhFunc(c1);
            double e1 = s3 * t2;

            Exit = e1;
            C = c1;
        }

        public double GetAnswer()
        {
            return Exit;
        }

        public void FreeDelta()
        {
            for (int k = 0; k < Amount; k++)
            {
                DeltaW[k] = 0;
            }
        }

        public void GetGradient(double[] Gradient, int offset)
        {
            for (int i = 0; i < Amount; i++)
            {
                Gradient[offset + i] = 0;
            }
            double c1 = C;
            double e1 = 1;
            for (int i = Enter.Count() - 1; i >= 0; i--)
            {
                double tc = TanhFunc(c1);
                double _tc = DTanhFunc(c1);

                double o = SigmaFunc(Sigma[i][3]);
                double _o = o * (1 - o);

                double p = SigmaFunc(Sigma[i][2]);
                double _p = p * (1 - p);

                double q = SigmaFunc(Sigma[i][1]);
                double _q = q * (1 - q);

                double f = SigmaFunc(Sigma[i][0]);
                double _f = f * (1 - f);

                double plus = _tc * o * e1;

                DeltaSigma[i][3] = _o * tc * e1;
                DeltaSigma[i][2] = _p * plus * q;
                DeltaSigma[i][1] = _q * plus * p;
                DeltaSigma[i][0] = _f * plus * Pipe[i];
                c1 = Pipe[i];

                e1 = DeltaSigma[i][3] * w[3 * CoeffCount] +
                DeltaSigma[i][2] * w[2 * CoeffCount] +
                DeltaSigma[i][1] * w[1 * CoeffCount] +
                DeltaSigma[i][0] * w[0 * CoeffCount];
            }

            for (int n = 0; n < Enter.Count; n++)
            {
                for (int k = 0; k < 4; k++)
                {
                    for (int i = 0; i < EnterCount + 1; i++)
                    {
                        Gradient[offset + k * CoeffCount + i] += Enter[n][i] * DeltaSigma[n][k];
                    }
                    Gradient[offset + (k + 1) * CoeffCount - 1] += DeltaSigma[n][k];
                }
            }
        }

        public void CalcDelta(double[] CorrectAnswer, double Alpha)
        {
            int c = 0;
            double s1 = (CorrectAnswer[0] - GetAnswer());
            double[] Gradient = new double[Amount];

            GetGradient(Gradient, 0);

            for (int k = 0; k < Amount; k++)
            {
                DeltaW[c] += Alpha * (Gradient[c] * s1);
                ++c;
            }
        }
        public void SetDifference(double[] Delta)
        {
            for (int k = 0; k < Amount; k++)
            {
                DeltaW[k] = Delta[k];
            }
        }

        public void AplyError(double h)
        {
            for (int k = 0; k < Amount; k++)
            {
                w[k] += h * DeltaW[k];
            }
        }

        private double[] w;
        private double[] DeltaW;
        private List<double[]> Enter;
        private List<double[]> Sigma;
        private List<double[]> DeltaSigma;
        private List<double> Pipe;
        private List<double> DeltaPipe;
        private double Exit;
        private double C;

    }

    class LSTM_Net
    {
        public int EnterCount;
        public int CoeffCount;
        public int Amount;
        public int LSTMCount;
        public int ExitCount;

        private String ReadWord(ref String s)
        {
            int k;
            k = 0;
            s = s.Trim() + " ";
            while (k + 1 < s.Length && !((s[k] == ' ') && (s[k + 1] != ' '))) k++;
            String Result = s.Substring(0, k);
            s = s.Substring(k + 1, s.Length - k - 1);
            return Result;
        }

        public LSTM_Net(String FileName)
        {

            if (File.Exists(FileName))
            {
                // Read entire text file content in one string    
                string[] text = File.ReadAllLines(FileName);
                int ec, nc, xc;
                String s, word;
                int line = 0;
                ec = int.Parse(text[line]); line++;
                nc = int.Parse(text[line]); line++;
                xc = int.Parse(text[line]); line++;
                SetEnterCount(ec, nc, xc);
                int count = 0;
                int a = Amount;
                for (int i = 0; i < LSTMCount; i++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        s = text[line]; line++;
                        for (int r = 0; r < CoeffCount; r++)
                        {
                            word = ReadWord(ref s);
                            double we = double.Parse(word);
                            w[count++] = we;
                        }
                    }
                }

                for (int k = 0; k < ExitCount; k++)
                {
                    s = text[line]; line++;
                    for (int r = 0; r < LSTMCount + 1; r++)
                    {
                        word = ReadWord(ref s);
                        double we = double.Parse(word);
                        w[count++] = we;
                    }
                }
            }
        }

        public void Save(String FileName)
        {
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(FileName, false))
            {
                String s;
                file.WriteLine(EnterCount.ToString());
                file.WriteLine(LSTMCount.ToString());
                file.WriteLine(ExitCount.ToString());
                for (int l = 0; l < LSTMCount; l++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        s = "";
                        for (int i = 0; i < CoeffCount; i++)
                        {
                            s += w[l * 4 * CoeffCount + k * CoeffCount + i].ToString() + " ";
                        }
                        file.WriteLine(s);
                    }
                }
                s = "";
                for (int k = 0; k < ExitCount; k++)
                {
                    for (int r = 0; r < LSTMCount + 1; r++)
                    {
                        s += w[4 * CoeffCount * LSTMCount + (LSTMCount + 1) * k + r] + " ";
                    }
                    file.WriteLine(s);
                }
            }
        }

        private void SetEnterCount(int EnterC, int LSTMCount, int ExitCount)
        {
            EnterCount = EnterC;
            CoeffCount = (EnterC + 2);
            this.LSTMCount = LSTMCount;
            this.ExitCount = ExitCount;
            Amount = 4 * CoeffCount * LSTMCount + (LSTMCount + 1) * ExitCount;
            int a = Amount;
            w = new double[a];
            DeltaW = new double[a];
            Out = new double[ExitCount];
            DeltaSigmaOut = new double[LSTMCount];
            Sigma = new List<double[]>[LSTMCount];
            DeltaSigma = new List<double[]>[LSTMCount];
            Enter = new List<double[]>[LSTMCount];
            Pipe = new List<double>[LSTMCount];
            Exit = new double[LSTMCount];
            C = new double[LSTMCount];

            for (int i = 0; i < LSTMCount; i++)
            {
                Enter[i] = new List<double[]>();
                Sigma[i] = new List<double[]>();
                DeltaSigma[i] = new List<double[]>();
                Pipe[i] = new List<double>();
            }

        }
        private double SigmaFunc(double Sigma)
        {
            if (Sigma > Math.Log(1.0E300))
            {
                return 1;
            }
            if (Sigma < -Math.Log(1.0E300))
            {
                return 0;
            }
            double res = 1.0 / (1.0 + Math.Exp(-Sigma));
            return res;
        }

        private double TanhFunc(double Sigma)
        {
            if (Sigma > Math.Log(1.0E300))
            {
                return 1;
            }
            if (Sigma < -Math.Log(1.0E300))
            {
                return -1;
            }
            double e1 = Math.Exp(Sigma);
            double e2 = Math.Exp(-Sigma);
            double res = (e1 - e2) / (e1 + e2);
            return res;
        }
        private double DTanhFunc(double Sigma)
        {
            if (Sigma > Math.Log(1.0E300))
            {
                return 1;
            }
            if (Sigma < -Math.Log(1.0E300))
            {
                return -1;
            }
            double e1 = Math.Exp(Sigma);
            double e2 = Math.Exp(-Sigma);
            double res = 2 / (e1 + e2);
            return res * res;
        }
        public void Fill(int flag)
        {
            Random rnd = new Random();
            for (int k = 0; k < Amount; k++)
            {
                if (flag < 0)
                    w[k] = rnd.NextDouble() * 2 - 1.0;
                else if (flag == 0)
                    w[k] = 0;
                else if (flag == 1)
                    w[k] = 1;
                else if (flag == 2)
                    w[k] = 0.5;
            }
        }

        public void Clear()
        {
            for (int i = 0; i < LSTMCount; i++)
            {
                Enter[i].Clear();
                Sigma[i].Clear();
                DeltaSigma[i].Clear();
                Pipe[i].Clear();

                Exit[i] = 0;
                C[i] = 0;
            }
        }

        private void NewStage()
        {
            for (int i = 0; i < LSTMCount; i++)
            {
                Enter[i].Add(new double[EnterCount + 1]);
                Sigma[i].Add(new double[4]);
                DeltaSigma[i].Add(new double[4]);
                Enter[i][Enter[i].Count - 1][0] = Exit[i];
                Pipe[i].Add(C[i]);
            }
        }

        public void Execute(double[] Enter)
        {
            NewStage();
            int count = 0;
            for (int i = 0; i < LSTMCount; i++)
            {
                int cur = this.Enter[i].Count - 1;
                for (int k = 0; k < EnterCount; k++)
                    this.Enter[i][cur][k + 1] = Enter[k];
                int p = CoeffCount;
                for (int k = 0; k < 4; k++)
                {
                    double sum = 0;
                    for (int j = 0; j < EnterCount + 1; j++)
                    {
                        double e = this.Enter[i][cur][j];
                        double ww = w[count++];
                        sum += ww * e;
                    }
                    sum += w[count++];
                    Sigma[i][cur][k] = sum;
                }
                double s = Sigma[i][cur][0];
                double s1 = SigmaFunc(s);
                s = Sigma[i][cur][1];
                double s2 = SigmaFunc(s);
                s = Sigma[i][cur][2];
                double t1 = TanhFunc(s);
                s = Sigma[i][cur][3];
                double s3 = SigmaFunc(s);
                double c1 = s1 * C[i] + s2 * t1;
                double t2 = TanhFunc(c1);
                double e1 = s3 * t2;
                Exit[i] = e1;
                C[i] = c1;
            }
            for (int e = 0; e < ExitCount; e++)
            {
                double sum = 0;
                for (int i = 0; i < LSTMCount; i++)
                {
                    sum += Exit[i] * w[count++];
                }
                sum += w[count++];
                Out[e] = SigmaFunc(sum);
            }
        }

        public double[] GetAnswer()
        {
            return Out;
        }

        public void FreeDelta()
        {
            for (int k = 0; k < Amount; k++)
            {
                DeltaW[k] = 0;
            }
        }

        public void GetGradient(double[] Gradient, int offset)
        {
            for (int i = 0; i < Amount; i++)
            {
                Gradient[offset + i] = 0;
            }
            for (int i = 0; i < LSTMCount; i++)
            {
                double sum = 0;
                for (int e = 0; e < ExitCount; e++)
                {
                    double dfo = Out[e] * (1 - Out[e]);
                    int index = LSTMCount * 4 * CoeffCount + e * (LSTMCount + 1) + i;
                    sum += dfo * w[index];
                }
                DeltaSigmaOut[i] = sum;
            }
            for (int l = 0; l < LSTMCount; l++)
            {
                double c1 = C[l];
                double de = 0;
                de = DeltaSigmaOut[l];
                double dc = 0;
                double prevf = 0;
                for (int i = Enter[l].Count - 1; i >= 0; i--)
                {
                    //double dp=DeltaPipe[i];
                    //de+=dp;
                    double tc = TanhFunc(c1);
                    double _tc = 1 - tc * tc;
                    double f = SigmaFunc(Sigma[l][i][0]); //f
                    double _f = f * (1 - f);
                    double q = SigmaFunc(Sigma[l][i][1]); //i
                    double _q = q * (1 - q);
                    double g = TanhFunc(Sigma[l][i][2]);  //g
                    double _g = 1 - g * g;
                    double o = SigmaFunc(Sigma[l][i][3]); //o
                    double _o = o * (1 - o);
                    dc = de * o * _tc + dc * prevf;
                    c1 = Pipe[l][i];
                    double res1 = dc * _f * c1;
                    DeltaSigma[l][i][0] = res1;
                    double res2 = dc * _q * g;
                    DeltaSigma[l][i][1] = res2;
                    double res3 = dc * _g * q;
                    DeltaSigma[l][i][2] = res3;
                    double res4 = de * _o * tc;
                    DeltaSigma[l][i][3] = res4;
                    prevf = f;
                    de = 0;
                    for (int k = 0; k < 4; k++)
                    {
                        de += DeltaSigma[l][i][k] * w[(4 * l + k) * CoeffCount];
                    }
                }
            }
            for (int l = 0; l < LSTMCount; l++)
            {
                double ds, e;
                for (int n = 0; n < Enter[l].Count; n++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        ds = DeltaSigma[l][n][k];
                        for (int i = 0; i < EnterCount + 1; i++)
                        {
                            e = Enter[l][n][i];
                            Gradient[offset + (4 * l + k) * CoeffCount + i] += e * ds;
                        }
                        ds = DeltaSigma[l][n][k];
                        Gradient[offset + (4 * l + k + 1) * CoeffCount - 1] += ds;
                    }
                }
            }
            for (int i = 0; i < ExitCount; i++)
            {
                double dfo = Out[i] * (1 - Out[i]);
                for (int l = 0; l < LSTMCount; l++)
                {
                    Gradient[offset + LSTMCount * 4 * CoeffCount + i * (LSTMCount + 1) + l] = Exit[l] * dfo;
                }
                Gradient[offset + LSTMCount * 4 * CoeffCount + i * (LSTMCount + 1) + LSTMCount] = dfo;
            }
        }

        public void CalcDelta(double[] CorrectAnswer, double Alpha)
        {
            double s1 = (CorrectAnswer[0] - GetAnswer()[0]);
            double[] Gradient = new double[Amount];
            GetGradient(Gradient, 0);
            for (int k = 0; k < Amount; k++)
            {
                DeltaW[k] = Alpha * DeltaW[k] + (Gradient[k] * s1);
            }
        }
        public void SetDifference(double[] Delta)
        {
            for (int k = 0; k < Amount; k++)
            {
                DeltaW[k] = Delta[k];
            }
        }

        public void AplyError(double h)
        {
            for (int k = 0; k < Amount; k++)
            {
                w[k] += h * DeltaW[k];
            }
        }

        private double[] w;
        private double[] DeltaW;
        private List<double[]>[] Enter;
        private List<double[]>[] Sigma;
        private List<double[]>[] DeltaSigma;
        private List<double>[] Pipe;
        private double[] DeltaSigmaOut;
        private double[] Exit;
        private double[] C;
        private double[] Out;
    }
}
