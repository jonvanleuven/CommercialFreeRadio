using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommercialFreeRadio.Impl;

namespace CommercialFreeRadio
{
    public class Program
    {
        private readonly CommandLineArgument args;
        private bool cancel;
        private readonly IRadioStation[] stations;
        private readonly  IRadioStation[] nonstopstations;
        public IPlayer Player { get; private set; }
        private readonly IRadioStation nonstop;

        public static void Main(string[] argString)
        {
            new Program(argString).Run();
        }

        public Program(string[] argString)
        {
            this.args = new CommandLineArgument(argString);
            this.stations = new IRadioStation[]
            {
                new StationSublimeFm(LogTrack, false),
                new Station3fm(),
                new StationArrowCaz(),
                new StationArrowClassicRock(),
                new StationWildFm(),
                new StationSkyRadio(LogTrack),
                new StationStreamWhatYouHear("192.168.178.11", 23138) //TODO variabel maken
                //new StationRadio538()
            };
            this.nonstopstations = new IRadioStation[]
            {
                new StationBlueMarlin(),
                new StationDeepFm(),
                new Station3fmAlternative()
            };
            AllStations = stations.Union(nonstopstations);
            this.nonstop = nonstopstations.SingleOrDefault(s => s.Name.Replace(" ", "").ToLower() == args.NonstopStationName.ToLower()) ?? nonstopstations.First();
            Logger.Info("Nonstop station: '" + nonstop.Name + "'");
            this.Player = CreatePlayer(args, stations);
            if (args.UseRandom)
                Player = new RandomPlayer(Player, stations, new TimeSpan(2, 0, 0));
            Logger.Info("Using player: " + Player.Name);
        }

        public IEnumerable<IRadioStation> AllStations { get; private set; }

        public void Run()
        {
            cancel = false;
            Logger.Init(args.UseVerbose);
            
            if (args.PrintUsage)
            {
                Logger.Debug("print argument usage and do nothing");
                args.ConsoleWriteUsage(stations, nonstopstations);
                return;
            }
            
            var poller = CreatePoller(new TimeSpan(0, 0, 1));
            var state = new State();
            state.ChangeHandler = (fromType, toType) =>
            {
                if (fromType == SoundType.CommercialBreak)
                {
                    Logger.Info("Commercial break ended!", toType);
                    if ((Player.IsPlaying(nonstop) ?? true))
                        Player.Play(state.Current);
                    else
                        Logger.Info("Not switching to '" + state.Current.Name + "', not playing '" + nonstop.Name + "'");
                }
                if (toType == SoundType.CommercialBreak)
                {
                    Logger.Info("Commercial break detected!", toType);
                    if ((Player.IsPlaying(state.Current) ?? true))
                        Player.Play(nonstop);
                    else
                        Logger.Info("Not switching to '" + nonstop.Name + "', not playing '" + state.Current.Name + "'");
                }
            };
            foreach (var now in poller)
            {
                try
                {
                    var current = stations.SingleOrDefault(s => Player.IsPlaying(s) ?? false);
                    if(!(Player.IsPlaying(nonstop) ?? false))
                        state.UpdateStation(current);
                    if (state.Current == null)
                        continue;
//                    if(state.Current.Name != "Wild FM")
//                        stations.Single(s => s.Name == "Wild FM").IsPlayingCommercialBreak(); //TEMP: remember lijst aanvullen

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
                    if (cancel)
                        return;
                    Thread.Sleep(10000);
                }
                if (cancel)
                    return;
            }
        }

        public void Cancel()
        {
            Logger.Info("Cancelling CommercialFreeRadio.....");
            cancel = true;
        } 

        private static void LogTrack(string artist, string title)
        {
            if (string.IsNullOrEmpty(title))
                Logger.Info(string.Format(@"""{0}""", artist));
            else if (string.IsNullOrEmpty(artist) )
                Logger.Info(string.Format(@"""{0}""", title));
            else
                Logger.Info(string.Format(@"""{0} - {1}""", artist, title));
        }

        private static IPlayer CreatePlayer(CommandLineArgument args, IEnumerable<IRadioStation> stations )
        {
            if (args.UseSonosPlayer)
                return new SonosPlayer(new UpnpInterface(args.SonosIp), "Sonos player (" + args.SonosIp + ")");
            var station = stations.SingleOrDefault(s => s.Name.Replace(" ", "").ToLower() == args.StationName.ToLower());
            if (station == null)
                throw new Exception("Invalid radio station name '" + args.StationName + "', valid stations: " + args.ValidRadioNames(stations));
            var player = args.UseVlcPlayer ?  new VlcPlayer() : new EmptyPlayer() as IPlayer;
            if (args.UseRecorder)
                player = new Recorder(player, args.RecordDirectory, stations);
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
                UseRecorder = args.Any(a => a.StartsWith("/recorder"));
                if( UseRecorder )
                    RecordDirectory = ArgumentValue("/recorder", args)??@"d:\temp";
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
            public bool UseRecorder { get; private set; }
            public string RecordDirectory { get; set; }

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
   [/random]                EXPERIMENTAL: Switch to other radio station after commercial break (every 2 hours)
   [/recorder]              EXPERIMENTAL: Record stream as mp3 to d:\temp\",
   ValidRadioNames(stations), ValidRadioNames(nonstopstations)));
            }
        }
    }
}
