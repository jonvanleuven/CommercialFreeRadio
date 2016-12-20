using System;

namespace CommercialFreeRadio.Impl
{
    public class StationSmoothJazzFloridaPlus : IRadioStation, ITuneinRadioStation
    {
        public string Name {
            get { return "Smooth Jazz Florida Plus (+)";  }
        }
        public string Uri { get { return "x-rincon-mp3radio://streaming.shoutcast.com/sjfplus"; } }
        public bool? IsPlayingCommercialBreak()
        {
            return false;
        }

        public bool? IsMyStream(string uri)
        {
            return uri != null && uri.Contains("sjfplus");
        }

        public int TuneinId { get { return 221605; } }
    }
}
