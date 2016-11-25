using System.Collections.Generic;
using System.Linq;

namespace CommercialFreeRadio.Impl
{
    /*public class AllSonosPlayers : IPlayer
    {
        private readonly IEnumerable<SonosPlayer> players;

        public AllSonosPlayers(string entryPoint)
        {
            this.players = new UpnpInterface(entryPoint).GetZoneGroupState().ZoneGroups.SelectMany(zg => zg.ZoneGroupMembers)
                .Distinct()
                .Select(m => new SonosPlayer(new UpnpInterface(m.Ip), m.Name));
        }

        public string Name
        {
            get { return "Sonos - " + string.Join(", ", this.players.Select(p => p.Name)); }
        }

        public void Play(IRadioStation station)
        {
            foreach (var p in players.Where(p => p.IsSlave()).Where(p => p.IsPlaying()))
            {
                p.Play(station);
            }
        }

        public bool? IsPlaying(IRadioStation station)
        {
            var isplaying = players.Where(p => p.IsSlave()).Select(p => p.IsPlaying(station)).ToList();
            if (isplaying.All(x => x == null))
                return null;
            return isplaying.Any(x => x ?? false);
        }
    }*/
}
