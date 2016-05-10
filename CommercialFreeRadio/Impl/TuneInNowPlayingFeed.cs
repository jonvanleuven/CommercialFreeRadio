using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;

namespace CommercialFreeRadio.Impl
{
    public class TuneInNowPlayingFeed
    {
        public RootObject Read(int id)
        {
            var serial = "9276ad87-a2e9-47c6-8e59-f04478dff520"; //Guid.NewGuid().ToString(); 
            var url = string.Format("https://feed.tunein.com/profiles/s{0}/nowplaying?itemToken=&partnerId=RadioTime&serial={1}", id, serial);
            var ser = new DataContractJsonSerializer(typeof(RootObject));
            return ((RootObject)ser.ReadObject(new MemoryStream(ReadJsonData(url))));
        }
        private static byte[] ReadJsonData(string url)
        {
            Logger.Debug(url);
            var req = WebRequest.Create(url);
            var response = req.GetResponse();
            var stream = response.GetResponseStream();
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }

    public class Header
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
    }

    public class Primary
    {
        public string GuideId { get; set; }
        public string Image { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
    }

    public class Secondary
    {
        public string GuideId { get; set; }
        public string Image { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public bool FullScreen { get; set; }
    }

    public class Ads
    {
        public bool CanShowAds { get; set; }
        public bool CanShowPrerollAds { get; set; }
        public bool CanShowCompanionAds { get; set; }
    }

    public class Echo
    {
        public bool CanEcho { get; set; }
        public int EchoCount { get; set; }
        public string TargetItemId { get; set; }
        public string Scope { get; set; }
        public string Url { get; set; }
        public string FeedTag { get; set; }
    }

    public class Donate
    {
        public bool CanDonate { get; set; }
    }

    public class Share
    {
        public bool CanShare { get; set; }
        public string ShareUrl { get; set; }
    }

    public class Option
    {
        public string Title { get; set; }
        public string GuideId { get; set; }
        public bool IsFollowing { get; set; }
        public string Url { get; set; }
    }

    public class Follow
    {
        public List<Option> Options { get; set; }
    }

    public class Classification
    {
        public string ContentType { get; set; }
        public bool IsEvent { get; set; }
        public bool IsOnDemand { get; set; }
        public bool IsFamilyContent { get; set; }
        public bool IsMatureContent { get; set; }
    }

    public class Link
    {
        public string WebUrl { get; set; }
    }

    public class RootObject
    {
        public Header Header { get; set; }
        public Primary Primary { get; set; }
        public Secondary Secondary { get; set; }
        public Ads Ads { get; set; }
        public Echo Echo { get; set; }
        public Donate Donate { get; set; }
        public Share Share { get; set; }
        public Follow Follow { get; set; }
        public Classification Classification { get; set; }
        public Link Link { get; set; }
        public int Ttl { get; set; }
        public string Token { get; set; }
    }
}
