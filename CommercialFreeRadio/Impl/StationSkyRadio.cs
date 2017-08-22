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
        private readonly Action<string, string> songChangeHandler;
        private readonly TimeSpanCache cache = new TimeSpanCache(new TimeSpan(0, 0, 5));
        private readonly SkyRadioPlaylistApi api = new SkyRadioPlaylistApi();
        private string currentTrackId;
        public StationSkyRadio(Action<string, string> songChangeHandler)
        {
            this.songChangeHandler = songChangeHandler;
        }

        public string Name { get { return "Sky Radio"; } }
        public string Uri {
            get { return "x-rincon-mp3radio://20103.live.streamtheworld.com/SKYRADIO.mp3"; }
        }
        public int TuneinId { get { return 9067; } }
        public bool? IsPlayingCommercialBreak()
        {
            var playlist = cache.ReadCached(() => api.ReadPlayList());
            var current = playlist.FirstOrDefault(t => t.PlayingNow());
            if (current == null)
                return null;
            if (currentTrackId != current.Id && current.Type == Track.EntryType.Track)
            {
                songChangeHandler(current.Artist, current.Title);
                cache.SetExpireTime(current.End.AddSeconds(-10));
            }   
            currentTrackId = current.Id ?? currentTrackId;
            return current.Type == Track.EntryType.CommercialBreak;
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
                var tracks = list.Where(t => t.type == "track").Select(t => Track.Parse(t, new TimeSpan(0, 1, 12))).OrderBy(t => t.Start).ToList();
                //voeg (commercial)breaks toe:
                var playlist = tracks.Aggregate(new List<Track>(), (current, next) =>
                {
                    var previous = current.LastOrDefault();
                    if (previous != null && previous.End < next.Start)
                    {
                        current.Add(Track.CreateBreak(previous.End, next.Start));
                    }
                    current.Add(next);
                    return current;
                });
                var lastEnd = playlist.LastOrDefault()?.End;
                if (lastEnd?.Minute >= 24 && lastEnd?.Minute <= 30)
                    playlist.Add(Track.CreateBreak(lastEnd.Value, lastEnd.Value.AddMinutes(10)));
                if (lastEnd?.Minute >= 54 || lastEnd?.Minute <= 0)
                    playlist.Add(Track.CreateBreak(lastEnd.Value, lastEnd.Value.AddMinutes(10)));
                return playlist;
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
                if (t.type != "track")
                    throw new Exception("Unexpected track type '" + t.type + "'");
                return new Track
                {
                    Id = t.id,
                    Title = t.title,
                    Artist = t.artist,
                    Start = (DateTime.Parse(t.startTime) + delay),
                    End = (DateTime.Parse(t.startTime).AddMilliseconds(t.duration) + delay),
                    Type = EntryType.Track
                };
            }
            public static Track CreateBreak(DateTime start, DateTime end)
            {
                return new Track
                {
                    Start = start,
                    End = end,
                    Type = (end - start > new TimeSpan(0, 2, 0)) ? EntryType.CommercialBreak : EntryType.Break
                };
            }
            private Track() { }
            public string Id { get; private set; }
            public string Title { get; private set; }
            public string Artist { get; private set; }
            public DateTime Start { get; private set; }
            public DateTime End { get; private set; }
            public EntryType Type { get; private set; }

            public bool PlayingNow()
            {
                return Start < DateTime.Now && End > DateTime.Now;
            }

            public override string ToString()
            {
                return string.Format("Id={0}, Artist={1}, Title={2}, Time={3:HH:mm:ss}-{4:HH:mm:ss}, Type={5}", Id, Artist, Title, Start, End, Type);
            }

            public enum EntryType
            {
                Track,
                Break,
                CommercialBreak
            }
        }
    }
}
