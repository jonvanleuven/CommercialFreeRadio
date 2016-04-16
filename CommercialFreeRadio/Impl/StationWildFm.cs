using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CommercialFreeRadio.Impl
{
    public class StationWildFm : IRadioStation, ITuneinRadioStation
    {
        private readonly TimeSpanCache cache = new TimeSpanCache(new TimeSpan(0, 0, 5));
        private readonly TuneInNowPlayingFeed feed = new TuneInNowPlayingFeed("https://feed.tunein.com/profiles/s77950/nowplaying?itemToken=eyJwIjpmYWxzZSwidCI6IjIwMTYtMDMtMTJUMTg6MzA6MjkuNTExNzIwNloifQ==&partnerId=RadioTime&serial=9276ad87-a2e9-47c6-8e59-f04478dff520");
        private readonly RememberTrackList tracks = new RememberTrackList(@"wildFm_temp.txt");
        private Track nowPlaying;

        public string Name
        {
            get { return "Wild FM"; }
        }
        public string Uri
        {
            get { return "x-rincon-mp3radio://149.210.223.94/wildfm.mp3"; }
        }
        public int TuneinId { get { return 77950; } }
        public string TuneinTitle { get { return "Wild FM"; } }

        public bool? IsPlayingCommercialBreak()
        {
            var result = cache.ReadCached(() =>
            {
                var feedResult = feed.Read();
                return (feedResult != null && feedResult.Secondary != null) ? feedResult.Secondary.Title : null;
            });

            if (nowPlaying == null || result != nowPlaying.Title)
            {
                if (nowPlaying != null)
                    nowPlaying.End = DateTime.Now;
                if (nowPlaying != null && nowPlaying.Start.HasValue && !IsCommercial(nowPlaying.Title))
                    tracks.Add(nowPlaying.Title, DateTime.Now-nowPlaying.Start.Value);
                var found = tracks.Find(result);
                nowPlaying = new Track
                {
                    Title = result,
                    Start = nowPlaying == null ? (DateTime?) null : DateTime.Now,
                    End = nowPlaying != null && found.HasValue ? DateTime.Now+found : null
                };
                Logger.Debug("Title: {0}", nowPlaying);
            }
            return IsCommercialBreal(nowPlaying);
        }

        private bool IsCommercialBreal(Track track)
        {
            if (track == null)
                return false;
            if (IsCommercial(track.Title))
                return true;

            if (DateTime.Now > track.End)
            {
                if ((track.End.Value.Minute >= 55 && track.End.Value.Minute < 60)
                    ||
                    (track.End.Value.Minute >= 27 && track.End.Value.Minute < 33))
                {
                    Logger.Debug("Commercial break detected based on track end time!");
                    return true;
                }
                if (DateTime.Now - track.End > new TimeSpan(0, 0, 30))
                {
                    Logger.Debug("Commercial break detected based on track end time!");
                    return true;
                }
            }
            return false;
        }

        private bool IsCommercial(string title)
        {
            return title != null && title.StartsWith("Wild ");
        }

        public bool? IsMyStream(string uri)
        {
            return Uri == uri || uri == "aac://149.210.223.94/mobile";
        }

        class Track
        {
            public string Title { get; set; }
            public DateTime? Start { get; set; }
            public DateTime? End { get; set; }
            public override string ToString()
            {
                return string.Format("Title={0}, Start={1:HH:mm:ss}, End={2:HH:mm:ss}", Title, Start, End);
            }

        }

        public class RememberTrackList
        {
            private readonly IDictionary<string, List<TimeSpan>> items;
            private readonly string filename;

            public RememberTrackList(string filename)
            {
                this.filename = filename;
                Logger.Debug("Loading '{0}'", filename);
                var content = File.ReadAllText(filename);
                items = content.Split('\n')
                    .Where(line => !string.IsNullOrEmpty(line))
                    .Select(line => line.Split('\t'))
                    .GroupBy(e => e[0], e => ToTimeSpan(e[1]))
                    .ToDictionary(e => e.Key, e => e.ToList());
                Logger.Debug("Done loading '{0}'", filename);
            }

            private static TimeSpan ToTimeSpan(string str)
            {
                var s = str.Split(':').Select(t => int.Parse(t.Substring(0, 2))).ToList();
                return new TimeSpan(s[0], s[1], s[2]);
            }

            public void Add(string title, TimeSpan timespan)
            {
                File.AppendAllText(filename, string.Format("{0}\t{1}\n", title, timespan));
                if( items.ContainsKey(title) )
                    items[title].Add(timespan);
                else 
                    items[title] = new List<TimeSpan> {timespan};
            }


            public TimeSpan? Find(string title)
            {
                if (!items.ContainsKey(title))
                    return null;
                var result = items[title].Where(t => t < new TimeSpan(0,5,0)).ToList();
                //add gewichten: tracktimes tuseen 2:00 en 3:59 minuten tellen zwaarder:
                result.AddRange(result.Where(t => t.Minutes == 3).SelectMany(t => Enumerable.Range(0, 10).Select(i => t)).ToList());
                result.AddRange(result.Where(t => t.Minutes == 2).SelectMany(t => Enumerable.Range(0, 5).Select(i => t)).ToList());
                return result.Count() > 1 ? new TimeSpan((long)result.Select(t => t.Ticks).Average()) : (TimeSpan?)null;
            }
        }
    }
}
