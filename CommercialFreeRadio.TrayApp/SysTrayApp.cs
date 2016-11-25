using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CommercialFreeRadio.Impl;

namespace CommercialFreeRadio.TrayApp
{
    public class SysTrayApp : Form
    {
        private readonly NotifyIcon trayIcon;
        private readonly System.ComponentModel.BackgroundWorker backgroundWorker1;
        private readonly CommercialFreeRadio.Program program;
        private readonly SonosPlayer sonosPlayer;
        private string lastBalloonMessage;

        public SysTrayApp(string[] args)
        {
            this.program = new CommercialFreeRadio.Program(args);
            sonosPlayer = program.Player as SonosPlayer;

            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);

            var trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Pause", OnToggle);
            var sonosMenuItem = trayMenu.MenuItems.Add("Sonos quickstart");
            foreach (var station in program.AllStations)
            {
                sonosMenuItem.MenuItems.Add(new MenuItem("Start " + station.Name + " on all Sonos players", (a, b) =>
                {
                    var all = new UpnpInterface(sonosPlayer.Ip).GetZoneGroupState().ZoneGroups.SelectMany(zg => zg.ZoneGroupMembers)
                                .Distinct()
                                .Select(p => new { IsMaster = p.Ip == sonosPlayer.Ip, Player = new UpnpInterface(p.Ip), GroupMember = p });
                    var master = all.Single(p => p.IsMaster);
                    foreach (var slave in all.Where(x => !x.IsMaster))
                    {
                        slave.Player.SetAVTransportURI("x-rincon:" + master.GroupMember.Id);
                        slave.Player.SetVolume(3);
                    }
                    sonosPlayer.Play(station);
                    sonosPlayer.SetVolume(6);
                }));
            }
            sonosMenuItem.Enabled = (sonosPlayer != null);
            trayMenu.MenuItems.Add("Exit", OnExit);

            trayIcon = new NotifyIcon();
            trayIcon.Text = Truncate(program.Player.Name, 64);
            trayIcon.Icon = new System.Drawing.Icon("CommercialFreeRadio.ico");
            trayIcon.MouseClick += (a, b) =>
            {
                if (lastBalloonMessage != null)
                {
                    trayIcon.BalloonTipText = lastBalloonMessage;
                    trayIcon.ShowBalloonTip(10000);
                }
            };

            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;

            Logger.AddInfoListerer(m =>
            {
                lastBalloonMessage = m;
                trayIcon.BalloonTipText = m;
                trayIcon.ShowBalloonTip(10000);
            });

            backgroundWorker1.RunWorkerAsync();
            backgroundWorker1.RunWorkerCompleted += WorkerCompleted;
        }

        private void WorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Logger.Info("CommercialFreeRadio stopped");
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.
            base.OnLoad(e);
        }

        private void OnToggle(object sender, EventArgs e)
        {
            var item = sender as MenuItem;
            if (item.Text == @"Resume")
            {
                OnResumeProgram(sender, e);
                item.Text = @"Pause";
            }
            else
            {
                OnPauseProgram(sender, e);
                item.Text = @"Resume";
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OnPauseProgram(object sender, EventArgs e)
        {
            trayIcon.Icon = new System.Drawing.Icon("CommercialFreeRadio_off.ico");
            program.Cancel();
        }

        private void OnResumeProgram(object sender, EventArgs e)
        {
            trayIcon.Icon = new System.Drawing.Icon("CommercialFreeRadio.ico");
            if ( !backgroundWorker1.IsBusy )
                backgroundWorker1.RunWorkerAsync();
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

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                program.Run();
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
        }

        private static string Truncate(string s, int l)
        {
            if (l < 3)
                l = 3;
            if (s == null)
                return s;
            if (s.Length <= l)
                return s;
            return s.Substring(0, l - 3) + "...";
        }
    }
}
