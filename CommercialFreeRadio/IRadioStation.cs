namespace CommercialFreeRadio
{
    public interface IRadioStation
    {
        string Name { get; }
        string Uri { get; }
        bool? IsPlayingCommercialBreak();
        bool? IsMyStream(string uri);
    }
}
