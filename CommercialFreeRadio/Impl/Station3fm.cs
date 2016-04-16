using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;

namespace CommercialFreeRadio.Impl
{
    public class Station3fm : IRadioStation, ITuneinRadioStation
    {
        private readonly Playlist3FmInterface playlist;
        public Station3fm(IJsonInterface3Fm reader = null)
        {
            playlist = new Playlist3FmInterface(reader);
        }

        public string Name
        {
            get { return "3FM"; }
        }

        public string Uri
        {
            get { return "x-rincon-mp3radio://icecast.omroep.nl/3fm-bb-mp3"; }
        }
        public int TuneinId { get { return 6707; } }
        public string TuneinTitle { get { return "NPO 3FM Serious Radio"; } }

        public bool? IsPlayingCommercialBreak()
        {
            var now = DateTime.Now;
            if ((now.Minute == 56 && now.Second >= 20) || now.Minute >= 57 || now.Minute <= 6)
            {
                try
                {
                    var currentTrack = playlist.ReadCurrentTrack();
                    return currentTrack != null && now > currentTrack.StartTime && now < currentTrack.StopTime;
                }
                catch (WebException e)
                {
                    Logger.Error(e);
                    return null;
                }
            }
            return false;
        }

        public bool? IsMyStream(string uri)
        {
            if (uri.Contains("3fm-alternative"))
                return false;
            return Uri == uri || uri.ToLower().Contains("3fm");
        }

        public interface IJsonInterface3Fm
        {
            byte[] ReadJsonData();
        }

        internal class HttpJsonInterface3Fm : IJsonInterface3Fm 
        {
            public byte[] ReadJsonData()
            {
                var url = string.Format("http://radioplayer.npo.nl/data/radiobox2/nowonair/3.json?ac={0}", DateTime.Now.Ticks);
                Logger.Debug(url);
                var req = WebRequest.Create(url);
                var response = req.GetResponse();
                var stream = response.GetResponseStream();
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }

        public class Playlist3FmInterface
        {
            private readonly IJsonInterface3Fm datareader;
            private readonly TimeSpan delay = new TimeSpan(0, 0, 18); //audiostream loop 18 seconden achter op json interface
            private DateTime lastCall;
            private Track lastTrack;

            public Playlist3FmInterface(IJsonInterface3Fm datareader = null)
            {
                this.datareader = datareader??new HttpJsonInterface3Fm();
            }

            public Track ReadCurrentTrack()
            {
                if (lastTrack != null && lastTrack.StopTime > DateTime.Now)
                    return lastTrack;
                if(DateTime.Now - lastCall < new TimeSpan(0, 0, 10))
                    return lastTrack;
                var jsonBytes = datareader.ReadJsonData();
                lastCall = DateTime.Now;
                var ser = new DataContractJsonSerializer(typeof(JsonResult));
                lastTrack = Track.Parse(((JsonResult)ser.ReadObject(new MemoryStream(jsonBytes))).results.FirstOrDefault(), delay);
                Logger.Debug(lastTrack);
                return lastTrack;
            }

            public class Track
            {
                internal static Track Parse(Song s, TimeSpan delay)
                {
                    if (s == null)
                        return null;
                    return new Track
                    {
                        Artist = s.songfile.artist,
                        Title = s.songfile.title,
                        StartTime = DateTime.Parse(s.startdatetime) + delay,
                        StopTime = DateTime.Parse(s.stopdatetime) + delay
                    };
                }
                public string Artist { get; private set; }
                public string Title { get; private set; }
                public DateTime StartTime { get; private set; }
                public DateTime StopTime { get; private set; }

                public override string ToString()
                {
                    return string.Format("Artist={0}, Title={1}, StartTime={2}, StopTime={3}", Artist, Title, StartTime, StopTime);
                }
            }
        }

        public class JsonResult
        {
            public List<Song> results { get; set; }
        }

        public class Song
        {
            public int id { get; set; }
            public string startdatetime { get; set; }
            public string stopdatetime { get; set; }
            public SongFile songfile { get; set; }
        }
        public class SongFile
        {
            public int id { get; set; }
            public string artist { get; set; }
            public string title { get; set; }
        }
    }
}
