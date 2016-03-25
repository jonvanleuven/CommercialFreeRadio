namespace CommercialFreeRadio.Impl
{
    public class EmptyPlayer : IPlayer
    {
        private IRadioStation currentStation;

        public string Name { get { return "EmptyPlayer"; } }

        public void Play(IRadioStation station)
        {
            currentStation = station;
            Logger.Debug("[EmptyPlayer] is 'playing' station " + station.Name);
        }
        public bool? IsPlaying(IRadioStation station)
        {
            return currentStation != null && currentStation.Uri == station.Uri;
        }
    }
}
