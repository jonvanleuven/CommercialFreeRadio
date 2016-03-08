namespace CommercialFreeRadio.Impl
{
    public class StationArrowCaz : IRadioStation
    {
        public string Name {
            get { return "Arrow Caz"; }
        }
        public string Uri {
            get { return "mms://81.23.249.10/caz_audio_01"; }
        }
        public bool? IsPlayingCommercialBreak()
        {
            return null;
        }
    }
}
