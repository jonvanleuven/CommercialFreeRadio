namespace CommercialFreeRadio.Impl
{
    public class StationStreamWhatYouHear : IRadioStation
    {
        private readonly string ip;
        private readonly int port;

        public StationStreamWhatYouHear(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }
        public string Name {
            get { return "StreamWhatYouHear (" + ip + ":" + port + ")"; }
        }
        public string Uri {
            get { return "x-rincon-mp3radio://" + ip + ":" + port + "/stream/swyh.mp3"; }
        }
        public bool? IsPlayingCommercialBreak()
        {
            return false;
        }

        public bool? IsMyStream(string uri)
        {
            return uri != null && uri.Contains(ip + ":" + port);
        }
    }
}
