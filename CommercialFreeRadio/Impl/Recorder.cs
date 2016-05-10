using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace CommercialFreeRadio.Impl
{
    public class Recorder : IPlayer
    {
        private readonly IPlayer player;
        private readonly string directory;
        private Thread thread;
        private IRadioStation station;
        private string filename;
        private bool stopRecording;
        private readonly IEnumerable<IRadioStation> stations;

        public Recorder(IPlayer player, string directory, IEnumerable<IRadioStation> stations )
        {
            this.player = player;
            this.directory = directory;
            this.stations = stations;
        }
        public string Name
        {
            get { return "Recorder using player: " + player.Name; }
        }
        public void Play(IRadioStation station)
        {
            if (thread != null && !stopRecording)
            {
                stopRecording = true;
                while (stopRecording)
                {
                    Logger.Debug("Waiting for recorder to stop");   
                    Thread.Sleep(100);
                }
                thread = null;
            }
            if (stations.Any(s => s.Name == station.Name))
            {
                this.station = station;
                this.filename = string.Format("{3}/{0:yyyyMMdd}_{1:HHmmss}_{2}.mp3", DateTime.Now, DateTime.Now, station.Name.Replace(" ", "_"), directory);

                thread = new Thread(StartRecording);
                thread.IsBackground = true;
                thread.Start();
            }
            player.Play(station);
        }
        public void StartRecording()
        {
            stopRecording = false;
            Stream readStream = null;
            try
            {
                var url = station.Uri.Replace("acc://", "http://").Replace("x-rincon-mp3radio://", "http://");
                Logger.Info("Start recording '" + url + "' to '" + filename + "'");
                var req = WebRequest.Create(url);
                var response = req.GetResponse();
                var Length = 256;
                var buffer = new Byte[Length];
                readStream = response.GetResponseStream();
                var bytesRead = readStream.Read(buffer, 0, Length);
                while (bytesRead > 0 && !stopRecording)
                {
                    var fileStream = new FileStream(filename, FileMode.Append);
                    fileStream.Write(buffer, 0, bytesRead);
                    fileStream.Close();
                    bytesRead = readStream.Read(buffer, 0, Length);
                }
                stopRecording = false;
                Logger.Info("Stream saved to '" + filename + "'");
                PostProcessFile(filename);
            }
            catch (System.Net.WebException e)
            {
                Logger.Error(e);
            }
            finally
            {
                if (readStream != null)
                    readStream.Close();
                stopRecording = false;
            }
        }

        private void PostProcessFile(string f)
        {
            var secondsToRemoveFromEnd = 0;
            if (f.Contains("Sublime"))
                secondsToRemoveFromEnd = 4;
            if (f.Contains("Sky"))
                secondsToRemoveFromEnd = 51;
            if (secondsToRemoveFromEnd == 0)
                return;
            var dest = f + "_temp";
            var content = File.ReadAllBytes(filename);
            var takeBytes = content.Length - (secondsToRemoveFromEnd*17000);
            if (takeBytes < 0)
                return;
            File.WriteAllBytes(dest, content.Take(takeBytes).ToArray());
            File.Delete(f);
            File.Move(dest, f);
        }

        public bool? IsPlaying(IRadioStation station)
        {
            return player.IsPlaying(station);
        }
    }
//
//    public class RecordSession : Thread
//    {
//        public static RecordSession Start()
//        {
//            return new RecordSession(StartRecording);
//        }
//
//        private RecordSession() : base(StartRecording)
//        {
//            
//        }
//
//        public void StartRecording()
//        {
//            
//        }
//    }
}
