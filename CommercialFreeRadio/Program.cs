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
            if (args.PrintUsage)
            {
                args.ConsoleWriteUsage();
                return;
            }
            Logger.IsDebugEnabled = args.UseVerbose;
            //IRadioStation station = new StationSublimeFm();
            IRadioStation station = new StationArrowCaz();
            //var station = new Station3fm();
            var nonstop = new StationBlueMarlin();
            var player = CreatePlayer(args);
            Logger.Info("Using player: " + player.Name);
            var poller = CreatePoller(new TimeSpan(0, 0, 1));
//            if ((player.IsPlaying(station) ?? true))
//                player.Play(station);
            var state = new State((fromType, toType) =>
            {
                Logger.Info("--- {0}", toType);
                if (fromType == SoundType.CommercialBreak)
                {
                    if( (player.IsPlaying(nonstop) ?? true) )
                        player.Play(station);
                    else
                        Logger.Info("Not switching to '" + station.Name + "', not playing '" + nonstop.Name + "'");
                }
                if (toType == SoundType.CommercialBreak)
                {
                    if ((player.IsPlaying(station) ?? true))
                        player.Play(nonstop);
                    else
                        Logger.Info("Not switching to '" + nonstop.Name + "', not playing '" + station.Name + "'");
                }
            });
            var stations = new IRadioStation[] {new StationSublimeFm(), new Station3fm(), new StationArrowCaz()};
            foreach (var now in poller)
            {
                try
                {
                    var nowPlaying = stations.SingleOrDefault(s => player.IsPlaying(s)??false);
                    if (nowPlaying != null)
                    {
                        if( station.Name != nowPlaying.Name )
                            Logger.Info("Switching to channel '{0}'", nowPlaying.Name);
                        station = nowPlaying;
                    }   
                    var commercialPlaying = station.IsPlayingCommercialBreak();
                    if (commercialPlaying == null)
                        Logger.Info("Kan niet bepalen of er een commercial wordt afgespeeld, IsPlayingCommercialBreak van station '{0}' returned null", station.Name);
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

        private static IPlayer CreatePlayer(CommandLineArgument args)
        {
            if (args.UseSonosPlayer)
                return new SonosPlayer(new UpnpInterface(args.SonosIp), "Sonos player (" + args.SonosIp + ")");
            if (args.UseVlcPlayer)
                return new VlcPlayer();
            return new EmptyPlayer();
        }

        public class State
        {
            private readonly Action<SoundType, SoundType> changeHandler;
            public State(Action<SoundType, SoundType> changeHandler)
            {
                Sound = SoundType.Music;
                this.changeHandler = changeHandler;
            }

            public SoundType Sound { get; private set; }

            public void UpdateSoundType(SoundType type)
            {
                if (type != Sound)
                    changeHandler(Sound, type);
                Sound = type;
            }
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
                UseVlcPlayer = args.Contains("/vlc");
                UseVerbose = args.Contains("/verbose");
                if (UseSonosPlayer)
                    SonosIp = args.Where(a => a.StartsWith("/sonos") && a.Split('=').Length>1).Select(a => a.Split('=')[1]).FirstOrDefault() ?? "192.168.1.16";
            }

            public bool PrintUsage { get; private set; }
            public bool UseSonosPlayer { get; private set; }
            public string SonosIp { get; private set; }
            public bool UseVlcPlayer { get; private set; }
            public bool UseVerbose { get; private set; }

            internal void ConsoleWriteUsage()
            {
                Console.WriteLine(@"Usage:
  Player options:
   /sonos[=<ip-address>]   Use sonos player. Default ip address=192.168.1.16
   /vlc                    Use VLC player 
  Other options:
   [/verbose]              Print verbose");
            }
        }
    }
}
