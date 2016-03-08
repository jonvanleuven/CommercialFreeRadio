using System;

namespace CommercialFreeRadio.Impl
{
    public class SonosPlayer : IPlayer
    {
        private readonly UpnpInterface player;

        public SonosPlayer(UpnpInterface player, string name)
        {
            this.player = player;
            Name = name;
        }

        public string Name { get; private set; }
        public void Play(IRadioStation station)
        {
            if (!station.Uri.StartsWith("x-rincon-mp3radio://"))
                throw new Exception("Unable to play station '" + station.Name + "', uri '" + station.Uri + "' is not supported ");
            if (player.GetTransportInfo().CurrentTransportState != "PLAYING")
            {
                Logger.Info("Sonos not playing");
                return;
            }
            player.SetAVTransportURI(station.Uri);
            player.Play();
        }

        public bool? IsPlaying(IRadioStation station)
        {
            //TODO
            //return player.GetPositionInfo().TrackURI == station.Uri;
            Logger.Debug("[IsPlaying] TODO player.GetPositionInfo().TrackURI == station.Uri ====> " + (player.GetPositionInfo().TrackURI == station.Uri));
            return null;
        }
    }
}
