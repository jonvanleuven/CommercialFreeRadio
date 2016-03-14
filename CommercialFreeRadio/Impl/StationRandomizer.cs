using System;
using System.Collections.Generic;
using System.Linq;

namespace CommercialFreeRadio.Impl
{
    public class StationRandomizer : IRadioStation
    {
        private readonly State state;

        public StationRandomizer(IList<IRadioStation> stations)
        {
            state = new State(stations);
        }

        public string Name { get { return state.Station.Name; } }
        public string Uri {
            get { return state.Station.Uri; }
        }
        public bool? IsPlayingCommercialBreak()
        {
            return state.Station.IsPlayingCommercialBreak();
        }

        public bool? IsMyStream(string uri)
        {
            return state.Station.IsMyStream(uri);
        }

        public IRadioStation SwitchStation()
        {
            state.SwitchStation();
            return state.Station;
        }

        internal class State
        {
            private readonly IList<IRadioStation> stations;
            private bool? wasCommercialBreak;

            internal State(IList<IRadioStation> stations)
            {
                this.stations = stations;
                Station = SwitchStation();
            }
            internal IRadioStation Station { get; set; }

            internal IRadioStation SwitchStation()
            {
                var r = new Random();
                var nextStations = stations.Where(s => Station == null || s.Name != Station.Name).ToList();
                var result = nextStations.ToList()[r.Next(nextStations.Count())];
                Logger.Info("Random switch to '{0}'", result.Name);
                return result;
            }
            
        }
    }
}
