using System;

namespace CommercialFreeRadio.Impl
{
    public class StationArrowClassicRock : IRadioStation, ITuneinRadioStation
    {
        private readonly TimeSpanCache cache = new TimeSpanCache(new TimeSpan(0, 0, 5));
        private readonly TuneInNowPlayingFeed feed = new TuneInNowPlayingFeed();
        private Track nowPlaying;
        private DateTime? noSongSince;

        public string Name {
            get { return "Arrow Classic Rock"; }
        }
        public string Uri {
            get { return "x-rincon-mp3radio://91.221.151.155/"; }
        }
        public int TuneinId { get { return 6702; } }

        public bool? IsPlayingCommercialBreak()
        {
            //TODO: http://www.arrow.nl/nowplaying/nowplaying.php
            var result = cache.ReadCached(() => feed.Read(TuneinId).Secondary.Title) ?? string.Empty;
            if (nowPlaying == null || nowPlaying.Title != result)
            {
                nowPlaying = new Track(result);
                Logger.Debug("Track: {0}", nowPlaying);
            }
            var isSong = IsSong(nowPlaying);
            if (isSong)
                noSongSince = null;
            if (!isSong && noSongSince == null)
                noSongSince = DateTime.Now;

            return !isSong && DateTime.Now - noSongSince > new TimeSpan(0, 0, 20);
        }

        private static bool IsSong(Track t)
        {
            if (t == null)
                return true;
            return t.Title.Contains(" - ");
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
                Start = DateTime.Now;
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
