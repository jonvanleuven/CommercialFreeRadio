using System;

namespace CommercialFreeRadio.Impl
{
    public class StationDeepFm : IRadioStation
    {
        public string Name {
            get { return "Deep FM"; }
        }
        public string Uri {
            get { return "x-rincon-mp3radio://sc.deep.fm/sd"; }
        }
        public bool? IsPlayingCommercialBreak()
        {
            return false;
        }

        public bool? IsMyStream(string uri)
        {
            return uri == Uri;
        }
    }
}
