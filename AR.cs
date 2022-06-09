using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    class AR
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

        public AR(String FileName)
        {
            if (File.Exists(FileName))
            {
                // Read entire text file content in one string
                string[] text = File.ReadAllLines(FileName);
                int ec;
                String s, word;
                int line = 0;
                ec = int.Parse(text[line]); line++;
                SetEnterCount(ec);
                s = text[line]; line++;
                for (int i = 0; i < ec + 1; i++)
                {
                    word = ReadWord(ref s);
                    double we = double.Parse(word);
                    w[i] = we;
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
                s = "";
                for (int i = 0; i < EnterCount + 1; i++)
                {
                    s += w[i].ToString() + " ";
                }
                file.WriteLine(s);
            }
        }

        public void SetEnterCount(int ECount)
        {            
            w = new double[ECount + 1];
            Delta = new double[ECount + 1];
            EnterCount = ECount;        
        }

        public int GetCoeffAmount()
        {
            return EnterCount + 1;
        }

        public void Fill(int flag)
        {
            var rand = new Random();
            for (int k = 0; k < w.Length; k++)
                if (flag < 0)
                    w[k] = (rand.NextDouble() - 0.5);
                else if (flag == 0)
                    w[k] = 0;
                else if (flag == 1)
                    w[k] = 1;
                else if (flag == 2)
                    w[k] = 0.5;
        }

        public double Execute(double[] Enter)
        {            
            double sum = 0;
            for (int i = 0; i < EnterCount; i++)
            {
                sum += w[i] * Enter[i];
            }
            sum += w[EnterCount];
            return sum;
        }

        public void FreeDelta()
        {
            for (int i = 0; i < EnterCount + 1; i++)
                Delta[i] = 0;
        }
        public void GetGradient(double[] Gradient)
        {
            for (int i = 0; i < EnterCount + 1; i++)
            {
                Gradient[i] = w[i];
            }
        }
        public void SetDifference(double[] Delta)
        {
            int count = 0;
            for (int k = 0; k < w.Length; k++)
                this.Delta[k] = Delta[count];
        }

        public void CalcDelta(double[] Enter,double CorrectAnswer, double Alpha)
        {
            double error = CorrectAnswer - Execute(Enter);            
            for (int i = 0; i < EnterCount; i++)
            {
                Delta[i] = error * Enter[i];
            }
            Delta[EnterCount] = error;
        }

        private void RandomDirection()
        {
            var rand = new Random();
            for (int k = 0; k < Delta.Length; k++)                
                Delta[k] = rand.NextDouble() * 2 - 2;            
        }

        public void AplyError(double h)
        {
            for (int k = 0; k < w.Length; k++)
                w[k] = w[k] + h * Delta[k];
        }

        private int EnterCount;

        private double[] w;        
        private double[] Delta;
    }
}
