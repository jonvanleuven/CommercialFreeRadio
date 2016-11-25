using System;
using System.Windows.Forms;
using CommercialFreeRadio.Impl;

namespace CommercialFreeRadio.TrayApp
{
    static class Program
    {
        [STAThread]
        static void Main(string[] argString)
        {
            try
            {
                //argString = new[] {"/sonos=localhost"};
                Application.Run(new SysTrayApp(argString));
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw e;
            }
        }
    }
}
