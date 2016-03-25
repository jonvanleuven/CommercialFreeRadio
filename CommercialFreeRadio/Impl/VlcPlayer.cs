using System.Threading;

namespace CommercialFreeRadio.Impl
{
    public class VlcPlayer : IPlayer
    {
        private const string Executable = @"""C:\Program Files (x86)\VideoLAN\VLC\vlc.exe""";
        private IRadioStation currentStation;

        public string Name
        {
            get { return string.Format("VLC player ({0})", Executable); }
        }

        public void Play(IRadioStation station)
        {
            var uri = station.Uri.Replace("acc://", "http://").Replace("x-rincon-mp3radio://", "http://");
            currentStation = station;
            ExecuteCommandAsync(string.Format("{0} {1}", Executable, uri));
        }

        public bool? IsPlaying(IRadioStation station)
        {
            return currentStation != null && station.Uri == currentStation.Uri;
        }

        private static void ExecuteCommandSync(object command)
        {
            var procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;
            var proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
            var result = proc.StandardOutput.ReadToEnd();
        }

        private static void ExecuteCommandAsync(string command)
        {
            var objThread = new Thread(new ParameterizedThreadStart(ExecuteCommandSync));
            objThread.IsBackground = true;
            objThread.Priority = ThreadPriority.AboveNormal;
            objThread.Start(command);
        }
    }
}
