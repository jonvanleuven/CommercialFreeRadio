namespace CommercialFreeRadio.Impl
{
    public class StationArrowClassicRock : IRadioStation
    {
        public string Name {
            get { return "Arrow Classic Rock"; }
        }
        public string Uri {
            get { return "x-rincon-mp3radio://91.221.151.155/"; }
        }
        public bool? IsPlayingCommercialBreak()
        {
            return null;
        }
    }
}
