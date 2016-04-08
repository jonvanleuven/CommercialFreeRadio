using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommercialFreeRadio.Impl;

namespace CommercialFreeRadio
{
    public class Program
    {
        static void Main(string[] argString)
        {
            var args = new CommandLineArgument(argString);
            Logger.Init(args.UseVerbose);
            var stations = new IRadioStation[]
            {
                new StationSublimeFm(),
                new Station3fm(),
                new StationArrowCaz(),
                new StationArrowClassicRock(),
                new StationWildFm(),
                new StationSkyRadio(),
                //new StationRadio538()
            };
            var nonstopstations = new IRadioStation[]
            {
                new StationBlueMarlin(),
                new StationDeepFm(),
                new Station3fmAlternative()
            };
            if (args.PrintUsage)
            {
                args.ConsoleWriteUsage(stations, nonstopstations);
                return;
            }

            var nonstop = nonstopstations.SingleOrDefault(s => s.Name.Replace(" ", "").ToLower() == args.NonstopStationName.ToLower()) ?? nonstopstations.First();
            Logger.Info("Nonstop station: '" + nonstop.Name + "'");
            var player = CreatePlayer(args, stations);
            if(args.UseRandom)
                player = new RandomPlayer(player, stations, new TimeSpan(2, 0, 0));
            Logger.Info("Using player: " + player.Name);
            var poller = CreatePoller(new TimeSpan(0, 0, 1));
            var state = new State();
            state.ChangeHandler = (fromType, toType) =>
            {
                Logger.Info("--- {0}", toType);
                if (fromType == SoundType.CommercialBreak)
                {
                    if ((player.IsPlaying(nonstop) ?? true))
                        player.Play(state.Current);
                    else
                        Logger.Info("Not switching to '" + state.Current.Name + "', not playing '" + nonstop.Name + "'");
                }
                if (toType == SoundType.CommercialBreak)
                {
                    if ((player.IsPlaying(state.Current) ?? true))
                        player.Play(nonstop);
                    else
                        Logger.Info("Not switching to '" + nonstop.Name + "', not playing '" + state.Current.Name + "'");
                }
            };
            foreach (var now in poller)
            {
                try
                {
                    var current = stations.SingleOrDefault(s => player.IsPlaying(s) ?? false);
                    if(!(player.IsPlaying(nonstop) ?? false))
                        state.UpdateStation(current);
                    if (state.Current == null)
                        continue;
                    var commercialPlaying = state.Current.IsPlayingCommercialBreak();
                    if (commercialPlaying == null)
                        Logger.Info("Kan niet bepalen of er een commercial wordt afgespeeld, IsPlayingCommercialBreak van station '{0}' returned null", state.Current.Name);
                    else if (commercialPlaying ?? false)
                        state.UpdateSoundType(SoundType.CommercialBreak);
                    else
                        state.UpdateSoundType(SoundType.Music);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    Thread.Sleep(10000);
                }
            }
        }

        private static IPlayer CreatePlayer(CommandLineArgument args, IEnumerable<IRadioStation> stations )
        {
            if (args.UseSonosPlayer)
                return new SonosPlayer(new UpnpInterface(args.SonosIp), "Sonos player (" + args.SonosIp + ")");
            var station = stations.SingleOrDefault(s => s.Name.Replace(" ", "").ToLower() == args.StationName.ToLower());
            if (station == null)
                throw new Exception("Invalid radio station name '" + args.StationName + "', valid stations: " + args.ValidRadioNames(stations));
            var player = args.UseVlcPlayer ?  new VlcPlayer() : new EmptyPlayer() as IPlayer;
            player.Play(station);
            return player;
        }

        public class State
        {
            public State()
            {
                Sound = SoundType.Music;
            }

            public Action<SoundType, SoundType> ChangeHandler { get; set; }

            private SoundType Sound { get; set; }

            public void UpdateSoundType(SoundType type)
            {
                if (type != Sound)
                    ChangeHandler(Sound, type);
                Sound = type;
            }

            public void UpdateStation(IRadioStation station)
            {
                if (station == null && Current != null)
                    Logger.Info("Player stopped playing '" + Current.Name + "'");
                if (station != null && Current == null)
                    Logger.Info("Player started playing '" + station.Name + "'");
                Current = station;
            }

            public IRadioStation Current { get; private set; }
        }

        public enum SoundType
        {
            Music,
            CommercialBreak
        }

        private static IEnumerable<DateTime> CreatePoller(TimeSpan interval)
        {
            while (true)
            {
                Thread.Sleep((int)interval.TotalMilliseconds);
                yield return DateTime.Now;
            }
        }

        public class CommandLineArgument
        {
            public CommandLineArgument(string[] args)
            {
                PrintUsage = args == null || args.Length == 0 || (args.Length == 1 && args[0].Contains("?"));
                if (PrintUsage) return;
                if (args == null) return;
                UseSonosPlayer = args.Any(a => a.StartsWith("/sonos"));
                UseVlcPlayer = args.Any(a => a.StartsWith("/vlc"));
                UseVerbose = args.Contains("/verbose");
                UseRandom = args.Contains("/random");
                if (UseSonosPlayer)
                    SonosIp = ArgumentValue("/sonos", args);
                StationName = ArgumentValue("/vlc", args) ?? ArgumentValue("/nop", args) ?? string.Empty;
                NonstopStationName = ArgumentValue("/nonstop", args) ?? string.Empty;
            }

            private string ArgumentValue(string arg, string[] args)
            {
                return args.Where(a => a.StartsWith(arg) && a.Split('=').Length > 1).Select(a => a.Split('=')[1]).FirstOrDefault();
            }

            public bool PrintUsage { get; private set; }
            public bool UseSonosPlayer { get; private set; }
            public string SonosIp { get; private set; }
            public bool UseVlcPlayer { get; private set; }
            public string StationName { get; private set; }
            public string NonstopStationName { get; private set; }
            public bool UseVerbose { get; private set; }
            public bool UseRandom { get; private set; }

            public string ValidRadioNames(IEnumerable<IRadioStation> stations)
            {
                return string.Join(", ", stations.Select(s => s.Name.Replace(" ", "")));
            }

            internal void ConsoleWriteUsage(IEnumerable<IRadioStation> stations, IEnumerable<IRadioStation> nonstopstations)
            {
                Console.WriteLine(string.Format(@"Usage:
  Player options:
   /sonos=<ip-address>      Use sonos player
   /vlc=<stationname>       Use VLC player (stationnames: {0})
   /nop=<stationname>       Use NOP player (stationnames: {0})
  Other options:
   [/nonstop=<stationname>] Use this station during commercial breaks (stationnames: {1})
   [/verbose]               Print verbose
   [/random]                EXPERIMENTAL: Switch to other radio station after commercial break (every 2 hours)",
   ValidRadioNames(stations), ValidRadioNames(nonstopstations)));
            }
        }
    }
}
