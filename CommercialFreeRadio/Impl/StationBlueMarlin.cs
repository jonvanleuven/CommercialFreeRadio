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
            get { return "x-rincon-mp3radio://stream3.ibizasonica.com:8635/"; }
        }
        public bool? IsPlayingCommercialBreak()
        {
            return false;
        }
    }
}
