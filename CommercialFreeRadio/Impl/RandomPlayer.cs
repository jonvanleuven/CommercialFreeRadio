using System;
using System.Collections.Generic;
using System.Linq;

namespace CommercialFreeRadio.Impl
{
    public class RandomPlayer : IPlayer
    {
        private readonly IPlayer player;
        private readonly IList<IRadioStation> stations;
        private readonly TimeSpan switchAfter;
        private IRadioStation lastRandomStation;
        private DateTime switchTime;

        public RandomPlayer(IPlayer player, IList<IRadioStation> stations, TimeSpan switchAfter)
        {
            this.player = player;
            this.stations = stations;
            this.switchAfter = switchAfter;
            this.switchTime = DateTime.Now + switchAfter;
        }

        public string Name {
            get { return "Random player for: " + player.Name; }
        }
        public void Play(IRadioStation station)
        {
            if (!stations.Select(s => s.Name).Contains(station.Name) || switchTime > DateTime.Now)
            {
                Logger.Info("Switching random station after {0:dd-MM-yyyy HH:mm:ss}", switchTime);
                player.Play(station);
                return;
            }
            lastRandomStation = RandomStation();
            player.Play(lastRandomStation);
            switchTime = DateTime.Now + switchAfter;
        }

        private IRadioStation RandomStation()
        {
            var r = new Random();
            var nextStations = stations.Where(s => lastRandomStation == null || s.Name != lastRandomStation.Name).ToList();
            var result = nextStations.ToList()[r.Next(nextStations.Count())];
            Logger.Info("Random switch to '{0}'", result.Name);
            return result;
        }

        public bool? IsPlaying(IRadioStation station)
        {
            return player.IsPlaying(station);
        }
    }
}
