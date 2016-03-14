using System;

namespace CommercialFreeRadio.Impl
{
    public class StationWildFm : IRadioStation
    {
        private readonly TimeSpanCache cache = new TimeSpanCache(new TimeSpan(0, 0, 5));
        private string nowPlaying;
        private DateTime sleepUntil = DateTime.Now;

        public string Name
        {
            get { return "Wild FM"; }
        }
        public string Uri
        {
            get { return "x-rincon-mp3radio://149.210.223.94/wildfm.mp3"; }
        }
        public bool? IsPlayingCommercialBreak()
        {
            if (sleepUntil > DateTime.Now)
                return IsCommercialBreal(nowPlaying);
            var result = cache.ReadCached(() =>
            {
                var feed = new TuneInNowPlayingFeed().Read("https://feed.tunein.com/profiles/s77950/nowplaying?itemToken=eyJwIjpmYWxzZSwidCI6IjIwMTYtMDMtMTJUMTg6MzA6MjkuNTExNzIwNloifQ==&partnerId=RadioTime&serial=9276ad87-a2e9-47c6-8e59-f04478dff520");
                return (feed != null && feed.Secondary != null) != null ? feed.Secondary.Title : null;
            });
            if (result != nowPlaying)
            {
                Logger.Debug("Title: {0}", result);
                sleepUntil = DateTime.Now.AddSeconds(30);
            }
            nowPlaying = result;
            return IsCommercialBreal(nowPlaying);
        }

        public bool IsCommercialBreal(string title)
        {
            return title == "Wild Hits";
        }

        public bool? IsMyStream(string uri)
        {
            return Uri == uri || uri == "aac://149.210.223.94/mobile";
        }
    }
}
