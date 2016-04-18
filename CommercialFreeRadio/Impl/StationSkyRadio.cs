using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;

namespace CommercialFreeRadio.Impl
{
    public class StationSkyRadio : IRadioStation, ITuneinRadioStation
    {
        private readonly TimeSpanCache cache = new TimeSpanCache(new TimeSpan(0, 0, 5));
        private readonly SkyRadioPlaylistApi api = new SkyRadioPlaylistApi();
        private Track current = null;
        private DateTime sleepUntil = DateTime.Now;

        public string Name { get { return "Sky Radio"; } }
        public string Uri {
            get { return "x-rincon-mp3radio://8603.live.streamtheworld.com/SKYRADIO.mp3"; }
        }
        public int TuneinId { get { return 9067; } }
        public bool? IsPlayingCommercialBreak()
        {
            if (sleepUntil > DateTime.Now)
                return current == null;

            var playlist = cache.ReadCached(() => api.ReadPlayList());

            var currentTrack = playlist.FirstOrDefault(track => track.Start < DateTime.Now && track.End > DateTime.Now);
            if( currentTrack == null )
                currentTrack = playlist.FirstOrDefault(track => track.Start.AddSeconds(-10) < DateTime.Now && track.End.AddSeconds(10) > DateTime.Now);

            if (GetId(currentTrack) != GetId(current))
            {
                Logger.Debug("Track: " + currentTrack);
                if (currentTrack != null)
                    sleepUntil = currentTrack.End.AddSeconds(-10);
            }
            current = currentTrack;
            return current == null;
        }

        private static string GetId(Track t)
        {
            return t != null ? t.Id : null;
        }

        public bool? IsMyStream(string uri)
        {
            if (uri.ToLower().Contains("skyradio"))
                return true;
            return uri == Uri;
        }

        private class SkyRadioPlaylistApi
        {
            public IList<Track> ReadPlayList()
            {
                var data = ReadJsonData("http://www.skyradio.nl/cdn/player_skyradio.json");
                var ser = new DataContractJsonSerializer(typeof(RootObject));
                var root = ((RootObject)ser.ReadObject(new MemoryStream(data)));
                var list = new List<JsonTrack>();
                list.AddRange(root.previous);
                list.Add(root.current);
                //list.AddRange(root.next); //heeft geen geldige duration
                return list.Where(t => t.type == "track").Select(t => Track.Parse(t, new TimeSpan(0, 1, 20))).OrderBy(t => t.Start).ToList();
            }

            private byte[] ReadJsonData(string url)
            {
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

        public class Previou : JsonTrack
        {
        }

        public class Current : JsonTrack
        {
            public string twitter { get; set; }
            public string facebook { get; set; }
            public string share { get; set; }
        }

        public class Next : JsonTrack
        {
        }

        public class JsonTrack
        {
            public string type { get; set; }
            public string id { get; set; }
            public string startTime { get; set; }
            public int duration { get; set; }
            public string title { get; set; }
            public string artist { get; set; }
            public string image { get; set; }
            public string image2 { get; set; }
        }

        public class RootObject
        {
            public string mount { get; set; }
            public List<Previou> previous { get; set; }
            public Current current { get; set; }
            public List<Next> next { get; set; }
            public string processtime { get; set; }
        }

            
        public class Track
        {
            public static Track Parse(JsonTrack t, TimeSpan delay)
            {
                return new Track
                {
                    Id = t.id,
                    Title = t.title,
                    Artist = t.artist,
                    Start = (DateTime.Parse(t.startTime) + delay),
                    End = (DateTime.Parse(t.startTime).AddMilliseconds(t.duration) + delay)
                };
            }
            private Track() { }
            public string Id { get; private set; }
            public string Title { get; private set; }
            public string Artist { get; private set; }
            public DateTime Start { get; private set; }
            public DateTime End { get; private set; }

            public override string ToString()
            {
                return string.Format("Id={0}, Artist={1}, Title={2}, Time={3:HH:mm:ss}-{4:HH:mm:ss}", Id, Artist, Title, Start, End);
            }
        }
    }
}
