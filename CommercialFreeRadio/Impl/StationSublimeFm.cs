using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace CommercialFreeRadio.Impl
{
    public class StationSublimeFm : IRadioStation
    {
        private readonly PlaylistSublimeFmInterface playlist;
        private string currentTrackId;

        public StationSublimeFm(IXmlInterfaceSublimeFm reader = null)
        {
            playlist = new PlaylistSublimeFmInterface(reader);
        }

        public string Name
        {
            get { return "SublimeFm"; }
        }

        public string Uri
        {
            get { return "x-rincon-mp3radio://82.201.47.68/SublimeFM2"; }
        }

        public bool? IsPlayingCommercialBreak()
        {
            var now = DateTime.Now;
            var track = playlist.ReadTracks().FirstOrDefault(t => t.StartTime < now && t.EndTime > now);
            if (track == null && currentTrackId != null)
            {
                playlist.ClearCache();
                track = playlist.ReadTracks().FirstOrDefault(t => t.StartTime < now && t.EndTime > now);
            }
            if (track != null && track.Id != currentTrackId)
                Logger.Debug("Current track: " + track);
            currentTrackId = track != null ? track.Id : null;
            return track == null
                ? (bool?)null
                : track.Type == PlaylistSublimeFmInterface.TrackType.Commercial ||
                    track.Type == PlaylistSublimeFmInterface.TrackType.News;
        }

        public bool? IsMyStream(string uri)
        {
            if (uri == Uri)
                return true;
            if (uri == "aac://82.201.47.68/SublimeFM")
                return true;
            return false;
        }

        public interface IXmlInterfaceSublimeFm
        {
            string ReadXmlData();
        }

        internal class HttpXmlInterfaceSublimeFm : IXmlInterfaceSublimeFm
        {
            public string ReadXmlData()
            {
                var url = string.Format("http://player.sublimefm.nl/Playlist/nowonair1.xml?_={0}", DateTime.Now.Ticks);
                Logger.Debug(url);
                var req = WebRequest.Create(url);
                var response = req.GetResponse();
                var stream = response.GetResponseStream();
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return new UTF8Encoding().GetString(memoryStream.ToArray());
                }
            }
        }

        public class PlaylistSublimeFmInterface
        {
            private readonly TimeSpanCache cache;
            private readonly IXmlInterfaceSublimeFm datareader;
            private readonly TimeSpan delay = new TimeSpan(0, 0, 8); //audiostream loop 8 seconden achter op interface

            public PlaylistSublimeFmInterface(IXmlInterfaceSublimeFm datareader = null)
            {
                this.datareader = datareader??new HttpXmlInterfaceSublimeFm();
                cache = new TimeSpanCache(new TimeSpan(0, 5, 0));
            }

            public IList<Track> ReadTracks()
            {
                return cache.ReadCached(() => ParseXml(datareader.ReadXmlData()));
            }

            public void ClearCache()
            {
                cache.Clear();
            }

            private IList<Track> ParseXml(string xml)
            {
                try
                {
                    var doc = XDocument.Parse(xml);
                    var previous = doc.XPathSelectElements("/OmniplayerBroadcastXml/Played/Previous").Select(ParseTrack).ToList();
                    var current = ParseTrack(doc.XPathSelectElement("/OmniplayerBroadcastXml/Current"));
                    var next = doc.XPathSelectElements("/OmniplayerBroadcastXml/Upcoming/Next").Select(ParseTrack).ToList();
                    var result = new List<Track>();
                    result.AddRange(previous);
                    result.Add(current);
                    result.AddRange(next);
                    result = result.Where((t, index) => t.Type != TrackType.Jingle ||
                           (index > 0 && result[index - 1].Type == TrackType.Song) || //'Jingle' voor muziek behouden
                           (index < result.Count - 1 && result[index + 1].Type == TrackType.Song) //'Jingle' na muziek behouden
                            ).ToList();
                    for (var i = 0; i < result.Count - 1; i++)
                        result[i].EndTime = result[i + 1].StartTime;
                    return result;
                }
                catch (XmlException e)
                {
                    Logger.Debug("Unable to parse: " + xml);
                    throw e;
                }
            }

            private Track ParseTrack(XElement node)
            {
                return new Track
                {
                    StartTime = DateTime.SpecifyKind(DateTime.Parse(node.XPathSelectElement("startTime.utc.iso").Value),DateTimeKind.Utc).ToLocalTime() + delay,
                    Id = node.XPathSelectElement("itemId").Value,
                    Artist = node.XPathSelectElement("artistName").Value,
                    Title = node.XPathSelectElement("titleName").Value,
                    CategoryName = node.XPathSelectElement("CategoryName").Value,
                    CategoryCode = node.XPathSelectElement("CategoryCode").Value
                };
            }

            public enum TrackType
            {
                Commercial,
                News,
                Song,
                Jingle
            }
        

            public class Track
            {
                public TrackType Type
                {
                    get
                    {
                        switch (CategoryName)
                        {
                            case "Reclame":
                            case "Nieuwe Reclame":
                                return TrackType.Commercial;
                            case "Nieuws/verkeer":
                                return TrackType.News;
                            case "Toth":
                            case "Jingles":
                                return TrackType.Jingle;
                        }
                        switch (CategoryCode)
                        {
                            case "442":
                            case "410":
                            case "225":
                                return TrackType.Commercial;
                        }
                        if (!string.IsNullOrEmpty(Artist) && !string.IsNullOrEmpty(Title))
                            return TrackType.Song;
                        return TrackType.Jingle;
                    }
                }

                public string Id { get; internal set; }
                public string Artist { get; internal set; }
                public string Title { get; internal set; }
                public DateTime StartTime { get; internal set; }
                public DateTime? EndTime { get; internal set; }
                public string CategoryName { get; set; }
                public string CategoryCode { get; set; }

                public override string ToString()
                {
                    return string.Format("Id={7},Artist={0},Title={1},Time={2:HH:mm:ss}-{6:HH:mm:ss},CategoryCode={4},CategoryName={5},Type={3}", Artist, Title, StartTime, Type, CategoryCode, CategoryName, EndTime, Id);
                }
            }
        }
    }
}
