using CsvHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            model = new My_Model();

            var c = new Chain();

            //c.Add(1, 1);
            //c.Add(2, 0.25);
            //c.Add(3, 0.11111111111111111111111);
            //c.CallProcessing();
            var x = c.Newton(4);
            panel2_Resize(0, new EventArgs());
        }

        private String Number(double x) {
            return (Math.Round(x * 10000) / 10000).ToString();
        }
            
        private void button3_Click(object sender, EventArgs e)
        {
            openFileDialog1.Multiselect = false;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string csvPath = openFileDialog1.FileName;
                var records = new List<Foo>();
                using (var reader = new System.IO.StreamReader(csvPath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        var record = new Foo
                        {
                            Data = csv.GetField("Data"),
                            First = csv.GetField<double>("First"),
                            Min = csv.GetField<double>("Min"),
                            Max = csv.GetField<double>("Max"),
                            Last = csv.GetField<double>("Last")
                        };
                        records.Add(record);
                    }
                }
                double x2;
                
                value = x2 = records[0].First;
                for (int i = 0; i < records.Count(); i++)
                {
                    double x1 = records[i].First;
                    records[i].First -= x2;
                    records[i].Min -= x2;
                    records[i].Max -= x2;
                    records[i].Last -= x2;
                    x2 = x1;
                }
                
                String s = "";
                foreach (var r in records)
                {
                    s += Number(r.First) +" " + Number(r.Min) + " " + Number(r.Max) + " " + Number(r.Last) + "\n";
                }
                richTextBox1.Text = s;
                s = "";
                /*
                double min = -0.5475006103515625;
                double max = 1.2924957275390625;

                foreach (var r in records)
                {
                    s += ((r.First-min)/(max-min)).ToString() + " " + ((r.Min - min)/(max - min)).ToString() + " " + ((r.Max - min)/(max - min)).ToString() + " " + ((r.Last - min)/(max - min)).ToString() + "\n";
                }
                richTextBox2.Text = s;
                */
            }
        }

        private void GetData(String FileName, out double[][] v, ref List<double[]> day)
        {
            string csvPath = FileName;
            var records = new List<Foo>();
            using (var reader = new System.IO.StreamReader(csvPath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    var record = new Foo
                    {
                        Data = csv.GetField("Data"),
                        First = csv.GetField<double>("First"),
                        Min = csv.GetField<double>("Min"),
                        Max = csv.GetField<double>("Max"),
                        Last = csv.GetField<double>("Last")
                    };
                    records.Add(record);
                }
            }
            double x2;

            value = x2 = records[0].First;
            for (int i = 0; i < records.Count(); i++)
            {
                double x1 = records[i].First;
                records[i].First -= x2;
                records[i].Min -= x2;
                records[i].Max -= x2;
                records[i].Last -= x2;
                x2 = x1;
            }

            List<double[]> vals = new List<double[]>();

            double min = records[0].First;
            double max= records[0].First;
            for (int i = 0; i < records.Count(); i++)
            {
                double[] tmp = new double[4];
                tmp[0] = records[i].First;
                tmp[1] = records[i].Min;
                tmp[2] = records[i].Max;
                tmp[3] = records[i].Last;
                vals.Add(tmp);

                if (min > records[i].Min) min = records[i].Min;                
                if (max < records[i].Max) max = records[i].Max;
            }
            v = vals.ToArray();
                        
            double[] tmp1 = new double[4];
            tmp1[0] = records[0].First;
            tmp1[1] = min;
            tmp1[2] = max;
            tmp1[3] = records[records.Count() - 1].Last;
            day.Add(tmp1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<double[]> vals = new List<double[]>();
            foreach(var line in richTextBox1.Lines)
            {
                if (line.Length > 0)
                {
                    String[] str = line.Split(' ');
                    double[] tmp = new double[4];
                    for (int i = 0; i < 4; i++)
                    {
                        tmp[i] = double.Parse(str[i]);
                    }
                    vals.Add(tmp);
                }
            }

            double[][] v = vals.ToArray();
            double[] ans = { };
            if (radioButton1.Checked) model.Predict(v, ref ans);
            else if (radioButton2.Checked) model.Predict2(v, ref ans);
            else if (radioButton3.Checked) model.Predict3(v, ref ans);
            else if (radioButton4.Checked) model.Predict4(v, ref ans);
            else if (radioButton5.Checked) model.Predict5(v, ref ans);
            else if (radioButton6.Checked) model.Predict6(v, ref ans);
            else if (radioButton8.Checked) model.Predict7(v, ref ans);
            else if (radioButton9.Checked) model.Predict8(v, ref ans);
            else if (radioButton10.Checked) model.Predict9(v, ref ans);

            if (checkBox0.Checked)
            {
                for (int i = 0; i < ans.Length; i++)
                    ans[i] = -ans[i];               
            }

            String s = "";
            foreach (var a in ans)
            {
                s += a.ToString() + "\n";
            }
            richTextBox2.Text = s;
        }

        My_Model model;

        private void button2_Click(object sender, EventArgs e)
        {
            List<double> v1 = new List<double>();
            List<double> w1 = new List<double>();
            foreach (var line in richTextBox1.Lines)
            {
                if (line.Length > 0)
                {
                    w1.Add(double.Parse(line.Split(' ')[0]));
                    v1.Add(double.Parse(line.Split(' ')[3]));
                }
            }
            double[] y=new double[w1.Count];
            double value1 = value;
            for (int i = 0; i < w1.Count; i++)
            {
                y[i] = value1 + v1[i];
                //Console.WriteLine(value1.ToString()+" "+y[i].ToString());
                value1 += w1[i];
            }
            List<double> v2 = new List<double>();
            foreach (var line in richTextBox2.Lines)
            {
                if (line.Length > 0)
                {
                    v2.Add(double.Parse(line));
                }
            }            
            int trend = 0;
            double dollars = 0;
            //double bank = 78807.75;
            double bank = 75239.41;
            double prev = bank;
            int count = 0;
            int damages = 0;

            double[] y1 = v1.ToArray();
            double[] y2 = v2.ToArray();
            double[] x = new double[y1.Count() + 1];
            for(int i = 0; i <= y1.Count(); i++)
            {
                x[i] = i;
            }
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            chart1.Series[2].Points.Clear();

            chart1.Series[3].Points.Clear();
            chart1.Series[4].Points.Clear();

            chart1.Series[5].Points.Clear();
            chart1.Series[6].Points.Clear();
            chart1.Series[7].Points.Clear();
            chart1.Series[8].Points.Clear();
            chart1.Series[9].Points.Clear();
            chart1.Series[10].Points.Clear();

            chart1.Legends[0].Enabled = false;

            int start = y1.Count() - y2.Count(); 
            int r = 0;
            int c = 0;
            //using (StreamWriter sw1 = new StreamWriter("testc.txt", false, System.Text.Encoding.Default))
            for (int i = 0; i <= y1.Count(); i++)
            {
                if (i < y1.Count())
                    chart1.Series[0].Points.AddXY(x[i], y1[i]);
                
                if (i > start) {
                    chart1.Series[1].Points.AddXY(x[i], y2[i - start - 1]);
                    //chart1.Series[2].Points.AddXY(x[i], y2[i - start]);
                }
                
                if (i > start && i < y1.Count() - 1)
                {
                    double r1 = y1[i]; //- y1[i];
                    double r2 = y2[i - start - 1]; // - y2[i - start];
                    c++;
                    if (r1 * r2 >= 0)
                    {
                        //chart1.Series[2].Points.AddXY(x[i], y1[i]);
                        r++;
                    }
                }

                if (i >= start && i < y1.Count())
                {
                    //trend = (y1[i] > 0) ? 1 : -1;
                    int current = 0;
                    if (y2[i - start] > 0) current = 1;
                    if (y2[i - start] < 0) current = -1;
                    if (Math.Abs(y2[i - start]) < 0.01) continue;

                    if (trend == 0 || current != trend)
                    {
                        if (current == 1 && bank > 1000 * y[i])
                        {
                            dollars += 1000;
                            double v = y[i];
                            bank -= 1000 * (v) * 1.0005;
                            count++;
                            chart1.Series[2].Points.AddXY(x[i], y1[i]);
                            //sw1.WriteLine(bank.ToString());
                        }
                        else if (current == -1 && dollars > 0)
                        {
                            double v = y[i];
                            bank += 1000 * (v) * 0.9995;
                            dollars -= 1000;
                            if (prev > bank)
                            {
                                damages++;
                                chart1.Series[3].Points.AddXY(x[i], y1[i]);
                            }
                            else
                            {
                                chart1.Series[4].Points.AddXY(x[i], y1[i]);
                            }
                            prev = bank;
                            //Console.WriteLine(bank.ToString());
                            count++;
                            //sw1.WriteLine(bank.ToString());
                        }
                    }
                    trend = current;
                }
            }
            if (dollars == 1000)
            {
                int i = y.Count() - 1;
                bank += dollars * (y[i]) * 0.9995;                
                if (prev > bank)
                {
                    damages++;
                    chart1.Series[3].Points.AddXY(x[i], y1[i]);
                }
                else {
                    chart1.Series[4].Points.AddXY(x[i], y1[i]);
                }
            }

            //Console.WriteLine(bank.ToString());

            double per = (double)r / c;
            textBox1.Text = (per * 100).ToString() + " %";
            textBox2.Text = bank.ToString() + " " + count.ToString() + " " + damages.ToString();

            chart1.ChartAreas[0].AxisY.IsStartedFromZero = false;
        }

        private void panel2_Resize(object sender, EventArgs e)
        {
            button1.Width = panel2.Width - button1.Left - 5;
            button2.Width = panel2.Width - button2.Left - 5;
            button3.Width = panel2.Width - button3.Left - 5;
            textBox1.Width = panel2.Width - textBox1.Left - 5;
            button4.Width = panel2.Width - button4.Left - 5;
            textBox2.Width = panel2.Width - textBox2.Left - 5;
        }

        private void FilesProcess(int index1) {
            String text = "";
            int r = 0;
            int c = 0;
            int trend = 0;
            //double dollars = 1106;
            double dollars = 0;
            //double my_start = 78807.75;
            double my_start = 75239.41;
            double bank = my_start;
            double bank_ = my_start;
            double prev = bank;
            int count = 0;
            int damages = 0;

            string writePath = @"Result.txt";
            StreamWriter sw = new StreamWriter(writePath, false, System.Text.Encoding.Default);

            int daycount = 0;

            int counter = -2;

            List<double[]> days = new List<double[]>();

            double kurs = 0;
            int damage_count = 0;            

            foreach (string csvPath in openFileDialog1.FileNames)
            {

                double[][] v;
                try
                {
                    GetData(csvPath, out v, ref days);
                }
                catch (Exception ex1)
                {
                    Console.WriteLine(csvPath);
                    Console.WriteLine(ex1.Message);
                    continue;
                }

                double[][] w = days.ToArray();

                double[] y2 = { };
                double[] y3 = { };

                if (index1 == 0)
                {
                    if (radioButton1.Checked) model.Predict(v, ref y2);
                    else if (radioButton2.Checked) model.Predict2(v, ref y2);
                    else if (radioButton3.Checked) model.Predict3(v, ref y2);
                    else if (radioButton4.Checked) model.Predict4(v, ref y2);
                    else if (radioButton5.Checked) model.Predict5(v, ref y2);
                    else if (radioButton6.Checked) model.Predict6(v, ref y2);
                    else if (radioButton8.Checked) model.Predict7(v, ref y2);
                    else if (radioButton9.Checked) model.Predict8(v, ref y2);
                    else if (radioButton10.Checked) model.Predict9(v, ref y2);

                    if (radioButton1.Checked) model.Predict(w, ref y3);
                    else if (radioButton2.Checked) model.Predict2(w, ref y3);
                    else if (radioButton3.Checked) model.Predict3(w, ref y3);
                    else if (radioButton4.Checked) model.Predict4(w, ref y3);
                    else if (radioButton5.Checked) model.Predict5(w, ref y3);
                    else if (radioButton6.Checked) model.Predict6(w, ref y3);
                    else if (radioButton8.Checked) model.Predict7(w, ref y3);
                    else if (radioButton9.Checked) model.Predict8(w, ref y3);
                    else if (radioButton10.Checked) model.Predict9(w, ref y3);

                    if (checkBox0.Checked)
                    {
                        for (int i = 0; i < y2.Length; i++)
                            y2[i] = -y2[i];
                        for (int i = 0; i < y3.Length; i++)
                            y3[i] = -y3[i];
                    }
                }
                else
                {
                    switch (index1)
                    {
                        case 1: model.Predict(v, ref y2); break;
                        case 2: model.Predict5(v, ref y2); break;
                        case 3: model.Predict3(v, ref y2); break;
                        case 4: model.Predict4(v, ref y2); break;
                        case 5: model.Predict6(v, ref y2); break;
                        case 6: model.Predict2(v, ref y2); break;
                        case 7: model.Predict8(v, ref y2); break;
                        case 8: model.Predict9(v, ref y2); break;
                        case 9: model.Predict7(v, ref y2); break;
                        
                    }
                    switch (index1)
                    {
                        case 1: model.Predict(w, ref y3); break;
                        case 2: model.Predict5(w, ref y3); break;
                        case 3: model.Predict3(w, ref y3); break;
                        case 4: model.Predict4(w, ref y3); break;
                        case 5: model.Predict6(w, ref y3); break;
                        case 6: model.Predict2(w, ref y3); break;
                        case 7: model.Predict8(w, ref y3); break;
                        case 8: model.Predict9(w, ref y3); break;
                        case 9: model.Predict7(w, ref y3); break;
                    }

                    if (checkBox0.Checked)
                    {
                        for (int i = 0; i < y2.Length; i++)
                            y2[i] = -y2[i];
                        for (int i = 0; i < y3.Length; i++)
                            y3[i] = -y3[i];
                    }
                }

                List<double> v1 = new List<double>();

                for (int i = 0; i < v.Length; i++)
                {
                    v1.Add(v[i][3]);
                }

                double[] y1 = v1.ToArray();
                //double[] y2 = v2.ToArray();

                double[] x = new double[y1.Count() + 1];
                for (int i = 0; i <= y1.Count(); i++)
                {
                    x[i] = i;
                }
                int start = y1.Count() - y2.Count();

                foreach (var y_ in y2)
                {
                    sw.WriteLine(y_.ToString());
                }
                int rr = 0;
                int cc = 0;
                for (int i = 0; i <= y1.Count(); i++)
                {
                    if (i > start && i < y1.Count() - 1)
                    {
                        double r1 = y1[i];  // - y1[i];
                        double r2 = y2[i - start - 1];  // - y2[i - start - 1];

                        c++;
                        cc++;
                        if (r1 * r2 >= 0)
                        {
                            r++;
                            rr++;
                        }
                    }
                }
                double perd = (double)rr / cc * 100;

                counter += start;

                double[] y = new double[v.Count()];
                double value1 = value;
                for (int i = 0; i < v.Count(); i++)
                {
                    y[i] = value1 + v[i][3];
                    //Console.WriteLine(value1.ToString() + " " + y[i].ToString());
                    value1 += v[i][0];
                }

                trend = 0;
                double history = bank;
                //using (StreamWriter sw1 = new StreamWriter("testb.txt", false, System.Text.Encoding.Default))
                for (int i = 0; i < y2.Count() - 1; i++)
                {
                    counter++;
                    int current = 0;
                    double r1 = y2[i];
                    if (r1 > 0) current = 1;
                    else if (r1 < 0) current = -1;
                    else continue;

                    if (Math.Abs(r1) < 0.01) continue;
                    //if (Math.Abs(r2) < 0.01) continue;
                    //if (r1 * r2 < 0) continue;
                    
                    /*
                    if (y3.Count() > 0)
                    {
                        if (y3[y3.Count() - 1] < 0.0) continue;
                    }*/

                    //else {
                    //    continue;
                    //}

                    if (/*trend == 0 ||*/ current != trend)
                    {
                        if (current == 1 && dollars == 0 /*&& bank > 1000 * (y[i + start])*/)
                        {   //Buy
                            dollars += 1000;
                            bank -= 1000 * (y[i + start]) * 1.0005;
                            count++;
                            //sw1.WriteLine(bank.ToString());
                        }
                        else if (current == -1 && dollars > 0)
                        {   //Sell

                            bank += 1000 * (y[i + start]) * 0.9995;
                            dollars -= 1000;
                            count++;
                            if (prev > bank)
                            {
                                damages++;
                            }
                            if (prev > bank && damage_count < 0)
                            {                                
                                bank -= 1000 * (y[i + start]) * 0.9995;
                                dollars += 1000;
                                damage_count++;
                            }
                            else
                            {
                                damage_count = 0;
                                prev = bank;
                                //sw1.WriteLine(bank.ToString());
                                //Console.WriteLine(bank.ToString());
                                if (index1 > 0)
                                    chart1.Series[index1 + 4].Points.AddXY(counter, bank);
                                else
                                    chart1.Series[1].Points.AddXY(counter, bank);
                            }
                        }
                    }
                    trend = current;
                }

                kurs = y[y.Count() - 1];

                if (dollars == 1000)
                {
                    count++;
                    bank += dollars * (y[y.Count() - 1]) * 0.9995;
                    dollars = 0;

                    
                    if (prev > bank)
                    {
                        damages++;
                    }
                    prev = bank;
                    if (index1 > 0)
                        chart1.Series[index1 + 4].Points.AddXY(counter, bank);
                    else
                        chart1.Series[1].Points.AddXY(counter, bank);
                    /**/

                    /*
                    if (prev > bank)
                    {
                        damages++;
                        bank -= 1000 * (y[y.Count() - 1]) * 0.9995;
                        dollars += 1000;
                    }
                    else
                    {
                        prev = bank;
                        //sw1.WriteLine(bank.ToString());
                        //Console.WriteLine(bank.ToString());
                        if (index1 > 0)
                            chart1.Series[index1 + 4].Points.AddXY(counter, bank);
                        else
                            chart1.Series[1].Points.AddXY(counter, bank);
                    }*/
                }

                //Console.WriteLine(bank.ToString());

                if (history < bank)
                {
                    daycount++;
                    text += csvPath.Substring(csvPath.LastIndexOf('\\') + 1) + " - " + perd.ToString() + " %, " + bank.ToString() + ", " + (bank - bank_).ToString() + "\n";
                }
                else
                {
                    text += csvPath.Substring(csvPath.LastIndexOf('\\') + 1) + " + " + perd.ToString() + " %, " + bank.ToString() + ", " + (bank - bank_).ToString() + "\n";
                }

                //dollars += bank / y1[y1.Count() - 1];
                //bank = 0;
                bank_ = bank;
            }

            if (dollars == 1000)
            {
                count++;
                bank += dollars * (kurs) * 0.9995;
                dollars = 0;
                
                if (prev > bank)
                {
                    damages++;
                }
            }

            sw.Close();
            sw.Dispose();

            double per = (double)r / c;
            textBox1.Text = (per * 100).ToString() + " %";
            textBox2.Text = dollars.ToString() + " " + bank.ToString() + " " + count.ToString() + " " + damages.ToString() + " " + daycount.ToString() + ", percent: " + (((bank - my_start) / my_start) * 100).ToString();
            //textBox2.Text = dollars.ToString() + " " + count.ToString() + " " + damages.ToString();

            richTextBox2.Text = text;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            
            openFileDialog1.Multiselect = true;
            foreach (var s in chart1.Series)
            {
                s.Points.Clear();
            }

            chart1.Legends[0].Enabled = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                
                if (checkBox1.Checked) FilesProcess(1);
                if (checkBox2.Checked) FilesProcess(2);
                if (checkBox3.Checked) FilesProcess(3);
                if (checkBox4.Checked) FilesProcess(4);
                if (checkBox5.Checked) FilesProcess(5);
                if (checkBox6.Checked) FilesProcess(6);
                if (checkBox7.Checked) FilesProcess(7);
                if (checkBox8.Checked) FilesProcess(8);
                if (checkBox9.Checked) FilesProcess(9);
               
            }
        }
        private double value;

      
    }
    public class Foo
    {
        public string Data { get; set; }
        public double First { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public double Last { get; set; }
    }
}
