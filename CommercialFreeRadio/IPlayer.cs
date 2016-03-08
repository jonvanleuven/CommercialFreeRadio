namespace CommercialFreeRadio
{
    public interface IPlayer
    {
        string Name { get; }
        void Play(IRadioStation station);
        bool? IsPlaying(IRadioStation station);
    }
}
