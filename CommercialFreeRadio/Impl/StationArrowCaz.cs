using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;

namespace CommercialFreeRadio.Impl
{
    public class StationArrowCaz : IRadioStation
    {
        private readonly PlaylistArrowCazInterface playlist;

        public StationArrowCaz()
        {
            this.playlist = new PlaylistArrowCazInterface();
        }

        public string Name {
            get { return "Arrow Caz"; }
        }
        public string Uri {
            get { return "x-rincon-mp3radio://stream.arrowcaz.nl/caz128kmp3"; }
        }
        public bool? IsPlayingCommercialBreak()
        {
            var current = playlist.ReadCurrentTrack();
            if (current == null)
                return null;
            return current.Title != null && current.Title.ToUpper().StartsWith("TME COMMERCIAL");
        }

        public bool? IsMyStream(string uri)
        {
            if (uri == Uri)
                return true;
            if (uri.EndsWith("mms://81.23.249.10/caz_audio_01"))
                return true;
            return false;
        }

        public interface IJsonInterfaceArrowCaz
        {
            byte[] ReadJsonData();
        }

        internal class HttpJsonInterfaceArrowCaz : IJsonInterfaceArrowCaz 
        {
            public byte[] ReadJsonData()
            {
                var url = string.Format("http://www.arrowcaz.nl/newplayer/backend/get_xml/http:**www.arrowcaz.nl*xmlinserter*onair.xml!rnd={0}", DateTime.Now.Ticks);
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

        public class PlaylistArrowCazInterface
        {
            private readonly IJsonInterfaceArrowCaz datareader;
            private readonly TimeSpan delay = new TimeSpan(0, 0, 5); //audiostream loop 5 seconden achter op json interface
            private Track lastTrack;

            public PlaylistArrowCazInterface(IJsonInterfaceArrowCaz datareader = null)
            {
                this.datareader = datareader??new HttpJsonInterfaceArrowCaz();
            }

            public Track ReadCurrentTrack()
            {
                if (lastTrack != null && lastTrack.StopTime > DateTime.Now.AddSeconds(5))
                    return lastTrack;
                var jsonBytes = datareader.ReadJsonData();
                var ser = new DataContractJsonSerializer(typeof(JsonResult));
                lastTrack = Track.Parse(((JsonResult)ser.ReadObject(new MemoryStream(jsonBytes))).BroadcastMonitor, delay);
                Logger.Debug(lastTrack);
                return lastTrack;
            }

            public class Track
            {
                internal static Track Parse(BroadcastMonitor s, TimeSpan delay)
                {
                    if (s == null)
                        return null;
                    return new Track
                    {
                        Artist = s.Current.artistName.value,
                        Title = s.Current.titleName.value,
                        StartTime = DateTime.Parse(s.Current.startTime.value) + delay,
                        StopTime = DateTime.Parse(s.Next.startTime.value) + delay
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


            public class JsonResult
            {
                public BroadcastMonitor BroadcastMonitor { get; set; }
            }
            public class Value
            {
                public string value { get; set; }
            }

            public class JsonTrack
            {
                public Value startTime { get; set; }
                public Value itemId { get; set; }
                public Value titleId { get; set; }
                public Value itemCode { get; set; }
                public List<object> itemReference { get; set; }
                public Value titleName { get; set; }
                public Value artistName { get; set; }
                public Value albumName { get; set; }
            }

            public class BroadcastMonitor
            {
                public Value updated { get; set; }
                public Value stationName { get; set; }
                public JsonTrack Current { get; set; }
                public JsonTrack Next { get; set; }
            }
        }
    }
}
