using System;
using System.Collections.Generic;
using System.Linq;

namespace WindowsFormsApp1
{
    class My_Model
    {
        private static readonly double min = -0.5475006103515625;
        private static readonly double max = 1.2924957275390625;

        public My_Model()
        {
            net = new TNeuroNet("Net.txt");
        }
        private void create_dataset(double[][] v, int look_back, ref double[,,] dataX)
        {
            int Count = v.Count() - look_back + 1;
            dataX = new double[Count, look_back, 4];
            for (int i = 0; i < Count; i++)
            {
                for (int j = 0; j < look_back; j++)
                {
                    for (int k = 0; k < 4; k++) {
                        dataX[i, j, k] = v[i + j][k];
                    }
                }
            }
        }
        public void Predict(double[][] v, ref double[] ans)
        {
            int look_back = 3;
            int train_size = v.Count() - look_back + 1;
            if (train_size <= 0)
            {
                return;
            }

            List<double[]> t = new List<double[]>();

            for (int i = 0; i < v.Count(); i++)
            {
                double[] t2 = new double[4];
                t2[0] = (v[i][0] - min) / (max - min);
                t2[1] = (v[i][1] - min) / (max - min);
                t2[2] = (v[i][2] - min) / (max - min);
                t2[3] = (v[i][3] - min) / (max - min);

                //t2[0] = v[i][0];
                //t2[1] = v[i][1];
                //t2[2] = v[i][2];
                //t2[3] = v[i][3];
                t.Add(t2);
            }

            ans = new double[train_size];

            double[] enter = new double[4 * look_back + 1];
            for (int i = 0; i < train_size; i++)
            {
                int c = 0;
                for (int j = 0; j < look_back; j++)
                {
                    enter[c++] = t[i + j][0];
                    enter[c++] = t[i + j][1];
                    enter[c++] = t[i + j][2];
                    enter[c++] = t[i + j][3];
                }
                enter[c++] = 1;

                net.Execute(enter);
                var x = net.GetAnswer();
                double p = x[0];
                ans[i] = min + p * (max - min);
            }
        }
        private TNeuroNet net;

        public void Predict2(double[][] v, ref double[] ans)
        {
            int look_back = 3;
            int train_size = v.Count() - look_back + 1;
            if (train_size <= 0)
            {
                return;
            }

            List<double> t = new List<double>();

            for (int i = 0; i < v.Count(); i++)
            {
                double t2 = (v[i][3] - min) / (max - min);
                t.Add(t2);
            }

            ans = new double[train_size];

            for (int i = 0; i < train_size; i++)
            {
                int c = 0;
                Chain c1 = new Chain();
                for (int j = 0; j < look_back; j++)
                {
                    c1.Add(j, t[i + j]);
                }

                c1.CallProcessing();
                double x = c1.Newton(look_back);
                double p = x;
                ans[i] = min + p * (max - min);
            }
        }

        public void Predict3(double[][] v, ref double[] ans)
        {
            int look_back = 2;
            int train_size = v.Count() - look_back + 1;
            if (train_size <= 0)
            {
                return;
            }

            List<double> t = new List<double>();

            for (int i = 0; i < v.Count(); i++)
            {
                double t2 = (v[i][3] - min) / (max - min);
                //t.Add(t2);
                //t2 = (v[i][2] - min) / (max - min);
                //t.Add(t2);
                //t2 = (v[i][3] - min) / (max - min);
                t.Add(t2);
            }

            ans = new double[train_size];

            for (int i = 0; i < train_size; i++)
            {
                Linear c1 = new Linear();
                for (int j = 0; j < look_back; j++)
                {
                    c1.Add(j, t[i + j]);
                }
                double y = c1.GetTrend(look_back);
                double p = y;
                ans[i] = min + p * (max - min);
            }
        }
        public void Predict4(double[][] v, ref double[] ans)
        {
            int look_back = 1;
            int train_size = v.Count() - look_back + 1;
            if (train_size <= 0)
            {
                return;
            }

            List<double> t = new List<double>();

            for (int i = 0; i < v.Count(); i++)
            {
                double t2 = (v[i][0] - min) / (max - min);
                t.Add(t2);
                t2 = (v[i][3] - min) / (max - min);
                t.Add(t2);
            }

            ans = new double[train_size];

            for (int i = 0; i < train_size; i++)
            {
                double sum = 0;

                sum += t[2 * i] + t[2 * i + 1];
                double y = sum / (look_back + 1);
                double p = y;
                ans[i] = min + p * (max - min);
            }
        }
        public void Predict5(double[][] v, ref double[] ans)
        {
            int look_back = 3;
            int train_size = v.Count() - look_back + 1;
            if (train_size <= 0)
            {
                return;
            }

            List<double[]> t = new List<double[]>();

            for (int i = 0; i < v.Count(); i++)
            {
                double[] t2 = new double[4];
                t2[0] = (v[i][0] - min) / (max - min);
                t2[1] = (v[i][1] - min) / (max - min);
                t2[2] = (v[i][2] - min) / (max - min);
                t2[3] = (v[i][3] - min) / (max - min);
                t.Add(t2);
            }

            ans = new double[train_size];

            AR a = new AR("AR.txt");
            double[] enter = new double[4 * look_back + 1];
            for (int i = 0; i < train_size; i++)
            {
                int c = 0;
                for (int j = 0; j < look_back; j++)
                {
                    enter[c++] = t[i + j][0];
                    enter[c++] = t[i + j][1];
                    enter[c++] = t[i + j][2];
                    enter[c++] = t[i + j][3];
                }
                enter[c++] = 1;

                var x = a.Execute(enter);
                double p = x;
                ans[i] = min + p * (max - min);
            }
        }
        public void Predict6(double[][] v, ref double[] ans)
        {
            int look_back = 2;
            int train_size = v.Count() - look_back + 1;
            if (train_size <= 0)
            {
                return;
            }
            List<double[]> t = new List<double[]>();
            for (int i = 0; i < v.Count(); i++)
            {
                double[] t2 = new double[4];
                t2[0] = (v[i][0] - min) / (max - min);
                t2[1] = (v[i][1] - min) / (max - min);
                t2[2] = (v[i][2] - min) / (max - min);
                t2[3] = (v[i][3] - min) / (max - min);
                t.Add(t2);
            }

            ans = new double[train_size];

            for (int i = 0; i < train_size; i++)
            {
                double p_ema = t[i][3];
                double ema = 0;
                double alpha = 2.0 / (1.0 + look_back);
                for (int j = 0; j < look_back; j++)
                {
                    double t2 = t[i + j][3];
                    ema = t2 * alpha + p_ema * (1 - alpha);
                    p_ema = ema;
                }
                ans[i] = min + ema * (max - min);
            }
        }

        public void Predict7(double[][] v, ref double[] ans)
        {            
            List<double> t = new List<double>();
            for (int i = 1; i < v.Count(); i++)
            {
                t.Add(v[i][3]);
            }
            t.Add(v[v.Count()-1][3]);
            ans = t.ToArray();
        }

        public void Predict8(double[][] v, ref double[] ans)
        {           
            int train_size = v.Count();           

            if (train_size <= 0)
            {
                return;
            }

            ans = new double[train_size];

            LSTM1 a = new LSTM1("LSTM.txt");
            a.Clear();
            for (int i = 0; i < train_size; i++)
            {
                double[] t = new double[4];
                t[0] = (v[i][0] - min) / (max - min);
                t[1] = (v[i][1] - min) / (max - min);
                t[2] = (v[i][2] - min) / (max - min);
                t[3] = (v[i][3] - min) / (max - min);

                a.Execute(t);
                double p = a.GetAnswer();
                ans[i] = min + p * (max - min);

            }
        }

        public void Predict9(double[][] v, ref double[] ans)
        {            
            int train_size = v.Count();            

            if (train_size <= 0)
            {
                return;
            }

            ans = new double[train_size];

            //LSTM_Net a = new LSTM_Net("LSTM_Net.txt");
            NN a = new NN();

            /*
            List<List<double>> tests = new List<List<double>>();
            List<double> test = new List<double>();
            test.Add(0);
            test.Add(1);
            test.Add(2);
            test.Add(3);
            test.Add(4);
            tests.Add(test);
            a.Load("NN.txt");
            a.Fill(1,2);
            //a.Save("NN132132132132.txt");
            help_func.conjugate_gradient(a, tests, 0, 100, 1);
            */

            a.Load("NN.txt");
            a.Clear();
            for (int i = 0; i < train_size; i++)
            {
                double[] t = new double[4];

                //t[0] = (v[i][0] - min) / (max - min);
                //t[1] = (v[i][1] - min) / (max - min);
                //t[2] = (v[i][2] - min) / (max - min);
                //t[3] = (v[i][3] - min) / (max - min);

                a.Execute(v[i]);
                double p = a.GetAnswer(i)[0];
                ans[i] = p; //min + p * (max - min);

            }
        }
    }
}
