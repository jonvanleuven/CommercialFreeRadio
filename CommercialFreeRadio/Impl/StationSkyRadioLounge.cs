namespace CommercialFreeRadio.Impl
{
    public class StationSkyRadioLounge : IRadioStation, ITuneinRadioStation
    {
        public string Name
        {
            get { return "SkyRadio Lounge"; }
        }
        public string Uri
        {
            get { return "x-rincon-mp3radio://live.icecast.kpnstreaming.nl/skyradiolive-SRGSTR07.mp3"; }
        }
        public int TuneinId { get { return 203967; } }

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
