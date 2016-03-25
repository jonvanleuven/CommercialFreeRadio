namespace CommercialFreeRadio.Impl
{
    public class StationBlueMarlin : IRadioStation
    {
        public string Name
        {
            get { return "Blue Marlin"; }
        }
        public string Uri
        {
            get { return "x-rincon-mp3radio://s3.sonicabroadcast.com:8635/"; }
        }
        public bool? IsPlayingCommercialBreak()
        {
            return false;
        }

        public bool? IsMyStream(string uri)
        {
            if (uri == "x-rincon-mp3radio://stream3.ibizasonica.com:8635/")
                return true;
            return uri == Uri;
        }
    }
}
