using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using CommercialFreeRadio.Impl;

namespace CommercialFreeRadio.Test.Impl
{
    [TestFixture]
    public class PlaylistSublimeFmInterfaceTest
    {
        [Test]
        public void ParseXml()
        {
            var sut = new StationSublimeFm.PlaylistSublimeFmInterface(false, CreateReader("sublimenowonair.xml"));

            var tracks = sut.ReadTracks();

            foreach (var t in tracks)
            {
                Console.WriteLine(t);
            }
            Assert.AreEqual(16, tracks.Count);
            Assert.AreEqual("4-3-2016 06:59:56", tracks.First().StartTime.ToString());
            Assert.AreEqual("4-3-2016 07:00:06", tracks.First().EndTime.ToString());
        }

        private StationSublimeFm.IXmlInterfaceSublimeFm CreateReader(string file)
        {
            return new SublimeFmFileInterface(typeof(PlaylistSublimeFmInterfaceTest).Assembly.GetManifestResourceStream("CommercialFreeRadio.Test.Impl." + file));
        }

        public class SublimeFmFileInterface : StationSublimeFm.IXmlInterfaceSublimeFm
        {
            private readonly Stream stream;

            public SublimeFmFileInterface(Stream stream)
            {
                this.stream = stream;
            }

            public string ReadXmlData()
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
