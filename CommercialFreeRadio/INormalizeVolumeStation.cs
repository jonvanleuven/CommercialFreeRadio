namespace CommercialFreeRadio
{
    public interface INormalizeVolumeStation : IRadioStation
    {
        int NormalizeLevel { get; }
    }
}
