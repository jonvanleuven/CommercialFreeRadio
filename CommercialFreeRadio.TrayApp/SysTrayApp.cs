using System;
using System.Drawing;
using System.Windows.Forms;
using CommercialFreeRadio.Impl;

namespace CommercialFreeRadio.TrayApp
{
    public class SysTrayApp : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        public SysTrayApp(string[] args)
        {
            // Create a simple tray menu with only one item.
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);

            // Create a tray icon. In this example we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            trayIcon = new NotifyIcon();
            trayIcon.Text = @"CommercialFreeRadio";
            trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);

            // Add menu to tray icon and show it.
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;

            Logger.AddInfoListerer(m =>
            {
                trayIcon.BalloonTipText = m;
                trayIcon.ShowBalloonTip(10000);
            });
            //TODO start in eigen thread
            CommercialFreeRadio.Program.Main(args);
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.
            base.OnLoad(e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // Release the icon resource.
                trayIcon.Dispose();
            }
            base.Dispose(isDisposing);
        }
        
    }
}
