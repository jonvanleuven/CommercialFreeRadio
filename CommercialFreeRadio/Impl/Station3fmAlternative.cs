namespace CommercialFreeRadio.Impl
{
    public class Station3fmAlternative : IRadioStation
    {
        public string Name
        {
            get { return "3fm Alternative"; }
        }
        public string Uri
        {
            get { return "x-rincon-mp3radio://icecast.omroep.nl/3fm-alternative-mp3"; }
        }
        public bool? IsPlayingCommercialBreak()
        {
            return false;
        }

        public bool? IsMyStream(string uri)
        {
//            if (uri == "x-rincon-mp3radio://stream3.ibizasonica.com:8635/")
//                return true;
            return uri == Uri;
        }
    }
}
