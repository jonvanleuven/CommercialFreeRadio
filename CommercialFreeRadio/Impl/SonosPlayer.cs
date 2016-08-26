using System;

namespace CommercialFreeRadio.Impl
{
    public class SonosPlayer : IPlayer
    {
        private readonly UpnpInterface player;
        private readonly TimeSpanCache getUriCache = new TimeSpanCache(new TimeSpan(0, 1, 0));
        private readonly TimeSpanCache isPlayingCache = new TimeSpanCache(new TimeSpan(0, 0, 5));
        private readonly TimeSpanCache tuneinTitleCache = new TimeSpanCache(new TimeSpan(1, 0, 0, 0));
        private DateTime? stoppedPlayingAt = DateTime.MinValue;
        private readonly VolumeNormalizer normalizer;

        public SonosPlayer(UpnpInterface player, string name)
        {
            this.player = player;
            Name = name;
            normalizer = new VolumeNormalizer( player.SetVolume, player.GetVolume );
        }

        public string Name { get; private set; }
        public void Play(IRadioStation station)
        {
            if (!station.Uri.StartsWith("x-rincon-mp3radio://") && !station.Uri.StartsWith("mms://") && !station.Uri.StartsWith("x-sonosapi-stream:"))
                throw new Exception("Unable to play station '" + station.Name + "', uri '" + station.Uri + "' is not supported ");
            var tuneinStream = station as ITuneinRadioStation;
            if (tuneinStream != null)
                SetTuninStream(tuneinStream.TuneinId);
            else
                player.SetAVTransportURI(station.Uri);

            player.Play();
            stoppedPlayingAt = null;
            getUriCache.Clear();
            isPlayingCache.Clear();
            normalizer.Normalize( (station is INormalizeVolumeStation)? ((INormalizeVolumeStation)station).NormalizeLevel : 0);


        }

        public bool? IsPlaying(IRadioStation station)
        {
            stoppedPlayingAt = isPlayingCache.ReadCached(() =>
            {
                var state = player.GetTransportInfo().CurrentTransportState;
                switch (state)
                {
                    case "PLAYING":
                        return (DateTime?)null;
                    case "STOPPED":
                        return stoppedPlayingAt ?? DateTime.Now;
                    case "TRANSITIONING":
                    default:
                        return stoppedPlayingAt;
                }
            });
            if (DateTime.Now - stoppedPlayingAt > new TimeSpan(0, 0, 30)) //is gestopt als halve minuut gestopt, om netwerk interrupties op te vangen
                return false;
            var uriPlaying = getUriCache.ReadCached(() => player.GetPositionInfo().TrackURI);
            if (uriPlaying == null)
                return false;
            return station.IsMyStream(uriPlaying);
        }
        public void SetTuninStream(int id)
        {
            var title = tuneinTitleCache.ReadCached(id.ToString(), () => new TuneInNowPlayingFeed().Read(id).Header.Title);
            player.SetAVTransportURI("x-sonosapi-stream:s" + id + "?sid=254&flags=8224&sn=0", @"<DIDL-Lite xmlns:dc=""http://purl.org/dc/elements/1.1/"" xmlns:upnp=""urn:schemas-upnp-org:metadata-1-0/upnp/"" xmlns:r=""urn:schemas-rinconnetworks-com:metadata-1-0/"" xmlns=""urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/""><item id=""-1"" parentID=""-1"" restricted=""true""><dc:title>" + title + @"</dc:title><upnp:class>object.item.audioItem.audioBroadcast</upnp:class><desc id=""cdudn"" nameSpace=""urn:schemas-rinconnetworks-com:metadata-1-0/"">SA_RINCON65031_</desc></item></DIDL-Lite>"); //65031 = tunein service id
        }
    }
}
