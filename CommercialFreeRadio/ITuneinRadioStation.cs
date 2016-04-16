namespace CommercialFreeRadio
{
    public interface ITuneinRadioStation : IRadioStation
    {
        int TuneinId { get; }
        string TuneinTitle { get; }
    }
}
