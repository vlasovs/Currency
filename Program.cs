using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>        
        [STAThread]
        static void Main()
        {
            try
            {
                Console.WriteLine("Start");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            catch (Exception err)
            {
                Console.WriteLine("Ошибка Form Load: " + err.ToString());
                Console.ReadKey();
                throw new Exception("Ошибка Form Load: " + err.ToString());
            }
        }
    }
}
