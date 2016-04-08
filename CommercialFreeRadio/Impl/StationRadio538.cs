using System;

namespace CommercialFreeRadio.Impl
{
    public class StationRadio538 : IRadioStation
    {
        private readonly TimeSpanCache cache = new TimeSpanCache(new TimeSpan(0, 0, 5));
        private readonly TuneInNowPlayingFeed feed = new TuneInNowPlayingFeed("https://feed.tunein.com/profiles/s6712/nowplaying?itemToken=&partnerId=RadioTime&serial=9276ad87-a2e9-47c6-8e59-f04478dff520");
        private Track nowPlaying;

        public string Name {
            get { return "Radio 538"; }
        }
        public string Uri {
            get { return "x-rincon-mp3radio://vip-icecast.538.lw.triple-it.nl/RADIO538_MP3"; }
        }
        public bool? IsPlayingCommercialBreak()
        {
            var result = cache.ReadCached(() => feed.Read().Secondary.Title) ?? string.Empty;
            if (nowPlaying == null || nowPlaying.Title != result)
            {
                nowPlaying = new Track(result);
                Logger.Debug("Track: {0}", nowPlaying);
            }
            return IsCommercialBreak( nowPlaying );
        }

        private static bool IsCommercialBreak(Track t)
        {
            if (t == null)
                return false;
            return !t.Title.Contains(" - ") && DateTime.Now - t.Start > new TimeSpan(0, 0, 15);
        }

        public bool? IsMyStream(string uri)
        {
            return Uri == uri;
        }

        class Track
        {
            internal Track(string title)
            {
                Title = title;
                Start = DateTime.Now.AddSeconds(10);
            }
            public string Title { get; private set; }
            public DateTime Start { get; private set; }

            public override string ToString()
            {
                return string.Format("Title={0}, Start={1:HH:mm:ss}", Title, Start);
            }
        }
    }
}
