using System;
using System.Windows.Forms;

namespace CommercialFreeRadio.TrayApp
{
    static class Program
    {
        [STAThread]
        static void Main(string[] argString)
        {
            Application.Run(new SysTrayApp(argString));
        }
    }
}
