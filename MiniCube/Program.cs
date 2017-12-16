using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;


namespace MiniCube
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            String mutexID = "cube mutex";
            using (Mutex mutex = new Mutex(false, mutexID))
            {
                if (!mutex.WaitOne(0, false))
                {
                    MessageBox.Show("Cube aleady running!");
                    return;
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new CubeForm());
            }

        }
    }
}
