using System;

namespace CommercialFreeRadio.Impl
{
    public class StationArrowClassicRock : IRadioStation
    {
        private readonly TimeSpanCache cache = new TimeSpanCache(new TimeSpan(0, 0, 5));
        private string nowPlaying;
        private DateTime sleepUntil = DateTime.Now;

        public string Name {
            get { return "Arrow Classic Rock"; }
        }
        public string Uri {
            get { return "x-rincon-mp3radio://91.221.151.155/"; }
        }
        public bool? IsPlayingCommercialBreak()
        {
            if (sleepUntil > DateTime.Now)
                return IsCommercialBreal( nowPlaying );
            var result = cache.ReadCached(() => new TuneInNowPlayingFeed().Read("https://feed.tunein.com/profiles/s6702/nowplaying?itemToken=&partnerId=RadioTime&serial=9276ad87-a2e9-47c6-8e59-f04478dff520").Secondary.Title);
            if (result != nowPlaying)
            {
                Logger.Debug("Title: {0}", result);
                sleepUntil = DateTime.Now.AddSeconds(30);
            }   
            nowPlaying = result;
            return IsCommercialBreal( nowPlaying );
        }

        public bool IsCommercialBreal(string title)
        {
            return title == "Roadrunner" || 
                   title == "Rocktemple";
        }

        public bool? IsMyStream(string uri)
        {
            return Uri == uri;
        }
    }
}
