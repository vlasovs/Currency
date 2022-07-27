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
        public static void shuffle(ref int[] arr)
        {
            Random r = new Random();
            for (int i = arr.Count() - 1; i >= 1; i--)
            {
                int j = r.Next() % (i + 1);
                int tmp = arr[j];
                arr[j] = arr[i];
                arr[i] = tmp;
            }
        }
        public static void conjugate_gradient(NN nn, List<List<double>> tests, double dropout, int tcount, int ts)
        {            
            //int size = 4 * look_back;
            double Alpha = 1.0 / 3.0;
            double Step = 0.001;
            int tt = tests.Count();
            if (ts > tt)
            {
                ts = tt;
            }
            double[] delta = new double[tt];
            double[] enter = new double[4];
            double[] ans = new double[1];
            //nn->ApplyDropout(dropout);
            for (int t = 0; t < tcount; t++)
            {
                nn.FreeDelta();
                nn.ApplyDropout(dropout);
                int[] x=new int[tt];
                for (int i = 0; i < tt; i++)
                {
                    x[i] = i;
                }
                shuffle(ref x);
                for (int i = 0; i < tt; i++)
                {
                    int index = x[i];
                    nn.Clear();
                    int size = tests[index].Count() - 1;
                    int tc = tests[index].Count() / 4;
                    for (int k = 0; k < tc; k++)
                    {
                        //for (int k = 0; k < look_back; k++) {
                        for (int j = 0; j < 4; j++)
                        {
                            enter[j] = tests[index][k * 4 + j];
                        }
                        nn.Execute(enter);
                    }
                    ans[0] = tests[index][size];
                    delta[i] = ans[0];
                    delta[i] -= nn.GetAnswer(tc - 1)[0];
                    nn.ClearDeltaSigma();
                    //nn->Backpropagation(delta + i, nn->ExtractLayer(nn->LayersCount() - 1)->GetEntersCount() - 1, 0);
                    nn.CalcNumericalDelta(tests[index], ans, Alpha);
                    nn.CalcDelta(Alpha);
                    nn.AplyError(Step);
                }
                //cout << NormL2_(delta, tt) << "\n" << NormC_(delta, tt) << "\n" << (t + 1) << " \n" << " \n";
            }            
        }
        //---------------------------------------------------------------------------




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
        protected bool[] Dropout;
        protected double[] DeltaW;
        protected List<double[]> wDeltaSigma;
        protected List<double[]> Enters;
        protected List<double[]> Outs;
        protected Layer next;
        protected Layer prev;
        protected string name;
        protected double pd;
        protected int dropcount;
        public static int flag = -33000000;

        public Layer()
        {
            r = new Random();
            w = null;
            DeltaW = null;
            wDeltaSigma = new List<double[]>();
            Enters = new List<double[]>();
            Outs = new List<double[]>();
            name = "Layer";
            next = null;
            prev = null;
            Amount = 0;
            EnterCount = 0;
        }

        public string GetName()
        {
            return name;
        }

        public virtual void SetCoeff(double val, int i)
        {
            w[i] = val;
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
        public virtual void SetEnter(double[] enter)
        {
            if (enter != null)
            {
                double[] Enter = new double[EnterCount];
                for (int k = 0; k < EnterCount; k++)
                {
                    Enter[k] = enter[k];
                }
                Enters.Add(Enter);
                double[] Out = new double[ExitCount];
                for (int k = 0; k < ExitCount; k++)
                {
                    Out[k] = 0;
                }
                Outs.Add(Out);
            }
            double[] DeltaSigma = new double[ExitCount];
            for (int i = 0; i < ExitCount; i++)
            {
                DeltaSigma[i] = 0;
            }
            wDeltaSigma.Add(DeltaSigma);
        }

        public int GetCoeffAmount()
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

        public void Fill(int flag, double Amplitude)
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

        public double[] GetAnswer(int stage)
        {
            return Outs[stage];
        }

        public virtual double[] GetDeltaSigma(int stage)
        {
            return wDeltaSigma[stage];
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

        public virtual void GetLayerInfo(out int n, out int m, out double[] array)
        {
            array = w;
            n = ExitCount;
            m = Amount / n;
        }
        public virtual void ApplyDropout(double x)
        {
            int amount = GetCoeffAmount();
            int c = 0;
            for (int i = 0; i < amount; i++)
            {
                double r1 = r.NextDouble();
                if (r1 < x)
                {
                    Dropout[i] = true;
                    c++;
                }
                else
                {
                    Dropout[i] = false;
                }
            }
            pd = (double)c / amount;
            dropcount = c;

        }

        public virtual int DropCount()
        {
            return dropcount;
        }

        public virtual bool IsDrop(int count)
        {
            return Dropout[count];
        }
        public int GetEntersCount()
        {
            return Enters.Count();
        }

        public virtual void Clear()
        {            
            Enters = new List<double[]>();
            Outs = new List<double[]>();
            wDeltaSigma = new List<double[]>();            
        }
        public virtual void ClearDeltaSigma() { }
        public virtual void SetEnterCount(int EnterCount, int NeuronCount, int ExitCount) {
            Enters = new List<double[]>();
            Outs = new List<double[]>();
        }
        public virtual void Execute(double[] Enter) { }
        public virtual double[] Backpropagation(double[] ds, int stage, int last_stage, int exit = -33000000) => null;
        public virtual void GetGradient(double[] Gradient, int offset) { }
        public virtual void CalcDelta(double Alpha) { }
    }
    class Dense : Layer {

        public Dense() : base() {
            name = "Dense";
        }

        public override void SetEnterCount(int EnterCount, int NeuronCount, int ExitCount)
        {            
            this.EnterCount = EnterCount;
            this.NeuronCount = NeuronCount;
            this.ExitCount = ExitCount;
            this.wDeltaSigma = new List<double[]>();

            Amount = NeuronCount * (EnterCount + 1);
            int a = Amount;
            w = new double[a];
            Dropout = new bool[a];
            DeltaW = new double[a];
            Enters = new List<double[]>();
            Outs = new List<double[]>();
        }

        public override void Execute(double[] Enter)
        {
            SetEnter(Enter);
            int count1 = 0;
            for (int i = 0; i < NeuronCount; i++)
            {
                double sum = 0;
                for (int e = 0; e < EnterCount; e++)
                {
                    if (!Dropout[count1])
                    {
                        sum += Enter[e] * w[count1++] / (1 - pd);
                    }
                    else
                    {
                        count1++;
                    }
                }
                if (!Dropout[count1])
                {
                    sum += w[count1++] / (1 - pd);
                }
                else
                {
                    count1++;
                }
                Outs.Last()[i] = sum;
            }
            if (next != null)
            {
                next.Execute(Outs.Last());
            }
        }

        public override double[] Backpropagation(double[] ds, int stage, int last_stage, int exit)
        {
            for (int e = 0; e < NeuronCount; e++)
            {
                double sum = 0;
                double dfo = 1;
                if (ds != null)
                {
                    dfo *= ds[e];
                }
                sum = dfo; //NeuronCount
                if (exit == flag)
                {
                    wDeltaSigma[stage][e] += sum;
                }
                else
                {
                    wDeltaSigma[stage][e] += e == exit ? sum : 0;
                }
            }
            double[] ds2 = new double[EnterCount];
            for (int k = 0; k < EnterCount; k++)
            {
                ds2[k] = 0;
                for (int n = 0; n < NeuronCount; n++)
                {
                    double fs = wDeltaSigma[stage][n];
                    double s = 0;
                    if (!Dropout[n * (EnterCount + 1) + k])
                    {
                        s = fs * w[n * (EnterCount + 1) + k] / (1 - pd);
                    }
                    ds2[k] += s;
                }
            }
            if (prev != null)
            {
                ds = prev.Backpropagation(ds2, stage, last_stage);
            }
            else
            {
                ds = ds2;
            }
            return ds;
        }

        public override void GetGradient(double[] Gradient, int offset)
        {
            for (int i = 0; i < Amount; i++)
            {
                Gradient[offset + i] = 0;
            }
            for (int s = 0; s < wDeltaSigma.Count(); s++)
            {
                for (int i = 0; i < NeuronCount; i++)
                {
                    double dfo = 0;
                    dfo = wDeltaSigma[s][i];
                    for (int l = 0; l < EnterCount; l++)
                    {
                        int t1 = i * (EnterCount + 1) + l;
                        //			if (!Dropout[t]) {
                        Gradient[offset + t1] += Enters[s][l] * dfo;
                        //			}
                    }
                    int t2 = i * (EnterCount + 1) + EnterCount;
                    //		if (!Dropout[t]) {
                    Gradient[offset + t2] += dfo;
                    //		}
                }
            }
            for (int i = 0; i < Amount; i++)
            {
                if (Dropout[i])
                {
                    Gradient[offset + i] = 0;
                }
                else
                {
                    Gradient[offset + i] /= 1 - pd;
                }
            }
        }

        public override void CalcDelta(double Alpha)
        {
            int count = 0;
            for (int i = 0; i < NeuronCount; i++)
            {
                for (int l = 0; l < EnterCount; l++)
                {
                    if (!Dropout[count])
                    {
                        double dfo = 0;
                        for (int s = 0; s < wDeltaSigma.Count(); s++)
                        {
                            dfo += Enters[s][l] * wDeltaSigma[s][i];
                        }
                        DeltaW[count] = Alpha * (DeltaW[count]) + dfo;
                    }
                    count++;
                }
                if (!Dropout[count])
                {
                    double dfo = 0;
                    for (int s = 0; s < wDeltaSigma.Count(); s++)
                    {
                        dfo += wDeltaSigma[s][i];
                    }
                    DeltaW[count] = Alpha * (DeltaW[count]) + dfo;
                }
                count++;
            }
        }

        public override void ClearDeltaSigma()
        {
            for (int s = 0; s < wDeltaSigma.Count(); s++)
            {
                for (int i = 0; i < NeuronCount; i++)
                {
                    wDeltaSigma[s][i] = 0;
                }
            }
        }
    }

    class DenseSoftmax : Dense {
        public DenseSoftmax() : base() {
            name = "DenseSoftmax";
        }

        public override void Execute(double[] Enter)
        {
            SetEnter(Enter);
            int count1 = 0;
            double sum1 = 0;
            for (int i = 0; i < NeuronCount; i++)
            {
                double sum2 = 0;
                for (int e = 0; e < EnterCount; e++)
                {
                    if (!Dropout[count1])
                    {
                        sum2 += Enter[e] * w[count1++] / (1 - pd);
                    }
                    else
                    {
                        count1++;
                    }
                }
                if (!Dropout[count1])
                {
                    sum2 += w[count1++] / (1 - pd);
                }
                else
                {
                    count1++;
                }
                Outs.Last()[i] = Math.Exp(sum2);
                sum1 += Outs.Last()[i];
            }
            for (int i = 0; i < NeuronCount; i++)
            {
                Outs.Last()[i] /= sum1;
            }
            if (next != null)
            {
                next.Execute(Outs.Last());
            }
        }

        public override double[] Backpropagation(double[] ds, int stage, int last_stage, int exit = -33000000)
        {
            for (int e = 0; e < NeuronCount; e++)
            {
                double sum = 0;
                double dfo;
                if (ds != null && exit == flag)
                {
                    for (int q = 0; q < NeuronCount; q++)
                    {
                        double r = (e == q ? 1 : 0);
                        dfo = Outs[stage][q] * (r - Outs[stage][e]);
                        dfo *= ds[q];
                        sum += dfo;//NeuronCount;
                    }
                }
                else
                {
                    double r = e == exit ? 1 : 0;
                    dfo = Outs[stage][exit] * (r - Outs[stage][e]);
                    sum += dfo;
                }
                wDeltaSigma[stage][e] += sum;
            }

            double[] ds2 = new double[EnterCount];
            for (int k = 0; k < EnterCount; k++)
            {
                ds2[k] = 0;
                for (int n = 0; n < NeuronCount; n++)
                {
                    double fs = wDeltaSigma[stage][n];
                    double s = 0;
                    if (!Dropout[n * (EnterCount + 1) + k])
                    {
                        s = fs * w[n * (EnterCount + 1) + k] / (1 - pd);
                    }
                    ds2[k] += s;
                }
            }

            if (prev != null)
            {
                ds = prev.Backpropagation(ds2, stage, last_stage);
            }
            else
            {
                ds = ds2;
            }

            return ds;
        }
    }
    /********************************************************************/

    class DenseSigmoid : Dense
    {
        public DenseSigmoid() : base() {
            name = "DenseSigmoid";
        }

        public override void Execute(double[] Enter)
        {
            SetEnter(Enter);
            int count1 = 0;
            for (int i = 0; i < NeuronCount; i++)
            {
                double sum2 = 0;
                for (int e = 0; e < EnterCount; e++)
                {
                    if (!Dropout[count1])
                    {
                        sum2 += Enter[e] * w[count1++] / (1 - pd);
                    }
                    else
                    {
                        count1++;
                    }
                }
                if (!Dropout[count1])
                {
                    sum2 += w[count1++] / (1 - pd);
                }
                else
                {
                    count1++;
                }
                Outs.Last()[i] = help_func.SigmaFunc(sum2);
            }
            if (next != null)
            {
                next.Execute(Outs.Last());
            }
        }

        public override double[] Backpropagation(double[] ds, int stage, int last_stage, int exit = -33000000)
        {
            //int e=exit;
            for (int e = 0; e < NeuronCount; e++)
            {
                double sum = 0;
                double dfo = Outs[stage][e] * (1 - Outs[stage][e]);
                if (ds != null)
                {
                    dfo *= ds[e];
                }
                if (exit == flag || exit == e)
                {
                    wDeltaSigma[stage][e] = dfo;
                }
                else
                {
                    wDeltaSigma[stage][e] = 0;
                }
            }

            double[] ds2 = new double[EnterCount];
            for (int k = 0; k < EnterCount; k++)
            {
                ds2[k] = 0;
                for (int n = 0; n < NeuronCount; n++)
                {
                    double fs = wDeltaSigma[stage][n];
                    double s = 0;
                    if (!Dropout[n * (EnterCount + 1) + k])
                    {
                        s = fs * w[n * (EnterCount + 1) + k] / (1 - pd);
                    }
                    ds2[k] += s;
                }
            }

            if (prev != null)
            {
                ds = prev.Backpropagation(ds2, stage, last_stage);
            }
            else
            {
                ds = ds2;
            }

            return ds;
        }

        /********************************************************************/
    }

    class LSTM : Layer {

        protected int CoeffCount;
        protected List<double[]> Stack;
        protected List<double[]>[] Sigma;
        protected List<double>[] Pipe;
        protected List<double[]>[] DeltaSigmas;
        protected double[] Exit;
        protected double[] C;

        protected double[] c1;
        protected double[] de;
        protected double[] dc;
        protected double[] prevf;

        public LSTM() : base() {
            name = "LSTM";
            Exit = null;
            C = null;
            c1 = null;
            de = null;
            dc = null;
            prevf = null;
        }

        public override void Clear()
        {
            Enters = new List<double[]>();
            Outs = new List<double[]>();
            Amount = 4 * CoeffCount * ExitCount;
            int a = Amount;
            DeltaW = new double[a];
            Stack = new List<double[]>();
            Sigma = new List<double[]>[ExitCount];
            DeltaSigmas = new List<double[]>[ExitCount];
            Pipe = new List<double>[ExitCount];
            Exit = new double[ExitCount];
            C = new double[ExitCount];
            for (int i = 0; i < ExitCount; i++)
            {
                Sigma[i] = new List<double[]>();
                DeltaSigmas[i] = new List<double[]>();
                Pipe[i] = new List<double>();
                Exit[i] = 0;
                C[i] = 0;
            }
            c1 = new double[ExitCount];
            de = new double[ExitCount];
            dc = new double[ExitCount];
            prevf = new double[ExitCount];
        }

        public override void SetEnter(double[] enter)
        {
            double[] Enter = new double[EnterCount];
            for (int k = 0; k < EnterCount; k++)
            {
                Enter[k] = enter[k];
            }
            Enters.Add(Enter);
            double[] Out = new double[ExitCount];
            for (int k = 0; k < ExitCount; k++)
            {
                Out[k] = 0;
            }
            Outs.Add(Out);
        }

        public override void SetEnterCount(int EnterC, int NeuronCount, int ExitCount)
        {
            Enters = new List<double[]>();
            Outs = new List<double[]>();
            this.ExitCount = ExitCount;
            EnterCount = EnterC;
            CoeffCount = (ExitCount / NeuronCount + EnterC + 1);
            this.NeuronCount = NeuronCount;
            Amount = 4 * CoeffCount * ExitCount;
            int a = Amount;
            w = new double[a];
            Dropout = new bool[a];
            DeltaW = new double[a];
            Stack = new List<double[]>();            
            Sigma = new List<double[]>[ExitCount];
            DeltaSigmas = new List<double[]>[ExitCount];
            Pipe = new List<double>[ExitCount];
            Exit = new double[ExitCount];
            C = new double[ExitCount];
            for (int i = 0; i < ExitCount; i++)
            {
                Sigma[i] = new List<double[]>();
                DeltaSigmas[i] = new List<double[]>();
                Pipe[i] = new List<double>();
                Exit[i] = 0;
                C[i] = 0;
            }            
            c1 = new double[ExitCount];
            de = new double[ExitCount];
            dc = new double[ExitCount];
            prevf = new double[ExitCount];
        }

        protected void NewStage()
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
                for (int j = 0; j < 4; j++)
                {
                    Sigma[i].Last()[j] = 0;
                    DeltaSigmas[i].Last()[j] = 0;
                }
                Pipe[i].Add(C[i]);
            }
        }

        public override void Execute(double[] Enter)
        {
            SetEnter(Enter);
            NewStage();
            int count1 = 0;
            int cur = Stack.Count() - 1;
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
                        double ww = 0;
                        if (!Dropout[count1])
                        {
                            ww = w[count1++] / (1 - pd);
                        }
                        else
                        {
                            count1++;
                        }
                        sum += ww * e;
                    }
                    for (int j = 0; j < EnterCount; j++)
                    {
                        double e = this.Stack[cur][ExitCount + j];
                        double ww = 0;
                        if (!Dropout[count1])
                        {
                            ww = w[count1++] / (1 - pd);
                        }
                        else
                        {
                            count1++;
                        }
                        sum += ww * e;
                    }
                    if (!Dropout[count1])
                    {
                        sum += w[count1++] / (1 - pd);
                    }
                    else
                    {
                        count1++;
                    }
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
                Outs.Last()[i] = Exit[i];
            }
            if (next != null)
            {
                next.Execute(Outs.Last());
            }
        }

        public override double[] Backpropagation(double[] ds, int stage, int last_stage, int exit)
        {
            int count = ExitCount / NeuronCount;
            if (stage == Stack.Count() - 1)
            {
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
            }
            else if (stage == last_stage)
            {
                for (int l = 0; l < ExitCount; l++)
                {
                    de[l] += ds[l];
                }
            }

            for (int i = stage; i >= last_stage; i--)
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
                            int t = (4 * r + k) * CoeffCount + number;
                            if (!Dropout[t])
                            {
                                de[p] += DeltaSigmas[r][i][k] * w[t] / (1 - pd);
                            }
                        }
                    }
                }
            }
            if (prev != null)
            {
                double[] ds2 = new double[EnterCount];
                int c = stage;
                //int i = c;
                for (int i = c; i >= last_stage; i--)
                {
                    for (int e = 0; e < EnterCount; e++)
                    {
                        ds2[e] = 0;
                        for (int l = 0; l < ExitCount; l++)
                        {
                            for (int k = 0; k < 4; k++)
                            {
                                double dss = DeltaSigmas[l][i][k];
                                int t = (4 * l + k) * CoeffCount + e + count;
                                if (!Dropout[t])
                                {
                                    ds2[e] += dss * w[t] / (1 - pd);
                                }
                            }
                        }
                    }
                    prev.Backpropagation(ds2, i, i);
                }
            }
            return null;
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
                for (int n = 0; n < Stack.Count(); n++)
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
                            int t1 = (4 * l + k) * CoeffCount + j;
                            //					if (!Dropout[t]) {
                            Gradient[offset + t1] += ed;
                            //					}
                        }
                        for (int i = 0; i < EnterCount; i++)
                        {
                            e = Stack[n][ExitCount + i];
                            ed = e * ds1;
                            int t1 = (4 * l + k) * CoeffCount + tcount + i;
                            //					if (!Dropout[t]) {
                            Gradient[offset + t1] += ed;
                            //					}
                        }
                        int t = (4 * l + k + 1) * CoeffCount - 1;
                        //				if (!Dropout[t]) {
                        Gradient[offset + t] += ds1;
                        //				}
                    }
                }
            }
            for (int i = 0; i < Amount; i++)
            {
                if (Dropout[i])
                {
                    Gradient[offset + i] = 0;
                }
                else
                {
                    Gradient[offset + i] /= 1 - pd;
                }
            }
        }

        public override void CalcDelta(double Alpha)
        {
            double[] Gradient = new double[Amount];
            for (int i = 0; i < Amount; i++)
            {
                Gradient[i] = 0;
            }
            GetGradient(Gradient, 0);
            for (int k = 0; k < Amount; k++)
            {
                if (!Dropout[k])
                {
                    DeltaW[k] = Alpha * DeltaW[k] + Gradient[k];
                }
            }
        }

        public override void ClearDeltaSigma()
        {
            for (int l = 0; l < ExitCount; l++)
            {
                for (int i = 0; i < Stack.Count(); i++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        DeltaSigmas[l][i][k] = 0;
                    }
                }
            }
        }
    }
    /********************************************************************/
    // Embedding

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
            //Enter = new double[1];
            int a = Amount = NeuronCount * ExitCount;
            w = new double[a];
            Dropout = new bool[a];
            DeltaW = new double[a];
            //DeltaSigma = new double[a];
            //Out = new double[ExitCount];
        }

        public override void Execute(double[] Enter)
        {
            SetEnter(Enter);
            for (int e = 0; e < EnterCount; e++)
            {
                for (int i = 0; i < ExitCount; i++)
                {
                    int t = (int)Enter[e] * ExitCount + i;
                    if (Dropout[t])
                    {
                        Outs.Last()[i] = 0;
                    }
                    else
                    {
                        Outs.Last()[i] = w[t] / (1 - pd);
                    }
                }
                if (next != null)
                {
                    next.Execute(Outs.Last());
                }
            }
        }

        public override double[] Backpropagation(double[] ds, int stage, int last_stage, int exit)
        {
            for (int e = 0; e < EnterCount; e++)
            {
                for (int i = 0; i < ExitCount; i++)
                {
                    int t = (int)Enters[stage][e] * ExitCount + i;
                    if (!Dropout[t])
                    {
                        wDeltaSigma[stage][t] += ds[i];
                    }
                }
            }
            return null;
        }

        public override void GetGradient(double[] Gradient, int offset)
        {
            for (int i = 0; i < Amount; i++)
            {
                for (int s = 0; s < wDeltaSigma.Count(); s++)
                {
                    double dfo = wDeltaSigma[s][i];
                    if (!Dropout[i])
                    {
                        Gradient[offset + i] += dfo;
                    }
                }
            }
        }

        public override void CalcDelta(double Alpha)
        {
            for (int i = 0; i < Amount; i++)
            {
                double dfo = 0;
                for (int s = 0; s < wDeltaSigma.Count(); s++)
                {
                    dfo += wDeltaSigma[s][i];
                }
                if (!Dropout[i])
                {
                    DeltaW[i] = Alpha * (DeltaW[i]) + dfo;
                }
            }
        }

        public override void ClearDeltaSigma()
        {
            for (int i = 0; i < Amount; i++)
            {
                for (int s = 0; s < wDeltaSigma.Count(); s++)
                {
                    wDeltaSigma[s][i] = 0;
                }
            }
        }

        public override void GetLayerInfo(out int n, out int m, out double[] array)
        {
            array = w;
            n = NeuronCount;
            m = Amount / n;
        }
    }
    // Adder

    class Adder : Layer {
        protected int offset;
        protected int count;

        public void SetCount(int Count)
        {
            EnterCount = Count;
            NeuronCount = Count;
            ExitCount = Count;
            Amount = Count;
        }

        public void SetOffset(int offset, int count)
        {
            this.offset = offset;
            this.count = count;
        }

        public override double[] Backpropagation(double[] ds, int stage, int last_stage, int exit = -33000000)
        {
            for (int i = 0; i < count; i++)
            {
                wDeltaSigma[stage][i + offset] += ds[i];
            }
            return null;
        }
        public override double[] GetDeltaSigma(int stage)
        {
            return wDeltaSigma[stage];
        }
    }

    // Flatten
    class Flatten : Layer {

        protected Adder a;
        protected double[] g;
        protected double[] v;
        protected double[] m;
        protected List<Layer> layers;

        public Flatten() : base() {
            name = "Flatten";
            layers = new List<Layer>();
            a = new Adder();
        }

        public override void SetEnter(double[] enter)
        {
            if (enter != null)
            {
                double[] Enter = new double[EnterCount];
                for (int k = 0; k < EnterCount; k++)
                {
                    Enter[k] = enter[k];
                }
                Enters.Add(Enter);
                double[] Out = new double[ExitCount];
                for (int k = 0; k < ExitCount; k++)
                {
                    Out[k] = 0;
                }

                Outs.Add(Out);
            }
        }

        public int FindIndex(ref int count)
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

        public override void Clear()
        {
            for(int i=0; i < LayersCount(); i++)
            {
                layers[i].Clear();
            }
            wDeltaSigma = new List<double[]>();
            Enters = new List<double[]>();
            Outs = new List<double[]>();
            a.Clear();           
        }

        public override void SetEnterCount(int EnterCount, int NeuronCount, int ExitCount)
        {
            wDeltaSigma = new List<double[]>();
            Enters = new List<double[]>();
            Outs = new List<double[]>();
            this.ExitCount = ExitCount;
            this.EnterCount = EnterCount;
            this.NeuronCount = NeuronCount;
            w = null;
            DeltaW = null;
            //DeltaSigma = new double[NeuronCount];
            //Enter = 0;
            //Out = new double[ExitCount];
            int am = 0;
            for (int i = 0; i < layers.Count(); i++)
            {
                am += layers[i].GetCoeffAmount();
            }
            Amount = am;
            g = new double[am];
            v = new double[am];
            m = new double[am];
            a.SetCount(EnterCount);
        }
        public override void Execute(double[] Enter)
        {
            a.SetEnter(null);
            SetEnter(Enter);
            int size = layers.Count();
            int count_e = 0;
            int count_x = 0;
            for (int i = 0; i < size; i++)
            {
                int ec = layers[i].GetEnterCount();
                int xc = layers[i].GetExitCount();
                double[] tmp = new double[ec];
                for (int k = 0; k < ec; k++)
                    tmp[k] = Enter[k + count_e];                
                layers[i].Execute(tmp);
                int lc = layers[i].GetEntersCount();
                for (int j = 0; j < xc; j++)
                {
                    double ex = layers[i].GetAnswer(lc - 1)[j];
                    Outs.Last()[j + count_x] = ex;
                }
                count_e += ec;
                count_x += xc;
            }
            if (next != null)
            {
                next.Execute(Outs.Last());
            }
        }

        public override double[] Backpropagation(double[] ds, int stage, int last_stage, int exit = -33000000)
        {
            //ClearDeltaSigma();
            double[] tmp;
            int s = layers.Count();
            int count_e = 0;
            int count_p = 0;

            for (int i = 0; i < s; i++)
            {
                int e = layers[i].GetEnterCount();
                int p = layers[i].GetExitCount();
                a.SetOffset(count_e, e);
                layers[i].SetNeighbors(null, a);
                double[] ds1 = null;
                if (ds != null)
                {
                    ds1 = new double[p];
                    for (int k = 0; k < p; k++)
                        ds1[k] = ds[k + count_p];
                }
                if (exit == flag)
                    layers[i].Backpropagation(ds1, stage, stage);
                else
                    layers[i].Backpropagation(ds1, stage, stage, exit - count_p);
                count_p += p;
                count_e += e;
            }

            if (stage == 0 && prev != null)
            {
                int c = GetEntersCount() - 1;
                for (int s1 = c; s1 >= 0; s1--)
                {
                    double max = 0;
                    for (int i = 0; i < EnterCount; i++)
                    {
                        if (max < Math.Abs(a.GetDeltaSigma(s1)[i]))
                        {
                            max = Math.Abs(a.GetDeltaSigma(s1)[i]);
                        }
                    }
                    if (max > 0)
                    {
                        prev.Backpropagation(a.GetDeltaSigma(s1), s1, s1);
                    }
                }
            }
            return null;
        }

        public override int DropCount()
        {
            int dc = 0;
            for (int i = 0; i < layers.Count(); i++)
            {
                dc += layers[i].DropCount();
            }
            return dc;
        }

        public override void GetGradient(double[] Gradient, int offset)
        {
            int s = layers.Count();
            for (int i = 0; i < s; i++)
            {
                layers[i].GetGradient(Gradient, offset);
                offset += layers[i].GetCoeffAmount();
            }
        }

        public void GetGradientWithout(double[] Gradient, int offset)
        {
            int a = GetCoeffAmount();
            double[] gr = new double[a];
            GetGradient(gr, 0);
            int s = layers.Count();
            int k = 0;
            int o = 0;
            for (int i = 0; i < s; i++)
            {
                int p = layers[i].GetCoeffAmount();
                for (int j = 0; j < p; j++)
                {
                    if (!layers[i].IsDrop(j))
                    {
                        Gradient[offset + k] = gr[o + j];
                        k++;
                    }
                }
                o += p;
            }
        }

        public void SetDifferenceWithout(double[] Delta)
        {
            int p = GetCoeffAmount() - DropCount();
            for (int i = Amount - 1; i >= 0; i--)
            {
                if (!IsDrop(i))
                {
                    SetDelta(Delta[--p], i);
                }
                else
                {
                    SetDelta(0, i);
                }
            }
        }

        public override void CalcDelta(double Alpha)
        {
            int s = layers.Count();
            for (int i = 0; i < s; i++)
            {
                layers[i].CalcDelta(Alpha);
            }
        }

        public override void FreeDelta()
        {
            int s = layers.Count();
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
            int s = layers.Count();
            for (int i = 0; i < s; i++)
            {
                layers[i].ClearDeltaSigma();
            }
            for (int s1 = 0; s < wDeltaSigma.Count(); s++)
            {
                for (int i = 0; i < EnterCount; i++)
                {
                    wDeltaSigma[s1][i] = 0;
                }
            }
            a.ClearDeltaSigma();
        }

        public override void ApplyDropout(double x)
        {
            int s = layers.Count();
            for (int i = 0; i < s; i++)
            {
                layers[i].ApplyDropout(x);
            }
        }

        public override bool IsDrop(int count)
        {
            int i = FindIndex(ref count);
            return layers[i].IsDrop(count);
        }

        public void GetInfo(int layer, out int n, out int m, out double[] array)
        {
            layers[layer].GetLayerInfo(out n, out m, out array);
        }

        public Layer ExtractLayer(int i)
        {
            return layers[i];
        }

        public void AddLayer(Layer l)
        {
            layers.Add(l);
        }

        public int LayersCount()
        {
            return layers.Count();
        }

        public virtual void Linking(Layer next, Layer prev)
        {
            int lsize = this.layers.Count();
            for (int i = 0; i < lsize; i++)
            {
                this.layers[i].SetNeighbors(null, null);
            }
        }
    }

    class NN : Flatten {
        public NN() : base() {
            name = "NN";
        }

        public override void Linking(Layer next, Layer prev)
        {
            int lsize = this.layers.Count();
            for (int i = 0; i < lsize; i++)
            {
                Layer next1 = next;
                Layer prev1 = prev;
                if (i > 0)
                {
                    prev1 = layers[i - 1];
                }
                if (i < layers.Count() - 1)
                {
                    next1 = layers[i + 1];
                }
                this.layers[i].SetNeighbors(next1, prev1);
            }
        }
        public void Load(string FileName)
        {
            int ec1 = 0, nc1 = 0, xc1 = 0;
            bool first = true;
            Stack<Flatten> ls = new Stack<Flatten>();
            ls.Push((Flatten)this);
            Stack<int> sec = new Stack<int>();
            Stack<int> snc = new Stack<int>();
            Stack<int> sxc = new Stack<int>();
            Layer prev = null;
            layers = new List<Layer>();
            if (File.Exists(FileName))
            {
                // Read entire text file content in one string    
                string[] text = File.ReadAllLines(FileName);
                int line = 0;
                while (line < text.Count() && text[line].Length < 1) line++;
                do
                {
                    Layer l;
                    if (text[line] == "Embedding")
                    {
                        l = new Embedding();
                    }
                    else if (text[line] == "LSTM")
                    {
                        l = new LSTM();
                    }
                    else if (text[line] == "DenseSoftmax")
                    {
                        l = new DenseSoftmax();
                    }
                    else if (text[line] == "DenseSigmoid")
                    {
                        l = new DenseSigmoid();
                    }
                    else if (text[line] == "Dense")
                    {
                        l = new Dense();
                    }
                    else if (text[line] == "Flatten")
                    {
                        ls.Push(new Flatten());
                        l = ls.Peek();
                    }
                    else if (text[line] == "Exit")
                    {
                        Flatten f = ls.Pop();
                        f.SetEnterCount(sec.Pop(), snc.Pop(), sxc.Pop());
                        ls.Peek().AddLayer(f);
                        line++;
                        continue;
                    }
                    else
                    {
                        break;
                    }
                    line++;

                    int ec, nc, xc;
                    //string text1, s, word;
                    //std::getline(f, text);
                    ec = int.Parse(text[line]); line++;
                    //std::getline(f, text);
                    nc = int.Parse(text[line]); line++;
                    //std::getline(f, text);
                    xc = int.Parse(text[line]); line++;
                    if (first)
                    {
                        ec1 = ec;
                        nc1 = nc;
                        first = false;
                    }
                    xc1 = xc;
                    if (l.GetName() == "Flatten")
                    {
                        sec.Push(ec);
                        snc.Push(nc);
                        sxc.Push(xc);
                        continue;
                    }
                    l.SetEnterCount(ec, nc, xc);
                    string s1 = text[line]; line++;
                    help_func.SwapComma(ref s1);
                    int count1 = l.GetCoeffAmount();
                    for (int r = 0; r < count1; r++)
                    {
                        string word = help_func.ReadWord(ref s1);
                        try
                        {
                            double we = double.Parse(word);
                            l.SetCoeff(we, r);
                        } catch (Exception exept){
                            break;
                        }
                    }
                    ls.Peek().AddLayer(l);
                    prev = l;
                    while (line < text.Count() && text[line].Length < 1) line++;
                } while (line < text.Count());
                SetEnterCount(ec1, nc1, xc1);
                Linking(null, null);
            }
        }

        public void Save(string FileName)
        {
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(FileName, false))
            {
                Stack<Flatten> ls = new Stack<Flatten>();
                Stack<int> index = new Stack<int>();
                ls.Push(this);
                int i = 0;
                while (ls.Count() > 0)
                {
                    Flatten f = ls.Peek();
                    Layer l = f.ExtractLayer(i);
                    file.WriteLine(l.GetName());
                    file.WriteLine(l.GetEnterCount());
                    file.WriteLine(l.GetNeuronCount());
                    file.WriteLine(l.GetExitCount());
                    if (l.GetName() == "Flatten")
                    {
                        index.Push(i);
                        ls.Push((Flatten)l);
                        i = 0;
                        continue;
                    }
                    int Amount = l.GetCoeffAmount();
                    for (int k = 0; k < Amount; k++)
                    {
                        file.Write(l.GetCoeff(k));
                        file.Write(" ");
                    }
                    file.WriteLine();
                    i++;
                    while (i >= ls.Peek().LayersCount())
                    {
                        ls.Pop();
                        if (ls.Count() == 0) break;
                        file.WriteLine("Exit");
                        i = index.Pop() + 1;
                    }
                }
            }
        }

        public void GetNumericalGradient(List<double> tests, double[] Gradient, int offset)
        {
            double delta = 1.0E-6;
            double[] enter = new double[20];
            double[] Gr = new double[Amount];
            //int ec=EnterCount;
            Clear();
            int tc = tests.Count() / 4;
            for (int k = 0; k < tc; k++)
            {
                //for (int k = 0; k < look_back; k++) {
                for (int j = 0; j < 4; j++)
                {
                    enter[j] = tests[k * 4 + j];
                }
                Execute(enter);
            }
            Backpropagation(null, tc - 1, 0, 0);
            GetGradient(Gr, 0);
            for (int i = 0; i < Amount; i++)
            {
                double derivative;
                double ww = GetCoeff(i);
                SetCoeff(ww - delta, i);
                Clear();
                for (int k = 0; k < tc; k++)
                {
                    //for (int k = 0; k < look_back; k++) {
                    for (int j = 0; j < 4; j++)
                    {
                        enter[j] = tests[k * 4 + j];
                    }
                    Execute(enter);
                }
                double r1 = GetAnswer(tc - 1)[0];
                SetCoeff(ww + delta, i);
                Clear();
                for (int k = 0; k < tc; k++)
                {
                    //for (int k = 0; k < look_back; k++) {
                    for (int j = 0; j < 4; j++)
                    {
                        enter[j] = tests[k * 4 + j];
                    }
                    Execute(enter);
                }
                double r2 = GetAnswer(tc - 1)[0];
                derivative = (r2 - r1) / (2 * delta);
                SetCoeff(ww, i);
                Gradient[offset + i] = derivative;
                if (Math.Abs(derivative - Gr[i]) > 1.0E-7)
                {
                    double d = derivative;
                    double r = Gr[i];
                    double tmp = r / d;
                    d -= r;
                }
            }
            double sum = 0;
            for (int i = 0; i < Amount; i++)
            {
                double d = Gradient[offset + i];
                d -= Gr[i];
                sum += Math.Abs(d);
            }
        }
        public void CalcNumericalDelta(List<double> tests, double[] r, double Alpha)
        {
            double[] Gradient = new double[Amount];
            for (int i = 0; i < Amount; i++)
            {
                Gradient[i] = 0;
            }
            GetNumericalGradient(tests, Gradient, 0);
            for (int k = 0; k < Amount; k++)
            {
                double kof = GetDelta(k);
                kof = kof * Alpha + Gradient[k] * r[0];
                SetDelta(kof, k);
            }            
        }

        public void AdaMax(int n, double beta1, double beta2)
        {
            int s = layers.Count();
            for (int i = 0; i < s; i++)
            {
                layers[i].CalcDelta(0);
            }
            int fc = Amount;

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

        public override void Execute(double[] Enter)
        {
            SetEnter(Enter);
            layers[0].Execute(Enter);
            int last = layers.Count() - 1;
            //int s = layers[last]->GetNeuronCount();
            int ec = GetEntersCount();
            int xc = GetExitCount();
            for (int i = 0; i < xc; i++)
            {
                double ex = layers[last].GetAnswer(ec - 1)[i];
                Outs.Last()[i] = ex;
            }
        }

        public override double[] Backpropagation(double[] ds, int stage, int last_stage, int exit=-33000000)
        {
            int s = layers.Count() - 1;
            layers[s].Backpropagation(ds, stage, last_stage, exit);
            return null;
        }

        public void Adam(int n, double beta1, double beta2)
        {
            int s = layers.Count();
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
